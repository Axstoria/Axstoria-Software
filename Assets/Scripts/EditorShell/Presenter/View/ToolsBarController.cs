using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EditorShell.Presenter.View
{
    public class ToolsBarController : MonoBehaviour
    {
        private const string SelectedClass = "tool-btn--selected";

        private List<Button> _toolButtons;
        private Button _btnSelect;

        public Action OnSelectToolClicked;

        public void Init(VisualElement root)
        {
            _btnSelect = root.Q<Button>("btn-select");

            _toolButtons = new List<Button>
            {
                root.Q<Button>("btn-layers"),
                root.Q<Button>("btn-move"),
                _btnSelect,
                root.Q<Button>("btn-fog"),
                root.Q<Button>("btn-measure"),
                root.Q<Button>("btn-visibility"),
            };

            foreach (Button btn in _toolButtons)
            {
                btn.clicked += () => SelectTool(btn);
            }

            _btnSelect.clicked += () => OnSelectToolClicked?.Invoke();

            // Select is the resting default
            SelectTool(_btnSelect);
        }

        private void SelectTool(Button selected)
        {
            foreach (Button btn in _toolButtons)
                btn.RemoveFromClassList(SelectedClass);
            selected.AddToClassList(SelectedClass);
        }
    }
}
