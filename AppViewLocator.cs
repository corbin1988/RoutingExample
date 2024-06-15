using System;
using ReactiveUI;
using RoutingExample.ViewModels;
using RoutingExample.Views;

namespace RoutingExample;

public class AppViewLocator : ReactiveUI.IViewLocator
{
  public IViewFor ResolveView<T>(T viewModel, string contract = null) => viewModel switch
  {
    FirstViewModel context => new FirstView { DataContext = context },
    _ => throw new ArgumentOutOfRangeException(nameof(viewModel))
  };
}