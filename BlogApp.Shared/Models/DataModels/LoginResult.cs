namespace BlogApp.Models.DataModels
{
    public class LoginResult
    {
        public bool Success { get; set; }
        public string Token { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"[(Success={Success}), (Token={Token}), (Error={Error})]";
        }
    }
}
