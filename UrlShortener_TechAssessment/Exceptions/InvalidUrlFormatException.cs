namespace UrlShortener_TechAssessment.Exceptions
{
    public class InvalidUrlFormatException : Exception
    {
        public InvalidUrlFormatException(string message) : base(message) { }
    }
}
