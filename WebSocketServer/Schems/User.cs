namespace WebSocketServer.Schems;

public class User
{
    public int ID { get; set; }
    public string Username { get; set; }

    public string Password { get; set; }
    public DateOnly Date_Create { get; set; }

    public int ID_Role { get; set; }
}
