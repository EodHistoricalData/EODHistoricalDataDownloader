using System;

namespace EODHistoricalDataDownloader.Utils
{
    public class APIException : Exception
    {
        public enum ErrorType
        {
            Success = 200,
            Unauthenticated = 401,
            Forbidden = 403,
            NotFound = 404,
        }

        public string StatusError
        {
            get
            {
                switch (Code)
                {
                    case (int)ErrorType.Unauthenticated:
                        return "API key is invalid";
                        
                    case (int)ErrorType.Forbidden:
                        return "Access to this data is denied";
                        
                    case (int)ErrorType.NotFound:
                        return "Data not found";

                    default:
                        return Message;
                }
            }
        }

        public int Code { get; private set; }

        public APIException(int code, string message) : base(message)
        {
            Code = code;
        }
    }
}
