namespace BlogApp.ViewComponents
{
    public class PaginationAction : ViewComponent
    {
        public IViewComponentResult Invoke(string controller, string action, int currentPage, int pageSize, int totalItemCount)
        {
            PaginationViewModel paginationViewModel = new(totalItemCount, pageSize, currentPage, controller, action);
            return View(paginationViewModel);
        }
    }
}
