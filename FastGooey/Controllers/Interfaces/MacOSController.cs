using FastGooey.Services;
using Microsoft.AspNetCore.Mvc;

namespace FastGooey.Controllers.Interfaces;

[Route("Interfaces/MacOS")]
public class MacOSController(
    ILogger<MacOSController> logger, 
    IKeyValueService keyValueService): 
    BaseStudioController(keyValueService)
{
    [HttpGet("InterfaceSelectorPanel")]
    public IActionResult MacOSInterfaceSelectorPanel()
    {
        return PartialView("~/Views/MacOS/Partials/MacOSInterfaceSelectorPanel.cshtml");
    }
    
    [HttpGet("Table")]
    public IActionResult Table()
    {
        return View();
    }
    
    [HttpGet("TableWorkspace")]
    public IActionResult TableWorkspace()
    {
        return PartialView("~/Views/MacOS/Workspaces/Table.cshtml");
    }
    
    [HttpGet("TableStructureWorkspace")]
    public IActionResult TableStructureWorkspace()
    {
        return PartialView("~/Views/MacOS/Workspaces/TableStructure.cshtml");
    }

    [HttpGet("TableHeaderOptionRow")]
    public IActionResult TableHeaderOptionRow()
    {
        return PartialView("~/Views/MacOS/Partials/TableHeaderOptionRow.cshtml");
    }
    
    [HttpGet("TableFieldEditorPanel")]
    public IActionResult TableFieldEditorPanel()
    {
        return PartialView("~/Views/MacOS/Partials/TableFieldEditorPanel.cshtml");
    }

    [HttpGet("TableItemEditorPanel")]
    public IActionResult TableItemEditorPanel()
    {
        return PartialView("~/Views/MacOS/Partials/TableItemEditorPanel.cshtml");    
    }
    
    // Source List
    [HttpGet("SourceList")]
    public IActionResult SourceList()
    {
        return View();
    }
    
    [HttpGet("SourceListWorkspace")]
    public IActionResult SourceListWorkspace()
    {
        return PartialView("~/Views/MacOS/Workspaces/SourceList.cshtml");
    }
    
    [HttpGet("SourceListEditorPanel")]
    public IActionResult SourceListEditorPanel()
    {
        return PartialView("~/Views/MacOS/Partials/SourceListEditorPanel.cshtml");
    }
    
    [HttpGet("SourceListItemEditorPanel")]
    public IActionResult SourceListItemEditorPanel()
    {
        return PartialView("~/Views/MacOS/Partials/SourceListItemEditorPanel.cshtml");
    }
    
    [HttpGet("Outline")]
    public IActionResult Outline()
    {
        return View();
    }
    
    [HttpGet("OutlineWorkspace")]
    public IActionResult OutlineWorkspace()
    {
        return PartialView("~/Views/MacOS/Workspaces/Outline.cshtml");
    }

    [HttpGet("OutlineViewItemEditorPanel")]
    public IActionResult OutlineViewItemEditorPanel()
    {
        return PartialView("~/Views/MacOS/Partials/OutlineViewItemEditorPanel.cshtml");
    }
    
    // Form
    [HttpGet("Form")]
    public IActionResult Form()
    {
        return View();
    }
    
    [HttpGet("FormWorkspace")]
    public IActionResult FormWorkspace()
    {
        return PartialView("~/Views/MacOS/Workspaces/Form.cshtml");
    }
    
    [HttpGet("FormEntriesWorkspace")]
    public IActionResult FormEntriesWorkspace()
    {
        return PartialView("~/Views/MacOS/Workspaces/FormEntriesWorkspace.cshtml");
    }
    
    [HttpGet("FormSubmissionViewerPanel")]
    public IActionResult FormSubmissionViewerPanel()
    {
        return PartialView("~/Views/MacOS/Partials/FormSubmissionViewerPanel.cshtml");
    }
    
    [HttpGet("FormFieldSelectorPanel")]
    public IActionResult FormFieldSelectorPanel()
    {
        return PartialView("~/Views/MacOS/Partials/Forms/FormFieldSelectorPanel.cshtml");
    }
    
    [HttpGet("FormFieldTextEditorPanel")]
    public IActionResult FormFieldTextEditorPanel()
    {
        return PartialView("~/Views/MacOS/Partials/Forms/FormFieldTextEditorPanel.cshtml");
    }
    
    [HttpGet("FormFieldLongTextEditorPanel")]
    public IActionResult FormFieldLongTextEditorPanel()
    {
        return PartialView("~/Views/MacOS/Partials/Forms/FormFieldLongTextEditorPanel.cshtml");
    }
    
    [HttpGet("FormFieldCheckboxEditorPanel")]
    public IActionResult FormFieldCheckboxEditorPanel()
    {
        return PartialView("~/Views/MacOS/Partials/Forms/FormFieldCheckboxEditorPanel.cshtml");
    }
    
    [HttpGet("FormFieldDateEditorPanel")]
    public IActionResult FormFieldDateEditorPanel()
    {
        return PartialView("~/Views/MacOS/Partials/Forms/FormFieldDateEditorPanel.cshtml");
    }
    
    [HttpGet("FormFieldTimeEditorPanel")]
    public IActionResult FormFieldTimeEditorPanel()
    {
        return PartialView("~/Views/MacOS/Partials/Forms/FormFieldTimeEditorPanel.cshtml");
    }
    
    [HttpGet("FormFieldDropDownEditorPanel")]
    public IActionResult FormFieldDropDownEditorPanel()
    {
        return PartialView("~/Views/MacOS/Partials/Forms/FormFieldDropDownEditorPanel.cshtml");
    }
    
    [HttpGet("FormFieldMultiSelectEditorPanel")]
    public IActionResult FormFieldMultiSelectEditorPanel()
    {
        return PartialView("~/Views/MacOS/Partials/Forms/FormFieldMultiSelectEditorPanel.cshtml");
    }
    
    [HttpGet("FormFieldFileEditorPanel")]
    public IActionResult FormFieldFileEditorPanel()
    {
        return PartialView("~/Views/MacOS/Partials/Forms/FormFieldFileEditorPanel.cshtml");
    }
    
    [HttpGet("FormFieldBlankOption")]
    public IActionResult FormFieldBlankOption()
    {
        return PartialView("~/Views/MacOS/Partials/Forms/FormFieldBlankOption.cshtml");
    }
    
    // Content
    [HttpGet("Content")]
    public IActionResult Content()
    {
        return View();
    }
    
    [HttpGet("ContentWorkspace")]
    public IActionResult ContentWorkspace()
    {
        return PartialView("~/Views/MacOS/Workspaces/Content.cshtml");
    }
    
    [HttpGet("ContentTypeSelectorPanel")]
    public IActionResult ContentTypeSelectorPanel()
    {
        return PartialView("~/Views/MacOS/Partials/ContentTypeSelectorPanel.cshtml");
    }

    [HttpGet("ContentHeadlineConfigurationPanel")]
    public IActionResult ContentHeadlineConfigurationPanel()
    {
        return PartialView("~/Views/MacOS/Partials/Content/ContentHeadlineConfigurationPanel.cshtml");
    }
    
    [HttpGet("ContentTextConfigurationPanel")]
    public IActionResult ContentTextConfigurationPanel()
    {
        return PartialView("~/Views/MacOS/Partials/Content/ContentTextConfigurationPanel.cshtml");
    }
    
    [HttpGet("ContentLinkConfigurationPanel")]
    public IActionResult ContentLinkConfigurationPanel()
    {
        return PartialView("~/Views/MacOS/Partials/Content/ContentLinkConfigurationPanel.cshtml");
    }
    
    [HttpGet("ContentImageConfigurationPanel")]
    public IActionResult ContentImageConfigurationPanel()
    {
        return PartialView("~/Views/MacOS/Partials/Content/ContentImageConfigurationPanel.cshtml");
    }
    
    [HttpGet("ContentVideoConfigurationPanel")]
    public IActionResult ContentVideoConfigurationPanel()
    {
        return PartialView("~/Views/MacOS/Partials/Content/ContentVideoConfigurationPanel.cshtml");
    }
}