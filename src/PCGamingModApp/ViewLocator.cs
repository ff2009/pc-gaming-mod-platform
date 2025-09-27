using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using PCGamingModApp.Core.ViewModels;

namespace PCGamingModApp;

public class ViewLocator : IDataTemplate
{
    
    public Control? Build(object? data)
    {
        if (data is null)
            return null;

        var viewModelType = data.GetType();
        var viewModelNamespace = viewModelType.Namespace;
        var viewModelName = viewModelType.Name;

        // Replace "PCGamingApp.Core.ViewModels" with "PCGamingApp.Views"
        var viewNamespace = viewModelNamespace?.Replace("PCGamingModApp.Core.ViewModels", "PCGamingModApp.Views");
        var viewName = viewModelName.Replace("ViewModel", "View");
        var viewTypeName = $"{viewNamespace}.{viewName}";

        var type = Type.GetType(viewTypeName);
        if (type is null)
            return new TextBlock { Text = $"Not Found: {viewTypeName}" };

        if (Activator.CreateInstance(type) is Control control)
        {
            control.DataContext = data;
            return control;
        }

        return null;
    }

    public bool Match(object? data) => data is ViewModelBase;
}
