using System.Data.SqlClient;
using RaceDay.Models;
using static RaceDay.Database.db_connection;

namespace RaceDay.Database
{//start of namespace

    public class enrolments_database
    {//start of class

        //joins the enrolment with the participant, category and event
        private const string select_views = @"
            SELECT en.EnrolmentId, en.ParticipantId, u.FullName AS ParticipantName,
                   en.CategoryId, c.CategoryName, c.EventId, ev.EventName,
                   en.Status, en.EnrolledTime
            FROM Enrolment en
            JOIN [User] u ON u.UserId = en.ParticipantId
            JOIN Category c ON c.CategoryId = en.CategoryId
            JOIN [Event] ev ON ev.EventId = c.EventId ";

        private static EnrolmentView read_view(SqlDataReader r)
        {
            return new EnrolmentView
            {
                EnrolmentId = (int)r["EnrolmentId"],
                ParticipantId = (string)r["ParticipantId"],
                ParticipantName = (string)r["ParticipantName"],
                CategoryId = (int)r["CategoryId"],
                CategoryName = (string)r["CategoryName"],
                EventId = (int)r["EventId"],
                EventName = (string)r["EventName"],
                Status = (string)r["Status"],
                EnrolledTime = (DateTime)r["EnrolledTime"]
            };
        }

        private static Enrolment read_enrolment(SqlDataReader r)
        {
            return new Enrolment
            {
                EnrolmentId = (int)r["EnrolmentId"],
                ParticipantId = (string)r["ParticipantId"],
                CategoryId = (int)r["CategoryId"],
                Status = (string)r["Status"],
                EnrolledTime = (DateTime)r["EnrolledTime"]
            };
        }

        //a participant can only enter the same category once
        public virtual bool enrolment_exists(string participant_id, int category_id)
        {
            object? count = scalar(
                "SELECT COUNT(*) FROM Enrolment WHERE ParticipantId = @participantId AND CategoryId = @categoryId;",
                param("@participantId", participant_id), param("@categoryId", category_id));

            return Convert.ToInt32(count) > 0;
        }

        public virtual Enrolment create_enrolment(Enrolment enrolment)
        {
            object? new_id = scalar(@"
                INSERT INTO Enrolment (ParticipantId, CategoryId, Status, EnrolledTime)
                OUTPUT INSERTED.EnrolmentId
                VALUES (@participantId, @categoryId, @status, @enrolledTime);",
                param("@participantId", enrolment.ParticipantId),
                param("@categoryId", enrolment.CategoryId),
                param("@status", enrolment.Status),
                param("@enrolledTime", enrolment.EnrolledTime));

            enrolment.EnrolmentId = Convert.ToInt32(new_id);
            return enrolment;
        }

        public virtual Enrolment? get_enrolment_by_id(int enrolment_id)
        {
            return query_single(
                "SELECT EnrolmentId, ParticipantId, CategoryId, Status, EnrolledTime FROM Enrolment WHERE EnrolmentId = @id;",
                read_enrolment, param("@id", enrolment_id));
        }

        //finds the Organiser who owns the event of this enrolment
        public virtual string? get_organiser_id(int enrolment_id)
        {
            object? owner = scalar(@"
                SELECT ev.OrganiserId FROM Enrolment en
                JOIN Category c ON c.CategoryId = en.CategoryId
                JOIN [Event] ev ON ev.EventId = c.EventId
                WHERE en.EnrolmentId = @id;",
                param("@id", enrolment_id));

            return owner as string;
        }

        public virtual EnrolmentView? get_view_by_id(int enrolment_id)
        {
            return query_single(select_views + "WHERE en.EnrolmentId = @id;", read_view, param("@id", enrolment_id));
        }

        public virtual List<EnrolmentView> get_views_by_participant(string participant_id)
        {
            return query_list(select_views + "WHERE en.ParticipantId = @id ORDER BY en.EnrolledTime DESC;",
                read_view, param("@id", participant_id));
        }

        public virtual List<EnrolmentView> get_views_by_event(int event_id)
        {
            return query_list(select_views + "WHERE c.EventId = @id ORDER BY en.EnrolledTime;",
                read_view, param("@id", event_id));
        }

        public virtual void cancel_enrolment(int enrolment_id)
        {
            execute("UPDATE Enrolment SET Status = 'Cancelled' WHERE EnrolmentId = @id;", param("@id", enrolment_id));
        }

    }//end of class

}//end of namespace