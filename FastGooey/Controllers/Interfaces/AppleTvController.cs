using FastGooey.Database;
using FastGooey.Services;
using Microsoft.AspNetCore.Components;

namespace FastGooey.Controllers.Interfaces;

[Route("Workspaces/{workspaceId:guid}/Interfaces/tvOS")]
public class AppleTvController(
    ILogger<AppleTvController> logger, 
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext): 
    BaseStudioController(keyValueService, dbContext)
{
    
}