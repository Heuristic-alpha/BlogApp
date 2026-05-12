using System.Diagnostics.Contracts;

namespace BlogApp.Infrastructures
{
    public partial class Constants
    {
        public const string HostAddress = "http://localhost:5000";
        public const string JWTAuthToken = "authToken";
        public const string JWTSecretName = "jwtSecret";
        public const int TokenValidationLifeTimeInHours = 24;

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
            public static string MaleUserProfileIcon => Path.Combine("images", "static", "icon-male.svg");
            public static string FemaleUserProfileIcon => Path.Combine("images", "static", "icon-female.svg");
        }
    }
}
