using UnityEngine;

namespace CharacterSheet.Domain.Widgets
{
    public class LabelWidget : SheetWidget
    {
        public int fontSize;
        public string prefix;
        public bool richText;
        public LabelWidget(string id, Rect layout) : base(id, layout) {}
    }
}