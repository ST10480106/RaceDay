using System.Data.SqlClient;
using RaceDay.Models;
using static RaceDay.Database.db_connection;

namespace RaceDay.Database
{//start of namespace

    public class categories_database
    {//start of class

        private const string select_categories =
            "SELECT CategoryId, EventId, CategoryName, Distance FROM Category ";

        private static Category read_category(SqlDataReader r)
        {
            return new Category
            {
                CategoryId = (int)r["CategoryId"],
                EventId = (int)r["EventId"],
                CategoryName = (string)r["CategoryName"],
                Distance = (decimal)r["Distance"]
            };
        }

        public virtual Category create_category(Category category)
        {
            object? new_id = scalar(@"
                INSERT INTO Category (EventId, CategoryName, Distance)
                OUTPUT INSERTED.CategoryId
                VALUES (@eventId, @categoryName, @distance);",
                param("@eventId", category.EventId),
                param("@categoryName", category.CategoryName),
                param("@distance", category.Distance));

            category.CategoryId = Convert.ToInt32(new_id);
            return category;
        }

        public virtual List<Category> get_categories_by_event(int event_id)
        {
            return query_list(select_categories + "WHERE EventId = @eventId ORDER BY Distance;",
                read_category, param("@eventId", event_id));
        }

        public virtual Category? get_category_by_id(int category_id)
        {
            return query_single(select_categories + "WHERE CategoryId = @id;",
                read_category, param("@id", category_id));
        }

        //finds the Organiser who owns the event this category belongs to
        public virtual string? get_organiser_id(int category_id)
        {
            object? owner = scalar(@"
                SELECT e.OrganiserId FROM Category c
                JOIN [Event] e ON e.EventId = c.EventId
                WHERE c.CategoryId = @id;",
                param("@id", category_id));

            return owner as string;
        }

        public virtual bool update_category(Category category)
        {
            int rows = execute(@"
                UPDATE Category SET CategoryName = @categoryName, Distance = @distance
                WHERE CategoryId = @id;",
                param("@categoryName", category.CategoryName),
                param("@distance", category.Distance),
                param("@id", category.CategoryId));

            return rows > 0;
        }

        //deletes the category with its routes, enrolments and results
        public virtual void delete_category(int category_id)
        {
            execute(@"
                SET XACT_ABORT ON;
                BEGIN TRANSACTION;

                DELETE FROM [Result] WHERE EnrolmentId IN
                    (SELECT EnrolmentId FROM Enrolment WHERE CategoryId = @id);
                DELETE FROM Enrolment WHERE CategoryId = @id;
                DELETE FROM [Route] WHERE CategoryId = @id;
                DELETE FROM Category WHERE CategoryId = @id;

                COMMIT TRANSACTION;",
                param("@id", category_id));
        }

    }//end of class

}//end of namespace