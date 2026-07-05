using CharacterSheet.Presenter.ViewModel;
using Loxodon.Framework.Views;

namespace CharacterSheet.Presenter.View
{
    public abstract class IStatContainerView : UIView
    {
        protected WidgetItemViewModel _vm;
    }
}