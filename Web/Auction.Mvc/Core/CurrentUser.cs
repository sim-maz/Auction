using Auction.Data.Ef;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Web;

namespace Auction.Mvc.Core
{
    //Finds the current user according to the windows authorization and returns Login in a string type.
    public static class CurrentUser
    {

        public static string FullName
        {
            get
            {
                string name = "";
                using (var context = new PrincipalContext(ContextType.Domain))
                {
                    var usr = UserPrincipal.FindByIdentity(context, HttpContext.Current.User.Identity.Name);
                    if (usr != null) { 
                        name = usr.DisplayName;
                    } else
                    {
                        name = Login;
                    }
                }
                return name;
            }
        }

        public static string Login
        {
            get { return HttpContext.Current?.User?.Identity?.Name.Substring(5, HttpContext.Current.User.Identity.Name.Length - 5); }
        }

        public static string Name
        {
            get { return HttpContext.Current?.User?.Identity?.Name; }
        }

        public static bool IsAdmin
        {
            get
            {
                using (var ctx = new EfContext())
                {
                    //use cache - nopcommerce for inspiration :)
                    return ctx.Admins.Any(x => Login == x);
                }
            }
        }
    }
}