using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GestorEventos.Filters
{
    public class AuthorizeFilter : IPageFilter
    {
        public void OnPageHandlerSelected(PageHandlerSelectedContext context)
        {

        }

        public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.Result = new RedirectToPageResult("/Auth/Login");
            }
        }

        public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
        {

        }
    }
}