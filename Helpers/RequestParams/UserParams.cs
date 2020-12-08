using DatingApp.API.Entities;

namespace DatingApp.API.Helpers.RequestParams
{
    public class UserParams : PaginationParams
    {
        public int UserId { get; set; }
        public string Gender { get; set; }
        public int MinAge { get; set; } = 18;
        public int MaxAge { get; set; } = 99;
        public int MinHeight { get; set; }
        public int MaxHeight { get; set; }

        public bool ForCards { get; set; }
        public bool Likees { get; set; } = false;
        public bool Likers { get; set; } = false;
        public bool IsMatched { get; set; }
        public bool TopPicks { get; set; }

        public bool ForAdmin { get; set; }
        public string Name { get; set; }
        public string OrderBy { get; set; }
        public string Verification { get; set; }
        public Status Status { get; set; }

        public UserParams() : base(10) { }
    }
}
