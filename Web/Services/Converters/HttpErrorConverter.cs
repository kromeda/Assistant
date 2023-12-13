using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace Assistant.Web
{
    internal class HttpErrorConverter : IProblemDetailsConverter
    {
        public int Order => 25;

        public bool IsEnabled => true;

        public ProblemDetails Convert(Stream stream)
        {
            if (Json.TryDeserialize(stream, out HttpError error))
            {
                var problemDetails = new ProblemDetails
                {
                    Title = ErrorTexts.RelatedSerivce,
                    Detail = error.Message
                };

                return problemDetails;
            }

            stream.Position = 0;
            return null;
        }

        public class HttpError
        {
            public HttpError(string message)
            {
                Message = message;
            }

            public string Message { get; set; }
        }
    }
}