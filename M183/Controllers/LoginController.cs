using Google.Authenticator;
using M183.Controllers.Dto;
using M183.Controllers.Helper;
using M183.Data;
using M183.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace M183.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly NewsAppContext _context;
        private readonly IConfiguration _configuration;
        private readonly string _appName = "InsecureApp";
        public readonly string _key = "63785462894692873649872364";


        public LoginController(NewsAppContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login-step1")]
        public ActionResult LoginStep1(LoginDto request)
        {
            if (request == null ||
                string.IsNullOrEmpty(request.Username) ||
                string.IsNullOrEmpty(request.Password))
            {
                return BadRequest();
            }

            var user = LoginAction(request);
            if (user == null)
            {
                return Unauthorized();
            }

            // Generate QR code
            var qrCodeUrl = GenerateQrCodeUrl(user.Username);

            return Ok(new { qrCodeUrl });
        }


        [HttpPost("login-step2")]
        public ActionResult LoginStep2(TwoFactorDto request)
        {
            if (request == null ||
                string.IsNullOrEmpty(request.Username) ||
                string.IsNullOrEmpty(request.TwoFactorToken))
            {
                return BadRequest();
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == request.Username);
            if (user == null)
            {
                return Unauthorized();
            }

            if (!ValidateTwoFactorPIN(_key, request.TwoFactorToken))
            {
                return Unauthorized("Invalid 2FA token");
            }

            // JWT generieren
            string token = GenerateJWT(user);
            Console.WriteLine("Generated JWT: " + token); // Debugging line

            return Ok(new { token });
        }



        [HttpGet("generate-qr-code")]
        public IActionResult GenerateQrCode(string username)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
            {
                return NotFound();
            }

            var tfa = new TwoFactorAuthenticator();
            var setupInfo = tfa.GenerateSetupCode(_appName, user.Username, _key, false, 3);
            return Ok(new { qrCodeUrl = setupInfo.QrCodeSetupImageUrl });
        }

        private string GenerateQrCodeUrl(string username)
        {
            var tfa = new TwoFactorAuthenticator();
            var setupInfo = tfa.GenerateSetupCode(_appName, username, _key, false, 3);
            return setupInfo.QrCodeSetupImageUrl;
        }

        private User? LoginAction(LoginDto request)
        {
            string sql = "SELECT * FROM Users WHERE username = @Username AND password = @Password";

            using (SqlConnection connection = new SqlConnection(
                _context.Database.GetDbConnection().ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Username", request.Username);
                    command.Parameters.AddWithValue("@Password", MD5Helper.ComputeMD5Hash(request.Password));

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Username = reader.GetString(reader.GetOrdinal("username")),
                                Password = reader.GetString(reader.GetOrdinal("password")),
                                IsAdmin = reader.GetBoolean(reader.GetOrdinal("IsAdmin")),
                            };
                        }
                    }
                }
            }
            return null;
        }

        private string GenerateJWT(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(ClaimTypes.Role, user.IsAdmin ? "admin" : "user")
            };

            string base64Key = _configuration["Jwt:Key"];
            var key = new SymmetricSecurityKey(Convert.FromBase64String(base64Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            string jwtToken = tokenHandler.WriteToken(token);

            return jwtToken;
        }

        private bool ValidateTwoFactorPIN(string secretKey, string twoFaktorToken)
        {
            var tfa = new TwoFactorAuthenticator();
            return tfa.ValidateTwoFactorPIN(secretKey, twoFaktorToken);
        }
    }
}
