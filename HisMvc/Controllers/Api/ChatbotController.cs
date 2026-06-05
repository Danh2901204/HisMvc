using HisMvc.Models.Chatbot;
using HisMvc.Services.Chatbot;
using Microsoft.AspNetCore.Mvc;

namespace HisMvc.Controllers.Api;

/// <summary>
/// API Chatbot AI phục vụ cổng thông tin người bệnh.
/// Endpoint: POST /api/chatbot/chat
/// </summary>
[ApiController]
[Route("api/chatbot")]
public class ChatbotController : ControllerBase
{
    private readonly IChatbotService _chatbotService;
    private readonly ILogger<ChatbotController> _logger;

    public ChatbotController(IChatbotService chatbotService, ILogger<ChatbotController> logger)
    {
        _chatbotService = chatbotService;
        _logger = logger;
    }

    /// <summary>Nhận tin nhắn từ widget chat và trả về phản hồi AI.</summary>
    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] ChatRequest? request, CancellationToken cancellationToken)
    {
        // Kiểm tra dữ liệu đầu vào
        if (request == null ||
            string.IsNullOrWhiteSpace(request.SessionId) ||
            string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new ChatResponse
            {
                Success = false,
                Reply = "SessionId và Message là bắt buộc, không được để trống.",
                Timestamp = DateTime.UtcNow
            });
        }

        try
        {
            var result = await _chatbotService.ProcessMessageAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Dữ liệu chatbot không hợp lệ. SessionId={SessionId}", request.SessionId);
            return BadRequest(new ChatResponse
            {
                Success = false,
                Reply = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi hệ thống khi xử lý chatbot. SessionId={SessionId}", request.SessionId);
            return StatusCode(500, new ChatResponse
            {
                Success = false,
                Reply = "Hệ thống tạm thời không xử lý được yêu cầu. Vui lòng thử lại sau hoặc gọi hotline 1900 1009.",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
