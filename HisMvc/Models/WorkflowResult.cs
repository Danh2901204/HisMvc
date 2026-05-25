namespace HisMvc.Models;

/// <summary>
/// Kết quả tra ve tu cac buoc luồng KCB.
/// Controller chi can doc Success + Message roi hien thi cho nguoi dung.
/// </summary>
public class WorkflowResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = "";

    public static WorkflowResult Ok(string message) => new() { Success = true, Message = message };
    public static WorkflowResult Fail(string message) => new() { Success = false, Message = message };
}
