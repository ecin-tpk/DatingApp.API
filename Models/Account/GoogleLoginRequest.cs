using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Models.Account
{
    public class GoogleLoginRequest
    {
        [Required]
        public string GoogleToken { get; set; }
    }
}
