using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;

namespace KymdanMM.Filters
{
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var authorized = base.AuthorizeCore(httpContext);
            if (!authorized)
            {
                // The user is not authorized => no need to go any further
                return false;
            }

            // We have an authenticated user, let's get his username
            string authenticatedUser = httpContext.User.Identity.Name;

            // and check if he has completed his profile
            if (!WebSecurity.UserExists(authenticatedUser))
            {
                return false;
            }

            return true;
        }
    }
}