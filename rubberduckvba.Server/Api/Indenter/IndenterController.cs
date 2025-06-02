using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using RubberduckServices;

namespace rubberduckvba.Server.Api.Indenter;

[AllowAnonymous]
[EnableCors(CorsPolicies.AllowAll)]
public class IndenterController : RubberduckApiController
{
    private readonly IIndenterService service;

    public IndenterController(IIndenterService service, ILogger<IndenterController> logger)
        : base(logger)
    {
        this.service = service;
    }

    [HttpGet("indenter/version")]
    [AllowAnonymous]
    public IActionResult Version() =>
        GuardInternalAction(() => Ok(service.IndenterVersion()));

    [HttpGet("indenter/defaults")]
    [AllowAnonymous]
    public IActionResult DefaultSettings() =>
        GuardInternalAction(() =>
        {
            var result = new IndenterViewModel
            {
                IndenterVersion = service.IndenterVersion(),
                Code = "Option Explicit\n\n'...comments...\n\nPublic Sub DoSomething()\n'...comments...\n\nEnd Sub\nPublic Sub DoSomethingElse()\n'...comments...\n\nIf True Then\nMsgBox \"Hello, world!\"\nElse\n'...comments...\nExit Sub\nEnd If\nEnd Sub\n",
                AlignCommentsWithCode = true,
                EmptyLineHandlingMethod = IndenterEmptyLineHandling.Indent,
                ForceCompilerDirectivesInColumn1 = true,
                GroupRelatedProperties = false,
                IndentSpaces = 4,
                IndentCase = true,
                IndentEntireProcedureBody = true,
                IndentEnumTypeAsProcedure = true,
                VerticallySpaceProcedures = true,
                LinesBetweenProcedures = 1,
                IndentFirstCommentBlock = true,
                IndentFirstDeclarationBlock = true,
                EndOfLineCommentStyle = IndenterEndOfLineCommentStyle.SameGap,
            };

            return Ok(result);
        });

    [HttpPost("indenter/indent")]
    [AllowAnonymous]
    public IActionResult Indent(IndenterViewModel model) =>
        GuardInternalAction(() =>
        {
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var result = service.IndentAsync(model).GetAwaiter().GetResult();
            return Ok(result);
        });
}
