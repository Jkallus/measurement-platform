using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MeasurementUI.Controls.Views
{
    /// <summary>
    /// Interaction logic for ComponentState.xaml
    /// </summary>
    public partial class ComponentState : UserControl
    {
        public ComponentState()
        {
            InitializeComponent();
        }



        public string ComponentStatus
        {
            get { return (string)GetValue(ComponentStatusProperty); }
            set { SetValue(ComponentStatusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ComponentStatus.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ComponentStatusProperty =
            DependencyProperty.Register("ComponentStatus", typeof(string), typeof(ComponentState));




        public string ComponentName
        {
            get { return (string)GetValue(ComponentNameProperty); }
            set { SetValue(ComponentNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ComponentName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ComponentNameProperty =
            DependencyProperty.Register("ComponentName", typeof(string), typeof(ComponentState));






    }
}
