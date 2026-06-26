using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace CharacterSheet.Domain
{
    public class Sheet
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public List<SheetWidget> Widgets { get; } = new();
        public List<StatValue> Stats { get; } = new();
        
        [field: JsonIgnore] public event Action<StatValue> OnStatAdded;
        [field: JsonIgnore] public event Action<StatValue> OnStatRemoved;
        [field: JsonIgnore] public event Action<SheetWidget> OnWidgetAdded;
        [field: JsonIgnore] public event Action<SheetWidget> OnWidgetRemoved;
        [field: JsonIgnore] public event Action OnAppearanceChanged;
        
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

        private bool _hasBorder;

        public bool HasBorder
        {
            get => _hasBorder;
            set 
            {
                if (_hasBorder == value) return;
                _hasBorder = value;
                OnAppearanceChanged?.Invoke();
            }
        }

        private float _borderThickness = 4F;

        public float BorderThickness
        {
            get => _borderThickness;
            set
            {
                if (_borderThickness == value)  return;
                _borderThickness = value;
                OnAppearanceChanged?.Invoke();
            }
        }
        
        private Color _borderColor = Color.black;

        public Color BorderColor
        {
            get => _borderColor;
            set
            {
                if (_borderColor == value) return;
                _borderColor = value;
                OnAppearanceChanged?.Invoke();
            }
        }
        private Color _backgroundColor = Color.white;

        public Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                if (_backgroundColor == value) return;
                _backgroundColor = value;
                OnAppearanceChanged?.Invoke();
            }
        }

        private string _backgroundImagePath;

        public string BackgroundImagePath
        {
            get => _backgroundImagePath;
            set
            {
                if (_backgroundImagePath == value) return;
                _backgroundImagePath = value;
                OnAppearanceChanged?.Invoke();
            }
        }
    }
}