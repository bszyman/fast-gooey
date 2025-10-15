using FastGooey.Services;
using Microsoft.AspNetCore.Mvc;

namespace FastGooey.Controllers;

public class InterfacesController(
    ILogger<InterfacesController> logger, 
    IKeyValueService keyValueService): 
    BaseStudioController(keyValueService)
{
    
    
    
}