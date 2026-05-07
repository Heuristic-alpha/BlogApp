namespace Microsoft.AspNetCore.Http
{
    public static class HttpRequestExtensions
    {
        public static string GetPathWithQuery(this HttpRequest httpRequest)
        {
            return httpRequest.Path.ToUriComponent() + httpRequest.QueryString.ToString();
        }
    }
}