using UnityEngine;

namespace CharacterSheet.Domain.Widgets
{
    public class PointGaugeWidget : SheetWidget
    {
        public int MaxPoints { get; set; }
        public bool FillByValue { get; set; } = true;
    }
}