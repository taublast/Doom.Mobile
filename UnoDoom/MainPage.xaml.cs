namespace UnoDoom;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
        
        // Focus the game control for keyboard input
        this.Loaded += (s, e) =>
        {
            DoomGame.Focus(FocusState.Programmatic);
        };
    }
}
