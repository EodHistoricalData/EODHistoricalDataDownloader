using System;
using System.Net;
using System.Text;

namespace EODHistoricalDataDownloader.Utils
{
    /// <summary>
    /// Класс для отправки и получения запросов от сторонних сервисов
    /// </summary>
    public static class Response
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Url"></param>
        /// <param name="Data"></param>
        /// <returns>Response string</returns>
        /// <exception cref="APIException"></exception>
        public static string GET(string Url, string Data = "")
        {
            byte[] qwe;
            if (Data == "")
            {
                qwe = Encoding.Unicode.GetBytes(Url);
            }
            else
            {
                qwe = Encoding.Unicode.GetBytes(Url + "?" + Data);
            }

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            WebRequest req = WebRequest.Create(Encoding.Unicode.GetString(qwe));

            try
            {
                WebResponse resp = req.GetResponse();
                System.IO.Stream stream = resp.GetResponseStream();
                System.IO.StreamReader sr = new System.IO.StreamReader(stream);
                string Out = sr.ReadToEnd();
                sr.Close();

                return Out;
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)ex.Response;
                    throw new APIException((int)httpResponse.StatusCode, ex.Message);
                }
                else
                {
                    throw new APIException(500, ex.Message);
                }

            }
            catch (Exception ex)
            {
                throw new APIException(0, ex.Message);
            }

        }
    }
}
