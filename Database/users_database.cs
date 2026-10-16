using System.Data.SqlClient;
using RaceDay.Models;
using static RaceDay.Database.db_connection;

namespace RaceDay.Database
{//start of namespace

    public class users_database
    {//start of class

        //converts one row into a User object
        private static User read_user(SqlDataReader r)
        {
            return new User
            {
                UserId = (string)r["UserId"],
                FullName = (string)r["FullName"],
                Email = (string)r["Email"],
                PasswordHash = (string)r["PasswordHash"],
                Role = (string)r["Role"],
                DateOfBirth = r["DateOfBirth"] is DateTime dob ? DateOnly.FromDateTime(dob) : (DateOnly?)null,
                Gender = r["Gender"] is string gender ? gender : null
            };
        }

        //checks if the email is already registered (ignoring one user when updating)
        public virtual bool email_exists(string email, string? except_user_id = null)
        {
            object? count = scalar(
                "SELECT COUNT(*) FROM [User] WHERE Email = @email AND (@except IS NULL OR UserId <> @except);",
                param("@email", email), param("@except", except_user_id));

            return Convert.ToInt32(count) > 0;
        }

        //inserts the user and generates the id: O0001 (Organiser) / P0001 (Participant)
        public virtual User create_user(User user)
        {
            string prefix = user.Role == "Organiser" ? "O" : "P";

            string query = @"
                SET XACT_ABORT ON;
                BEGIN TRANSACTION;

                DECLARE @next INT = ISNULL((SELECT MAX(CAST(SUBSTRING(UserId, 2, 10) AS INT))
                                            FROM [User] WITH (UPDLOCK, HOLDLOCK)
                                            WHERE UserId LIKE @prefix + '%'), 0) + 1;
                DECLARE @id VARCHAR(20) = @prefix + RIGHT('0000' + CAST(@next AS VARCHAR(10)), 4);

                INSERT INTO [User] (UserId, FullName, Email, PasswordHash, Role, DateOfBirth, Gender)
                VALUES (@id, @fullName, @email, @passwordHash, @role, @dob, @gender);

                COMMIT TRANSACTION;
                SELECT @id;";

            object? new_id = scalar(query,
                param("@prefix", prefix),
                param("@fullName", user.FullName),
                param("@email", user.Email),
                param("@passwordHash", user.PasswordHash),
                param("@role", user.Role),
                param("@dob", user.DateOfBirth?.ToDateTime(TimeOnly.MinValue)),
                param("@gender", user.Gender));

            user.UserId = (string)new_id!;
            return user;
        }

        public virtual User? get_user_by_email(string email)
        {
            return query_single(
                "SELECT UserId, FullName, Email, PasswordHash, Role, DateOfBirth, Gender FROM [User] WHERE Email = @email;",
                read_user, param("@email", email));
        }

        public virtual User? get_user_by_id(string user_id)
        {
            return query_single(
                "SELECT UserId, FullName, Email, PasswordHash, Role, DateOfBirth, Gender FROM [User] WHERE UserId = @id;",
                read_user, param("@id", user_id));
        }

        //returns true if the user was found and updated
        public virtual bool update_user(User user)
        {
            int rows = execute(@"
                UPDATE [User]
                SET FullName = @fullName, Email = @email, DateOfBirth = @dob, Gender = @gender
                WHERE UserId = @id;",
                param("@fullName", user.FullName),
                param("@email", user.Email),
                param("@dob", user.DateOfBirth?.ToDateTime(TimeOnly.MinValue)),
                param("@gender", user.Gender),
                param("@id", user.UserId));

            return rows > 0;
        }

    }//end of class

}//end of namespace