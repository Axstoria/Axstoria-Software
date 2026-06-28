using CharacterSheet.Presenter.ViewModel;
using CharacterSheet.Presenter.ViewModel.Widgets;
using Loxodon.Framework.Binding;

namespace CharacterSheet.Presenter.View.Widgets.CounterWidget
{
    public class CounterWidgetView : WidgetView
    {
        private CounterWidgetViewModel _vm;
        
        protected override void Start()
        {
            base.Start();

            _vm = this.BindingContext().DataContext as CounterWidgetViewModel;
            if (_vm == null) return;

            var bindingSet = this.CreateBindingSet<WidgetView, WidgetViewModel>();

            bindingSet.Build();
            _vm.AddStatCommand.Execute("attack");
        }
    }
}