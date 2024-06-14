using System;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace RoutingExample
{
    public class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            this.WhenActivated(disposables => { });
            AvaloniaXamlLoader.Load(this);
        }

        private void WhenActivated(Action<object> value)
        {
            throw new NotImplementedException();
        }
    }
}