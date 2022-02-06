using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MeasurementUI.Controls.ViewModels
{
    public class ConnectionControlStubViewModel: ObservableRecipient
    {

        public int TestNum { get; set; }

        public ConnectionControlStubViewModel()
        {
            TestNum = 0;
        }
    }
}
