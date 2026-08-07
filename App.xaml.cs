using Microsoft.Extensions.DependencyInjection;

namespace SokobanGame;

public partial class App : Application
{
	public App()
	{
		// This sets up the application when it starts.
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		// Creates the main window and loads the AppShell navigation.
		return new Window(new AppShell());
	}
}