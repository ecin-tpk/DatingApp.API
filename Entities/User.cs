using System;
using System.Collections.Generic;

namespace DatingApp.API.Entities
{
    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public byte[] PasswordHash { get; set; }

        public byte[] PasswordSalt { get; set; }

        public string VerificationToken { get; set; }

        public DateTime? Verified { get; set; }

        public DateTime? PasswordReset { get; set; }

        public bool IsVerified => Verified.HasValue || PasswordReset.HasValue;

        public string ResetToken { get; set; }

        public DateTime? ResetTokenExpires { get; set; }

        public DateTime Created { get; set; }

        public DateTime? Updated { get; set; }

        public string Phone { get; set; }

        public string Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public bool HideAge { get; set; }

        public DateTime LastActive { get; set; }

        public string Introduction { get; set; }

        public string Interests { get; set; }

        public int Height { get; set; }

        public string SexualOrientation { get; set; }

        public string JobTitle { get; set; }

        public string School { get; set; }

        public string Company { get; set; }

        public string City { get; set; }

        public string Country { get; set; }

        public Role Role { get; set; }

        public Status Status { get; set; }

        public ICollection<Photo> Photos { get; set; }

        public ICollection<Like> Likers { get; set; }

        public ICollection<Like> Likees { get; set; }

        public ICollection<Message> MessagesSent { get; set; }

        public ICollection<Message> MessagesReceived { get; set; }

        public List<RefreshToken> RefreshTokens { get; set; }

        public bool OwnsToken(string token)
        {
            return this.RefreshTokens?.Find(t => t.Token == token) != null;
        }
    }
}
