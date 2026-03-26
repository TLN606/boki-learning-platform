using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Boki_ELearning.Helpers;

namespace Boki_ELearning.Filters
{
    public class SessionAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            return httpContext != null
                   && httpContext.Session != null
                   && httpContext.Session[SessionKeys.AccountId] != null;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var returnUrl = filterContext.HttpContext.Request.RawUrl;
            filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary(
                    new
                    {
                        controller = "Account",
                        action = "Login",
                        returnUrl
                    }));
        }
    }
}
