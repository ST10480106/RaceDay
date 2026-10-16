using System.Data.SqlClient;
using RaceDay.Models;
using static RaceDay.Database.db_connection;

namespace RaceDay.Database
{//start of namespace

    public class results_database
    {//start of class

        //joins the result with the enrolment, participant, category and event
        private const string select_views = @"
            SELECT rs.ResultId, rs.EnrolmentId, en.ParticipantId, u.FullName AS ParticipantName,
                   c.EventId, ev.EventName, ev.OrganiserId, en.CategoryId, c.CategoryName,
                   rs.FinishTime, rs.Position, rs.CapturedBy
            FROM [Result] rs
            JOIN Enrolment en ON en.EnrolmentId = rs.EnrolmentId
            JOIN [User] u ON u.UserId = en.ParticipantId
            JOIN Category c ON c.CategoryId = en.CategoryId
            JOIN [Event] ev ON ev.EventId = c.EventId ";

        private static ResultView read_view(SqlDataReader r)
        {
            return new ResultView
            {
                ResultId = (int)r["ResultId"],
                EnrolmentId = (int)r["EnrolmentId"],
                ParticipantId = (string)r["ParticipantId"],
                ParticipantName = (string)r["ParticipantName"],
                EventId = (int)r["EventId"],
                EventName = (string)r["EventName"],
                OrganiserId = (string)r["OrganiserId"],
                CategoryId = (int)r["CategoryId"],
                CategoryName = (string)r["CategoryName"],
                FinishTime = r["FinishTime"] is TimeSpan finish ? finish : (TimeSpan?)null,
                Position = r["Position"] is int position ? position : (int?)null,
                CapturedBy = (string)r["CapturedBy"]
            };
        }

        public virtual bool result_exists(int enrolment_id)
        {
            object? count = scalar("SELECT COUNT(*) FROM [Result] WHERE EnrolmentId = @id;", param("@id", enrolment_id));
            return Convert.ToInt32(count) > 0;
        }

        public virtual Result create_result(Result result)
        {
            object? new_id = scalar(@"
                INSERT INTO [Result] (EnrolmentId, CapturedBy, FinishTime, Position)
                OUTPUT INSERTED.ResultId
                VALUES (@enrolmentId, @capturedBy, @finishTime, @position);",
                param("@enrolmentId", result.EnrolmentId),
                param("@capturedBy", result.CapturedBy),
                param("@finishTime", result.FinishTime),
                param("@position", result.Position));

            result.ResultId = Convert.ToInt32(new_id);
            return result;
        }

        public virtual ResultView? get_view_by_id(int result_id)
        {
            return query_single(select_views + "WHERE rs.ResultId = @id;", read_view, param("@id", result_id));
        }

        //all results across every event owned by one Organiser
        public virtual List<ResultView> get_views_by_organiser(string organiser_id)
        {
            return query_list(select_views + "WHERE ev.OrganiserId = @id ORDER BY ev.EventId, c.CategoryId, rs.Position;",
                read_view, param("@id", organiser_id));
        }

        public virtual List<ResultView> get_views_by_event(int event_id)
        {
            return query_list(select_views + "WHERE c.EventId = @id ORDER BY c.CategoryId, rs.Position;",
                read_view, param("@id", event_id));
        }

        public virtual List<ResultView> get_views_by_participant(string participant_id)
        {
            return query_list(select_views + "WHERE en.ParticipantId = @id ORDER BY ev.EventDate DESC;",
                read_view, param("@id", participant_id));
        }

    }//end of class

}//end of namespace