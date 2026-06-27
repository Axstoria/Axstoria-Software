using CharacterSheet.Domain.Widgets;

namespace CharacterSheet.App.UseCase
{
    public class UpdateTextWidgetUseCase
    {
        public  void Execute(TextWidget widget, string text)
        {
            widget.TextTemplate = text;
        }
    }
}