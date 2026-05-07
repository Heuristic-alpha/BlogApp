using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace BlogApp.Pages
{
    public class ErrorModel : PageModel
    {
        public int Id { get; set; }
        public string? ErrorMessage { get; set; }

        public void OnGet([FromRoute] int id)
        {
            Id = id;
            if (Id != 0 && Id >= 400 && Id <= 600)
            {
                HttpStatusCode statusCode = (HttpStatusCode)Id!;
                ErrorMessage = statusCode.ToString();
            }
        }
    }
}
