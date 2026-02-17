using System.Text.Json;
using FastGooey.Attributes;
using FastGooey.Database;
using FastGooey.Models;
using FastGooey.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FastGooey.Controllers.Interfaces;

[Authorize]
[AuthorizeWorkspaceAccess]
[Route("Workspaces/{workspaceId:guid}/Interfaces/tvOS/Main")]
public class AppleTvMainController(
    ILogger<AppleTvMainController> logger,
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext) :
    BaseInterfaceController(keyValueService, dbContext)
{
    [HttpGet("{interfaceId}")]
    public IActionResult Index(string interfaceId)
    {
        if (!TryParseInterfaceId(interfaceId, out _))
        {
            return NotFound();
        }

        return View();
    }

    [HttpGet("workspace/{interfaceId}")]
    public IActionResult Workspace(string interfaceId)
    {
        if (!TryParseInterfaceId(interfaceId, out _))
        {
            return NotFound();
        }

        return PartialView("~/Views/AppleTvMain/Workspace.cshtml");
    }

    [HttpPost("create-interface")]
    public async Task<IActionResult> CreateInterface()
    {
        if (await InterfaceLimitReachedAsync())
        {
            return Forbid();
        }

        var workspace = GetWorkspace();
        if (workspace is null)
        {
            return NotFound();
        }

        var contentNode = new GooeyInterface
        {
            WorkspaceId = workspace.Id,
            Workspace = workspace,
            Platform = "AppleTv",
            ViewType = "Main",
            Name = "New Main Interface",
            Config = JsonSerializer.SerializeToDocument(new { })
        };

        await dbContext.GooeyInterfaces.AddAsync(contentNode);
        await dbContext.SaveChangesAsync();

        Response.Headers.Append("HX-Trigger", "refreshInterfaces");

        return PartialView("~/Views/AppleTvMain/Index.cshtml");
    }
}
