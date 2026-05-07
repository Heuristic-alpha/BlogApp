using BlogApp.Models;

namespace BlogApp.ViewModels
{
    public class PaginateItemsViewModel<TItem>
    {
        public PaginateItemsViewModel(int currentPage, int pageSize, TItem[] selectedItems, int allItemsCount)
        {
            CurrentPage = currentPage;
            PageSize = pageSize;
            SelectedItems = selectedItems;
            AllItemsCount = allItemsCount;
        }

        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public TItem[] SelectedItems { get; set; }
        public int AllItemsCount { get; set; }
    }
}
