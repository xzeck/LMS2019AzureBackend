using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace LMS
{
    class GenerateHash
    {
        StringBuilder sb = new StringBuilder(); //Strinbuilder for building email+OTP hash
        StringBuilder HashString = new StringBuilder(); //Saves Hash
        byte[] Hash = null; //Saves Hash bytes
        

        public string Generate(string email, string VerifCode)
        {
            try
            {
                SHA256 sha256 = SHA256.Create(); //Create SHA256 instance
                Encoding enc = Encoding.UTF8; //Set encoding to UTF8

                //Below foreach loops are for generating a string which consists of email+OTP
                //format = <emailOTP>
                foreach (var un in email)
                {
                    sb.Append(un); 
                }

                foreach (var vc in VerifCode)
                {
                    sb.Append(vc);
                }

                //Generate hash and save its bytes to a byte array
                Hash = sha256.ComputeHash(enc.GetBytes(sb.ToString()));

            }
            catch (Exception e)
            {
                Console.WriteLine("Error while generating hash :" + e); //Throw error if Hash cannot be generated
            }

            foreach (var h in Hash)
            {
                //Convert Hash to hex
                HashString.Append(h.ToString("x2")); 
            }

            //Return the generated hash as string
            return HashString.ToString();
        }

        public string GenerateSalt()
        {
            Guid g = Guid.NewGuid();
            
            string GuidString = Convert.ToBase64String(g.ToByteArray());

            string salt = GuidString.Substring(0, 8);

            Console.WriteLine(salt);

            return salt;
        }

        public string GeneratePasswordHash(string pswrd)
        {
            SHA256 sha256 = SHA256.Create();
            Encoding enc = Encoding.UTF8;
            StringBuilder hashbuilder = new StringBuilder();
            byte[] PasswordHash = sha256.ComputeHash(enc.GetBytes(pswrd));
            string hashedpassword = null; 

            foreach (var h in PasswordHash)
                hashbuilder.Append(h.ToString("x2"));

            hashedpassword = hashbuilder.ToString();

            return hashedpassword;
        }
    }
}
