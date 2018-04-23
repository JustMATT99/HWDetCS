using System;
using System.Collections.Generic;
using System.Management.Instrumentation;
using System.Management;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HWDetCS
{
    /// <summary>
    /// Interaction logic for CPUBase.xaml
    /// </summary>
    public partial class CPUBase : Page
    {


        public List<string> names = new List<string>();
        public List<string> values = new List<string>();
        public int i = 0;

        public CPUBase()
        {
            // Auto generated stuff, don't touch!
            InitializeComponent();

            // Actually run all the detection stuff
            CPUDet();
        }

        // This thing does all the work
        public void CPUDet()
        {

            ManagementClass CPUClass = new ManagementClass("Win32_Processor");
            CPUClass.Options.UseAmendedQualifiers = true;

            PropertyDataCollection dataCollection = CPUClass.Properties;

            ManagementObjectCollection instanceCollection = CPUClass.GetInstances();

            foreach (PropertyData property in dataCollection)
            {
                names.Add(property.Name);
                foreach (ManagementObject instance in instanceCollection)
                {
                    if (instance.Properties[property.Name.ToString()].Value == null)
                    {
                        values.Add("null");
                    }
                    else
                    {
                        values.Add(instance.Properties[property.Name.ToString()].Value.ToString());
                    }
                }
                i++;
                
            }

            
            

            for (int x = 0; x < names.Count - 1; x++)
            {
                Console.WriteLine(x.ToString());
                Console.WriteLine(names[x]);
                Console.WriteLine(values[x]);
            }

            CPUNameText.Content = values[29];
        }
    }
}
