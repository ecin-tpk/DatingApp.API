using System;
using System.Collections.Generic;

namespace DatingApp.API.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string FacebookUID { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public string VerificationToken { get; set; }
        public DateTime? Verified { get; set; }
        public DateTime? PasswordReset { get; set; }
        public bool IsVerified => Verified.HasValue || PasswordReset.HasValue;
        public string ResetToken { get; set; }
        public DateTime? ResetTokenExpires { get; set; }
        public Role Role { get; set; }
        public Status Status { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }

        public string Name { get; set; }
        public string Gender { get; set; }
        public string LookingFor { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime LastActive { get; set; }
        public string Location { get; set; }
        public string Bio { get; set; }
        public string JobTitle { get; set; }
        public string School { get; set; }
        public string Company { get; set; }
        // More details
        public string Ethnicity { get; set; }
        public string Religion { get; set; }
        public byte Height { get; set; }
        public byte Weight { get; set; }
        public string SexualOrientation { get; set; }
        public string HairColor { get; set; }
        public string EyeColor { get; set; }
        public string LiveWith { get; set; }
        public string Children { get; set; }
        public string FamilyPlan { get; set; }
        public string Smoking { get; set; }
        public string Drinking { get; set; }

        public bool HideAge { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public List<FcmToken> FcmTokens { get; set; }

        public List<RefreshToken> RefreshTokens { get; set; }
        public ICollection<Photo> Photos { get; set; }
        public ICollection<Interest> Activities { get; set; }
        public ICollection<Like> Likers { get; set; }
        public ICollection<Like> Likees { get; set; }
        public ICollection<Message> MessagesSent { get; set; }
        public ICollection<Message> MessagesReceived { get; set; }

        public bool OwnsToken(string token)
        {
            return RefreshTokens?.Find(t => t.Token == token) != null;
        }

        public bool OwnsFcmToken(string token)
        {
            return FcmTokens?.Find(t => t.Token == token) != null;
        }
    }
}
