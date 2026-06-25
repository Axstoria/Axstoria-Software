using System;
using CharacterSheet.Domain;
using CharacterSheet.Domain.Widgets;
using UnityEngine;

namespace CharacterSheet.App
{
    public class WidgetFactory
    {
        public static SheetWidget Create(WidgetType type)
        {
            switch (type)
            {
                /*case WidgetType.StatBlock:
                    new */
                case WidgetType.PointGauge:
                    return new PointGaugeWidget();
                case WidgetType.Text:
                    return new TextWidget();
                case WidgetType.Bar:
                    return new BarWidget();
                default: throw new InvalidOperationException($"Widget type {type} not supported");
            }
        }
    }
}
