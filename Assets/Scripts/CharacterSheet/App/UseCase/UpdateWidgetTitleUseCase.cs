using CharacterSheet.Domain;

namespace CharacterSheet.App.UseCase
{
    public class UpdateWidgetTitleUseCase
    {
        public void Execute(SheetWidget widget, string text)
        {
            if (widget == null) return;
            widget.Title = text;
        }
    }
}