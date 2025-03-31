using Google.Authenticator;
using M183.Models;
using Microsoft.EntityFrameworkCore;

namespace M183.Data
{
    public class NewsAppInitializer
    {
        private readonly ModelBuilder modelBuilder;
        public readonly string _key = "63785462894692873649872364";

        public NewsAppInitializer(ModelBuilder modelBuilder)
        {
            this.modelBuilder = modelBuilder;
        }

        public void Seed()
        {
            modelBuilder.Entity<User>()
                .HasData(
                    new User
                    {
                        Id = 2,
                        Username = "user",
                        Password = "966154FCCAF76A277B4F69B738C89C96",
                        IsAdmin = false,
                        SecretKey = GenerateSetupCode("user")
                    },
                    new User
                    {
                        Id = 1,
                        Username = "administrator",
                        Password = "4E2539B3E890F7765063E00230C684A0",
                        IsAdmin = true,
                        SecretKey = GenerateSetupCode("administrator")
                    }
            );

            modelBuilder.Entity<News>()
               .HasData(
                   new News
                   {
                       Id = 1,
                       Header = "Normaler Beitrag",
                       Detail = "Das ist ein normaler News Beitrag vom Benutzer 'user'",
                       AuthorId = 2,
                       IsAdminNews = false,
                       PostedDate = new DateTime()
                   },
                   new News
                   {
                       Id = 2,
                       Header = "Admin News",
                       Detail = "Das ist ein News Beitrag vom Benutzer 'admin'",
                       AuthorId = 1,
                       IsAdminNews = true,
                       PostedDate = new DateTime()
                   }
               );
        }

        private string GenerateSetupCode(string username)
        {
            var tfa = new TwoFactorAuthenticator();
          
            var setupInfo = tfa.GenerateSetupCode("InsecureApp", username, _key, false, 3);
            return setupInfo.ManualEntryKey;
        }
    }
}
