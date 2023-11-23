using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TDList.Models
{
    public class User : IdentityUser<int>
    {
        [Required]
        public string? Login { get; set; }
        public string? RoleName { get; set; }
        public bool IsConfirmed { get; set; } = false;
    }
}
