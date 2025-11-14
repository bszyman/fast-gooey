namespace FastGooey.Models.JsonDataModels.Mac;

public class MacOutlineJsonDataModel
{
    public Guid Identifier { get; set; } = Guid.Empty;
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public List<MacOutlineJsonDataModel> Children { get; set; } = [];
    
    public MacOutlineJsonDataModel? FindById(Guid id)
    {
        var stack = new Stack<MacOutlineJsonDataModel>();
        stack.Push(this);

        while (stack.Count > 0)
        {
            var current = stack.Pop();

            if (current.Identifier == id)
            {
                return current;
            }

            // This handles "arrays on arrays": each node’s Children is another list
            foreach (var child in current.Children)
            {
                stack.Push(child);
            }
        }

        return null;
    }
    
    public bool TryFindParentListFor(Guid id, out List<MacOutlineJsonDataModel>? parentList)
    {
        // Special-case: root itself
        if (Identifier == id)
        {
            parentList = null;
            return true;
        }

        var stack = new Stack<(MacOutlineJsonDataModel Node, List<MacOutlineJsonDataModel> Children)>();
        stack.Push((this, Children));

        while (stack.Count > 0)
        {
            var (node, children) = stack.Pop();

            foreach (var child in children)
            {
                if (child.Identifier == id)
                {
                    parentList = children;
                    return true;
                }

                stack.Push((child, child.Children));
            }
        }

        parentList = null;
        return false;
    }
}
