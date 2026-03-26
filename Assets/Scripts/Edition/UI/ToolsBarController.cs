using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ToolsBarController : MonoBehaviour
{
    private const string SelectedClass = "tool-btn--selected";

    private List<Button> _toolButtons;

    public void Init(VisualElement root)
    {
        _toolButtons = new List<Button>
        {
            root.Q<Button>("btn-layers"),
            root.Q<Button>("btn-move"),
            root.Q<Button>("btn-select"),
            root.Q<Button>("btn-fog"),
            root.Q<Button>("btn-measure"),
            root.Q<Button>("btn-visibility"),
        };

        foreach (Button btn in _toolButtons)
        {
            btn.clicked += () => SelectTool(btn);
        }
    }

    private void SelectTool(Button selected)
    {
        foreach (Button btn in _toolButtons)
            btn.RemoveFromClassList(SelectedClass);
        selected.AddToClassList(SelectedClass);
    }
}
