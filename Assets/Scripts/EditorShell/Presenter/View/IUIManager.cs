using UnityEngine;
using UnityEngine.UIElements;

public interface IUIManager
{
    public void Init(VisualElement root, VisualElement managedUI, ThemeStyleSheet theme);
    public void ToggleUI(DropdownMenuAction action);

    public string Name
    {
        get;
    }
}
