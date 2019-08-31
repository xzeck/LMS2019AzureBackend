using System;
using System.Collections.Generic;
using System.Text;

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

        public Boolean IsTokenValid(string token)
        {
            byte[] data= Convert.FromBase64String(token);

            DateTime token_gen_time = DateTime.FromBinary(BitConverter.ToInt64(data, 0)); 

            if(token_gen_time < DateTime.UtcNow.AddHours(-24))
            {
                return false;
            }
            else
                return true;
            
        }
    }
}
