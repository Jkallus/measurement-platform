using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MeasurementApp.Controls
{
    public sealed partial class ComponentState : UserControl
    {
        public ComponentState()
        {
            this.InitializeComponent();
        }





        public Brush StatusColor
        {
            get { return (Brush)GetValue(StatusColorProperty); }
            set { SetValue(StatusColorProperty, value); }
        }

        public static readonly DependencyProperty StatusColorProperty = DependencyProperty.Register("StatusColor", typeof(Brush), typeof(ComponentState), new PropertyMetadata(null));

        public string ComponentStatus
        {
            get { return (string)GetValue(ComponentStatusProperty); }
            set { SetValue(ComponentStatusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ComponentStatus.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ComponentStatusProperty =
            DependencyProperty.Register("ComponentStatus", typeof(string), typeof(ComponentState), new PropertyMetadata(null));




        public string ComponentName
        {
            get { return (string)GetValue(ComponentNameProperty); }
            set { SetValue(ComponentNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ComponentName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ComponentNameProperty =
            DependencyProperty.Register("ComponentName", typeof(string), typeof(ComponentState), new PropertyMetadata(null));
    }
}
