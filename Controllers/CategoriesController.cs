using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RaceDay.Database;
using RaceDay.Models;

namespace RaceDay.Controllers
{//start of namespace

    [ApiController]
    public class CategoriesController : BaseApiController
    {//start of class

        private readonly categories_database _categories;
        private readonly events_database _events;

        public CategoriesController(categories_database categories, events_database events)
        {
            _categories = categories;
            _events = events;
        }

        //a category needs a name and a distance greater than 0
        private static bool invalid(CategoryRequest request)
        {
            return string.IsNullOrWhiteSpace(request.CategoryName) || request.Distance <= 0;
        }

        /// <summary>Lists all categories of an event (open to guests).</summary>
        /// <remarks>No body. Returns 200 with the categories, or 404 if the event does not exist.</remarks>
        [HttpGet("api/events/{eventId:int}/categories")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetCategories(int eventId)
        {
            //public endpoint, but the event must exist
            if (_events.get_event_by_id(eventId) == null)
            {
                return NotFound(new { message = $"Event {eventId} not found." });
            }

            return Ok(_categories.get_categories_by_event(eventId));
        }

        /// <summary>Creates a category under an event owned by the logged-in Organiser.</summary>
        /// <remarks>
        /// Expects categoryName (e.g. "Under 20", "10km") and distance in km (greater than 0).
        /// Returns 201 with the category, 400 bad data, 401 not logged in, 403 not an Organiser or not your event, 404 event not found.
        /// </remarks>
        [HttpPost("api/events/{eventId:int}/categories")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult CreateCategory(int eventId, CategoryRequest request)
        {
            IActionResult? denied = RequireRole("Organiser");
            if (denied != null) return denied;

            if (invalid(request))
            {
                return BadRequest(new { message = "Category name and a distance greater than 0 are required." });
            }

            //the event must exist and belong to the logged-in Organiser
            Event? ev = _events.get_event_by_id(eventId);
            if (ev == null) return NotFound(new { message = $"Event {eventId} not found." });
            if (ev.OrganiserId != CurrentUserId) return Forbidden("You can only add categories to your own events.");

            Category category = new Category
            {
                EventId = eventId,
                CategoryName = request.CategoryName.Trim(),
                Distance = request.Distance
            };

            Category created = _categories.create_category(category);
            return Created($"/api/events/{eventId}/categories", created);
        }

        /// <summary>Updates a category that belongs to the Organiser's event.</summary>
        /// <remarks>Expects categoryName and distance. Returns 200 with the category, 400, 401, 403 or 404.</remarks>
        [HttpPut("api/categories/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateCategory(int id, CategoryRequest request)
        {
            IActionResult? denied = RequireRole("Organiser");
            if (denied != null) return denied;

            if (invalid(request))
            {
                return BadRequest(new { message = "Category name and a distance greater than 0 are required." });
            }

            Category? category = _categories.get_category_by_id(id);
            if (category == null) return NotFound(new { message = $"Category {id} not found." });

            //ownership check goes through the event that the category belongs to
            if (_categories.get_organiser_id(id) != CurrentUserId) return Forbidden("This category belongs to another Organiser's event.");

            category.CategoryName = request.CategoryName.Trim();
            category.Distance = request.Distance;

            _categories.update_category(category);
            return Ok(category);
        }

        /// <summary>Deletes a category (with its routes, enrolments and results) from the Organiser's event.</summary>
        /// <remarks>No body. Returns 200 with a confirmation, 401, 403 or 404.</remarks>
        [HttpDelete("api/categories/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteCategory(int id)
        {
            IActionResult? denied = RequireRole("Organiser");
            if (denied != null) return denied;

            if (_categories.get_category_by_id(id) == null) return NotFound(new { message = $"Category {id} not found." });
            if (_categories.get_organiser_id(id) != CurrentUserId) return Forbidden("This category belongs to another Organiser's event.");

            _categories.delete_category(id);
            return Ok(new { message = $"Category {id} deleted." });
        }

    }//end of class

}//end of namespace