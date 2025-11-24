using Avalonia.Controls;
using Avalonia.Interactivity;
using PCGamingModApp.Core.ViewModels;
using System;

namespace PCGamingModApp.Views;

public partial class DownloadsPageView : UserControl
{
    public DownloadsPageView()
    {
        InitializeComponent();
    }

    private async void InputElement_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (this.DataContext is DownloadsPageViewModel viewModel)
            {
                await viewModel.GetMetadata();
            }
        }
        catch (Exception ex)
        {
            // TODO handle exception
        }
    }
}