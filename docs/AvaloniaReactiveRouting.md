# AvaloniaUI Routing Using ReactiveUI

ReactiveUI routing consists of an **IScreen** that contains current **RoutingState**, several **IRoutableViewModels**, and a platform-specific XAML control called **RoutedViewHost**. `RoutingState `manages the view model navigation stack and allows view models to navigate to other view models. IScreen is the root of a navigation stack; despite the name, its views don't have to occupy the whole screen. RoutedViewHost monitors an instance of RoutingState, responding to any changes in the navigation stack by creating and embedding the appropriate view.

## Highlevel Overview of How ReactUI Routing Works

[![ReactiveUI Routing](http://img.youtube.com/vi/q6uWPtKw3UQ/0.jpg)](https://youtu.be/q6uWPtKw3UQ?si=jLwzXvB-31NtL7rG&t=1088)

## Routing Example

Create a new empty project from Avalonia templates. To use those, clone the `avalonia-dotnet-templates` repository, install the templates and create a new project named `RoutingExample` based on `avalonia.app` template. Install Avalonia.ReactiveUI package into the project.

```
git clone https://github.com/AvaloniaUI/avalonia-dotnet-templates
dotnet new --install ./avalonia-dotnet-templates
dotnet new avalonia.app -o RoutingExample
cd ./RoutingExample
dotnet add package Avalonia.ReactiveUI
```

### FirstViewModel.cs

First, create routable view models and corresponding views. We derive routable view models from the IRoutableViewModel interface from ReactiveUI namespace, and from ReactiveObject as well. ReactiveObject is the base class for view model classes, and it implements INotifyPropertyChanged.

```C#
namespace RoutingExample.ViewModels;
public class FirstViewModel : ReactiveObject, IRoutableViewModel
{
  // Reference to IScreen that owns the routable view model.
  public IScreen HostScreen { get; }

  // Unique identifier for the routable view model.
  public string UrlPathSegment { get; } = "first";

  public FirstViewModel(IScreen screen) => HostScreen = screen;
}
```

### FirstView.axaml

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="clr-namespace:RoutingExample.ViewModels"
             x:Class="RoutingExample.Views.FirstView"
             x:DataType="vm:FirstViewModel">
    <StackPanel HorizontalAlignment="Center"
                VerticalAlignment="Center">
        <TextBlock Text="Hi, I'm the first view!" />
        <TextBlock Text="{Binding UrlPathSegment}" />
    </StackPanel>
</UserControl>
```

### FirstView.axaml.cs

If we need to handle view model activation and deactivation, then we add a call to WhenActivated to the view. Generally, a rule of thumb is to always add WhenActivated to your views, see Activation docs for more info.

```C#
namespace RoutingExample.Views;
public partial class FirstView : ReactiveUserControl<FirstViewModel>
{
  public FirstView()
  {
    this.WhenActivated(disposables => { });
    AvaloniaXamlLoader.Load(this);
  }
}
```

### MainWindowViewModel.cs

Then, create a view model implementing the `IScreen` interface. It contains current `RoutingState` that manages the navigation stack. `RoutingState` also contains helper commands allowing you to navigate back and forward.

Actually, you can use as many `IScreens` as you need in your application. Despite the name, it doesn't have to occupy the whole screen. You can use nested routing, place `IScreens `side-by-side, etc.

```C#
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
```

### MainWindow.axaml

Now we need to place the `RoutedViewHost` XAML control to our main view. It will resolve and embed appropriate views for the view models based on the supplied `IViewLocator` implementation and the passed `Router` instance of type `RoutingState`. Note, that you need to import `rxui` namespace for `RoutedViewHost` to work. Additionally, you can override animations that are played when RoutedViewHost changes a view — simply override `RoutedViewHost.PageTransition` property in XAML.

```XML
<Window xmlns="https://github.com/avaloniaui"
        xmlns:rxui="http://reactiveui.net"
        xmlns:app="clr-namespace:RoutingExample"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        mc:Ignorable="d"
        d:DesignWidth="800"
        d:DesignHeight="450"
        x:Class="RoutingExample.MainWindow"
        x:DataType="app:ViewModels.MainWindowViewModel"
        Title="RoutingExample">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="*" />
            <RowDefinition Height="Auto" />
        </Grid.RowDefinitions>
        <rxui:RoutedViewHost Grid.Row="0"
                             Router="{Binding Router}">
            <rxui:RoutedViewHost.DefaultContent>
                <TextBlock Text="Default content"
                           HorizontalAlignment="Center"
                           VerticalAlignment="Center" />
            </rxui:RoutedViewHost.DefaultContent>
            <rxui:RoutedViewHost.ViewLocator>
                <!-- See AppViewLocator.cs section below -->
                <app:AppViewLocator />
            </rxui:RoutedViewHost.ViewLocator>
        </rxui:RoutedViewHost>
        <StackPanel Grid.Row="1"
                    Orientation="Horizontal"
                    Margin="15">
            <StackPanel.Styles>
                <Style Selector="StackPanel > :is(Control)">
                    <Setter Property="Margin"
                            Value="2" />
                </Style>
                <Style Selector="StackPanel > TextBlock">
                    <Setter Property="VerticalAlignment"
                            Value="Center" />
                </Style>
            </StackPanel.Styles>
            <Button Content="Go next"
                    Command="{Binding GoNext}" />
            <Button Content="Go back"
                    Command="{Binding GoBack}" />
            <TextBlock Text="{Binding Router.NavigationStack.Count}" />
        </StackPanel>
    </Grid>
</Window>
```

### AppViewLocator.cs

The `AppViewLocator` that we are passing to the `RoutedViewHost` control declared in the MainWindow.xaml markup shown above is responsible for resolving a View based on the type of the ViewModel. The `IScreen.Router` instance of type `RoutingState` determines which ViewModel should be currently shown. See `View Location` for details. The simplest possible `IViewLocator` implementation based on pattern matching might look like this:

```C#
namespace RoutingExample;

public class AppViewLocator : ReactiveUI.IViewLocator
{
  public IViewFor ResolveView<T>(T viewModel, string contract = null) => viewModel switch
  {
    FirstViewModel context => new FirstView { DataContext = context },
    _ => throw new ArgumentOutOfRangeException(nameof(viewModel))
  };
}
```

### MainWindow.axaml.cs

Here is the code-behind for `MainWindow.xaml` declared above.

```C#
namespace RoutingExample;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
    }
}
```

### App.axaml.cs

Make sure you initialize the `DataContext` of your root view in `App.axaml.cs`

```C#
public override void OnFrameworkInitializationCompleted()
{
    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
        desktop.MainWindow = new MainWindow
        {
            DataContext = new MainWindowViewModel(),
        };
    }

    base.OnFrameworkInitializationCompleted();
}
```

Finally, add  `.UseReactiveUI()`   to your `AppBuilder:` in `Program.cs`

```C#

namespace RoutingExample; 

class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UseReactiveUI()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
```

### Run

`dotnet run`

### How this works

The `MainWindowViewModel.cs` is the initial ViewModel that is loaded when the application starts. The `MainWindowViewModel.cs` and instantiates `public RoutingState Router { get; } = new RoutingState();` is bound to `MainWindow.axaml` which is the inital view. In `MainWindow.axaml` the `Router` is bound to `<rxui:RoutedViewHost Grid.Row="0" Router="{Binding Router}">` and that

### How this works

The `MainWindowViewModel.cs` is the initial ViewModel that is loaded when the application starts. This ViewModel contains a `RoutingState` instance, which is responsible for managing navigation between different views in the application:

```csharp
public RoutingState Router { get; } = new RoutingState();
```

This ViewModel is bound to `MainWindow.axaml`, which is the initial view displayed when the application starts.

In `MainWindow.axaml`, the `Router` property of the ViewModel is bound to a `RoutedViewHost` control. This control is responsible for displaying the current view as dictated by the `Router:`

```xml
<rxui:RoutedViewHost Grid.Row="0" Router="{Binding Router}">
```

So, when the application starts, `MainWindowViewModel` becomes the active ViewModel, and `MainWindow.axaml` becomes the active view. The RoutedViewHost control in `MainWindow.axaml` then displays the view associated with the current state of the `Router`.


When a user clicks a button to navigate to `FirstView.axaml`:

1. In your ViewModel (let's say `MainWindowViewModel`), you would have a `ReactiveCommand` that is invoked when the button is clicked. This command calls `Router.Navigate.Execute` with an instance of `FirstViewModel`:



```C#
public class MainWindowViewModel : ReactiveObject, IScreen
{
    public RoutingState Router { get; }
    public ReactiveCommand<Unit, IRoutableViewModel> GoToFirstViewCommand { get; }

    public MainWindowViewModel()
    {
        Router = new RoutingState();
        GoToFirstViewCommand = ReactiveCommand.CreateFromTask(async () => 
            await Router.Navigate.Execute(new FirstViewModel(this)));
    }
}
```

2. In your MainWindow.axaml, you bind the button's Command property to GoToFirstViewCommand:

```XML
<Button Content="Go to First View" Command="{Binding GoToFirstViewCommand}" />
```

3. When the user clicks the button, `GoToFirstViewCommand` is invoked. This command calls `Router.Navigate.Execute(new FirstViewModel(this))`, which changes the `Router`'s internal state to indicate that `FirstViewModel` is now the current ViewModel.

4. The `RoutedViewHost` control in `MainWindow.axaml` is bound to the `Router`:

```XML
<rxui:RoutedViewHost Grid.Row="0" Router="{Binding Router}">
```
5. `RoutedViewHost` listens for changes in the `Router`'s state. When it detects that the current ViewModel has changed to `FirstViewModel`, it looks up the associated view (`FirstView.axaml`), and displays it.

## Bonus Section

### Change The Landing Screen

All you have to do is force a redirect in the constructor in `MainWindowViewModel.cs`.

```C#
public class MainWindowViewModel : ReactiveObject, IScreen
{
  // The Router associated with this Screen.
  // Required by the IScreen interface.
  public RoutingState Router { get; } = new RoutingState();

  public MainWindowViewModel()
  {
    Router.Navigate.Execute(new FirstViewModel(this));
  }
}
```

### How to not use a ViewLocator

If you want to navigate to `FirstView.axaml` without using a `ViewLocator`, you can manually map your view models to their corresponding views. Here's how you can do it:

1. In your `App.xaml`.cs or `App.axaml.cs`, register your views and view models in the `BuildAvaloniaApp` method:

```C#
public static AppBuilder BuildAvaloniaApp()
    => AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .UseReactiveUI()
        .AfterSetup(_ =>
        {
            ViewLocator.Current = new FuncValueConverter(viewModel =>
            {
                switch (viewModel)
                {
                    case MainWindowViewModel _:
                        return new MainWindow();
                    case FirstViewModel _:
                        return new FirstView();
                    default:
                        throw new Exception($"Unknown ViewModel: {viewModel}");
                }
            });
        });
```
2.In your `MainWindowViewModel`, create a command that navigates to `FirstViewModel` when invoked:

```C#
public class MainWindowViewModel : ReactiveObject, IScreen
{
    public RoutingState Router { get; }

    public ICommand GoToFirstViewCommand { get; }

    public MainWindowViewModel()
    {
        Router = new RoutingState();
        GoToFirstViewCommand = ReactiveCommand.Create(() => Router.Navigate.Execute(new FirstViewModel(this)));
    }
}
```
3 In your MainWindow.axaml, bind a button's Command property to GoToFirstViewCommand:

```XML
<Button Content="Go to First View" Command="{Binding GoToFirstViewCommand}" />
```

4. Also in `MainWindow.axaml`, bind a `RoutedViewHost` to the `Router`:

```XML
<rxui:RoutedViewHost Grid.Row="0" Router="{Binding Router}" />
```

Now, when you click the button in `MainWindow.axaml`, the application will navigate to `FirstView.axaml`. This is because the `GoToFirstViewCommand` changes the `Router`'s state to `FirstViewModel`, and the `RoutedViewHost` updates to display the view associated with `FirstViewModel` (which is `FirstView.axaml`, as defined in the `ViewLocator`).
