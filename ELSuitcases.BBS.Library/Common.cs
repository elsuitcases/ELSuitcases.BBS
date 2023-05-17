using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ELSuitcases.BBS.Library
{
    public static class Common
    {
        public static async Task<byte[]> AESDecrypt(byte[] encryptedData, string key, string iv)
        {
            if ((string.IsNullOrEmpty(key)) || (string.IsNullOrEmpty(iv)))
                throw new ArgumentNullException();

            byte[] result = null;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = Encoding.UTF8.GetBytes(iv);

                ICryptoTransform cryptoTransform = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream(encryptedData))
                {
                    using (CryptoStream cs = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            result = Convert.FromBase64String(await sr.ReadToEndAsync());
                        }
                    }
                }
            }

            return result;
        }
        
        public static async Task<byte[]> AESEncrypt(byte[] data, string key, string iv)
        {
            if ((string.IsNullOrEmpty(key)) || (string.IsNullOrEmpty(iv)))
                throw new ArgumentNullException();
            
            byte[] result = null;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = Encoding.UTF8.GetBytes(iv);

                ICryptoTransform cryptoTransform = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            await sw.WriteAsync(Convert.ToBase64String(data));
                            await sw.FlushAsync();
                        }

                        result = ms.ToArray();
                    }
                }
            }

            return result;
        }
        
        public static List<DTO> ConvertFromDataTableToDTOList(DataTable dt)
        {
            List<DTO> result = new List<DTO>();

            if (dt != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    DTO dto = new DTO();
                    foreach (DataColumn c in dr.Table.Columns)
                    {
                        var value = ((dr[c.ColumnName] == null) || (dr[c.ColumnName] == DBNull.Value)) ? null : dr[c.ColumnName];
                        dto.SetValue(c.ColumnName, value);
                    }
                    result.Add(dto);
                }
            }

            return result;
        }

        public static List<ArticleDTO> ConvertFromDataTableToArticleDTOList(DataTable dt)
        {
            List<ArticleDTO> result = new List<ArticleDTO>();

            if (dt != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    ArticleDTO dto = new ArticleDTO();
                    foreach (DataColumn c in dr.Table.Columns)
                    {
                        var value = ((dr[c.ColumnName] == null) || (dr[c.ColumnName] == DBNull.Value)) ? null : dr[c.ColumnName];
                        dto.SetValue(c.ColumnName, value);
                    }
                    result.Add(dto);
                }
            }

            return result;
        }

        public static List<BoardDTO> ConvertFromDataTableToBoardDTOList(DataTable dt)
        {
            List<BoardDTO> result = new List<BoardDTO>();

            if (dt != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    BoardDTO dto = new BoardDTO();
                    foreach (DataColumn c in dr.Table.Columns)
                    {
                        var value = ((dr[c.ColumnName] == null) || (dr[c.ColumnName] == DBNull.Value)) ? null : dr[c.ColumnName];
                        dto.SetValue(c.ColumnName, value);
                    }
                    result.Add(dto);
                }
            }

            return result;
        }

        public static string Generate16IdentityCode(DateTime dt, Random randomNo)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(dt.Year.ToString("D4"))
              .Append(dt.Month.ToString("D2"))
              .Append(dt.Day.ToString("D2"))
              .Append(dt.Hour.ToString("D2"))
              .Append(dt.Minute.ToString("D2"))
              .Append(dt.Second.ToString("D2"))
              .Append(randomNo.Next(1, 99).ToString("D2"));

            return sb.ToString();
        }

        public static void PrintDebugFail(Exception ex, string moduleName = "", string message = "")
        {
            if (ex == null) return;

            string message1 = string.Format("--> {0} {1} E {2} : {3}",
                                DateTime.Now,
                                (!string.IsNullOrEmpty(moduleName)) ? moduleName : Assembly.GetCallingAssembly().GetName().Name,
                                ex.HResult,
                                ex.Message);

            if (!string.IsNullOrEmpty(message))
                message1 = string.Format("{0} ({1})", message1, message);
#if DEBUG
            Debug.Fail(message1);
#endif
            Console.WriteLine(message1);
        }

        public static void PrintDebugInfo(string info, string moduleName = "")
        {
            string message = string.Format("--> {0} {1} I {2}",
                                DateTime.Now,
                                (!string.IsNullOrEmpty(moduleName)) ? moduleName : Assembly.GetCallingAssembly().GetName().Name,
                                info);
#if DEBUG
            Debug.WriteLine(message);
#endif
            Console.WriteLine(message);
        }

        public static void ThrowException(Exception error, bool isThrow, string funcName, string message)
        {
            Common.PrintDebugFail(error, funcName, message);

            if (isThrow)
                throw error;
        }
    }
}
