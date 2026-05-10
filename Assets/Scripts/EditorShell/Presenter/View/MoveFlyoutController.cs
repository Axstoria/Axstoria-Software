using UnityEngine;
using UnityEngine.UIElements;

namespace EditorShell.Presenter.View
{
    public class MoveFlyoutController : MonoBehaviour
    {
        private const string FlyoutVisibleClass = "move-flyout--visible";
        private const string SelectedClass = "tool-btn--selected";

        private Button _mainButton;
        private VisualElement _flyout;
        private VisualElement _wrapper;
        private Button _btnMove;
        private Button _btnRotate;
        private Button _btnScale;

        private IVisualElementScheduledItem _hideSchedule;
        private string _activeToolClass = "tool-btn--move";

        public void Init(VisualElement root)
        {
            _wrapper = root.Q<VisualElement>("move-tool-wrapper");
            _mainButton = root.Q<Button>("btn-move");
            _flyout = root.Q<VisualElement>("move-flyout");
            _btnMove = root.Q<Button>("btn-flyout-move");
            _btnRotate = root.Q<Button>("btn-flyout-rotate");
            _btnScale = root.Q<Button>("btn-flyout-scale");

            _mainButton.RegisterCallback<MouseEnterEvent>(evt => ShowFlyout());
            _wrapper.RegisterCallback<MouseLeaveEvent>(evt => ScheduleHideFlyout());
            _flyout.RegisterCallback<MouseEnterEvent>(evt => CancelHide());

            _btnMove.clicked += () => SelectFlyoutTool("tool-btn--move", "Move");
            _btnRotate.clicked += () => SelectFlyoutTool("tool-btn--rotate", "Rotate");
            _btnScale.clicked += () => SelectFlyoutTool("tool-btn--scale", "Scale");

            _btnMove.AddToClassList(SelectedClass);
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

            _btnMove.RemoveFromClassList(SelectedClass);
            _btnRotate.RemoveFromClassList(SelectedClass);
            _btnScale.RemoveFromClassList(SelectedClass);

            switch (toolClass)
            {
                case "tool-btn--move":
                    _btnMove.AddToClassList(SelectedClass);
                    break;
                case "tool-btn--rotate":
                    _btnRotate.AddToClassList(SelectedClass);
                    break;
                case "tool-btn--scale":
                    _btnScale.AddToClassList(SelectedClass);
                    break;
            }

            _flyout.RemoveFromClassList(FlyoutVisibleClass);
        }
    }
}
