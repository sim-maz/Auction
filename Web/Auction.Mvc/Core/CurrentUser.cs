using Auction.Data.Ef;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Web;

namespace Auctions.Mvc.Core
{
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
                    if (usr != null)
                        name = usr.DisplayName;
                }
                return name;
            }
        }

        public static string Login
        {
            get { return HttpContext.Current?.User?.Identity?.Name; }
        }

        public static bool IsAdmin
        {
            get
            {
                using (var ctx = new EfContext())
                {
                    //todo use string split in login
                    //use cache - nopcommerce for inspiration :)
                    return ctx.AppAdmins.Any(x => Login.Contains(x));
                }
            }
        }
    }
}