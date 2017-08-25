using System.Security.Claims;

namespace TqCcnetDashboard
{
    public class TqUserPrincipal : ClaimsPrincipal
    {
        public TqUserPrincipal(TqUserIdentity identity) : base(identity)
        {
        }

        public TqUserPrincipal(ClaimsPrincipal claimsPrincipal) : base(claimsPrincipal)
        {
        }
    }
}