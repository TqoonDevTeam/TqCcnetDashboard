using Spring.Core.CDH.Autowire;
using System.Web.Http;
using TqCcnetDashboard.Web.Mvc;

namespace TqCcnetDashboard
{
    [TqoonDevTeamExceptionApiFilter]
    [Authorize]
    public abstract class AbstractApiController : ApiController
    {
        public AbstractApiController()
        {
            SpringAutowire.Autowire(this);
        }
    }
}