using System.Collections.Generic;
using System.Security.Claims;

namespace TqCcnetDashboard
{
    public class TqUserIdentity : ClaimsIdentity
    {
        public TqUserIdentity(IEnumerable<Claim> claims, string authenticationType)
            : base(claims, authenticationType: authenticationType)
        {
            AddClaims(claims);
        }
    }
}