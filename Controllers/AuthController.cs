using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Database;
using RaceDay.Models;

namespace RaceDay.Controllers
{//start of namespace

    [Route("api/auth")]
    [ApiController]
    public class AuthController : BaseApiController
    {//start of class

        private readonly users_database _users;

        public AuthController(users_database users)
        {
            _users = users;
        }

        /// <summary>Registers a new Organiser or Participant account.</summary>
        /// <remarks>
        /// Expects fullName, email, password (min 6 characters) and role ("Organiser" or "Participant").
        /// Participants must also send dateOfBirth (and optionally gender).
        /// The generated id starts with O (Organiser) or P (Participant), e.g. O0001 / P0001.
        /// Returns 201 with the new profile (no password), 400 for validation errors, 409 if the email is already registered.
        /// </remarks>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Register(RegisterRequest request)
        {//start of register

            //validate the input and collect every problem so the client sees them all at once
            List<string> errors = new List<string>();
            if (string.IsNullOrWhiteSpace(request.FullName)) errors.Add("Full name is required.");
            if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@')) errors.Add("A valid email is required.");
            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6) errors.Add("Password must be at least 6 characters.");

            //the user picks their role at registration, only these two roles exist
            if (request.Role != "Organiser" && request.Role != "Participant")
            {
                errors.Add("Role must be 'Organiser' or 'Participant'.");
            }
            else if (request.Role == "Participant" && request.DateOfBirth == null)
            {
                errors.Add("Date of birth is required for Participants.");
            }

            if (errors.Count > 0)
            {
                return BadRequest(new { errors });
            }

            //emails are stored in lower case so "Ann@x.com" and "ann@x.com" are the same account
            string email = request.Email.Trim().ToLowerInvariant();

            if (_users.email_exists(email))
            {
                return Conflict(new { message = "Email already registered." });
            }

            //hash the password, the original password is never stored
            User user = new User
            {
                FullName = request.FullName.Trim(),
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Role,
                //date of birth and gender only apply to Participants
                DateOfBirth = request.Role == "Participant" ? request.DateOfBirth : null,
                Gender = request.Role == "Participant" ? request.Gender : null
            };

            //the database class generates the id (O0001 / P0001)
            User created = _users.create_user(user);

            return Created("/api/users/profile", ProfileOf(created));

        }//end of register

        /// <summary>Logs in a registered user and starts a session.</summary>
        /// <remarks>
        /// Expects email and password. The password is verified against the stored hash.
        /// Returns 200 with id, name and role, or 401 if the credentials are wrong or no account exists (register first).
        /// </remarks>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Login(LoginRequest request)
        {//start of login

            string email = (request.Email ?? "").Trim().ToLowerInvariant();
            User? user = _users.get_user_by_email(email);

            //the same message is used for "no account" and "wrong password" so nothing is leaked
            //BCrypt compares the typed password with the stored hash
            if (user == null || string.IsNullOrEmpty(request.Password) ||
                !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid credentials or no account found. Please register first." });
            }

            //start the session: the server remembers the user id and role for every later request
            HttpContext.Session.SetString("UserId", user.UserId);
            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetString("FullName", user.FullName);

            return Ok(new { user.UserId, user.FullName, user.Role });

        }//end of login

        /// <summary>Ends the current session.</summary>
        /// <remarks>No body. Returns 200 when logged out, or 401 if nobody was logged in.</remarks>
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Logout()
        {//start of logout

            //only a logged-in user can log out
            IActionResult? denied = RequireLogin();
            if (denied != null) return denied;

            //wipes the user id and role, the user is a guest again
            HttpContext.Session.Clear();
            return Ok(new { message = "Logged out." });

        }//end of logout

    }//end of class

}//end of namespace