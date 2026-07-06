using CharacterSheet.App.DTO;
using CharacterSheet.Domain.Widgets;
using UnityEngine;

namespace CharacterSheet.App.UseCase
{
    public class UpdatePointGaugeWidgetUseCase
    {
        public  void Execute(PointGaugeWidget widget, PointGaugeConfigDTO dto)
        {
            widget.FillByValue = dto.FillByValue;
            widget.MaxPoints = dto.MaxPoints;
        }
    }
}