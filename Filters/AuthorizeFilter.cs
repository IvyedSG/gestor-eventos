using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GestorEventos.Filters
{
    public class AuthorizeFilter : IPageFilter
    {
        public void OnPageHandlerSelected(PageHandlerSelectedContext context)
        {
            // No hacemos nada aquí
        }

        public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.Result = new RedirectToPageResult("/Account/Login");
            }
        }

        public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
        {
            // No hacemos nada aquí
        }
    }
}