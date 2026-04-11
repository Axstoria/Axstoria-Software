using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.UIElements;

public class ViewportUIManager : MonoBehaviour, IUIManager
{
    private ThemeStyleSheet theme;
    private VisualElement root;
    private VisualElement managedUI;
    private bool isActive = true;
    private const string UIName = "Viewport";

    string IUIManager.Name => UIName;

    public void Init(VisualElement root, VisualElement managedUI, ThemeStyleSheet theme)
    {
        this.root = root;
        this.managedUI = managedUI;
        this.theme = theme;
    }

    private void SetActive(bool isActive)
    {
        this.isActive = isActive;
        if (isActive)
            root.Add(managedUI);
        else
            root.Remove(managedUI);
    }

    void Start()
    {
    }

    void IUIManager.ToggleUI(DropdownMenuAction action)
    {
        SetActive(!isActive);
    }
}
