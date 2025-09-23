using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace CollabSphere.Application.Common
{
    public static class SHA256Encoding
    {
        public static string ComputeSHA256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                //Compute the has as a byte array
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                //Convert the array to a hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    sb.Append(bytes[i].ToString("x2"));
                }

                return sb.ToString();
            }
        }
    }
}
