using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FastGooey.Utils;

public static class HtmlHelpers
{
    public static IHtmlContent AntiForgeryTokenValue(this IHtmlHelper helper)
    {
        var antiforgery = helper.ViewContext.HttpContext.RequestServices.GetService<IAntiforgery>();
        var tokens = antiforgery.GetAndStoreTokens(helper.ViewContext.HttpContext);

        var tokenValue = tokens.RequestToken;

        return new HtmlString(tokenValue);
    }
}