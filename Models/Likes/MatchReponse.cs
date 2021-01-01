using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Models.Likes
{
    public class MatchReponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime LastActive { get; set; }
        public string PhotoUrl { get; set; }
        public string Gender { get; set; }

        public IEnumerable<string> FcmTokens { get; set; }
    }
}
