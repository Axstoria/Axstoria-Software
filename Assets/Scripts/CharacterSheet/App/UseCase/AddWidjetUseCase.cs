using System;
using System.Linq;
using CharacterSheet.Domain;
using UnityEngine;

namespace CharacterSheet.App.UseCase
{
    public class AddWidgetUseCase
    {
        public string Execute(Sheet sheet, WidgetType type, Rect layout)
        {
           var id = Guid.NewGuid().ToString();
           var widget = WidgetFactory.Create(id, type, layout);
           sheet.AddWidget(widget);
           return id;
        }
    }
}
