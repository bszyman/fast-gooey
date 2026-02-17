using System.ComponentModel.DataAnnotations;
using FastGooey.Models.FormModels;

namespace FastGooey.Tests.Models;

public class MacTableItemEditorPanelFormModelTests
{
    [Fact]
    public void Validate_ReturnsError_WhenNameMissing()
    {
        var model = new MacTableItemEditorPanelFormModel();
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(model, context, results, validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(MacTableItemEditorPanelFormModel.GooeyName)));
    }

    [Fact]
    public void Validate_Passes_WhenNameProvided()
    {
        var model = new MacTableItemEditorPanelFormModel
        {
            GooeyName = "Table Item"
        };
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(model, context, results, validateAllProperties: true);

        Assert.True(isValid);
    }
}
