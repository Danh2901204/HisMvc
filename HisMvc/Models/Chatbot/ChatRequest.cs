namespace HisMvc.Models.Chatbot;

/// <summary>Dữ liệu gửi lên API chatbot từ cổng người bệnh.</summary>
public class ChatRequest
{
    /// <summary>Mã phiên hội thoại (UUID) để lưu ngữ cảnh.</summary>
    public string SessionId { get; set; } = "";

    /// <summary>Nội dung tin nhắn của người bệnh.</summary>
    public string Message { get; set; } = "";
}
