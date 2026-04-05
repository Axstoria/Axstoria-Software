using UnityEngine;
using UnityEngine.UIElements;

public class LayersFlyoutController : MonoBehaviour
{
    private const string FlyoutVisibleClass = "layers-flyout--visible";
    private const string SelectedClass = "tool-btn--selected";

    private Button _mainButton;
    private VisualElement _flyout;
    private VisualElement _wrapper;
    private Button _btnAllLayers;
    private Button _btnTerrain;
    private Button _btnBuildings;
    private Button _btnObjects;
    private Button _btnPawns;

    private IVisualElementScheduledItem _hideSchedule;
    private string _activeToolClass = "tool-btn--layers";

    public void Init(VisualElement root)
    {
        _wrapper = root.Q<VisualElement>("layers-tool-wrapper");
        _mainButton = root.Q<Button>("btn-layers");
        _flyout = root.Q<VisualElement>("layers-flyout");
        _btnAllLayers = root.Q<Button>("btn-flyout-all-layers");
        _btnTerrain = root.Q<Button>("btn-flyout-terrain");
        _btnBuildings = root.Q<Button>("btn-flyout-buildings");
        _btnObjects = root.Q<Button>("btn-flyout-objects");
        _btnPawns = root.Q<Button>("btn-flyout-pawns");

        _mainButton.RegisterCallback<MouseEnterEvent>(evt => ShowFlyout());
        _wrapper.RegisterCallback<MouseLeaveEvent>(evt => ScheduleHideFlyout());
        _flyout.RegisterCallback<MouseEnterEvent>(evt => CancelHide());

        _btnAllLayers.clicked += () => SelectFlyoutTool("tool-btn--layers", "All Layers");
        _btnTerrain.clicked += () => SelectFlyoutTool("tool-btn--terrain", "Terrain");
        _btnBuildings.clicked += () => SelectFlyoutTool("tool-btn--buildings", "Buildings");
        _btnObjects.clicked += () => SelectFlyoutTool("tool-btn--objects", "Objects");
        _btnPawns.clicked += () => SelectFlyoutTool("tool-btn--pawns", "Pawns");

        _btnAllLayers.AddToClassList(SelectedClass);
    }

    public System.Action OnFlyoutOpened;

    private void ShowFlyout()
    {
        CancelHide();
        OnFlyoutOpened?.Invoke();
        _flyout.AddToClassList(FlyoutVisibleClass);
    }

    public void HideImmediately()
    {
        CancelHide();
        _flyout.RemoveFromClassList(FlyoutVisibleClass);
    }

    private void ScheduleHideFlyout()
    {
        _hideSchedule = _flyout.schedule.Execute(() =>
        {
            _flyout.RemoveFromClassList(FlyoutVisibleClass);
        }).StartingIn(200);
    }

    private void CancelHide()
    {
        if (_hideSchedule != null)
        {
            _hideSchedule.Pause();
            _hideSchedule = null;
        }
    }

    private void SelectFlyoutTool(string toolClass, string tooltipText)
    {
        _mainButton.RemoveFromClassList(_activeToolClass);
        _mainButton.AddToClassList(toolClass);
        _mainButton.tooltip = tooltipText;
        _activeToolClass = toolClass;

        _btnAllLayers.RemoveFromClassList(SelectedClass);
        _btnTerrain.RemoveFromClassList(SelectedClass);
        _btnBuildings.RemoveFromClassList(SelectedClass);
        _btnObjects.RemoveFromClassList(SelectedClass);
        _btnPawns.RemoveFromClassList(SelectedClass);

        switch (toolClass)
        {
            case "tool-btn--layers":
                _btnAllLayers.AddToClassList(SelectedClass);
                break;
            case "tool-btn--terrain":
                _btnTerrain.AddToClassList(SelectedClass);
                break;
            case "tool-btn--buildings":
                _btnBuildings.AddToClassList(SelectedClass);
                break;
            case "tool-btn--objects":
                _btnObjects.AddToClassList(SelectedClass);
                break;
            case "tool-btn--pawns":
                _btnPawns.AddToClassList(SelectedClass);
                break;
        }

        _flyout.RemoveFromClassList(FlyoutVisibleClass);
    }
}
