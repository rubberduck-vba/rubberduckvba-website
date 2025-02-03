using Microsoft.AspNetCore.Mvc;
using rubberduckvba.Server;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace rubberduckvba.Server;

[ApiController]
public abstract class RubberduckApiController : ControllerBase
{
    private readonly ILogger _logger;

    protected RubberduckApiController(ILogger logger)
    {
        _logger = logger;
    }


    protected async Task<IActionResult> GuardInternalActionAsync(Func<Task<IActionResult>> method, [CallerMemberName] string name = default!)
    {
        var sw = Stopwatch.StartNew();
        IActionResult result = NoContent();
        var success = false;
        try
        {
            _logger.LogTrace("GuardInternalActionAsync:{name} | ▶ Invoking controller action", name);
            result = await method.Invoke();
            success = true;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "GuardInternalActionAsync:{name} | ❌ An exception was caught", name);
            throw;
        }
        finally
        {
            sw.Stop();
            if (success)
            {
                _logger.LogTrace("GuardInternalActionAsync:{name} | ✔️ Controller action completed | ⏱️ {elapsed}", name, sw.Elapsed);
            }
            else
            {
                _logger.LogWarning("GuardInternalActionAsync:{name} | ⚠️ Controller action completed with errors", name);
            }
        }

        return result;
    }

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

        return result;
    }
}
