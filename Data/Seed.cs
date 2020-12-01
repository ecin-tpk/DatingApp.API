using System.Collections.Generic;
using System.Linq;
using DatingApp.API.Entities;
using DatingApp.API.Helpers;
using Newtonsoft.Json;

namespace DatingApp.API.Data
{
    public static class Seed
    {
        public static void SeedUsers(DataContext context)
        {
            if (!context.Users.Any())
            {
                var userData = System.IO.File.ReadAllText("Data/UserSeedData.json");
                var users = JsonConvert.DeserializeObject<List<User>>(userData);
                foreach (var user in users)
                {
                    byte[] passwordHash, passwordSalt;
                    CreatePasswordHash("password", out passwordHash, out passwordSalt);

                    user.PasswordHash = passwordHash;
                    user.PasswordSalt = passwordSalt;
                    user.Email = user.Email.ToLower();
                    context.Users.Add(user);
                }

                context.SaveChanges();
            }

            if (!context.Activities.Any())
            {
                var activityData = System.IO.File.ReadAllText("Data/ActivitySeedData.json");
                var activities = JsonConvert.DeserializeObject<List<Activity>>(activityData);
                foreach (var activity in activities)
                {
                    context.Activities.Add(activity);
                }

                context.SaveChanges();
            }
        }

        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (
            var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
    }
}
