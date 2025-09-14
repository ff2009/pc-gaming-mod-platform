using Avalonia.Controls;
using Avalonia.Controls.Templates;
using PCGamingModApp.ViewModels;
using System;

namespace PCGamingModApp;

public class ViewLocator : IDataTemplate
{

    public Control? Build(object? data)
    {
        if (data is null)
            return null;

        var name = data.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
        var type = Type.GetType(name);

        if (type is null)
            return new TextBlock { Text = "Not Found: " + name };

        var control = Activator.CreateInstance(type) as Control;
        if (control is null)
            return null;

        control.DataContext = data;
        return control;
    }

    public bool Match(object? data) => data is ViewModelBase;
}
