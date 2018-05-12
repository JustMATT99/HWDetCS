using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Management;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HWDetCS
{
    /// <summary>
    /// Interaction logic for CPUBase.xaml
    /// </summary>
    public partial class CPUBase : Page,INotifyPropertyChanged
    {

        // lists are better than arrays, fite me!
        public List<string> names = new List<string>();
        public List<string> values = new List<string>();
        public int i = 0;

        // Set up a timer to be enabled later
        public Timer CPUDetRefreshTimer;
        

        public CPUBase()
        {
            // Auto generated stuff, don't touch!
            InitializeComponent();
            // Check if we are running on Windows 10, Microsoft pls make this work again without requiring an app manifest, thanks
            if(Environment.OSVersion.Version.Major == 10)
            {
                // If we are, make the text color the same as the users Accent Color
                OSColor = SystemParameters.WindowGlassBrush;

            }else
            {
                // If we aren't, switch to Dodger Blue
                OSColor = Brushes.DodgerBlue;
            }

            // Actually run all the detection stuff
            CPUDet();

            // Start up the Timer, and get it ready
            CPUDetRefreshTimer = new Timer
            {
                AutoReset = true,
                Interval = 500,
                Enabled = true
            };
            CPUDetRefreshTimer.Elapsed += OnCPUDetEvent;
            
        }

        // This thing does all the work
        public void CPUDet()
        {
            // Run the actual detection for the first time...
            CPUPropDet();
            
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
            CPUManuText.Content = values[27];
            // Get the number of CORES (NOT THREADS!)
            CPUCoreCountText.Content = values[30];
            // Get the Family (Caption)
            CPUFamilyText.Content = values[4];
            // Get the number of Logical Cores (Not physical cores, these are threads!)
            CPULogicalCoresText.Content = values[32];
            // Get the Socket Designation -WIP -TODO: Parse from a table (hopefully pre-made, so I dont have to manually enter every socket ever used) of sockets
            // https://github.com/tianocore/edk2/blob/master/MdePkg/Include/IndustryStandard/SmBios.h line 753-810 looks helpful
            CPUSocketDesignationText.Content = values[52];


        }
        
        public void OnCPUDetEvent(Object obj, ElapsedEventArgs args)
        {
            CPUPropDet();
            
            // Get the current clock speed safely
            CPUSpeed = values[10] + "MHz";


            // Get the current Voltage safely
            try
            {
                CPUVolts = (Convert.ToDouble(values[11]) / 10).ToString() + " Volts";
            } catch (FormatException e) {
                CPUVolts = "Voltage Error";
            }
        }


        // This makes everything able to update without throwing a fit
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
            
        }

        // This handles the CPU Speed...
        string speed;
        public string CPUSpeed
        {
            get
            {
                return speed;
            }
            set
            {
                speed = value;
                NotifyPropertyChanged(nameof(CPUSpeed));
            }
        }

        // and this handles Voltage
        string volts;
        public string CPUVolts
        {
            get
            {
                return volts;
            }
            set
            {
                volts = value;
                NotifyPropertyChanged(nameof(CPUVolts));
            }
        }

        // This handles the Text Color
        Brush color;
        public Brush OSColor
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
                NotifyPropertyChanged(nameof(OSColor));
            }
        }

        // Its technically not a loop....
        public void CPUPropDet()
        {
            // Get the CPU Management class, this makes it the CPU we get info off of rather than nothing, because if it wasnt set to the CPU, it would error and break and cry a lot... dont change it.
            ManagementClass CPUClass = new ManagementClass("Win32_Processor");
            CPUClass.Options.UseAmendedQualifiers = true;

            // Clear the lists in case this is the second detection, not doing this leads to no update on screen as the new values are added to a full list onto indexes I'm not accounting for
            names.Clear();
            values.Clear();


            // Set up a data collection to get the data off of, this and the next thing SHOULD NEVER BE IN A LOOP! IT WILL BREAK YOUR CPU LIKE A FUCKING BALLOON!
            PropertyDataCollection dataCollection = CPUClass.Properties;

            // Get the instance of the class, for some reason this is required to work, dont touch AND DONT PUT IT IN A LOOP WHY CANT YOU LISTEN!?
            ManagementObjectCollection instanceCollection = CPUClass.GetInstances();

            // This is a loop, its very fragile, dont touch it, it gets the list of data we are collecting
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
                    else if (instance.Properties[property.Name.ToString()].Value.ToString() == "")
                    {
                        // differentiate between actually null values and just blank strings
                        values.Add("BLANK");
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
            // Reset the counter!
            i = 0;
            
        }
    }

    
}
