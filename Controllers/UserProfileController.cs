using Microsoft.AspNetCore.Mvc;
using RaceDay.Database;
using RaceDay.Models;

namespace RaceDay.Controllers
{//start of namespace

    [Route("api/users/profile")]
    [ApiController]
    public class UserProfileController : BaseApiController
    {//start of class

        private readonly users_database _users;

        public UserProfileController(users_database users)
        {
            _users = users;
        }

        /// <summary>Gets the logged-in user's own profile.</summary>
        /// <remarks>No body. Returns 200 with the profile, or 401 if not logged in.</remarks>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetProfile()
        {
            //any logged-in user (either role) may view their own profile
            IActionResult? denied = RequireLogin();
            if (denied != null) return denied;

            //the id comes from the session, so a user can only ever see their own profile
            User? user = _users.get_user_by_id(CurrentUserId!);
            if (user == null) return Unauthorized(new { message = "Account no longer exists." });

            return Ok(ProfileOf(user));
        }

        /// <summary>Updates the logged-in user's own profile.</summary>
        /// <remarks>
        /// Expects fullName and email. Participants may also send dateOfBirth and gender.
        /// Returns 200 with the updated profile, 400 for bad data, 401 if not logged in, 409 if the email belongs to someone else.
        /// </remarks>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult UpdateProfile(UpdateProfileRequest request)
        {
            IActionResult? denied = RequireLogin();
            if (denied != null) return denied;

            if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
            {
                return BadRequest(new { message = "Full name and a valid email are required." });
            }

            User? user = _users.get_user_by_id(CurrentUserId!);
            if (user == null) return Unauthorized(new { message = "Account no longer exists." });

            //the new email may not belong to a different user (the user's own email is ignored)
            string email = request.Email.Trim().ToLowerInvariant();
            if (_users.email_exists(email, user.UserId))
            {
                return Conflict(new { message = "Email already in use." });
            }

            user.FullName = request.FullName.Trim();
            user.Email = email;

            //only Participants have a date of birth and gender
            if (user.Role == "Participant")
            {
                if (request.DateOfBirth != null) user.DateOfBirth = request.DateOfBirth;
                if (request.Gender != null) user.Gender = request.Gender;
            }

            _users.update_user(user);
            return Ok(ProfileOf(user));
        }

    }//end of class

}//end of namespace