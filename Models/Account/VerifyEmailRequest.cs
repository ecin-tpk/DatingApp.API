using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Models.Account
{
    public class VerifyEmailRequest
    {
        [Required]
        public string Token { get; set; }
    }
}
