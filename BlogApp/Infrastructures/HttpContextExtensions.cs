namespace Microsoft.AspNetCore.Http
{
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Get current language in session storage. if is empty then return default( ENG )
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static Language GetCurrentLanguage(this HttpContext httpContext)
        {
            Language lang = Language.ENG;
            if (Enum.TryParse<Language>(httpContext.Session.GetString(Constants.CookieNames.Language) ?? "ENG", true, out Language result)) { lang = result; }
            return lang;
        }
    }
}
