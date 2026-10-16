using Microsoft.AspNetCore.Mvc;
using RaceDay.Database;
using RaceDay.Models;

namespace RaceDay.Controllers
{//start of namespace

    [ApiController]
    public class RoutesController : BaseApiController
    {//start of class

        //routes_database uses Entity Framework Core, categories_database uses SqlClient
        private readonly routes_database _routes;
        private readonly categories_database _categories;

        public RoutesController(routes_database routes, categories_database categories)
        {
            _routes = routes;
            _categories = categories;
        }

        /// <summary>Lists all route maps of a category (open to guests).</summary>
        /// <remarks>No body. Returns 200 with the routes, or 404 if the category does not exist.</remarks>
        [HttpGet("api/categories/{categoryId:int}/routes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetRoutes(int categoryId)
        {
            //public endpoint, but the category must exist
            if (_categories.get_category_by_id(categoryId) == null)
            {
                return NotFound(new { message = $"Category {categoryId} not found." });
            }

            return Ok(_routes.get_routes_by_category(categoryId));
        }

        /// <summary>Adds a route map to a category owned by the logged-in Organiser.</summary>
        /// <remarks>Expects mapFileUrl. Returns 201 with the route, 400, 401, 403 or 404.</remarks>
        [HttpPost("api/categories/{categoryId:int}/routes")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult CreateRoute(int categoryId, RouteRequest request)
        {
            IActionResult? denied = RequireRole("Organiser");
            if (denied != null) return denied;

            if (string.IsNullOrWhiteSpace(request.MapFileUrl))
            {
                return BadRequest(new { message = "mapFileUrl is required." });
            }

            //the category must exist and belong to one of the Organiser's events
            if (_categories.get_category_by_id(categoryId) == null) return NotFound(new { message = $"Category {categoryId} not found." });
            if (_categories.get_organiser_id(categoryId) != CurrentUserId) return Forbidden("This category belongs to another Organiser's event.");

            RouteMap route = new RouteMap { CategoryId = categoryId, MapFileUrl = request.MapFileUrl.Trim() };
            RouteMap created = _routes.create_route(route);

            return Created($"/api/categories/{categoryId}/routes", created);
        }

        /// <summary>Updates a route map.</summary>
        /// <remarks>Expects mapFileUrl. Returns 200 with the route, 400, 401, 403 or 404.</remarks>
        [HttpPut("api/routes/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateRoute(int id, RouteRequest request)
        {
            IActionResult? denied = RequireRole("Organiser");
            if (denied != null) return denied;

            if (string.IsNullOrWhiteSpace(request.MapFileUrl))
            {
                return BadRequest(new { message = "mapFileUrl is required." });
            }

            RouteMap? route = _routes.get_route_by_id(id);
            if (route == null) return NotFound(new { message = $"Route {id} not found." });

            //ownership check: route -> category -> event -> organiser
            if (_routes.get_organiser_id(id) != CurrentUserId) return Forbidden("This route belongs to another Organiser's event.");

            route.MapFileUrl = request.MapFileUrl.Trim();
            _routes.update_route(route);

            return Ok(route);
        }

        /// <summary>Deletes a route map.</summary>
        /// <remarks>No body. Returns 200 with a confirmation, 401, 403 or 404.</remarks>
        [HttpDelete("api/routes/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteRoute(int id)
        {
            IActionResult? denied = RequireRole("Organiser");
            if (denied != null) return denied;

            if (_routes.get_route_by_id(id) == null) return NotFound(new { message = $"Route {id} not found." });
            if (_routes.get_organiser_id(id) != CurrentUserId) return Forbidden("This route belongs to another Organiser's event.");

            _routes.delete_route(id);
            return Ok(new { message = $"Route {id} deleted." });
        }

    }//end of class

}//end of namespace