using Microsoft.AspNetCore.Mvc;
using RaceDay.Models;

namespace RaceDay.Controllers
{//start of namespace

    //every controller inherits from this class, so the login and role checks are written only once
    public abstract class BaseApiController : ControllerBase
    {//start of class

        //who is logged in (stored in the session at login)
        protected string? CurrentUserId => HttpContext.Session.GetString("UserId");
        protected string? CurrentRole => HttpContext.Session.GetString("Role");

        //returns 401 if nobody is logged in, otherwise null
        protected IActionResult? RequireLogin()
        {
            if (CurrentUserId == null)
            {
                return Unauthorized(new { message = "You must be logged in." });
            }
            return null;
        }

        //returns 401 (not logged in) or 403 (wrong role), otherwise null
        protected IActionResult? RequireRole(string role)
        {
            //401 = we do not know who you are, 403 = we know you, but you may not do this
            IActionResult? not_logged_in = RequireLogin();
            if (not_logged_in != null) return not_logged_in;

            if (CurrentRole != role)
            {
                return Forbidden($"Only {role}s can use this endpoint.");
            }
            return null;
        }

        protected IActionResult Forbidden(string message)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message });
        }

        //profile without the password hash
        protected static object ProfileOf(User user)
        {
            return new { user.UserId, user.FullName, user.Email, user.Role, user.DateOfBirth, user.Gender };
        }

    }//end of class

}//end of namespace