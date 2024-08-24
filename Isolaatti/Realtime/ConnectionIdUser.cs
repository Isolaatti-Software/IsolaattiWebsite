using Microsoft.EntityFrameworkCore;

namespace Isolaatti.Realtime;
[PrimaryKey("ConnectionId")]
public class ConnectionIdUser
{
    public string ConnectionId { get; set; }
    public int UserId { get; set; }
}