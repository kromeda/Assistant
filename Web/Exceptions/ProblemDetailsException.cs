using Microsoft.AspNetCore.Mvc;
using System;

namespace Assistant.Web
{
    [Serializable]
    internal class ProblemDetailsException : Exception
    {
        public ProblemDetailsException(ProblemDetails problemDetails)
        {
            ProblemDetails = problemDetails;
        }

        public ProblemDetails ProblemDetails { get; }
    }
}