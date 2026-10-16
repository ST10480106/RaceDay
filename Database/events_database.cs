using Microsoft.Extensions.Logging;
using RaceDay.Models;
using System.Data.SqlClient;
using static RaceDay.Database.db_connection;

namespace RaceDay.Database
{//start of namespace

    public class events_database
    {//start of class

        private const string select_events =
            "SELECT EventId, OrganiserId, EventName, Description, EventDate, Location, EventType FROM [Event] ";

        private static Event read_event(SqlDataReader r)
        {
            return new Event
            {
                EventId = (int)r["EventId"],
                OrganiserId = (string)r["OrganiserId"],
                EventName = (string)r["EventName"],
                Description = r["Description"] is string description ? description : null,
                EventDate = DateOnly.FromDateTime((DateTime)r["EventDate"]),
                Location = (string)r["Location"],
                EventType = (string)r["EventType"]
            };
        }

        public virtual Event create_event(Event ev)
        {
            object? new_id = scalar(@"
                INSERT INTO [Event] (OrganiserId, EventName, Description, EventDate, Location, EventType)
                OUTPUT INSERTED.EventId
                VALUES (@organiserId, @eventName, @description, @eventDate, @location, @eventType);",
                param("@organiserId", ev.OrganiserId),
                param("@eventName", ev.EventName),
                param("@description", ev.Description),
                param("@eventDate", ev.EventDate.ToDateTime(TimeOnly.MinValue)),
                param("@location", ev.Location),
                param("@eventType", ev.EventType));

            ev.EventId = Convert.ToInt32(new_id);
            return ev;
        }

        public virtual List<Event> get_all_events()
        {
            return query_list(select_events + "ORDER BY EventDate;", read_event);
        }

        public virtual Event? get_event_by_id(int event_id)
        {
            return query_single(select_events + "WHERE EventId = @id;", read_event, param("@id", event_id));
        }

        public virtual bool update_event(Event ev)
        {
            int rows = execute(@"
                UPDATE [Event]
                SET EventName = @eventName, Description = @description, EventDate = @eventDate,
                    Location = @location, EventType = @eventType
                WHERE EventId = @id;",
                param("@eventName", ev.EventName),
                param("@description", ev.Description),
                param("@eventDate", ev.EventDate.ToDateTime(TimeOnly.MinValue)),
                param("@location", ev.Location),
                param("@eventType", ev.EventType),
                param("@id", ev.EventId));

            return rows > 0;
        }

        //deletes the event and everything that belongs to it (all in one transaction)
        public virtual void delete_event(int event_id)
        {
            execute(@"
                SET XACT_ABORT ON;
                BEGIN TRANSACTION;

                DELETE FROM [Result] WHERE EnrolmentId IN
                    (SELECT EnrolmentId FROM Enrolment WHERE CategoryId IN
                        (SELECT CategoryId FROM Category WHERE EventId = @id));
                DELETE FROM Enrolment WHERE CategoryId IN
                    (SELECT CategoryId FROM Category WHERE EventId = @id);
                DELETE FROM [Route] WHERE CategoryId IN
                    (SELECT CategoryId FROM Category WHERE EventId = @id);
                DELETE FROM Category WHERE EventId = @id;
                DELETE FROM [Event] WHERE EventId = @id;

                COMMIT TRANSACTION;",
                param("@id", event_id));
        }

    }//end of class

}//end of namespace