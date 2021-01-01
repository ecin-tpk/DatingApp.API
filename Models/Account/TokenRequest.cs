using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Models.Account
{
    public class TokenRequest
    {
        [Required]
        public string Token { get; set; }
    }
}
