using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RaceDay.Database;
using RaceDay.Models;

namespace RaceDay.Controllers
{//start of namespace

    [ApiController]
    public class EnrolmentsController : BaseApiController
    {//start of class

        private readonly enrolments_database _enrolments;
        private readonly categories_database _categories;
        private readonly events_database _events;

        public EnrolmentsController(enrolments_database enrolments, categories_database categories, events_database events)
        {
            _enrolments = enrolments;
            _categories = categories;
            _events = events;
        }

        /// <summary>Enrols the logged-in Participant into a category.</summary>
        /// <remarks>
        /// Expects categoryId. The enrolment links the Participant, the category and (through the category) the event, with status Pending.
        /// Returns 201 with the enrolment, 400 bad data, 401 not logged in, 403 not a Participant, 404 category not found, 409 already enrolled.
        /// </remarks>
        [HttpPost("api/enrolments")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Enrol(EnrolRequest request)
        {
            //only Participants can enrol (Organisers get 403, guests get 401)
            IActionResult? denied = RequireRole("Participant");
            if (denied != null) return denied;

            if (request.CategoryId <= 0)
            {
                return BadRequest(new { message = "A valid categoryId is required." });
            }

            if (_categories.get_category_by_id(request.CategoryId) == null)
            {
                return NotFound(new { message = $"Category {request.CategoryId} not found." });
            }

            //a participant can only enter the same category once
            if (_enrolments.enrolment_exists(CurrentUserId!, request.CategoryId))
            {
                return Conflict(new { message = "You are already enrolled in this category." });
            }

            //the Participant id comes from the session, so nobody can enrol someone else
            Enrolment enrolment = new Enrolment
            {
                ParticipantId = CurrentUserId!,
                CategoryId = request.CategoryId,
                Status = "Pending",
                EnrolledTime = DateTime.Now
            };

            Enrolment created = _enrolments.create_enrolment(enrolment);

            //returns the enrolment with the participant, category and event names filled in
            EnrolmentView? view = _enrolments.get_view_by_id(created.EnrolmentId);

            return Created("/api/enrolments/my", view);
        }

        /// <summary>Lists all enrolments of the logged-in Participant.</summary>
        /// <remarks>No body. Returns 200 with the enrolments, 401 not logged in, 403 not a Participant.</remarks>
        [HttpGet("api/enrolments/my")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult MyEnrolments()
        {
            IActionResult? denied = RequireRole("Participant");
            if (denied != null) return denied;

            return Ok(_enrolments.get_views_by_participant(CurrentUserId!));
        }

        /// <summary>Cancels the Participant's own enrolment (status becomes Cancelled).</summary>
        /// <remarks>No body. Returns 200 with the enrolment, 401, 403 (not a Participant or not your enrolment) or 404.</remarks>
        [HttpPut("api/enrolments/{id:int}/cancel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Cancel(int id)
        {
            IActionResult? denied = RequireRole("Participant");
            if (denied != null) return denied;

            Enrolment? enrolment = _enrolments.get_enrolment_by_id(id);
            if (enrolment == null) return NotFound(new { message = $"Enrolment {id} not found." });

            //a Participant can only cancel their own enrolment
            if (enrolment.ParticipantId != CurrentUserId) return Forbidden("You can only cancel your own enrolments.");

            //the row is kept, only its status changes to Cancelled
            _enrolments.cancel_enrolment(id);
            return Ok(_enrolments.get_view_by_id(id));
        }

        /// <summary>Lists all enrolments for an event owned by the logged-in Organiser.</summary>
        /// <remarks>No body. Returns 200 with the enrolments, 401, 403 (not an Organiser or not your event) or 404 event not found.</remarks>
        [HttpGet("api/events/{eventId:int}/enrolments")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult EventEnrolments(int eventId)
        {
            IActionResult? denied = RequireRole("Organiser");
            if (denied != null) return denied;

            Event? ev = _events.get_event_by_id(eventId);
            if (ev == null) return NotFound(new { message = $"Event {eventId} not found." });

            //an Organiser only sees the enrolments of their own events
            if (ev.OrganiserId != CurrentUserId) return Forbidden("You can only view enrolments for your own events.");

            return Ok(_enrolments.get_views_by_event(eventId));
        }

    }//end of class

}//end of namespace