using System.ComponentModel.DataAnnotations;
using FastGooey.Features.Interfaces.Mac.Shared.Models.FormModels.Mac;
using FastGooey.Models.FormModels;

namespace FastGooey.Tests.Models;

public class LinkContentFormModelTests
{
    [Fact]
    public void Validate_ReturnsErrors_WhenRequiredFieldsMissing()
    {
        var model = new LinkContentFormModel();
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(model, context, results, validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(LinkContentFormModel.Title)));
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(LinkContentFormModel.Url)));
    }

    [Fact]
    public void Validate_Passes_WhenRequiredFieldsProvided()
    {
        var model = new LinkContentFormModel
        {
            Title = "Example title",
            Url = "https://example.com"
        };
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(model, context, results, validateAllProperties: true);

        Assert.True(isValid);
    }
}
