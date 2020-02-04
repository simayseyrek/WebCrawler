using System;
using System.Data.SqlClient;
using System.Text;

namespace crawlerUI
{
    /* TEST
            MyDatabase db = new MyDatabase();
            db.ConnectToSQL();
            db.CreateDatabase();
            db.CreateTable();

            db.Insert("Simay", "Mersin2");
            db.Insert("Simay2", "Ankara2");

            string readValue = db.ReadTable();
            Console.WriteLine(readValue);
            Console.WriteLine("After Update");

            db.Update("Simay", "Adana");
            Console.WriteLine(db.ReadTable());

            Console.ReadKey(true);
    */
    class MyDatabase
    {
        // Class variables
        private const string CONNECTION_STRING = "Server = localhost; Database = master; Trusted_Connection = True;"; // 2019103019
        private const string DATABASE_NAME = "crawlerDB"; // 2019103019
        private const string TABLE_NAME = "crawlerTable"; // 2019103019
        private SqlConnection connection;
        private SqlCommand command;
        private string sql;

        // Default Constructor 2019103002
        public MyDatabase()
        {
            sql = "";
            command = new SqlCommand();
            connection = new SqlConnection(CONNECTION_STRING);
        }

        // Destructer - 2019103038
        ~MyDatabase()
        {
        }

        // Makes connection to SQL Server
        public void ConnectToSQL()
        {
            try
            {
                // Connecting to SQL Server...
                connection.Open();
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        // Function that creates a database if not exists already
        public void CreateDatabase()
        {
            sql = "IF NOT EXISTS (SELECT name FROM master.sys.databases WHERE name = N'" + DATABASE_NAME + "') " +
                "CREATE DATABASE [" + DATABASE_NAME + "]";
            try
            {
                command = new SqlCommand(sql, connection);
                command.ExecuteNonQuery();
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        // Function that creates table if not exist already
        public void CreateTable()
        {
            StringBuilder sb = new StringBuilder(); // 2019103033
            sb.Append("USE " + DATABASE_NAME + "; ");
            sb.Append("IF NOT EXISTS (SELECT* FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'" + TABLE_NAME + "')");
            sb.Append("CREATE TABLE " + TABLE_NAME + " ( ");
            // TODO: buraya database design tasarlancak - Ne row, ne data saklancak.....
            sb.Append(" Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY, ");
            sb.Append(" Name NVARCHAR(50), ");
            sb.Append(" Location NVARCHAR(50) ");
            sb.Append("); ");
            sql = sb.ToString();

            try
            {
                command = new SqlCommand(sql, connection);
                command.ExecuteNonQuery();
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        // Function that inserts a new ROW into the table
        public void Insert(string name, string location)
        {
            StringBuilder sb = new StringBuilder();  // 2019103033
            sb.Append("INSERT " + TABLE_NAME + "(Name, Location) ");
            sb.Append("VALUES (@name, @location);");
            sql = sb.ToString();

            try
            {
                command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@location", location);
                command.ExecuteNonQuery();
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        // Function that updates an existing ROW into the table
        public void Update(string name, string newLocation)
        {
            sql = "UPDATE " + TABLE_NAME + " SET Location = N'" + newLocation + "' WHERE Name = @name";

            try
            {
                command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@name", name);
                command.ExecuteNonQuery();
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        // Function that reads all the available data in the database
        public string ReadTable()
        {
            StringBuilder sb = new StringBuilder();  // 2019103033
            sql = "SELECT Name, Location FROM " + TABLE_NAME + ";"; // where isThisCrawled = " + selector + "

            try
            {
                command = new SqlCommand(sql, connection);

                using (SqlDataReader reader = command.ExecuteReader()) // 2019103030
                {
                    while (reader.Read())
                    {
                        sb.AppendLine(reader.GetString(0) + " " + reader.GetString(1));
                    }
                } // sqldatareader burda çıkacak...
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }

            return sb.ToString();
        }

        /*
        // using System.Web.Script.Serialization;
        public string Serialize()
        {
            return new JavaScriptSerializer().Serialize(this);
        }

        public static Crawler Deserialize(string json)
        {
            return (Crawler) new JavaScriptSerializer().DeserializeObject(json);
            // https://stackoverflow.com/questions/2246694/how-to-convert-json-object-to-custom-c-sharp-object
        }
        */
    }
}
