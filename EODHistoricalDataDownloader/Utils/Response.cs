using System;
using System.Net.Http;

namespace EODHistoricalDataDownloader.Utils
{
    /// <summary>
    /// Класс для отправки и получения запросов от сторонних сервисов
    /// </summary>
    public static class Response
    {
        private static readonly HttpClient _httpClient = new();

        /// <summary>
        /// Synchronous GET request
        /// </summary>
        /// <param name="Url"></param>
        /// <param name="Data"></param>
        /// <returns>Response string</returns>
        /// <exception cref="APIException"></exception>
        public static string GET(string Url, string Data = "")
        {
            string requestUrl = string.IsNullOrEmpty(Data) ? Url : Url + "?" + Data;

            try
            {
                using var response = _httpClient.GetAsync(requestUrl).GetAwaiter().GetResult();
                if (!response.IsSuccessStatusCode)
                {
                    throw new APIException((int)response.StatusCode, response.ReasonPhrase ?? "Unknown error");
                }
                return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
            catch (HttpRequestException ex)
            {
                throw new APIException(500, ex.Message);
            }
            catch (APIException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new APIException(0, ex.Message);
            }
        }
    }
}
