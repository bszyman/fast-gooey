using FastGooey.Services;

namespace FastGooey.Controllers.Interfaces;

public class AppleTvController(
    ILogger<AppleTvController> logger, 
    IKeyValueService keyValueService): 
    BaseStudioController(keyValueService)
{
    
}