using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Othello_API.Models;

public class ApplicationUser : IdentityUser
{
    [JsonIgnore]
    public virtual List<UserGame> UserGames { get; set; } = new();

    [JsonIgnore]
    public virtual LeaderBoard? LeaderBoard { get; set; }
}
