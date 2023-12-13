namespace Assistant.Web
{
    internal sealed class Error
    {
        public Error(string code, string description = null)
        {
            Code = code;
            Description = description;
        }

        public string Code { get; set; }

        public string Description { get; set; }
    }
}
