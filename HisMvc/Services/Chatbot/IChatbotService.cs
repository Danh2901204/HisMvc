using HisMvc.Models.Chatbot;

namespace HisMvc.Services.Chatbot;

/// <summary>Dịch vụ xử lý hội thoại chatbot AI cho người bệnh.</summary>
public interface IChatbotService
{
    /// <summary>Xử lý tin nhắn và trả về câu trả lời AI/FAQ.</summary>
    Task<ChatResponse> ProcessMessageAsync(ChatRequest request, CancellationToken cancellationToken = default);
}
