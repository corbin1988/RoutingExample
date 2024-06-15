using ReactiveUI;

namespace RoutingExample.ViewModels;
public class FirstViewModel : ReactiveObject, IRoutableViewModel
{
  // Reference to IScreen that owns the routable view model.
  public IScreen HostScreen { get; }

  // Unique identifier for the routable view model.
  public string UrlPathSegment { get; } = "first";

  public FirstViewModel(IScreen screen) => HostScreen = screen;
}