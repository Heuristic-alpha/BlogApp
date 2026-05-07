namespace BlogApp.Models.JoinModels
{
    public class BlogCategory
    {
        public long BlogCategoryId { get; set; }

        public long BlogId { get; set; }
        public long CategoryId { get; set; }

        public virtual Blog? Blog { get; set; }
        public virtual Category? Category { get; set; }
    }
}
