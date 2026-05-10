using CharacterSheet.Domain;
using UnityEngine;

namespace CharacterSheet.App
{
    public class WidgetFactory
    {
        public static SheetWidget Create(string id, WidgetType type, Rect layout)
        {
            switch (type)
            {
                /*case WidgetType.StatBlock:
                    new */
                case WidgetType.PointGauge:
                    return new PointGaugeWidget(id, layout);
                case WidgetType.Text:
                    return new TextWidget(id, layout);
                case WidgetType.Bar:
                    return new BarWidget(id, layout);
                default: return null;
            }
        }
    }
}
