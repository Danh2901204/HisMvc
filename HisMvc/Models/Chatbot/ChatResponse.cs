namespace HisMvc.Models.Chatbot;

/// <summary>Phản hồi JSON trả về cho frontend chat widget.</summary>
public class ChatResponse
{
    public bool Success { get; set; }
    public string Reply { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    /// <summary>lookup:APT... | navigate:dat-lich | call:19001009 | call:115</summary>
    public string? Action { get; set; }
    public List<string> Suggestions { get; set; } = [];
}
