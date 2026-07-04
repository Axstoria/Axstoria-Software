namespace CharacterSheet.Domain.Widgets
{
    public class TextWidget : SheetWidget
    {
        private string _textTemplate = "";
        public string TextTemplate
        {
            get => _textTemplate;
            set
            {
                if (_textTemplate == value) return;
                _textTemplate = value;
                RaiseContentChanged();
            }
        }
    }
}