using System.ComponentModel.DataAnnotations;
using FastGooey.Features.Interfaces.Mac.Shared.Models.FormModels.Mac;

namespace FastGooey.Tests.Models;

public class MacLinkContentFormModelTests
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
}
