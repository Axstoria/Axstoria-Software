using System;
using CharacterSheet.Presenter.ViewModel;
using Loxodon.Framework.Commands;
using Loxodon.Framework.Contexts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CharacterSheet.Presenter.View
{
    public class SheetToolbarUIManager : MonoBehaviour
    {
        [SerializeField] private Button fileButton;
        [SerializeField] private Button editButton;
        [SerializeField] private Button windowButton;
        [SerializeField] private Button toolsButton;
        [SerializeField] private Button helpButton;
        [SerializeField] private SheetPanelsController panelsController;
        [SerializeField] private Sprite checkmarkSprite;

        private const float MenuWidth       = 160f;
        private const float ItemHeight      = 20f;
        private const float MenuPadding     = 4f;
        private const float MenuBorderWidth = 1f;
        private const float ItemTextIndent  = 22f;

        private static readonly Color MenuBackgroundColor = new Color32(220, 220, 220, 255);
        private static readonly Color MenuBorderColor     = new Color32(190, 190, 190, 255);
        private static readonly Color ItemHoverColor      = new Color32(194, 211, 219, 255);
        private static readonly Color TextColor           = new Color32(5, 5, 5, 255);
        private static readonly Color DisabledTextColor   = new Color(0.02f, 0.02f, 0.02f, 0.3f);

        private CharacterSheetEditorViewModel _vm;
        private Canvas _canvas;
        private TMP_FontAsset _font;
        private GameObject _openMenu;
        private GameObject _blocker;
        private Button _openMenuOwner;

        private void Start()
        {
            _vm     = Context.GetApplicationContext().GetContainer().Resolve<CharacterSheetEditorViewModel>();
            _canvas = GetComponentInParent<Canvas>();

            var label = fileButton != null ? fileButton.GetComponentInChildren<TMP_Text>() : null;
            _font = label != null ? label.font : TMP_Settings.defaultFontAsset;

            Bind(fileButton, new (string, Action)[]
            {
                ("Import Sheet",      () => Execute(_vm?.ImportCommand)),
                ("Export Sheet",      () => Execute(_vm?.ExportCommand))
            });

            Bind(editButton, new (string, Action)[]
            {
                ("Undo", () => Execute(_vm?.UndoCommand)),
                ("Redo", () => Execute(_vm?.RedoCommand))
            });

            Bind(windowButton, new (string, Action, Func<bool>)[]
            {
                ("Elements", () => { if (panelsController != null) panelsController.ToggleLeft(); },
                             () => panelsController != null && panelsController.IsLeftPresent),
                ("Details",  () => { if (panelsController != null) panelsController.ToggleRight(); },
                             () => panelsController != null && panelsController.IsRightPresent)
            });

            Bind(toolsButton, Array.Empty<(string, Action)>());

            Bind(helpButton, new (string, Action)[]
            {
                ("About", null)
            });
        }

        private static void Execute(ICommand command)
        {
            if (command != null && command.CanExecute(null))
                command.Execute(null);
        }

        private void Bind(Button button, (string Label, Action Callback)[] items)
        {
            var withStatus = new (string, Action, Func<bool>)[items.Length];
            for (int i = 0; i < items.Length; i++)
                withStatus[i] = (items[i].Label, items[i].Callback, null);
            Bind(button, withStatus);
        }

        private void Bind(Button button, (string Label, Action Callback, Func<bool> Status)[] items)
        {
            if (button == null) return;
            button.onClick.AddListener(() => ToggleMenu(button, items));
        }

        private void ToggleMenu(Button owner, (string Label, Action Callback, Func<bool> Status)[] items)
        {
            bool wasOpen = _openMenuOwner == owner;
            CloseMenu();
            if (wasOpen || items.Length == 0) return;
            OpenMenu(owner, items);
        }

        private void OpenMenu(Button owner, (string Label, Action Callback, Func<bool> Status)[] items)
        {
            _openMenuOwner = owner;
            _blocker = CreateBlocker();
            _openMenu = CreateMenuPanel(owner, items);
        }

        private void CloseMenu()
        {
            if (_openMenu != null) Destroy(_openMenu);
            if (_blocker != null) Destroy(_blocker);

            var eventSystem = UnityEngine.EventSystems.EventSystem.current;
            if (_openMenuOwner != null && eventSystem != null &&
                eventSystem.currentSelectedGameObject == _openMenuOwner.gameObject)
                eventSystem.SetSelectedGameObject(null);

            _openMenu      = null;
            _blocker       = null;
            _openMenuOwner = null;
        }

        private GameObject CreateBlocker()
        {
            var go = new GameObject("ToolbarMenuBlocker", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            var rt = (RectTransform)go.transform;
            rt.SetParent(_canvas.transform, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            var image = go.GetComponent<Image>();
            image.color = Color.clear;

            var button = go.GetComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(CloseMenu);
            return go;
        }

        private GameObject CreateMenuPanel(Button owner, (string Label, Action Callback, Func<bool> Status)[] items)
        {
            var go = new GameObject("ToolbarMenu", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(_canvas.transform, false);
            rt.pivot     = new Vector2(0f, 1f);
            rt.sizeDelta = new Vector2(MenuWidth, items.Length * ItemHeight + (MenuPadding + MenuBorderWidth) * 2f);

            var corners = new Vector3[4];
            ((RectTransform)owner.transform).GetWorldCorners(corners);
            rt.position = corners[0];

            go.GetComponent<Image>().color = MenuBorderColor;

            var bgGo = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var bgRt = (RectTransform)bgGo.transform;
            bgRt.SetParent(rt, false);
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = new Vector2(MenuBorderWidth, MenuBorderWidth);
            bgRt.offsetMax = new Vector2(-MenuBorderWidth, -MenuBorderWidth);
            bgGo.GetComponent<Image>().color = MenuBackgroundColor;

            for (int i = 0; i < items.Length; i++)
                CreateMenuItem(rt, i, items[i].Label, items[i].Callback, items[i].Status);

            return go;
        }

        private void CreateMenuItem(RectTransform parent, int index, string label, Action callback, Func<bool> status)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot     = new Vector2(0.5f, 1f);
            rt.offsetMin = new Vector2(MenuBorderWidth, 0f);
            rt.offsetMax = new Vector2(-MenuBorderWidth, 0f);
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, ItemHeight);
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, -MenuBorderWidth - MenuPadding - index * ItemHeight);

            var image = go.GetComponent<Image>();
            image.color = Color.white;

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor      = MenuBackgroundColor;
            colors.highlightedColor = ItemHoverColor;
            colors.selectedColor    = ItemHoverColor;
            colors.pressedColor     = ItemHoverColor;
            colors.disabledColor    = MenuBackgroundColor;
            button.colors = colors;

            var textGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            var textRt = (RectTransform)textGo.transform;
            textRt.SetParent(rt, false);
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(ItemTextIndent, 0f);
            textRt.offsetMax = Vector2.zero;

            var text = textGo.GetComponent<TextMeshProUGUI>();
            text.font      = _font;
            text.fontSize  = 12f;
            text.color     = callback != null ? TextColor : DisabledTextColor;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.text      = label;

            if (status != null && status() && checkmarkSprite != null)
            {
                var checkGo = new GameObject("Check", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                var checkRt = (RectTransform)checkGo.transform;
                checkRt.SetParent(rt, false);
                checkRt.anchorMin        = new Vector2(0f, 0.5f);
                checkRt.anchorMax        = new Vector2(0f, 0.5f);
                checkRt.pivot            = new Vector2(0f, 0.5f);
                checkRt.anchoredPosition = new Vector2(5f, 0f);
                checkRt.sizeDelta        = new Vector2(12f, 12f);

                var checkImage = checkGo.GetComponent<Image>();
                checkImage.sprite        = checkmarkSprite;
                checkImage.color         = TextColor;
                checkImage.raycastTarget = false;
            }

            if (callback != null)
            {
                button.onClick.AddListener(() =>
                {
                    CloseMenu();
                    callback();
                });
            }
            else
            {
                button.interactable = false;
            }
        }
    }
}
