namespace BlogApp.ViewModels
{
    public class PaginationViewModel
    {
        public PaginationViewModel(int totalItemCount, int pageSize, int currentPage, string controllerName, string actionName)
        {
            TotalCount = totalItemCount;
            PageSize = pageSize;
            CurrentPage = currentPage;
            ControllerName = controllerName;
            ActionName = actionName;

            TotalPage = (int)MathF.Ceiling((float)TotalCount / (float)pageSize);
        }

        public int PageSize { get; init; }
        public int CurrentPage { get; init; }
        public string ControllerName { get; init; }
        public string ActionName { get; init; }
        public int TotalCount { get; init; }
        public int TotalPage { get; init; }

        public bool HasNextPage => CurrentPage < TotalPage;
        public bool HasPrevPage => CurrentPage > 1;


    }
}
