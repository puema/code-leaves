using System.Linq;
using HoloToolkit.Unity;

public class InteractionManager : Singleton<InteractionManager>
{
    public UiNode Root;
    
    public void Start()
    {
        Root = ApplicationManager.Instance.Root;
    }

    public void HandleNodeClick(string id)
    {
        UiNode selected = FindUiNode(id);
        if (selected == null) return;
        selected.IsSelected.Value ^= true;
    }

    public void HandleNodeFocusEnter(string id)
    {
        var focused = FindUiNode(id);
        if (focused == null) return;
        focused.IsFocused.Value = true;
    }
    
    public void HandleNodeFocusExit(string id)
    {
        var focused = FindUiNode(id);
        if (focused == null) return;
        focused.IsFocused.Value = false;
    }

    public void HandleEmptyClick()
    {
        Root
            .Traverse(x => (x as UiInnerNode)?.Children)
            .ToList()
            .ForEach(x => x.IsSelected.Value = false);
    }

    private UiNode FindUiNode(string id)
    {
        return Root
            .Traverse(x => (x as UiInnerNode)?.Children)
            .FirstOrDefault(x => x.Id == id);
    }
}