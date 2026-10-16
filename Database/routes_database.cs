using Microsoft.EntityFrameworkCore;
using RaceDay.Database;
using RaceDay.Models;

namespace RaceDay.Database
{//start of namespace

    //the Routes resource uses Entity Framework Core (the other resources use SqlClient)
    public class routes_database
    {//start of class

        //the EF Core context is handed in by dependency injection (see Program.cs)
        private readonly RaceDayContext _context;

        public routes_database(RaceDayContext context)
        {
            _context = context;
        }

        //INSERT: EF fills in the generated RouteId after SaveChanges
        public virtual RouteMap create_route(RouteMap route)
        {
            _context.Routes.Add(route);
            _context.SaveChanges();
            return route;
        }

        //SELECT all routes of one category (AsNoTracking = read only, a bit faster)
        public virtual List<RouteMap> get_routes_by_category(int category_id)
        {
            return _context.Routes.AsNoTracking().Where(r => r.CategoryId == category_id).ToList();
        }

        public virtual RouteMap? get_route_by_id(int route_id)
        {
            return _context.Routes.AsNoTracking().FirstOrDefault(r => r.RouteId == route_id);
        }

        //finds the Organiser who owns the event this route belongs to
        //(route -> category -> event -> organiser, used for the "is this your route?" check)
        public virtual string? get_organiser_id(int route_id)
        {
            return (from r in _context.Routes
                    join c in _context.Categories on r.CategoryId equals c.CategoryId
                    join e in _context.Events on c.EventId equals e.EventId
                    where r.RouteId == route_id
                    select e.OrganiserId).FirstOrDefault();
        }

        //UPDATE: returns true when a row was changed
        public virtual bool update_route(RouteMap route)
        {
            _context.Routes.Update(route);
            return _context.SaveChanges() > 0;
        }

        //DELETE straight in the database without loading the row first
        public virtual void delete_route(int route_id)
        {
            _context.Routes.Where(r => r.RouteId == route_id).ExecuteDelete();
        }

    }//end of class

}//end of namespace