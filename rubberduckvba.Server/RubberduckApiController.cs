using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using rubberduckvba.Server;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace rubberduckvba.Server;

[ApiController]
[EnableCors("CorsPolicy")]
public abstract class RubberduckApiController : ControllerBase
{
    private readonly ILogger _logger;

    protected RubberduckApiController(ILogger logger)
    {
        _logger = logger;
    }

    protected ILogger Logger => _logger;

    protected IActionResult GuardInternalAction(Func<IActionResult> method, [CallerMemberName] string name = default!)
    {
        var sw = Stopwatch.StartNew();
        IActionResult result = NoContent();
        var success = false;
        try
        {
            _logger.LogTrace("GuardInternalAction:{name} | ▶ Invoking controller action", name);
            result = method.Invoke();
            success = true;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "GuardInternalAction:{name} | ❌ An exception was caught", name);
            throw;
        }
        finally
        {
            sw.Stop();
            if (success)
            {
                _logger.LogTrace("GuardInternalAction:{name} | ✔️ Controller action completed | ⏱️ {elapsed}", name, sw.Elapsed);
            }
            else
            {
                _logger.LogWarning("GuardInternalAction:{name} | ⚠️ Controller action completed with errors", name);
            }
        }

        //Response.Headers.AccessControlAllowOrigin = "*";
        return result;
    }
}
