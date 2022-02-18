using MeasurementUI.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace MeasurementUI.Core.Services
{
    public class DialogService : IDialogService
    {
        private readonly IServiceProvider serviceProvider;

        public DialogService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        
        // mapping of Views and ViewModels, key is ViewModel and value is the associated View
        static Dictionary<Type, Type> mappings = new Dictionary<Type, Type>();

        // function to add a pair to the mapping
        public static void RegisterDialog<TView, TViewModel>()
        {
            mappings.Add(typeof(TViewModel), typeof(TView)); // key is viewmodel, value is view
        }

        
        public void ShowDialog(string name, Action<string> callback) // name is name of view
        {
            var type = Type.GetType($"MeasurementUI.Controls.Views.{name}, MeasurementUI.Controls");
            ShowDialogImpl(type, callback, null); // show dialog with no viewmodel
        }

        public void ShowDialog<TViewModel>(Action<string> callback) // main public API, templated with the type of ViewModel to use as key
        {
            var type = mappings[typeof(TViewModel)]; // get the view out of the mapping
            ShowDialogImpl(type, callback, typeof(TViewModel)); // call the implementation providing the types of view and viewmodel
        }


        private void ShowDialogImpl(Type type, Action<string> callback, Type vmType) // type is view type
        {
            var dialog = new DialogWindow();

            EventHandler closeEventHandler = null;
            closeEventHandler = (sender, e) =>
            {
                callback(dialog.DialogResult.ToString());
                dialog.Closed -= closeEventHandler;
            };
            dialog.Closed += closeEventHandler;

            var content = Activator.CreateInstance(type);
            
            if(vmType != null)
            {
                var vm = serviceProvider.GetService(vmType);
                (content as FrameworkElement).DataContext = vm;
            }
            
            dialog.Content = content;
            dialog.ShowDialog();
        }


    }
}