using M183.Controllers.Dto;
using M183.Controllers.Helper;
using M183.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace M183.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly NewsAppContext _context;

        public UserController(NewsAppContext context)
        {
            _context = context;
        }

        /// <summary>
        /// update password
        /// </summary>
        /// <response code="200">Password updated successfully</response>
        /// <response code="400">Bad request</response>
        /// <response code="404">User not found</response>
        [HttpPatch("password-update")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public ActionResult PasswordUpdate(PasswordUpdateDto request)
        {
            if (request == null)
            {
                return BadRequest();
            }

            var user = _context.Users.Find(request.UserId);
            if (user == null)
            {
                return NotFound(string.Format("User {0} not found", request.UserId));
            }

            var passwordRuleResult = IsPasswordConfirm(request.NewPassword);
            if (passwordRuleResult.Any(x => x.Item1 == true))
            {

                return BadRequest(passwordRuleResult.Where(x => x.Item1 == true).Select(x => x.Item2).ToList());
            }

            if (ConfirmPassword(request.OldPassword, request.UserId))
            {
                user.Password = MD5Helper.ComputeMD5Hash(request.NewPassword);

            }
            else
            {
                return BadRequest("Old password not confirmed");
            }
            _context.Users.Update(user);
            _context.SaveChanges();

            HttpContext.SignOutAsync();
            return Ok("Password changed");
        }

        private (bool, string)[] IsPasswordConfirm(string password)
        {
            (bool, string)[] breach = new (bool, string)[4];

            if (password.Length < 8)
            {
                breach[0] = (true, "Password must be at least 8 characters long.");
            }
            if (!password.Any(char.IsLower))
            {
                breach[1] = (true, "Password must contain at least one lowercase letter.");
            }
            if (!password.Any(char.IsUpper))
            {
                breach[2] = (true, "Password must contain at least one uppercase letter.");
            }
            if (!password.Any(char.IsNumber))
            {
                breach[3] = (true, "Password must contain at least one number.");
            }

            return breach;

        }

        private bool ConfirmPassword(string oldPassword, int userId)
        {
            var User = _context.Users.Find(userId);
            if (User == null) { return false; }

            if (MD5Helper.ComputeMD5Hash(oldPassword) == User.Password)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
