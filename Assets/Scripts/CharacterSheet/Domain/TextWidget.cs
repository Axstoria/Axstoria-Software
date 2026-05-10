using UnityEngine;

namespace CharacterSheet.Domain
{
    public class TextWidget : SheetWidget
    {
        public string Template { get; }

        public TextWidget(string id, Rect layout, string template = "enter text") : base(id, layout)
        {
            Template = template;
        }
    }
}