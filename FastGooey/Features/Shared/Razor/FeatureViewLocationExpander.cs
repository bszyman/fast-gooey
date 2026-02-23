using Microsoft.AspNetCore.Mvc.Razor;

namespace FastGooey.Features.Shared.Razor;

public sealed class FeatureViewLocationExpander : IViewLocationExpander
{
    private const string FeaturePathKey = "feature-path";

    private static readonly IReadOnlyDictionary<string, string> ControllerViewRoots =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Auth"] = "Auth/Login/Views/Auth",
            ["SignUp"] = "Auth/SignUp/Views/SignUp",
            ["AccountManagement"] = "Account/Management/Views/AccountManagement",
            ["WorkspaceSelector"] = "Workspaces/Selector/Views/WorkspaceSelector",
            ["Workspaces"] = "Workspaces/Home/Views/Workspaces",
            ["WorkspaceManagement"] = "Workspaces/Management/Views/WorkspaceManagement",
            ["Media"] = "Media/Library/Views/Media",
            ["MediaPalette"] = "Media/Palette/Views/MediaPalette",
            ["LinkEditor"] = "Media/LinkEditor/Views/LinkEditor",
            ["Widgets"] = "Widgets/Shell/Views/Widgets",
            ["Clock"] = "Widgets/Clock/Views/Clock",
            ["Map"] = "Widgets/Map/Views/Map",
            ["RssFeed"] = "Widgets/RssFeed/Views/RssFeed",
            ["Weather"] = "Widgets/Weather/Views/Weather",
            ["MacOS"] = "Interfaces/Mac/Shell/Views/MacOS",
            ["MacTable"] = "Interfaces/Mac/Table/Views/MacTable",
            ["MacCollection"] = "Interfaces/Mac/Collection/Views/MacCollection",
            ["MacContent"] = "Interfaces/Mac/Content/Views/MacContent",
            ["MacOutline"] = "Interfaces/Mac/Outline/Views/MacOutline",
            ["MacSourceList"] = "Interfaces/Mac/SourceList/Views/MacSourceList",
            ["AppleMobile"] = "Interfaces/AppleMobile/Shell/Views/AppleMobile",
            ["AppleMobileList"] = "Interfaces/AppleMobile/List/Views/AppleMobileList",
            ["AppleMobileCollection"] = "Interfaces/AppleMobile/Collection/Views/AppleMobileCollection",
            ["AppleMobileContent"] = "Interfaces/AppleMobile/Content/Views/AppleMobileContent",
            ["AppleMobileForm"] = "Interfaces/AppleMobile/List/Views/AppleMobileList",
            ["AppleTv"] = "Interfaces/AppleTv/Shell/Views/AppleTv",
            ["AppleTvAlert"] = "Interfaces/AppleTv/Alert/Views",
            ["AppleTvCatalog"] = "Interfaces/AppleTv/Catalog/Views/AppleTvCatalog",
            ["AppleTvCompilation"] = "Interfaces/AppleTv/Compilation/Views/AppleTvCompilation",
            ["AppleTvDescriptiveAlert"] = "Interfaces/AppleTv/DescriptiveAlert/Views",
            ["AppleTvDiv"] = "Interfaces/AppleTv/Div/Views/AppleTvDiv",
            ["AppleTvForm"] = "Interfaces/AppleTv/Form/Views/AppleTvForm",
            ["AppleTvList"] = "Interfaces/AppleTv/List/Views/AppleTvList",
            ["AppleTvMediaGrid"] = "Interfaces/AppleTv/MediaGrid/Views",
            ["AppleTvLoading"] = "Interfaces/AppleTv/Loading/Views/AppleTvLoading",
            ["AppleTvMedia"] = "Interfaces/AppleTv/Media/Views",
            ["AppleTvMain"] = "Interfaces/AppleTv/Main/Views",
            ["AppleTvMenuBar"] = "Interfaces/AppleTv/MenuBar/Views/AppleTvMenuBar",
            ["AppleTvOneUp"] = "Interfaces/AppleTv/OneUp/Views/AppleTvOneUp",
            ["AppleTvParade"] = "Interfaces/AppleTv/Parade/Views/AppleTvParade",
            ["AppleTvProduct"] = "Interfaces/AppleTv/Product/Views",
            ["AppleTvProductBundle"] = "Interfaces/AppleTv/ProductBundle/Views/AppleTvProductBundle",
            ["AppleTvRating"] = "Interfaces/AppleTv/Rating/Views/AppleTvRating",
            ["AppleTvSearch"] = "Interfaces/AppleTv/Search/Views/AppleTvSearch",
            ["AppleTvShowcase"] = "Interfaces/AppleTv/Showcase/Views/AppleTvShowcase",
            ["AppleTvStack"] = "Interfaces/AppleTv/Stack/Views/AppleTvStack"
        };

    public void PopulateValues(ViewLocationExpanderContext context)
    {
        if (!string.IsNullOrWhiteSpace(context.ControllerName) &&
            ControllerViewRoots.TryGetValue(context.ControllerName, out var root))
        {
            context.Values[FeaturePathKey] = root;
        }
    }

    public IEnumerable<string> ExpandViewLocations(
        ViewLocationExpanderContext context,
        IEnumerable<string> viewLocations)
    {
        if (context.Values.TryGetValue(FeaturePathKey, out var root) && !string.IsNullOrWhiteSpace(root))
        {
            yield return $"/Features/{root}/{{0}}.cshtml";
        }

        foreach (var location in viewLocations)
        {
            yield return location;
        }
    }
}
