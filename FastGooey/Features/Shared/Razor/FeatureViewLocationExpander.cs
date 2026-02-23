using Microsoft.AspNetCore.Mvc.Razor;

namespace FastGooey.Features.Shared.Razor;

public sealed class FeatureViewLocationExpander : IViewLocationExpander
{
    private const string FeaturePathKey = "feature-path";

    private static readonly IReadOnlyDictionary<string, string> ControllerViewRoots =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Auth"] = "Auth/Login/Views",
            ["SignUp"] = "Auth/SignUp/Views",
            ["AccountManagement"] = "Account/Management/Views",
            ["WorkspaceSelector"] = "Workspaces/Selector/Views",
            ["Workspaces"] = "Workspaces/Home/Views",
            ["WorkspaceManagement"] = "Workspaces/Management/Views",
            ["Media"] = "Media/Library/Views/Media",
            ["MediaPalette"] = "Media/Palette/Views",
            ["LinkEditor"] = "Media/LinkEditor/Views",
            ["Widgets"] = "Widgets/Shell/Views",
            ["Clock"] = "Widgets/Clock/Views",
            ["Map"] = "Widgets/Map/Views",
            ["RssFeed"] = "Widgets/RssFeed/Views",
            ["Weather"] = "Widgets/Weather/Views",
            ["MacOS"] = "Interfaces/Mac/Shell/Views",
            ["MacTable"] = "Interfaces/Mac/Table/Views",
            ["MacCollection"] = "Interfaces/Mac/Collection/Views",
            ["MacContent"] = "Interfaces/Mac/Content/Views",
            ["MacOutline"] = "Interfaces/Mac/Outline/Views",
            ["MacSourceList"] = "Interfaces/Mac/SourceList/Views",
            ["AppleMobile"] = "Interfaces/AppleMobile/Shell/Views",
            ["AppleMobileList"] = "Interfaces/AppleMobile/List/Views",
            ["AppleMobileCollection"] = "Interfaces/AppleMobile/Collection/Views",
            ["AppleMobileContent"] = "Interfaces/AppleMobile/Content/Views",
            ["AppleMobileForm"] = "Interfaces/AppleMobile/List/Views",
            ["AppleTv"] = "Interfaces/AppleTv/Shell/Views",
            ["AppleTvAlert"] = "Interfaces/AppleTv/Alert/Views",
            ["AppleTvCatalog"] = "Interfaces/AppleTv/Catalog/Views",
            ["AppleTvCompilation"] = "Interfaces/AppleTv/Compilation/Views",
            ["AppleTvDescriptiveAlert"] = "Interfaces/AppleTv/DescriptiveAlert/Views",
            ["AppleTvDiv"] = "Interfaces/AppleTv/Div/Views",
            ["AppleTvForm"] = "Interfaces/AppleTv/Form/Views",
            ["AppleTvList"] = "Interfaces/AppleTv/List/Views",
            ["AppleTvMediaGrid"] = "Interfaces/AppleTv/MediaGrid/Views",
            ["AppleTvLoading"] = "Interfaces/AppleTv/Loading/Views",
            ["AppleTvMedia"] = "Interfaces/AppleTv/Media/Views",
            ["AppleTvMain"] = "Interfaces/AppleTv/Main/Views",
            ["AppleTvMenuBar"] = "Interfaces/AppleTv/MenuBar/Views",
            ["AppleTvOneUp"] = "Interfaces/AppleTv/OneUp/Views",
            ["AppleTvParade"] = "Interfaces/AppleTv/Parade/Views",
            ["AppleTvProduct"] = "Interfaces/AppleTv/Product/Views",
            ["AppleTvProductBundle"] = "Interfaces/AppleTv/ProductBundle/Views",
            ["AppleTvRating"] = "Interfaces/AppleTv/Rating/Views",
            ["AppleTvSearch"] = "Interfaces/AppleTv/Search/Views",
            ["AppleTvShowcase"] = "Interfaces/AppleTv/Showcase/Views",
            ["AppleTvStack"] = "Interfaces/AppleTv/Stack/Views"
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
