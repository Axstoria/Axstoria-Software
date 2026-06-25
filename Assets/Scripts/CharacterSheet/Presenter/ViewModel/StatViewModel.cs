using CharacterSheet.Domain;
using Loxodon.Framework.Observables;
using Shared.Domain;
using UnityEngine;

namespace CharacterSheet.Presenter.ViewModel
{
    public class StatViewModel : ObservableObject
    {
        private readonly WidgetStatBinding _binding;
        private readonly Stat _definition;
        
        public string Id => _binding.StatId;
        public bool ShowLabel => _binding.ShowLabel;
        public Color Color => _binding.ColorOverride ?? Color.white;
        
        public string DisplayName => !string.IsNullOrEmpty(_binding.DisplayNameOverride) 
            ? _binding.DisplayNameOverride 
            : _definition.Name;
        
        public float CurrentValue { get; private set; }
        public float MaxValue { get; private set; }
        public bool IsRanged => MaxValue > CurrentValue;
        
        public StatViewModel(WidgetStatBinding binding, Stat definition)
        {
            _binding = binding;
            _definition = definition;
            
            MaxValue = _definition.AbsoluteMax; 
            CurrentValue = _definition.AbsoluteMax * 0.75f;
        }

        public void Dispose()
        {
            
        }
    }
}
