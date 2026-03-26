using UnityEngine;
using UnityEngine.UIElements;

public class ViewSwitcherController : MonoBehaviour
{
    private const string SelectedClass = "view-btn--selected";

    private Button _isoBtn;
    private Button _topBtn;

    public void Init(VisualElement root)
    {
        _isoBtn = root.Q<Button>("btn-iso-view");
        _topBtn = root.Q<Button>("btn-top-view");

        _isoBtn.clicked += () => SelectView(_isoBtn, _topBtn);
        _topBtn.clicked += () => SelectView(_topBtn, _isoBtn);
    }

    private void SelectView(Button selected, Button other)
    {
        selected.AddToClassList(SelectedClass);
        other.RemoveFromClassList(SelectedClass);
    }
}
