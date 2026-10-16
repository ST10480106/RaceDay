using Microsoft.AspNetCore.Mvc;
using RaceDay.Database;
using RaceDay.Models;

namespace RaceDay.Controllers
{//start of namespace

    [ApiController]
    public class ResultsController : BaseApiController
    {//start of class

        private readonly results_database _results;
        private readonly enrolments_database _enrolments;
        private readonly events_database _events;

        public ResultsController(results_database results, enrolments_database enrolments, events_database events)
        {
            _results = results;
            _enrolments = enrolments;
            _events = events;
        }

        /// <summary>Captures a finish time and position for a Participant's enrolment in the Organiser's event.</summary>
        /// <remarks>
        /// Inputs are query parameters: enrolmentId, finishTime (hh:mm:ss) and/or position (1 or higher),
        /// e.g. ?enrolmentId=1&amp;finishTime=00:45:10&amp;position=1.
        /// Returns 201 with the result, 400 bad data or cancelled enrolment, 401, 403 (not an Organiser or not your event),
        /// 404 enrolment not found, 409 result already captured.
        /// </remarks>
        [HttpPost("api/results")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult CaptureResult([FromQuery] ResultRequest request)
        {
            IActionResult? denied = RequireRole("Organiser");
            if (denied != null) return denied;

            if (request.FinishTime == null && request.Position == null)
            {
                return BadRequest(new { message = "Provide a finishTime and/or a position." });
            }
            if (request.Position != null && request.Position < 1)
            {
                return BadRequest(new { message = "Position must be 1 or higher." });
            }
            if (request.FinishTime != null && request.FinishTime < TimeSpan.Zero)
            {
                return BadRequest(new { message = "Finish time cannot be negative." });
            }

            Enrolment? enrolment = _enrolments.get_enrolment_by_id(request.EnrolmentId);
            if (enrolment == null) return NotFound(new { message = $"Enrolment {request.EnrolmentId} not found." });
            if (_enrolments.get_organiser_id(request.EnrolmentId) != CurrentUserId) return Forbidden("This enrolment is not for one of your events.");
            if (enrolment.Status == "Cancelled") return BadRequest(new { message = "This enrolment was cancelled." });

            if (_results.result_exists(request.EnrolmentId))
            {
                return Conflict(new { message = "A result was already captured for this enrolment." });
            }

            Result result = new Result
            {
                EnrolmentId = request.EnrolmentId,
                CapturedBy = CurrentUserId!,
                FinishTime = request.FinishTime,
                Position = request.Position
            };

            Result created = _results.create_result(result);
            ResultView? view = _results.get_view_by_id(created.ResultId);

            return Created($"/api/results/{created.ResultId}", view);
        }

        /// <summary>Lists the results of all participants across every event owned by the logged-in Organiser.</summary>
        /// <remarks>Returns 200 with the results (participant, event, category, finish time, position), 401 or 403.</remarks>
        [HttpGet("api/results")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult AllMyEventResults()
        {
            IActionResult? denied = RequireRole("Organiser");
            if (denied != null) return denied;

            return Ok(_results.get_views_by_organiser(CurrentUserId!));
        }

        /// <summary>Lists the results of all participants for one event owned by the logged-in Organiser.</summary>
        /// <remarks>Returns 200 with the results, 401, 403 (not an Organiser or not your event) or 404.</remarks>
        [HttpGet("api/events/{eventId:int}/results")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult EventResults(int eventId)
        {
            IActionResult? denied = RequireRole("Organiser");
            if (denied != null) return denied;

            Event? ev = _events.get_event_by_id(eventId);
            if (ev == null) return NotFound(new { message = $"Event {eventId} not found." });
            if (ev.OrganiserId != CurrentUserId) return Forbidden("You can only view results for your own events.");

            return Ok(_results.get_views_by_event(eventId));
        }

        /// <summary>Lists all results belonging to the logged-in Participant.</summary>
        /// <remarks>Returns 200 with the results, 401 not logged in, 403 not a Participant.</remarks>
        [HttpGet("api/results/my")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult MyResults()
        {
            IActionResult? denied = RequireRole("Participant");
            if (denied != null) return denied;

            return Ok(_results.get_views_by_participant(CurrentUserId!));
        }

        /// <summary>Gets one result (only the owning Participant or the Organiser of that event).</summary>
        /// <remarks>Returns 200 with the result, 401 not logged in, 403 not yours, 404 not found.</remarks>
        [HttpGet("api/results/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetResult(int id)
        {
            IActionResult? denied = RequireLogin();
            if (denied != null) return denied;

            ResultView? view = _results.get_view_by_id(id);
            if (view == null) return NotFound(new { message = $"Result {id} not found." });

            bool allowed = view.ParticipantId == CurrentUserId
                        || view.CapturedBy == CurrentUserId
                        || view.OrganiserId == CurrentUserId;

            if (!allowed) return Forbidden("You cannot view this result.");

            return Ok(view);
        }

    }//end of class

}//end of namespace