using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CharacterSheet.Domain
{
    public class Sheet
    {
        public string Id { get; }
        public List<SheetWidget> Widgets { get; } = new();
        public List<StatValue> Stats { get; } = new();
        
        public event Action<StatValue> OnStatAdded;
        public event Action<StatValue> OnStatRemoved;
        public event Action<SheetWidget> OnWidgetAdded;
        public event Action<SheetWidget> OnWidgetRemoved;
        public event Action OnAppearanceChanged;
        
        public StatValue GetStat(string statId) => Stats.FirstOrDefault(s => s.Id == statId);
        public bool HasStat(string statId) => Stats.Any(s => s.Id == statId);

        public void AddStat(StatValue stat)
        {
            Stats.Add(stat);
            OnStatAdded?.Invoke(stat);
        }

        public void RemoveStat(string statId)
        {
            var stat = Stats.FirstOrDefault(s => s.Id == statId);
            if (stat == null) return;
            Stats.Remove(stat);
            OnStatRemoved?.Invoke(stat);
        }
        
        public SheetWidget GetWidget(string widgetId) => Widgets.FirstOrDefault(w => w.Id == widgetId);
        public bool HasWidget(string widgetId) => Widgets.Any(s => s.Id == widgetId);

        public void AddWidget(SheetWidget widget)
        {
            Widgets.Add(widget);
            OnWidgetAdded?.Invoke(widget);
        }
        
        public void RemoveWidget(string widgetId)
        {
            var widget = Widgets.FirstOrDefault(w => w.Id == widgetId);
            if (widget == null) return;
            Widgets.Remove(widget);
            OnWidgetRemoved?.Invoke(widget);
        }
        
        public bool HasBorder { get; set; }
        public float BorderThickness { get; set; }
        public Color BorderColor { get; set; }
        public Color BackgroundColor { get; set; }
        public string BackgroundImagePath { get; set; }
        
        public Sheet(string id)
        {
            Id = id;
        }
    }
}