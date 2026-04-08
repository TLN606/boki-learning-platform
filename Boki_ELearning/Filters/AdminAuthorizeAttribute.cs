using System.Web.Mvc;
using Boki_ELearning.Helpers;

namespace Boki_ELearning.Filters
{
    public class AdminAuthorizeAttribute : SessionAuthorizeAttribute
    {
        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
        {
            if (!base.AuthorizeCore(httpContext))
            {
                return false;
            }

            var role = httpContext.Session?[SessionKeys.Role] as string;
            return RoleHelper.IsAdmin(role);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext?.Session?[SessionKeys.AccountId] == null)
            {
                base.HandleUnauthorizedRequest(filterContext);
                return;
            }

            filterContext.Result = new RedirectToRouteResult(
                new System.Web.Routing.RouteValueDictionary(
                    new
                    {
                        controller = "Learning",
                        action = "Index"
                    }));
        }
    }
}
