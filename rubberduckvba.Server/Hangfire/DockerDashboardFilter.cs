using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace rubberduckvba.com.Server.Hangfire;

public class DockerDashboardFilter : IDashboardAuthorizationFilter
{
    public bool Authorize([NotNull] DashboardContext context)
    {
        if (string.IsNullOrEmpty(context.Request.RemoteIpAddress))
        {
            return false;
        }

        if (context.Request.RemoteIpAddress == "127.0.0.1" || context.Request.RemoteIpAddress == "::ffff:172.17.0.1")
        {
            return true;
        }

        if (context.Request.RemoteIpAddress == context.Request.LocalIpAddress)
        {
            return true;
        }

        return false;
    }
}
