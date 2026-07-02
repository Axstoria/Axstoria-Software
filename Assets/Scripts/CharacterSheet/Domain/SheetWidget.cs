using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace CharacterSheet.Domain
{
    public abstract class SheetWidget : IAppearanceTarget
    {
        public string Id { get; } = Guid.NewGuid().ToString();
        
        [field: JsonIgnore] public event Action<WidgetStatBinding> OnStatAdded;
        [field: JsonIgnore] public event Action<WidgetStatBinding> OnStatRemoved;
        [field: JsonIgnore] public event Action OnLayoutChanged;
        [field: JsonIgnore] public event Action OnAppearanceChanged;
        [field: JsonIgnore] public event Action OnTitleChanged;
        [field: JsonIgnore] public event Action OnContentChanged;
        
        
        private Rect _layout =  new Rect(0, 0, 250, 200);

        public Rect Layout
        {
            get => _layout;
            set
            {
                if (_layout == value) return;
                _layout = value;
                OnLayoutChanged?.Invoke();
            }
        }
        
        public List<WidgetStatBinding> Stats { get; } = new();

        private bool _hasBorder = true;

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

        private float _borderThickness = 2F;

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
        
        private string _title = "Title";

        public string Title
        {
            get => _title;
            set
            {
                if (_title == value) return;
                _title = value;
                OnTitleChanged?.Invoke();
            }
        }

        public void AddStat(WidgetStatBinding stat)
        {
            Stats.Add(stat);
            OnStatAdded?.Invoke(stat);
        }

        public void RemoveStat(string statId)
        {
            var stat = Stats.FirstOrDefault(s => s.StatId == statId);
            if (stat == null) return;
            Stats.Remove(stat);
            OnStatRemoved?.Invoke(stat);
        }

        protected void RaiseContentChanged()
        {
            OnContentChanged?.Invoke();
        }
    }
}