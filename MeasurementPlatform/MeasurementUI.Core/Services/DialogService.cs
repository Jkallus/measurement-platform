using MeasurementUI.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using MeasurementUI;

namespace MeasurementUI.Core.Services
{
    public class DialogService : IDialogService
    {
        static Dictionary<Type, Type> mappings = new Dictionary<Type, Type>();

        public static void RegisterDialog<TView, TViewModel>()
        {
            mappings.Add(typeof(TViewModel), typeof(TView));
        }

        public void ShowDialog(string name, Action<string> callback)
        {
            var type = Type.GetType($"MeasurementUI.Controls.Views.{name}, MeasurementUI.Controls");
            ShowDialogImpl(type, callback, null);
        }

        private static void ShowDialogImpl(Type type, Action<string> callback, Type vmType)
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
            
            /*
            if(vmType != null)
            {
                var vm = Activator.CreateInstance(vmType);
                //var vm = Application.Current.Services.GetService<type>();
                (content as FrameworkElement).DataContext = vm;
            }
            */
            
            dialog.Content = content;
            dialog.ShowDialog();
        }

        public void ShowDialog<TViewModel>(Action<string> callback)
        {
            var type = mappings[typeof(TViewModel)];
            ShowDialogImpl(type, callback, typeof(TViewModel));
        }
    }
}
