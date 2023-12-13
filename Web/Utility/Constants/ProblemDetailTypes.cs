using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Assistant.Web
{
    internal class ProblemDetailTypes
    {
        private static readonly IDictionary<int, string> _links = new Dictionary<int, string>()
        {
            { StatusCodes.Status400BadRequest,          "https://tools.ietf.org/html/rfc7231#section-6.5.1" },
            { StatusCodes.Status408RequestTimeout,      "https://tools.ietf.org/html/rfc7231#section-6.5.7" },
            { StatusCodes.Status500InternalServerError, "https://tools.ietf.org/html/rfc7231#section-6.6.1" },
            { StatusCodes.Status502BadGateway,          "https://tools.ietf.org/html/rfc7231#section-6.6.3" },
        };

        internal string this[int statusCode]
        {
            get
            {
                return _links.ContainsKey(statusCode) ? _links[statusCode] : null;
            }
        }
    }
}