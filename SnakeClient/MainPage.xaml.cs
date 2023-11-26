namespace SnakeGame;
using Controller;
using Model;
/// <summary>
/// Class for handling GUI events.
/// </summary>
public partial class MainPage : ContentPage
{
    GameController GC = new GameController();
    World world;

    public MainPage()
    {
        InitializeComponent();
        graphicsView.Invalidate();

        GC.InitialMessagesArrived += InitialWorldUpdate;
        GC.MessagesArrived += WorldUpdate;

        GC.Error += NetworkErrorHandler;
    }

    /// <summary>
    /// Method for ScrollView event.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>

    void OnTapped(object sender, EventArgs args)
    {
        keyboardHack.Focus();
    }

    /// <summary>
    /// Method to handle user keyboard commands.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>

    void OnTextChanged(object sender, TextChangedEventArgs args)
    {
        Entry entry = (Entry)sender;
        String text = entry.Text.ToLower();

        // if input is acceptable send the request
        if (text.Equals("w") || text.Equals("a") || text.Equals("s") || text.Equals("d"))
        {
            GC.MoveRequest(text);
        }
        // clear text box to be ready for next input
        entry.Text = "";
    }

    /// <summary>
    /// Quit application in event of network error.
    /// </summary>
    /// <param name="error"></param>

    private void NetworkErrorHandler(string error)
    {
        Application.Current.Quit();
    }


    /// <summary>
    /// Event handler for the connect button
    /// We will put the connection attempt interface here in the view.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void ConnectClick(object sender, EventArgs args)
    {
        if (serverText.Text == "")
        {
            DisplayAlert("Error", "Please enter a server address", "OK");
            return;
        }
        if (nameText.Text == "")
        {
            DisplayAlert("Error", "Please enter a name", "OK");
            return;
        }
        if (nameText.Text.Length > 16)
        {
            DisplayAlert("Error", "Name must be less than 16 characters", "OK");
            return;
        }

        // Begin Client connection attempt to Server.

        GC.Connect(serverText.Text, nameText.Text);

        keyboardHack.Focus();
    }

    /// <summary>
    /// Client draws the world.
    /// </summary>
    public void OnFrame()
    {
        Dispatcher.Dispatch(() => graphicsView.Invalidate());
    }

    /// <summary>
    /// Displays user keyboard input.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>

    private void ControlsButton_Clicked(object sender, EventArgs e)
    {
        DisplayAlert("Controls",
                     "W:\t\t Move up\n" +
                     "A:\t\t Move left\n" +
                     "S:\t\t Move down\n" +
                     "D:\t\t Move right\n",
                     "OK");
    }

    /// <summary>
    /// Displays developer information.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>

    private void AboutButton_Clicked(object sender, EventArgs e)
    {
        DisplayAlert("About",
      "SnakeGame solution\nArtwork by Jolie Uk and Alex Smith\nGame design by Daniel Kopta and Travis Martin\n" +
      "Implementation by Eric Nee and Vasko Vassilev\n" +
        "CS 3500 Fall 2023, University of Utah", "OK");
    }

    /// <summary>
    /// Method for ContentPage.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>

    private void ContentPage_Focused(object sender, FocusEventArgs e)
    {
        if (!connectButton.IsEnabled)
            keyboardHack.Focus();
    }

    /// <summary>
    /// This is the main drawing method that gets called whenever there is something new to be drawn.
    /// Registered to GC Messages Arrived event.
    /// </summary>
    /// <param name="newMessages">Messages from server.</param>
    private void WorldUpdate(IEnumerable<string> newMessages)
    {

        graphicsView.Invalidate();

    }

    /// <summary>
    /// Handler for the controller's InitialMessagesArrived event
    /// This method is executed to finish game setup/handshake
    /// </summary>
    /// <param name="newMessages">Initial handshake messages from Server.</param>
    private void InitialWorldUpdate(IEnumerable<string> newMessages, World world)
    {

        // save the world
        this.world = world;

        worldPanel.SetWorld(world, GC.playerID);



    }
}