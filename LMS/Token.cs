using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;


namespace LMS
{
    class Token
    {
        public string GenerateToken()
        {
            byte[] time = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
            byte[] key = Guid.NewGuid().ToByteArray();
            byte[] token_byte = new byte[time.Length + key.Length];

            Buffer.BlockCopy(time, 0, token_byte, 0, time.Length);
            Buffer.BlockCopy(key, 0, token_byte, time.Length, key.Length);

            string token = Convert.ToBase64String(token_byte);

            Console.WriteLine(token);

            return token;
        }

        public bool IsTokenValid(string token)
        {
            byte[] data= Convert.FromBase64String(token);

            DateTime token_gen_time = DateTime.FromBinary(BitConverter.ToInt64(data, 0)); 

            if(token_gen_time < DateTime.UtcNow.AddHours(1))
                return false;
            else
                return true;
            
        }

        public void DeleteToken(string token)
        {
            DatabaseConnector DBConn = new DatabaseConnector();
            SqlConnection connection = DBConn.connector("Users");

            SqlCommand cmd = new SqlCommand("Update Users set session_token=null where session_token=@token");
            cmd.Parameters.AddWithValue("@token", token);

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            


        }
    }
}
