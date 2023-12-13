namespace Assistant.Web;

internal class DsupErrorModelConverter : IProblemDetailsConverter
{
    public int Order => 100;

    public bool IsEnabled => true;

    public ProblemDetails Convert(Stream stream)
    {
        if (Json.TryDeserialize(stream, out List<ErrorModel> errors) && errors?.Count > 0)
        {
            var detailsBuilder = new StringBuilder();

            if (errors?.Count > 0)
            {
                detailsBuilder.Append(ErrorTexts.RequestViolation);
                detailsBuilder.Append(string.Join("; ", errors
                    .Where(x => !string.IsNullOrWhiteSpace(x.Message))
                    .SelectMany(error => error.RefKeys
                        .Select(key =>
                        {
                            string critical = error.IsCritical ? "да" : "нет";
                            return $"поле: '{key}', описание: '{error.Message}', критично: {critical}";
                        }))));

                detailsBuilder.Append('.');
            }

            return new ProblemDetails
            {
                Title = ErrorTexts.RelatedSerivce,
                Detail = detailsBuilder.ToString()
            };
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
