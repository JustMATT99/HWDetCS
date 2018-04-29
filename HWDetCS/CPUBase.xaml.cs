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

        // lists are better than arrays, fite me!
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
            // Get the CPU Management class, this makes it the CPU we get info off of rather than nothing, because if it wasnt set to the CPU, it would error and break and cry a lot... dont change it.
            ManagementClass CPUClass = new ManagementClass("Win32_Processor");
            CPUClass.Options.UseAmendedQualifiers = true;

            // Set up a data collection to get the data off of, this and the next thing SHOULD NEVER BE IN A LOOP! IT WILL BREAK YOUR CPU LIKE A FUCKING BALLOON!
            PropertyDataCollection dataCollection = CPUClass.Properties;

            // get the instance of the class, for some reason this is required to work, dont touch AND DONT PUT IT IN A LOOP WHY CANT YOU LISTEN!?
            ManagementObjectCollection instanceCollection = CPUClass.GetInstances();

            // this is a loop, its very fragile, dont touch it, it gets the list of data we are collecting
            foreach (PropertyData property in dataCollection)
            {

                // adds the names into one nice readable-ish list!
                names.Add(property.Name);

                // loop through all the instances and grabs the actual data off of it
                foreach (ManagementObject instance in instanceCollection)
                {
                    // makes sure we dont get null reference errors, I HATE THOSE SO MUCH! I KNOW ITS NULL JUST SHUT UP!
                    if (instance.Properties[property.Name.ToString()].Value == null)
                    {
                        // if its null, dont add the actual property data, INSTEAD, add a string that says null so we know not to fuck with it
                        values.Add("null");
                    }
                    else
                    {
                        // otherwise, go right ahead
                        values.Add(instance.Properties[property.Name.ToString()].Value.ToString());
                    }
                }
                // counting....
                i++;
                
            }

            
            
            // Debug stuff, dont release uncommented!
            // TODO: COMMENT THIS OUT!
            for (int x = 0; x < names.Count - 1; x++)
            {
                Console.WriteLine(x.ToString());
                Console.WriteLine(names[x]);
                Console.WriteLine(values[x]);
            }

            // Get the name
            CPUNameText.Content = values[29];
            // Get the manufacturer
            CPUManuText.Content = values[28];
            // Get the current clock speed  -TODO: put this in a loop to have it update
            CPUClockSpeedText.Content = values[11];
            // Get the number of CORES (NOT THREADS!)
            CPUCoreCountText.Content = values[30];
        }
    }
}
