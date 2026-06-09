namespace BlogApp.Infrastructures
{
    public partial class Constants
    {
        public const string HostAddress = "http://localhost:5000";

        public struct CookieNames
        {
            public const string Language = "Language";
        }

        public struct RateLimiterNames
        {
            public const string PublicFixLimit = "PublicLimit";
            public const string AdminFixLimit = "AdminLimit";
        }

        public struct JWTAuthentication
        {
            public const string JWTAuthToken = "authToken";
            public const string JWTSecretName = "jwtSecret";
            public const int TokenValidationLifeTimeInHours = 24;
        }

        public struct BootstarpColor
        {
            public static string Black => "black";
            public static string White => "white";
            public static string Gray => "secondary";
            public static string Yellow => "warning";
            public static string Red => "danger";
            public static string Blue => "primary";
            public static string Sky => "info";
            public static string Green => "success";
        }

        public struct Roles
        {
            public const string Manager = "Manager";
            public const string Admins = "Admins";
            public const string Members = "Members";
        }

        public struct StaticImagesURL
        {
            public const string MaleUserProfileIcon = "images/static/icon-male.svg";
            public const string FemaleUserProfileIcon = "images/static/icon-female.svg";

            public const string BooleanTrueIcon = "images/static/icon-true.svg";
            public const string BooleanFalseIcon = "images/static/icon-false.svg";

            public const string FaceHappyIcon = "images/static/icon-happy-face.png";
            public const string FaceUnhappyIcon = "images/static/icon-unhappy-face.png";

            public const string SmallArrowUpIcon = "images/static/small-arrow-up.png";
            public const string SmallArrowDownIcon = "images/static/small-arrow-down.png";

            public const string HeartIcon = "images/static/icon-heart.png";
            public const string UnHeartIcon = "images/static/icon-unheart.png";
        }
    }
}
