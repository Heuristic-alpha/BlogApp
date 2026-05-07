namespace BlogApp.Models.SearchModels
{
    public class UserSearch : SearchModelBase
    {
        public long? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public bool? IsMale { get; set; }
    }
}
