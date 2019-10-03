using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace LMS
{
    class DatabaseConnector
    {
        /// <summary>
        /// connects to the specified database string
        /// </summary>
        /// <returns>SqlConnection</returns>
       public SqlConnection connector(String Catalog)
        {
            SqlConnection connection = null; 
            try
            {
                SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder();
                sb.DataSource = "lms2019.database.windows.net"; // database URL
                sb.UserID = "accessor"; // ID 
                sb.Password = "LMS2019@"; // Password
                sb.InitialCatalog = Catalog; // Table name

                connection = new SqlConnection(sb.ConnectionString); //connection

                Console.WriteLine("Connected to databasae"); //Log if connected
            }catch(Exception e)
            {
                Console.WriteLine("Cannot connect to database : " + e); //Throw error if not connected
            }

            return connection; //return SqlConnection
        }
    }
}
