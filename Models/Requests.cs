using System.ComponentModel.DataAnnotations;

namespace RaceDay.Models
{//start of namespace

    //these classes are the JSON bodies the client sends
    //the /// comments and <example> values are shown in Swagger under "Example Value" and "Schema"

    /// <summary>Data needed to create an account.</summary>
    public class RegisterRequest
    {
        /// <summary>Full name of the person.</summary>
        /// <example>Sam Mokoena</example>
        [Required] public string FullName { get; set; } = string.Empty;

        /// <summary>Email address (used to log in, must be unique).</summary>
        /// <example>sam@example.com</example>
        [Required] public string Email { get; set; } = string.Empty;

        /// <summary>Password, at least 6 characters (stored hashed).</summary>
        /// <example>Pass123!</example>
        [Required] public string Password { get; set; } = string.Empty;

        /// <summary>"Organiser" or "Participant"</summary>
        /// <example>Participant</example>
        [Required] public string Role { get; set; } = string.Empty;

        /// <summary>Participants only (yyyy-MM-dd)</summary>
        /// <example>2000-01-31</example>
        public DateOnly? DateOfBirth { get; set; }

        /// <summary>Participants only</summary>
        /// <example>Male</example>
        public string? Gender { get; set; }
    }

    /// <summary>Credentials used to log in.</summary>
    public class LoginRequest
    {
        /// <summary>The email used when registering.</summary>
        /// <example>sam@example.com</example>
        [Required] public string Email { get; set; } = string.Empty;

        /// <summary>The account password.</summary>
        /// <example>Pass123!</example>
        [Required] public string Password { get; set; } = string.Empty;
    }

    /// <summary>New values for the logged-in user's own profile.</summary>
    public class UpdateProfileRequest
    {
        /// <example>Sam Mokoena</example>
        [Required] public string FullName { get; set; } = string.Empty;

        /// <example>sam@example.com</example>
        [Required] public string Email { get; set; } = string.Empty;

        /// <summary>Participants only (yyyy-MM-dd)</summary>
        /// <example>2000-01-31</example>
        public DateOnly? DateOfBirth { get; set; }

        /// <summary>Participants only</summary>
        /// <example>Male</example>
        public string? Gender { get; set; }
    }

    /// <summary>Details of an event.</summary>
    public class EventRequest
    {
        /// <example>Comrades Marathon</example>
        [Required] public string EventName { get; set; } = string.Empty;

        /// <example>Celebration of human resilience</example>
        public string? Description { get; set; }

        /// <summary>Date of the event (yyyy-MM-dd)</summary>
        /// <example>2027-06-13</example>
        public DateOnly EventDate { get; set; }

        /// <example>Durban</example>
        [Required] public string Location { get; set; } = string.Empty;

        /// <summary>Run, Walk or Cycle</summary>
        /// <example>Run</example>
        [Required] public string EventType { get; set; } = "Run";
    }

    /// <summary>An age or distance group inside an event.</summary>
    public class CategoryRequest
    {
        /// <summary>For example Under 20, Senior, 10km, 21km</summary>
        /// <example>10km</example>
        [Required] public string CategoryName { get; set; } = string.Empty;

        /// <summary>Distance in km (greater than 0)</summary>
        /// <example>10</example>
        public decimal Distance { get; set; }
    }

    /// <summary>A route map for a category.</summary>
    public class RouteRequest
    {
        /// <summary>Link to the route map file</summary>
        /// <example>https://example.com/maps/10km.pdf</example>
        [Required] public string MapFileUrl { get; set; } = string.Empty;
    }

    /// <summary>The category a Participant wants to enter.</summary>
    public class EnrolRequest
    {
        /// <summary>Id of the category to enter</summary>
        /// <example>1</example>
        public int CategoryId { get; set; }
    }

    /// <summary>The finish time and position of one enrolment.</summary>
    public class ResultRequest
    {
        /// <summary>Id of the enrolment the result belongs to</summary>
        /// <example>1</example>
        public int EnrolmentId { get; set; }

        /// <summary>Format hh:mm:ss e.g. 00:42:15</summary>
        /// <example>00:45:10</example>
        public TimeSpan? FinishTime { get; set; }

        /// <summary>Finishing position (1 or higher)</summary>
        /// <example>1</example>
        public int? Position { get; set; }
    }

}//end of namespace