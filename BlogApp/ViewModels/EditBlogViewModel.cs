using BlogApp.Models;

namespace BlogApp.ViewModels
{
    public class EditBlogViewModel
    {
        public Blog Blog { get; set; } = new Blog();
        public string[] CategoryNames { get; set; } = Array.Empty<string>();
        public bool[] CategoriesValue { get; set; } = Array.Empty<bool>();
    }
}
