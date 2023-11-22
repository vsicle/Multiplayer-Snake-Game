namespace SnakeGame;
using Controller;
using Model;
using System.Diagnostics;

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
        GC.Connected += HandleConnected;
        GC.Error += NetworkErrorHandler;


    }


    /// <summary>
    /// Handler for the controller's Connected event
    /// </summary>
    private void HandleConnected()
    {
        

    }

    void OnTapped(object sender, EventArgs args)
    {
        keyboardHack.Focus();
    }

    void OnTextChanged(object sender, TextChangedEventArgs args)
    {
        Entry entry = (Entry)sender;
        String text = entry.Text.ToLower();
        if(text.Equals("w") || text.Equals("a") || text.Equals("s") || text.Equals("d"))
        {
            GC.MoveRequest(text);
        }
        entry.Text = "";
    }

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

        GC.Connect(serverText.Text, nameText.Text);


        //DisplayAlert("Delete this", "Code to start the controller's connecting process goes here", "OK");

        keyboardHack.Focus();
    }

    /// <summary>
    /// Use this method as an event handler for when the controller has updated the world
    /// </summary>
    public void OnFrame()
    {
        Dispatcher.Dispatch(() => graphicsView.Invalidate());
    }

    private void ControlsButton_Clicked(object sender, EventArgs e)
    {
        DisplayAlert("Controls",
                     "W:\t\t Move up\n" +
                     "A:\t\t Move left\n" +
                     "S:\t\t Move down\n" +
                     "D:\t\t Move right\n",
                     "OK");
    }

    private void AboutButton_Clicked(object sender, EventArgs e)
    {
        DisplayAlert("About",
      "SnakeGame solution\nArtwork by Jolie Uk and Alex Smith\nGame design by Daniel Kopta and Travis Martin\n" +
      "Implementation by ...\n" +
        "CS 3500 Fall 2022, University of Utah", "OK");
    }

    private void ContentPage_Focused(object sender, FocusEventArgs e)
    {
        if (!connectButton.IsEnabled)
            keyboardHack.Focus();
    }

    /// <summary>
    /// This is the main drawing method that gets called whenever there is something new to be drawn
    /// </summary>
    /// <param name="newMessages"></param>
    private void WorldUpdate(IEnumerable<string> newMessages)
    {


        graphicsView.Invalidate();

    }

    /// <summary>
    /// Handler for the controller's InitialMessagesArrived event
    /// NO DRAWING HAPPENS HERE
    /// This method is executed to finish game setup/handshake
    /// </summary>
    /// <param name="newMessages"></param>
    private void InitialWorldUpdate(IEnumerable<string> newMessages, World world)
    {

        // save the world
        this.world = world;

        worldPanel.SetWorld(world, GC.playerID);

        

    }
}