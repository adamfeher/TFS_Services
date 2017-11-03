using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;

namespace RainforestExcavator.Core
{
    /// <summary>
    /// Custom exception to repackage WebExceptions caught from Service.HTTP.MakeRequest().
    /// </summary>
    public class HttpException : Exception
    {
        public HttpStatusCode StatusCode { get; set; }
        public HttpMethod CallType { get; set; }
        public string Uri { get; set; }
        public override string Message { get; }
        public HttpException(HttpMethod type, string uri, HttpStatusCode statusCode, string message = null)
        {
            this.CallType = type;
            this.Uri = uri;
            this.StatusCode = statusCode;

            var anon = new { error = string.Empty };
            try
            {
                this.Message = JsonConvert.DeserializeAnonymousType(message, anon).error;
            }
            catch { this.Message = message; }
        } 
    }
}
