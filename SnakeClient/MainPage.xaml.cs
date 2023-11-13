﻿namespace SnakeGame;
using Controller;
using System.Diagnostics;

public partial class MainPage : ContentPage
{
    GameController GC = new GameController();

    public MainPage()
    {

        InitializeComponent();
        graphicsView.Invalidate();
        GC.MessagesArrived += WorldUpdate;
        GC.Connected += HandleConnected;
        
        
    }


    /// <summary>
    /// Handler for the controller's Connected event
    /// </summary>
    private void HandleConnected()
    {
        /*
        // Just print a message saying we connected
        Dispatcher.Dispatch(() => messages.Text = "Connected\n" + messages.Text);
        */
        Debug.WriteLine("Connected");
        
    }

    void OnTapped(object sender, EventArgs args)
    {
        keyboardHack.Focus();
    }

    void OnTextChanged(object sender, TextChangedEventArgs args)
    {
        Entry entry = (Entry)sender;
        String text = entry.Text.ToLower();
        if (text == "w")
        {
            // Move up
        }
        else if (text == "a")
        {
            // Move left
        }
        else if (text == "s")
        {
            // Move down
        }
        else if (text == "d")
        {
            // Move right
        }
        entry.Text = "";
    }

    private void NetworkErrorHandler()
    {
        DisplayAlert("Error", "Disconnected from server", "OK");
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
    /// Handler for the controller's MessagesArrived event
    /// </summary>
    /// <param name="newMessages"></param>
    private void WorldUpdate(IEnumerable<string> newMessages)
    {
        // display each new message in the text area
        foreach (string p in newMessages)
        {
            Debug.WriteLine(p);
            /*
            Dispatcher.Dispatch(
              () => messages.Text = p + messages.Text);
            */
        }
    }
}