namespace WebSocketServer.Schems;

public class Message
{
    public int ID_Message { get; set; }
    public string Message_Text { get; set; }

    public DateTime Datetime_Create { get; set; }
    public int ID_User { get; set; }
}
