using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FakeMW2SA
{
    public static class Helper
    {
        public static string GetLocalFileContent(string fileName)
        {
            try
            {
                string parentPath = AppDomain.CurrentDomain.BaseDirectory;
                parentPath = parentPath.Replace(@"\bin\Debug\", "\\");

                string filePath = parentPath += "Html" + "\\" + fileName;

                return File.ReadAllText(filePath);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public static string GetCdnContent(string cdnPath)
        {
            try
            {
                var webRequest = WebRequest.Create(cdnPath);
                string strContent = "";
                using (var response = webRequest.GetResponse())
                using (var content = response.GetResponseStream())
                using (var reader = new StreamReader(content))
                {
                    strContent = reader.ReadToEnd();
                }
                return strContent;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
    }
}
