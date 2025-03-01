using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;


public class ApplicationUser : IdentityUser
{
    [JsonIgnore]
    public virtual List<UserGame> UserGames { get; set; } = new();

    [JsonIgnore]
    public virtual LeaderBoard? LeaderBoard { get; set; }
}
