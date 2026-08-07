namespace SokobanGame;

public partial class MainPage : ContentPage
{
    // Keeps track of how many times the button was clicked
    int count = 0;

    public MainPage()
    {
        InitializeComponent();
    }

    // Runs when the counter button is pressed
    private void OnCounterClicked(object? sender, EventArgs e)
    {
        count++;

        // Changes the text depending on how many times it was clicked
        if (count == 1)
            CounterBtn.Text = $"Clicked {count} time";
        else
            CounterBtn.Text = $"Clicked {count} times";

        // Reads the new button text out loud for accessibility
        SemanticScreenReader.Announce(CounterBtn.Text);
    }
}