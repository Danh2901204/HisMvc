using HisMvc.Models.Chatbot;

namespace HisMvc.Services.Chatbot;

/// <summary>Tạo phản hồi chatbot thống nhất.</summary>
public static class ChatbotResponseBuilder
{
    public static IReadOnlyList<string> DefaultSuggestions { get; } =
    [
        "Đặt lịch khám",
        "Tra cứu APT",
        "Xem khung giờ",
        "Hủy lịch",
        "Gọi hotline"
    ];

    public static ChatResponse Create(
        string reply,
        IReadOnlyList<string>? suggestions = null,
        string? action = null) =>
        new()
        {
            Success = true,
            Reply = reply,
            Timestamp = DateTime.UtcNow,
            Action = action,
            Suggestions = suggestions?.ToList() ?? DefaultSuggestions.ToList()
        };
}
