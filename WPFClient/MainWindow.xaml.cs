using System;
using System.Windows;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace WPFClient;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    HubConnection connection;
    HubConnection counterConnection;

    public MainWindow()
    {
        InitializeComponent();

        counterConnection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7287/counterhub")
            .WithAutomaticReconnect()
            .Build();

        connection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7287/chathub")
            .WithAutomaticReconnect()
            .Build();

        connection.Reconnecting += (sender) =>
        {
            this.Dispatcher.Invoke(() =>
            {
                var newMessage = "Attempting to reconnect. . .";
                messages.Items.Add(newMessage);
            });

            return Task.CompletedTask;
        };

        connection.Reconnected += (sender) =>
        {
            this.Dispatcher.Invoke(() =>
            {
                var newMessage ="Reconnected to the server!";
                messages.Items.Clear();
                messages.Items.Add(newMessage);
            });

            return Task.CompletedTask;
        };

        connection.Closed += (sender) =>
        {
            this.Dispatcher.Invoke(() =>
            {
                var newMessage = "Connection closed.";
                messages.Items.Add(newMessage);
                openConnection.IsEnabled = true;
                sendMessage.IsEnabled = false;
            });

            return Task.CompletedTask;
        };

        counterConnection.Reconnecting += (sender) =>
        {
            this.Dispatcher.Invoke(() =>
            {
                var newMessage = "Attempting to reconnect counter connection. . .";
                messages.Items.Add(newMessage);
            });

            return Task.CompletedTask;
        };

        counterConnection.Reconnected += (sender) =>
        {
            this.Dispatcher.Invoke(() =>
            {
                var newMessage = "Reconnected to the counter server!";
                messages.Items.Clear();
                messages.Items.Add(newMessage);
            });

            return Task.CompletedTask;
        };

        counterConnection.Closed += (sender) =>
        {
            this.Dispatcher.Invoke(() =>
            {
                var newMessage = "Counter connection closed.";
                messages.Items.Add(newMessage);
                openConnection.IsEnabled = true;
                sendMessage.IsEnabled = false;
            });

            return Task.CompletedTask;
        };
    }

    private async void openConnection_Click(object sender, RoutedEventArgs e)
    {
        connection.On<string, string>("ReceiveMessage", (user, message) =>
        {
            this.Dispatcher.Invoke(() =>
            {
                var newMessage = $"{user}: {message}";
                messages.Items.Add(newMessage);
            });
        });

        try
        {
            await connection.StartAsync();
            messages.Items.Add("Connection started!");
            openConnection.IsEnabled = false;
            sendMessage.IsEnabled = true;
        }
        catch (Exception ex)
        {
            messages.Items.Add(ex.Message);
        }
    }

    private async void sendMessage_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await connection.InvokeAsync("SendMessage",
                "WPF Client", messageInput.Text);
        }
        catch (Exception ex)
        {
            messages.Items.Add(ex.Message);
        }
    }

    private async void openCounterConnection_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await counterConnection.StartAsync();
        }
        catch (Exception ex)
        {
            messages.Items.Add(ex.Message);
        }
    }

    private async void incrementCounter_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await counterConnection.InvokeAsync("AddToTotal", "WPF Cliet", 1);
        }
        catch (Exception ex)
        {
            messages.Items.Add(ex.Message);
        }
    }
}
