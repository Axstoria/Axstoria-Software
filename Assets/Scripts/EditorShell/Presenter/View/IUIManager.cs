using UnityEngine.UIElements;

namespace EditorShell.Presenter.View
{
    public interface IUIManager
    {
        void Init(VisualElement root, VisualElement managedUI, ThemeStyleSheet theme);
        void ToggleUI(DropdownMenuAction action);
        string Name { get; }
    }
}
