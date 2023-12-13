using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace Assistant.Web
{
    internal class ProblemDetailsConverter : IProblemDetailsConverter
    {
        public int Order => 50;

        public bool IsEnabled => true;

        public ProblemDetails Convert(Stream stream)
        {
            if (Json.TryDeserialize(stream, out ProblemDetails problemDetails) &&
                !string.IsNullOrWhiteSpace(problemDetails.Title))
            {
                return problemDetails;
            }

            stream.Position = 0;
            return null;
        }
    }
}