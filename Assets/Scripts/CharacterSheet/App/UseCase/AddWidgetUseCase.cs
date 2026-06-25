using System;
using System.Linq;
using CharacterSheet.Domain;
using UnityEngine;

namespace CharacterSheet.App.UseCase
{
    public class AddWidgetUseCase
    {
        public string Execute(Sheet sheet, WidgetType type)
        {
           var widget = WidgetFactory.Create(type);
           sheet.AddWidget(widget);
           return widget.Id;
        }
    }
}
