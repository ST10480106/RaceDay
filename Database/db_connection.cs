using System.Data.SqlClient;

namespace RaceDay.Database
{//start of namespace

    public static class db_connection
    {//start of class

        //change the Data source to the server name you use in SQL Server Management Studio
        public static readonly string connection_string =
            @"Data source=localhost\SQLEXPRESS01;database=RaceDayDB;Integrated Security=True;TrustServerCertificate=True;";

        //opens and returns a new connection
        public static SqlConnection open_connection()
        {
            SqlConnection connect = new SqlConnection(connection_string);
            connect.Open();
            return connect;
        }

        //creates a parameter and turns null into a database NULL
        public static SqlParameter param(string name, object? value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }

        //runs a SELECT and converts every row to an object using the map method
        public static List<T> query_list<T>(string query, Func<SqlDataReader, T> map, params SqlParameter[] parameters)
        {
            List<T> list = new List<T>();

            using (SqlConnection connect = open_connection())
            using (SqlCommand run_query = new SqlCommand(query, connect))
            {
                run_query.Parameters.AddRange(parameters);

                using (SqlDataReader reader = run_query.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(map(reader));
                    }
                }
            }

            return list;
        }

        //runs a SELECT and returns the first row (or null)
        public static T? query_single<T>(string query, Func<SqlDataReader, T> map, params SqlParameter[] parameters) where T : class
        {
            return query_list(query, map, parameters).FirstOrDefault();
        }

        //runs INSERT / UPDATE / DELETE and returns the rows affected
        public static int execute(string query, params SqlParameter[] parameters)
        {
            using (SqlConnection connect = open_connection())
            using (SqlCommand run_query = new SqlCommand(query, connect))
            {
                run_query.Parameters.AddRange(parameters);
                return run_query.ExecuteNonQuery();
            }
        }

        //runs a query and returns the first value (e.g. COUNT or an inserted id)
        public static object? scalar(string query, params SqlParameter[] parameters)
        {
            using (SqlConnection connect = open_connection())
            using (SqlCommand run_query = new SqlCommand(query, connect))
            {
                run_query.Parameters.AddRange(parameters);
                return run_query.ExecuteScalar();
            }
        }

    }//end of class

}//end of namespace