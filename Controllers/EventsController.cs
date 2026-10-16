using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RaceDay.Database;
using RaceDay.Models;

namespace RaceDay.Controllers
{//start of namespace

    [Route("api/events")]
    [ApiController]
    public class EventsController : BaseApiController
    {//start of class

        private readonly events_database _events;

        public EventsController(events_database events)
        {
            _events = events;
        }

        //checks the event data and returns a list of problems (empty list = valid)
        private static List<string> validate(EventRequest request)
        {
            List<string> errors = new List<string>();
            if (string.IsNullOrWhiteSpace(request.EventName)) errors.Add("Event name is required.");
            if (string.IsNullOrWhiteSpace(request.Location)) errors.Add("Location is required.");
            if (request.EventDate == default) errors.Add("Event date is required (yyyy-MM-dd).");
            if (request.EventType != "Run" && request.EventType != "Walk" && request.EventType != "Cycle")
            {
                errors.Add("Event type must be Run, Walk or Cycle.");
            }
            return errors;
        }

        /// <summary>Lists all events (open to guests).</summary>
        /// <remarks>No body. Returns 200 with the list of events ordered by date.</remarks>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetEvents()
        {
            //public endpoint: no login check, guests can browse events
            return Ok(_events.get_all_events());
        }

        /// <summary>Gets one event's details (open to guests).</summary>
        /// <remarks>No body. Returns 200 with the event, or 404 if it does not exist.</remarks>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetEvent(int id)
        {
            Event? ev = _events.get_event_by_id(id);
            if (ev == null) return NotFound(new { message = $"Event {id} not found." });

            return Ok(ev);
        }

        /// <summary>Creates a new event owned by the logged-in Organiser.</summary>
        /// <remarks>
        /// Expects eventName, description, eventDate (yyyy-MM-dd), location and eventType (Run, Walk or Cycle).
        /// Returns 201 with the event, 400 for bad data, 401 if not logged in, 403 if not an Organiser.
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult CreateEvent(EventRequest request)
        {
            //role check first: Participants get 403, guests get 401
            IActionResult? denied = RequireRole("Organiser");
            if (denied != null) return denied;

            List<string> errors = validate(request);
            if (errors.Count > 0) return BadRequest(new { errors });

            //the owner is taken from the session, never from the request body
            Event ev = new Event
            {
                OrganiserId = CurrentUserId!,
                EventName = request.EventName.Trim(),
                Description = request.Description,
                EventDate = request.EventDate,
                Location = request.Location.Trim(),
                EventType = request.EventType
            };

            Event created = _events.create_event(ev);
            return Created($"/api/events/{created.EventId}", created);
        }

        /// <summary>Updates an event owned by the logged-in Organiser.</summary>
        /// <remarks>
        /// Expects eventName, description, eventDate, location and eventType.
        /// Returns 200 with the updated event, 400 bad data, 401 not logged in, 403 not an Organiser or not your event, 404 not found.
        /// </remarks>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateEvent(int id, EventRequest request)
        {
            IActionResult? denied = RequireRole("Organiser");
            if (denied != null) return denied;

            List<string> errors = validate(request);
            if (errors.Count > 0) return BadRequest(new { errors });

            Event? ev = _events.get_event_by_id(id);
            if (ev == null) return NotFound(new { message = $"Event {id} not found." });

            //ownership check: an Organiser can only change their own events
            if (ev.OrganiserId != CurrentUserId) return Forbidden("You can only change your own events.");

            ev.EventName = request.EventName.Trim();
            ev.Description = request.Description;
            ev.EventDate = request.EventDate;
            ev.Location = request.Location.Trim();
            ev.EventType = request.EventType;

            _events.update_event(ev);
            return Ok(ev);
        }

        /// <summary>Deletes an event owned by the logged-in Organiser (with its categories, routes, enrolments and results).</summary>
        /// <remarks>No body. Returns 200 with a confirmation, 401 not logged in, 403 not an Organiser or not your event, 404 not found.</remarks>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteEvent(int id)
        {
            IActionResult? denied = RequireRole("Organiser");
            if (denied != null) return denied;

            Event? ev = _events.get_event_by_id(id);
            if (ev == null) return NotFound(new { message = $"Event {id} not found." });
            if (ev.OrganiserId != CurrentUserId) return Forbidden("You can only delete your own events.");

            //the database class also removes everything that belongs to the event
            _events.delete_event(id);
            return Ok(new { message = $"Event {id} deleted." });
        }

    }//end of class

}//end of namespace