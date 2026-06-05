using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using HisMvc.Data;
using HisMvc.Entities;
using HisMvc.Models.Chatbot;
using HisMvc.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace HisMvc.Services.Chatbot;

/// <summary>Chatbot: Gemini 2.5 Flash, FAQ dự phòng, luồng đặt/hủy lịch.</summary>
public class ChatbotService : IChatbotService
{
    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ChatbotService> _logger;
    private readonly IChatbotFlowService _flow;
    private readonly IPublicAppointmentService _appointments;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private const int MaxHistoryMessages = 16;
    private static readonly TimeSpan SessionTtl = TimeSpan.FromHours(2);

    public ChatbotService(
        AppDbContext db,
        IMemoryCache cache,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<ChatbotService> logger,
        IChatbotFlowService flow,
        IPublicAppointmentService appointments)
    {
        _db = db;
        _cache = cache;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
        _flow = flow;
        _appointments = appointments;
    }

    public async Task<ChatResponse> ProcessMessageAsync(ChatRequest request, CancellationToken cancellationToken = default)
    {
        var sessionId = (request.SessionId ?? "").Trim();
        var message = (request.Message ?? "").Trim();

        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("SessionId không được để trống.");
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message không được để trống.");
        if (message.Length > 500)
            throw new ArgumentException("Tin nhắn không được vượt quá 500 ký tự.");

        var history = GetHistory(sessionId);

        if (_flow.IsExitCommand(message))
        {
            _flow.ClearFlow(sessionId);
            return Remember(sessionId, history, message,
                Reply("Đã **thoát luồng** hiện tại. Bạn có thể hỏi tự do hoặc chọn nút bên dưới."));
        }

        var flowReply = await _flow.TryContinueFlowAsync(sessionId, message, cancellationToken);
        if (flowReply != null)
            return Remember(sessionId, history, message, flowReply);

        var lookupCode = ChatbotTextHelper.ExtractAppointmentCode(message)
                         ?? (IsLookupIntent(message) ? ExtractCodeFromHistory(history) : null);

        if (!string.IsNullOrEmpty(lookupCode))
            return Remember(sessionId, history, message, await LookupAppointmentAsync(lookupCode, cancellationToken));

        if (IsLookupIntent(message))
        {
            return Remember(sessionId, history, message, Reply(
                "Vui lòng **dán mã đặt lịch** (ví dụ: APT20260527143022) vào đây, tôi sẽ tra cứu và trả kết quả ngay cho bạn.",
                ["Tra cứu APT", "Đặt lịch khám"]));
        }

        if (IsEmergency(message))
        {
            return Remember(sessionId, history, message, Reply(
                "⚠️ **Trường hợp khẩn cấp:** Nếu có dấu hiệu nguy hiểm, hãy **gọi 115** hoặc đến **khoa Cấp cứu 24/7** ngay. Hotline bệnh viện: **1900 1009**.",
                action: "call:115",
                suggestions: ["Gọi hotline"]));
        }

        if (_flow.IsBookingIntent(message))
            return Remember(sessionId, history, message, await _flow.StartBookingFlowAsync(sessionId, cancellationToken));

        if (_flow.IsCancelIntent(message))
        {
            var code = ChatbotTextHelper.ExtractAppointmentCode(message);
            return Remember(sessionId, history, message, await _flow.StartCancelFlowAsync(sessionId, code, cancellationToken));
        }

        if (_flow.IsSlotsIntent(message))
            return Remember(sessionId, history, message, await _flow.QueryAvailableSlotsAsync(message, cancellationToken));

        if (IsGeminiEnabled())
        {
            var deptContext = await BuildDepartmentContextAsync(cancellationToken);
            var aiReply = await CallGeminiAsync(message, history, deptContext, cancellationToken);
            if (!string.IsNullOrWhiteSpace(aiReply))
                return Remember(sessionId, history, message, Reply(aiReply));

            _logger.LogWarning("Gemini không phản hồi cho session {SessionId}, chuyển FAQ", sessionId);
        }

        return Remember(sessionId, history, message, Reply(BuildFaqReply(message)));
    }

    private static ChatResponse Reply(
        string reply,
        IReadOnlyList<string>? suggestions = null,
        string? action = null) =>
        ChatbotResponseBuilder.Create(reply, suggestions, action);

    private ChatResponse Remember(string sessionId, List<ChatTurn> history, string userMessage, ChatResponse response)
    {
        AppendHistory(sessionId, history, userMessage, response.Reply);
        return response;
    }

    private List<ChatTurn> GetHistory(string sessionId) =>
        _cache.TryGetValue(CacheKey(sessionId), out List<ChatTurn>? list) && list != null
            ? list
            : [];

    private void AppendHistory(string sessionId, List<ChatTurn> history, string userMsg, string botReply)
    {
        history.Add(new ChatTurn("user", userMsg));
        history.Add(new ChatTurn("assistant", botReply));
        if (history.Count > MaxHistoryMessages)
            history.RemoveRange(0, history.Count - MaxHistoryMessages);

        _cache.Set(CacheKey(sessionId), history, SessionTtl);
    }

    private static string CacheKey(string sessionId) => $"chatbot:session:{sessionId}";

    private bool IsGeminiEnabled() =>
        _configuration.GetValue("Gemini:Enabled", true) &&
        !string.IsNullOrWhiteSpace(_configuration["Gemini:ApiKey"]);

    private async Task<string> BuildDepartmentContextAsync(CancellationToken ct)
    {
        try
        {
            var names = await DepartmentBookingRules
                .BookableForPublic(_db.Departments)
                .OrderBy(d => d.Name)
                .Select(d => d.Name)
                .Take(8)
                .ToListAsync(ct);

            return names.Count > 0
                ? "\n- Chuyên khoa đặt lịch online: " + string.Join(", ", names)
                : "";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Không tải được danh sách chuyên khoa cho chatbot");
            return "";
        }
    }

    private async Task<string?> CallGeminiAsync(
        string userMessage,
        IReadOnlyList<ChatTurn> history,
        string hospitalContext,
        CancellationToken ct)
    {
        var apiKey = _configuration["Gemini:ApiKey"]!;
        var model = _configuration["Gemini:Model"] ?? "gemini-2.5-flash";
        var baseUrl = (_configuration["Gemini:BaseUrl"] ?? "https://generativelanguage.googleapis.com/v1beta").TrimEnd('/');

        var systemPrompt = """
            Bạn là trợ lý AI của Bệnh viện Đa khoa trên cổng thông tin công cộng Việt Nam.
            Trả lời bằng TIẾNG VIỆT CÓ DẤU, ngắn gọn, thân thiện, chuyên nghiệp.

            QUY TẮC:
            1. KHÔNG chẩn đoán bệnh, KHÔNG kê đơn, KHÔNG thay thế bác sĩ.
            2. Triệu chứng nguy hiểm: khuyên gọi 115 hoặc cấp cứu ngay.
            3. Hướng dẫn tự nhiên về đặt lịch, hotline, giờ làm việc, chuyên khoa, BHYT.
            4. Nếu người dùng gửi mã APT, hệ thống đã tự tra cứu — bạn chỉ bổ sung giải thích nếu cần.
            5. Hệ thống hỗ trợ **đặt lịch**, **hủy lịch**, **xem khung giờ khám** ngay trong chat — gợi người dùng nhắn "đặt lịch", "hủy lịch", "xem khung giờ" khi phù hợp.
            6. Trả lời linh hoạt, không bắt buộc chuyển sang trang web nếu có thể trả lời trực tiếp.

            THÔNG TIN BV:
            - Hotline: 1900 1009 | Cấp cứu: 115 (24/7)
            - Địa chỉ: 78 Giải Phóng, Phường Phương Mai, Quận Đống Đa, Hà Nội
            - Giờ làm việc: T2–T6 7:00–17:00; T7 7:00–12:00; Cấp cứu 24/7
            """ + hospitalContext;

        var contents = history.TakeLast(8).Select(turn => new
        {
            role = turn.Role == "assistant" ? "model" : "user",
            parts = new[] { new { text = turn.Content } }
        }).Append(new
        {
            role = "user",
            parts = new[] { new { text = userMessage } }
        }).ToList<object>();

        var payload = new
        {
            systemInstruction = new { parts = new[] { new { text = systemPrompt } } },
            contents,
            generationConfig = new { temperature = 0.4, maxOutputTokens = 600 }
        };

        try
        {
            var client = _httpClientFactory.CreateClient("Gemini");
            var url = $"{baseUrl}/models/{model}:generateContent?key={Uri.EscapeDataString(apiKey)}";
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(JsonSerializer.Serialize(payload, JsonOpts), Encoding.UTF8, "application/json");

            using var response = await client.SendAsync(request, ct);
            var body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Gemini HTTP {Status}: {Body}", response.StatusCode, body);
                return null;
            }

            using var doc = JsonDocument.Parse(body);
            var candidates = doc.RootElement.GetProperty("candidates");
            if (candidates.GetArrayLength() == 0) return null;

            var parts = candidates[0].GetProperty("content").GetProperty("parts");
            return parts.GetArrayLength() == 0 ? null : parts[0].GetProperty("text").GetString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi gọi Google Gemini");
            return null;
        }
    }

    private static string BuildFaqReply(string message)
    {
        if (ChatbotTextHelper.Matches(message, "xin chao", "chao", "hello", "hi"))
            return "Xin chào! Tôi có thể hướng dẫn **đặt lịch khám**, **tra cứu mã APT**, **giờ làm việc** và **hotline 1900 1009**.";

        if (ChatbotTextHelper.Matches(message, "dat lich", "dang ky", "hen kham", "kham benh", "lich hen"))
            return "Nhắn **\"đặt lịch\"** để tôi hướng dẫn đặt ngay trong chat (chọn khoa → bác sĩ → giờ → thông tin BN).";

        if (ChatbotTextHelper.Matches(message, "tra cuu", "kiem tra", "ma lich", "ma hen", "trang thai"))
            return "Bạn có thể **dán mã APT** vào đây (ví dụ APT20260527143022), tôi sẽ tra cứu và hiển thị kết quả ngay trong khung chat.";

        if (ChatbotTextHelper.Matches(message, "khung gio", "gio trong", "con lich"))
            return "Nhắn **\"xem khung giờ\"** kèm tên khoa và ngày (vd: khung giờ Nội ngày mai) để xem các giờ có thể đặt.";

        if (ChatbotTextHelper.Matches(message, "gio lam viec", "mo cua", "may gio", "thu 7"))
            return "Giờ làm việc: T2–T6 **7:00–17:00**, T7 **7:00–12:00**. **Cấp cứu 24/7**. Hotline: **1900 1009**.";

        if (ChatbotTextHelper.Matches(message, "hotline", "dien thoai", "lien he", "tu van"))
            return "Hotline **1900 1009** | Cấp cứu **115** | 78 Giải Phóng, Hà Nội.";

        if (ChatbotTextHelper.Matches(message, "bhyt", "bao hiem", "the bhyt"))
            return "Mang **thẻ BHYT** và **CMND/CCCD** khi đến khám. Chi tiết hưởng BHYT được hướng dẫn tại quầy Tiếp đón.";

        if (ChatbotTextHelper.Matches(message, "huy lich", "doi lich", "huy hen"))
            return "Nhắn **\"hủy lịch\"** kèm mã APT — tôi sẽ xác thực SĐT và hủy ngay trong chat.";

        return "Tôi chưa hiểu rõ câu hỏi. Bạn có thể nhắn **đặt lịch**, **tra cứu APT**, **xem khung giờ**, **hủy lịch** hoặc gọi **1900 1009**.";
    }

    private static bool IsEmergency(string text) =>
        ChatbotTextHelper.Matches(text, "dau nguc", "kho tho", "mat y thuc", "co giat", "chay mau nhieu", "ngat", "dot quy", "cap cuu");

    private async Task<ChatResponse> LookupAppointmentAsync(string code, CancellationToken ct)
    {
        var appointment = await _appointments.FindByCodeAsync(code, tracking: false, ct);
        if (appointment == null)
        {
            return Reply(
                $"❌ Không tìm thấy lịch hẹn với mã **{code.Trim().ToUpperInvariant()}**.\n\nVui lòng kiểm tra lại mã hoặc gọi hotline **1900 1009** để được hỗ trợ.");
        }

        var timeSlot = appointment.TimeSlot != null
            ? $"{appointment.TimeSlot.Start:HH:mm} - {appointment.TimeSlot.End:HH:mm}"
            : "—";
        var doctor = string.IsNullOrWhiteSpace(appointment.Doctor?.FullName)
            ? "Chưa phân công"
            : appointment.Doctor.FullName;
        var noteBlock = string.IsNullOrWhiteSpace(appointment.Note)
            ? ""
            : $"\n**Ghi chú:** {appointment.Note}";

        var reply = $"""
            ✅ **Kết quả tra cứu lịch hẹn**

            **Mã đặt lịch:** {appointment.Code}
            **Bệnh nhân:** {appointment.Patient?.FullName ?? "—"}
            **Chuyên khoa:** {appointment.Department?.Name ?? "—"}
            **Bác sĩ:** {doctor}
            **Ngày khám:** {appointment.Date:dd/MM/yyyy}
            **Giờ khám:** {timeSlot}
            **Trạng thái:** {GetStatusLabel(appointment.Status)}{noteBlock}
            """;

        return Reply(reply.Trim(), ["Tra cứu APT", "Hủy lịch", "Đặt lịch khám"], $"lookup:{appointment.Code}");
    }

    private static string GetStatusLabel(AppointmentStatus status) => status switch
    {
        AppointmentStatus.Booked => "Chờ tiếp đón",
        AppointmentStatus.CheckedIn => "Đang khám",
        AppointmentStatus.Completed => "Đã hoàn tất",
        AppointmentStatus.Cancelled => "Đã hủy",
        AppointmentStatus.NoShow => "Không đến khám",
        _ => "Không xác định"
    };

    private static string? ExtractCodeFromHistory(IReadOnlyList<ChatTurn> history)
    {
        foreach (var turn in history.AsEnumerable().Reverse().Take(6))
        {
            if (turn.Role != "user") continue;
            var code = ChatbotTextHelper.ExtractAppointmentCode(turn.Content);
            if (code != null) return code;
        }

        return null;
    }

    private static bool IsLookupIntent(string message) =>
        ChatbotTextHelper.Matches(message, "tra cuu", "kiem tra", "ma lich", "ma hen", "trang thai", "check", "xem lich");

    private sealed record ChatTurn(string Role, string Content);
}
