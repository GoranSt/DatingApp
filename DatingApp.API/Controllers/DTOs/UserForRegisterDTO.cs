using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Controllers.DTOs
{
    public class UserForRegisterDTO
    {
        [Required]
        public string Username { get; set; }     

        [Required]
        [StringLength(8, MinimumLength = 4, ErrorMessage = "You must specify password between 0 and 8 characters")]
        public string Password { get; set; } 
    }
}