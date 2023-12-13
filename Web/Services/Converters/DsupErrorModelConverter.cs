namespace Assistant.Web;

internal class DsupErrorModelConverter : IProblemDetailsConverter
{
    public int Order => 100;

    public bool IsEnabled => true;

    public ProblemDetails Convert(Stream stream)
    {
        if (Json.TryDeserialize(stream, out List<ErrorModel> errorModels) && errorModels?.Count > 0)
        {
            var problemDetails = new ProblemDetails
            {
                Title = ErrorTexts.RelatedSerivce,
                Detail = ErrorTexts.RequestViolation
            };

            if (errorModels?.Count > 0)
            {
                var errors = errorModels
                    .Where(error => !string.IsNullOrWhiteSpace(error.Message))
                    .SelectMany(error => error.RefKeys
                        .Select(key => new Error(key, error.Message)))
                    .ToArray();

                problemDetails.Extensions.Add(new("errors", errors));
            }

            return problemDetails;
        }

        stream.Position = 0;
        return null;
    }

    public class ErrorModel
    {
        public ICollection<string> RefKeys { get; set; }

        public string Message { get; set; }

        public bool IsCritical { get; set; }
    }
}
