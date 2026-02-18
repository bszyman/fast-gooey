using FastGooey.Attributes;
using FastGooey.Controllers;
using FastGooey.Database;
using FastGooey.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FastGooey.Features.Interfaces.AppleMobile.Form.Controllers;

[Authorize]
[AuthorizeWorkspaceAccess]
[Route("Workspaces/{workspaceId:guid}/Interfaces/AppleMobile/Form")]
public class AppleMobileFormController(
    ILogger<AppleMobileFormController> logger,
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext) :
    BaseStudioController(keyValueService, dbContext)
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("workspace")]
    public IActionResult Workspace()
    {
        return PartialView("Workspaces/Form");
    }

    // [HttpPost("create-widget")]
    // public async Task<IActionResult> CreateWidget()
    // {
    //     var workspace = GetWorkspace();
    //     var data = new AppleMobileListJsonDataModel();
    //     
    //     var contentNode = new GooeyInterface
    //     {
    //         WorkspaceId = workspace.Id,
    //         Workspace = workspace,
    //         Platform = "AppleMobile",
    //         ViewType = "Form",
    //         Name = "New List Interface",
    //         Config = JsonSerializer.SerializeToDocument(data)
    //     };
    //
    //     await dbContext.GooeyInterfaces.AddAsync(contentNode);
    //     await dbContext.SaveChangesAsync();
    //
    //     var workspaceViewModel = await WorkspaceViewModelForInterfaceId(contentNode.DocId);
    //     var viewModel = new RssViewModel
    //     {
    //         WorkspaceViewModel = workspaceViewModel
    //     };
    //     
    //     Response.Headers.Append("HX-Trigger", "refreshNavigation");
    //     
    //     return PartialView("Index", viewModel);
    // }

    [HttpGet("entries-workspace")]
    public IActionResult FormEntriesWorkspace()
    {
        return PartialView("Workspaces/FormEntriesWorkspace");
    }

    [HttpGet("FormFieldSelectorPanel")]
    public IActionResult FormFieldSelectorPanel()
    {
        return PartialView("Partials/Forms/FormFieldSelectorPanel");
    }

    [HttpGet("FormSubmissionViewerPanel")]
    public IActionResult FormSubmissionViewerPanel()
    {
        return PartialView("Partials/Forms/FormSubmissionViewerPanel");
    }

    [HttpGet("FormFieldTextEditorPanel")]
    public IActionResult FormFieldTextEditorPanel()
    {
        return PartialView("Partials/Forms/FormFieldTextEditorPanel");
    }

    [HttpGet("FormFieldLongTextEditorPanel")]
    public IActionResult FormFieldLongTextEditorPanel()
    {
        return PartialView("Partials/Forms/FormFieldLongTextEditorPanel");
    }

    [HttpGet("FormFieldCheckboxEditorPanel")]
    public IActionResult FormFieldCheckboxEditorPanel()
    {
        return PartialView("Partials/Forms/FormFieldCheckboxEditorPanel");
    }

    [HttpGet("FormFieldDateEditorPanel")]
    public IActionResult FormFieldDateEditorPanel()
    {
        return PartialView("Partials/Forms/FormFieldDateEditorPanel");
    }

    [HttpGet("FormFieldTimeEditorPanel")]
    public IActionResult FormFieldTimeEditorPanel()
    {
        return PartialView("Partials/Forms/FormFieldTimeEditorPanel");
    }

    [HttpGet("FormFieldDropDownEditorPanel")]
    public IActionResult FormFieldDropDownEditorPanel()
    {
        return PartialView("Partials/Forms/FormFieldDropDownEditorPanel");
    }

    [HttpGet("FormFieldMultiSelectEditorPanel")]
    public IActionResult FormFieldMultiSelectEditorPanel()
    {
        return PartialView("Partials/Forms/FormFieldMultiSelectEditorPanel");
    }

    [HttpGet("FormFieldFileEditorPanel")]
    public IActionResult FormFieldFileEditorPanel()
    {
        return PartialView("Partials/Forms/FormFieldFileEditorPanel");
    }

    [HttpGet("FormFieldBlankOption")]
    public IActionResult FormFieldBlankOption()
    {
        return PartialView("Partials/Forms/FormFieldBlankOption");
    }
}