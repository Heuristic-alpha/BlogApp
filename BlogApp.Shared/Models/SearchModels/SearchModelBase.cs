namespace BlogApp.Models.SearchModels
{
    public abstract class SearchModelBase
    {
        public int? Skip { get; set; }
        public int? Take { get; set; }
    }
}
