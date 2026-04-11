namespace CharacterSheet.Domain
{
    public class TextWidget : SheetWidget
    {
        public string Template { get; }

        public TextWidget(string id, string template) : base(id)
        {
            Template = template;
        }
    }
}