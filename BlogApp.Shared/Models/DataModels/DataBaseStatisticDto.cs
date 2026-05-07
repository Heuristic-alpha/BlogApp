namespace BlogApp.Models.DataModels
{
    public class DataBaseStatisticDto
    {
        public int IdentityAppUserCount { get; set; }
        public int AppUsersCount { get; set; }
        public int BlogsCount { get; set; }
        public int CommentsCount { get; set; }
        public int CategoriesCount { get; set; }
        public int AppUserCommentsCount { get; set; }
        public int BlogCategoriesCount { get; set; }
    }
}
