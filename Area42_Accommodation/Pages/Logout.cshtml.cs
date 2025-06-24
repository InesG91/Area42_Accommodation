using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Area42_Accommodation.Pages
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            
            HttpContext.Session.Clear();

            TempData["SuccessMessage"] = "You have been successfully logged out.";
            return RedirectToPage("/Login");
        }

        public IActionResult OnPost()
        {
            return OnGet();
        }
    }
}
