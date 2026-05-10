using CharacterSheet.Domain;
using Loxodon.Framework.Observables;

namespace CharacterSheet.Presentation.ViewModel
{
    public class StatViewModel : ObservableObject
    {
        private readonly StatValue _stat;
        
        public string Id => _stat.Id;
        
        public  StatViewModel(StatValue stat)
        {
            _stat = stat;
        }

        public void Dispose()
        {
            
        }
    }
}
