using System;
using System.Reactive;
using ReactiveUI;

namespace RoutingExample.ViewModels;

public class MainWindowViewModel : ReactiveObject, IScreen
{
  // The Router associated with this Screen.
  // Required by the IScreen interface.
  public RoutingState Router { get; } = new RoutingState();

  // The command that navigates a user to first view model.
  public ReactiveCommand<Unit, IRoutableViewModel> GoNext { get; }

  // The command that navigates a user back.
  public ReactiveCommand<Unit, IRoutableViewModel> GoBack => Router.NavigateBack;

  public MainWindowViewModel()
  {
    GoNext = ReactiveCommand.CreateFromObservable(
        () => Router.Navigate.Execute(new FirstViewModel(this))
    );

    GoNext.ThrownExceptions.Subscribe((Exception ex) =>
    {
      // Log the exception, show a message to the user, etc.
      Console.WriteLine($"An error occurred: {ex}");
    });
  }
}