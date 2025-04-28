using Google.Authenticator;
using M183.Controllers.Dto;
using M183.Controllers.Helper;
using M183.Data;
using M183.Logging;
using M183.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<LoginController> _logger;

        private readonly IConfiguration _configuration;
        private readonly string _appName = "InsecureApp";
        public readonly string _key = "63785462894692873649872364";


        public LoginController(NewsAppContext context, IConfiguration configuration, ILogger<LoginController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;

        }

        [HttpPost("login-step1")]
        public ActionResult LoginStep1(LoginDto request)
        {
            var logEntry = new LoggingModel
            {
                Username = request.Username,
                IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Action = "Login Attempt",
                Input = $"Username: {request.Username}, Password: [REDACTED]",
                Detail = "User attempted to log in."
            };
            LoggingSystem.Log(logEntry);
            _logger.LogInformation("Login attempt for user {Username}", request.Username);



            if (request == null ||
                string.IsNullOrEmpty(request.Username) ||
                string.IsNullOrEmpty(request.Password))
            {
                logEntry.Status = "Failed";
                logEntry.ErrorMessage = "Request was null or missing required fields.";
                LoggingSystem.Log(logEntry);
                _logger.LogWarning("Login failed: Request was null or missing required fields.");

                return BadRequest();
            }

            var user = LoginAction(request);
            if (user == null)
            {
                logEntry.Status = "Failed";
                logEntry.ErrorMessage = "Invalid username or password.";
                LoggingSystem.Log(logEntry);
                _logger.LogWarning("Login failed: Invalid username or password for user {Username}", request.Username);

                return Unauthorized();
            }

            // Generate QR code
            var qrCodeUrl = GenerateQrCodeUrl(user.Username);

            logEntry.Status = "Success";
            logEntry.Detail = "User authenticated successfully. QR code generated for 2FA.";
            LoggingSystem.Log(logEntry);
            _logger.LogInformation("Login successful for user {Username}. QR code generated for 2FA.", user.Username);


            return Ok(new { qrCodeUrl });
        }


        [HttpPost("login-step2")]
        public ActionResult LoginStep2(TwoFactorDto request)
        {
            var logEntry = new LoggingModel
            {
                Username = request.Username,
                IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Action = "2FA Validation Attempt",
                Detail = "User attempted to validate 2FA token."
            };
            LoggingSystem.Log(logEntry);
            _logger.LogInformation("2FA validation attempt for user {Username}", request.Username);


            if (request == null ||
                string.IsNullOrEmpty(request.Username) ||
                string.IsNullOrEmpty(request.TwoFactorToken))
            {

                logEntry.Action = "2FA Validation Failed";
                logEntry.Detail = "Request was null or missing required fields.";
                LoggingSystem.Log(logEntry);
                _logger.LogWarning("2FA validation failed: Request was null or missing required fields.");

                return BadRequest();
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == request.Username);
            if (user == null)
            {
                logEntry.Action = "2FA Validation Failed";
                logEntry.Detail = "User not found.";
                LoggingSystem.Log(logEntry);
                _logger.LogWarning("2FA validation failed: User {Username} not found.", request.Username);

                return Unauthorized();
            }

            if (!ValidateTwoFactorPIN(_key, request.TwoFactorToken))
            {
                logEntry.Action = "2FA Validation Failed";
                logEntry.Detail = "Invalid 2FA token.";
                LoggingSystem.Log(logEntry);
                _logger.LogWarning("2FA validation failed: Invalid token for user {Username}.", request.Username);

                return Unauthorized("Invalid 2FA token");
            }

            // JWT generieren
            string token = GenerateJWT(user);

            logEntry.Action = "2FA Validation Successful";
            logEntry.Detail = "2FA token validated successfully. JWT generated.";
            LoggingSystem.Log(logEntry);
            _logger.LogInformation("2FA validation successful for user {Username}. JWT generated.", request.Username);


            return Ok(new { token });
        }



        [HttpGet("generate-qr-code")]
        public IActionResult GenerateQrCode(string username)
        {
            var logEntry = new LoggingModel
            {
                Username = username,
                IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Action = "QR Code Generation Attempt",
                Detail = "User requested QR code generation."
            };
            LoggingSystem.Log(logEntry);


            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
            {
                logEntry.Action = "QR Code Generation Failed";
                logEntry.Detail = "User not found.";
                LoggingSystem.Log(logEntry);
                return NotFound();
            }

            var tfa = new TwoFactorAuthenticator();
            var setupInfo = tfa.GenerateSetupCode(_appName, user.Username, _key, false, 3);

            logEntry.Action = "QR Code Generation Successful";
            logEntry.Detail = "QR code generated successfully.";
            LoggingSystem.Log(logEntry);

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

            var logEntry = new LoggingModel
            {
                Username = request.Username,
                IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Action = "Database Query",
                Detail = "Attempting to retrieve user from database."
            };
            LoggingSystem.Log(logEntry);
            _logger.LogInformation("Attempting to authenticate user {Username}", request.Username);


            string sql = "SELECT * FROM Users WHERE username = @Username AND password = @Password";

            try
            {
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
                                logEntry.Action = "Database Query Successful";
                                logEntry.Detail = "User found in database.";
                                LoggingSystem.Log(logEntry);
                                _logger.LogInformation("User {Username} authenticated successfully.", request.Username);


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
                logEntry.Action = "Database Query Failed";
                logEntry.Detail = "User not found or invalid credentials.";
                LoggingSystem.Log(logEntry);
                _logger.LogWarning("Authentication failed for user {Username}.", request.Username);


                return null;
            }
            catch (Exception ex)
            {
                logEntry.Action = "Database Query Failed";
                logEntry.Detail = $"Error: {ex.Message}";
                LoggingSystem.Log(logEntry);
                _logger.LogError(ex, "Database query failed for user {Username}.", request.Username);

                throw;
            }
        }

        private string GenerateJWT(User user)
        {
            var logEntry = new LoggingModel
            {
                Username = user.Username,
                IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Action = "JWT Generation",
                Detail = "Attempting to generate JWT."
            };
            LoggingSystem.Log(logEntry);
            _logger.LogInformation("Generating JWT for user {Username}", user.Username);


            try
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

                logEntry.Action = "JWT Generation Successful";
                logEntry.Detail = "JWT generated successfully.";
                LoggingSystem.Log(logEntry);
                _logger.LogInformation("JWT generated successfully for user {Username}", user.Username);


                return jwtToken;
            }
            catch(Exception ex)
            {
                logEntry.Action = "JWT Generation Failed";
                logEntry.Detail = $"Error: {ex.Message}";
                LoggingSystem.Log(logEntry);
                _logger.LogError(ex, "JWT generation failed for user {Username}.", user.Username);

                throw;
            }
        }


        private bool ValidateTwoFactorPIN(string secretKey, string twoFaktorToken)
        {
            var tfa = new TwoFactorAuthenticator();
            return tfa.ValidateTwoFactorPIN(secretKey, twoFaktorToken);
        }
    }
}
