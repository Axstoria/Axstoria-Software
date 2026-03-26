using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TooltipController : MonoBehaviour
{
    private const float ShowDelay = 1f;
    private const float OffsetY = 4f;

    private VisualElement _root;
    private VisualElement _tooltipEl;
    private Label _tooltipLabel;
    private Coroutine _pendingShow;
    private bool _tooltipWasShown = false;

    private readonly HashSet<VisualElement> _registeredGroups = new();

    public void Init(VisualElement root)
    {
        _root = root;
        _tooltipLabel = new Label();
        _tooltipLabel.AddToClassList("tooltip__label");

        _tooltipEl = new VisualElement();
        _tooltipEl.AddToClassList("tooltip");
        _tooltipEl.pickingMode = PickingMode.Ignore;
        _tooltipEl.style.display = DisplayStyle.None;
        _tooltipEl.Add(_tooltipLabel);

        root.Add(_tooltipEl);

        RegisterTooltips(root);
    }

    private void RegisterTooltips(VisualElement element)
    {
        foreach (VisualElement child in element.Children())
        {
            if (!string.IsNullOrEmpty(child.tooltip))
            {
                child.RegisterCallback<MouseEnterEvent>(OnButtonMouseEnter);
                child.RegisterCallback<MouseLeaveEvent>(OnButtonMouseLeave);

                if (element != null && _registeredGroups.Add(element))
                    element.RegisterCallback<MouseLeaveEvent>(OnGroupMouseLeave);
            }
            RegisterTooltips(child);
        }
    }

    private void OnButtonMouseEnter(MouseEnterEvent evt)
    {
        if (_pendingShow != null)
        {
            StopCoroutine(_pendingShow);
            _pendingShow = null;
        }

        if (evt.target is not VisualElement target) return;

        if (_tooltipWasShown)
            ShowTooltip(target);
        else
            _pendingShow = StartCoroutine(ShowAfterDelay(target));
    }

    private void OnButtonMouseLeave(MouseLeaveEvent evt)
    {
        if (_pendingShow != null)
        {
            StopCoroutine(_pendingShow);
            _pendingShow = null;
        }
        _tooltipEl.style.display = DisplayStyle.None;
    }

    private void OnGroupMouseLeave(MouseLeaveEvent evt)
    {
        _tooltipWasShown = false;
    }

    private IEnumerator ShowAfterDelay(VisualElement target)
    {
        yield return new WaitForSeconds(ShowDelay);
        ShowTooltip(target);
        _pendingShow = null;
    }

    private void ShowTooltip(VisualElement target)
    {
        _tooltipLabel.text = target.tooltip;
        _tooltipWasShown = true;

        Rect bounds = target.worldBound;
        _tooltipEl.style.left = -9999f;
        _tooltipEl.style.top = bounds.yMax + OffsetY;
        _tooltipEl.style.display = DisplayStyle.Flex;

        _tooltipEl.RegisterCallback<GeometryChangedEvent, Rect>(OnTooltipGeometryChanged, bounds);
    }

    private void OnTooltipGeometryChanged(GeometryChangedEvent evt, Rect targetBounds)
    {
        _tooltipEl.UnregisterCallback<GeometryChangedEvent, Rect>(OnTooltipGeometryChanged);

        float tooltipWidth = _tooltipEl.resolvedStyle.width;
        float rootWidth = _root.resolvedStyle.width;
        float leftPos = Mathf.Min(targetBounds.xMin, rootWidth - tooltipWidth);

        _tooltipEl.style.left = leftPos;
    }
}
