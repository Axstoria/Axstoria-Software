using UnityEngine;
using CharacterSheet.Domain;

namespace CharacterSheet.App.UseCase
{
    public class RemoveWidgetUseCase
    {
        public void Execute(Sheet sheet, string id)
        {
            if (!sheet.HasWidget(id)) return;
            sheet.RemoveWidget(id);
        }
    }
}
