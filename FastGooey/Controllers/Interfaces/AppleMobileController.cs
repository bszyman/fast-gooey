using FastGooey.Services;
using Microsoft.AspNetCore.Mvc;

namespace FastGooey.Controllers.Interfaces;

[Route("Interfaces/AppleMobile")]
public class AppleMobileController(
    ILogger<AppleMobileController> logger, 
    IKeyValueService keyValueService): 
    BaseStudioController(keyValueService)
{
    [HttpGet("InterfaceSelectorPanel")]
    public IActionResult InterfaceSelectorPanel()
    {
        return PartialView("~/Views/AppleMobile/Partials/AppleMobileInterfaceSelectorPanel.cshtml");
    }
    
    // iOS List
    [HttpGet("List")]
    public IActionResult List()
    {
        return View();
    }
    
    [HttpGet("ListWorkspace")]
    public IActionResult ListWorkspace()
    {
        return PartialView("~/Views/AppleMobile/Workspaces/List.cshtml");
    }

    [HttpGet("ListItemEditorPanel")]
    public IActionResult ListItemEditorPanel()
    {
        return PartialView("~/Views/AppleMobile/Partials/ListItemEditorPanel.cshtml");
    }
    
    // iOS Content
    [HttpGet("Content")]
    public IActionResult Content()
    {
        return View();
    }
    
    [HttpGet("ContentWorkspace")]
    public IActionResult ContentWorkspace()
    {
        return PartialView("~/Views/AppleMobile/Workspaces/Content.cshtml");
    }

    [HttpGet("ContentTypeSelectorPanel")]
    public IActionResult ContentTypeSelectorPanel()
    {
        return PartialView("~/Views/AppleMobile/Partials/AppleMobileContentTypeSelectorPanel.cshtml");
    }

    [HttpGet("ContentHeadlineConfigurationPanel")]
    public IActionResult ContentHeadlineConfigurationPanel()
    {
        return PartialView("~/Views/AppleMobile/Partials/Content/ContentHeadlineConfigurationPanel.cshtml");
    }
    
    [HttpGet("ContentTextConfigurationPanel")]
    public IActionResult ContentTextConfigurationPanel()
    {
        return PartialView("~/Views/AppleMobile/Partials/Content/ContentTextConfigurationPanel.cshtml");
    }
    
    [HttpGet("ContentLinkConfigurationPanel")]
    public IActionResult ContentLinkConfigurationPanel()
    {
        return PartialView("~/Views/AppleMobile/Partials/Content/ContentLinkConfigurationPanel.cshtml");
    }
    
    [HttpGet("ContentImageConfigurationPanel")]
    public IActionResult ContentImageConfigurationPanel()
    {
        return PartialView("~/Views/AppleMobile/Partials/Content/ContentImageConfigurationPanel.cshtml");
    }
    
    [HttpGet("ContentVideoConfigurationPanel")]
    public IActionResult ContentVideoConfigurationPanel()
    {
        return PartialView("~/Views/AppleMobile/Partials/Content/ContentVideoConfigurationPanel.cshtml");
    }
    
    // iOS Form
    [HttpGet("Form")]
    public IActionResult Form()
    {
        return View();
    }
    
    [HttpGet("FormWorkspace")]
    public IActionResult FormWorkspace()
    {
        return PartialView("~/Views/AppleMobile/Workspaces/Form.cshtml");
    }
    
    [HttpGet("FormEntriesWorkspace")]
    public IActionResult FormEntriesWorkspace()
    {
        return PartialView("~/Views/AppleMobile/Workspaces/FormEntriesWorkspace.cshtml");
    }
    
    [HttpGet("FormFieldSelectorPanel")]
    public IActionResult FormFieldSelectorPanel()
    {
        return PartialView("~/Views/AppleMobile/Partials/Forms/FormFieldSelectorPanel.cshtml");
    }
    
    [HttpGet("FormSubmissionViewerPanel")]
    public IActionResult FormSubmissionViewerPanel()
    {
        return PartialView("~/Views/AppleMobile/Partials/Forms/FormSubmissionViewerPanel.cshtml");
    }
    
    [HttpGet("FormFieldTextEditorPanel")]
    public IActionResult FormFieldTextEditorPanel()
    {
        return PartialView("~/Views/AppleMobile/Partials/Forms/FormFieldTextEditorPanel.cshtml");
    }
    
    [HttpGet("FormFieldLongTextEditorPanel")]
    public IActionResult FormFieldLongTextEditorPanel()
    {
        return PartialView("~/Views/AppleMobile/Partials/Forms/FormFieldLongTextEditorPanel.cshtml");
    }
    
    [HttpGet("FormFieldCheckboxEditorPanel")]
    public IActionResult FormFieldCheckboxEditorPanel()
    {
        return PartialView("~/Views/AppleMobile/Partials/Forms/FormFieldCheckboxEditorPanel.cshtml");
    }
    
    [HttpGet("FormFieldDateEditorPanel")]
    public IActionResult FormFieldDateEditorPanel()
    {
        return PartialView("~/Views/AppleMobile/Partials/Forms/FormFieldDateEditorPanel.cshtml");
    }
    
    [HttpGet("FormFieldTimeEditorPanel")]
    public IActionResult FormFieldTimeEditorPanel()
    {
        return PartialView("~/Views/AppleMobile/Partials/Forms/FormFieldTimeEditorPanel.cshtml");
    }
    
    [HttpGet("FormFieldDropDownEditorPanel")]
    public IActionResult FormFieldDropDownEditorPanel()
    {
        return PartialView("~/Views/AppleMobile/Partials/Forms/FormFieldDropDownEditorPanel.cshtml");
    }
    
    [HttpGet("FormFieldMultiSelectEditorPanel")]
    public IActionResult FormFieldMultiSelectEditorPanel()
    {
        return PartialView("~/Views/AppleMobile/Partials/Forms/FormFieldMultiSelectEditorPanel.cshtml");
    }
    
    [HttpGet("FormFieldFileEditorPanel")]
    public IActionResult FormFieldFileEditorPanel()
    {
        return PartialView("~/Views/AppleMobile/Partials/Forms/FormFieldFileEditorPanel.cshtml");
    }
    
    [HttpGet("FormFieldBlankOption")]
    public IActionResult FormFieldBlankOption()
    {
        return PartialView("~/Views/AppleMobile/Partials/Forms/FormFieldBlankOption.cshtml");
    }
}