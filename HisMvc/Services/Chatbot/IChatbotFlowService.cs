using HisMvc.Models.Chatbot;

namespace HisMvc.Services.Chatbot;

/// <summary>Luồng đặt lịch, hủy lịch và tra khung giờ trống trong chatbot.</summary>
public interface IChatbotFlowService
{
    bool IsExitCommand(string message);
    bool IsBookingIntent(string message);
    bool IsCancelIntent(string message);
    bool IsSlotsIntent(string message);
    Task<ChatResponse?> TryContinueFlowAsync(string sessionId, string message, CancellationToken ct);
    Task<ChatResponse> StartBookingFlowAsync(string sessionId, CancellationToken ct);
    Task<ChatResponse> StartCancelFlowAsync(string sessionId, string? code, CancellationToken ct);
    Task<ChatResponse> QueryAvailableSlotsAsync(string message, CancellationToken ct);
    void ClearFlow(string sessionId);
}
