namespace мне_бы_жить_в_шоколаде.Entities;

public class UiChatMessage
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public Guid SenderId { get; set; }
    public string MessageText { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public bool IsOwnMessage { get; set; }
}