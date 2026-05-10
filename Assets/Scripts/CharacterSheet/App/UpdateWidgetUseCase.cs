using System.Linq;
using CharacterSheet.Domain;
using UnityEngine;

namespace CharacterSheet.App
{
    public class UpdateWidgetUseCase
    {
        public void Execute(Sheet sheet, string widgetId, Rect newPosition)
        {
            var widget = sheet.GetWidget(widgetId);
            if (widget == null) return;
            widget.Layout = newPosition;
        }
    }
}
