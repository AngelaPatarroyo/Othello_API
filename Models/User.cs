using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Othello_API.Models
{
    public class ApplicationUser : IdentityUser
    {
        // You can add custom fields if needed
        public virtual List<UserGame> UserGames { get; set; } = new();
        public virtual LeaderBoard? LeaderBoard { get; set; }
    }
    
}
