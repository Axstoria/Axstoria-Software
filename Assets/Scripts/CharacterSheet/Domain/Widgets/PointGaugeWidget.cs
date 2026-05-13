using UnityEngine;

namespace CharacterSheet.Domain
{
    public class PointGaugeWidget : SheetWidget
    {
        public int MaxPoints { get; set; }
        public bool FillByValue { get; set; } = true;

        public PointGaugeWidget(string id, Rect layout) : base(id, layout) {}
    }
}