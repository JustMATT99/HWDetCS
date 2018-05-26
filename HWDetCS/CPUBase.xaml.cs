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

        // Who needs proper good code supported by a good API when you can have an array you have to manually update everytime Intel or AMD release a new Socket?
        public List<string> SocketList = new List<string>();
        

        public CPUBase()
        {
            // Auto generated stuff, don't touch!
            InitializeComponent();

            SocketListPopulate();

            // Check if we are running on Windows 10, Microsoft pls make this work again without requiring an app manifest, thanks
            if(Environment.OSVersion.Version.Major == 10)
            {
                // If we are, make the text color the same as the users Accent Color
                TextColor = SystemParameters.WindowGlassBrush;

            }else
            {
                // If we aren't, switch to Dodger Blue
                TextColor = Brushes.DodgerBlue;
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
            
            // Check if the CPU is 64bit or 32 (or something else?)
            // Using the AddressWidth property, might change to something else that does the same thing later
            if (Convert.ToUInt16(values[0]) == 64)
            {
                // If it is 64bit, make the text true, and change its color to forest green
                _64BitCheckText.Content = "True";
                _64BitCheckColor = Brushes.DarkGreen;
            }else
            {
                // If it is NOT 64bit (32bit or similar), make the text false, and change its color to crimson
                _64BitCheckText.Content = "False";
                _64BitCheckColor = Brushes.Crimson;
            }

            // Get the name
            CPUNameText.Content = values[29];
            
            // Get the manufacturer
            CPUManuText.Content = values[27];

            // Get the Status
            CPUStatusText.Content = values[45];

            // Get the max clock speed
            CPUMaxClockSpeedText.Content = values[28] + "MHz"; // NOTE: Doesn't seem to count boost clock speeds, may have to change this later
            
            // Get the number of CORES (NOT THREADS!)
            CPUCoreCountText.Content = values[30];
            
            // Get the number of Logical Cores (Not physical cores, these are threads!)
            CPULCoreCountText.Content = values[32];

            // Get the size of the L2 Cache
            CPUL2CacheSizeText.Content = values[20] + "kB";

            // Get the size of the L3 Cache
            CPUL3CacheSizeText.Content = values[22] + "kB";
            
            // Get the Family (Caption)
            CPUFamilyText.Content = values[4];
            
            
            
            // Get the Socket Designation -WIP -TODO: Parse from a table (wasnt pre-made, I entered it all myself...)
            CPUSocketDesignationText.Content = SocketList[Convert.ToInt32(values[52]) - 1]; // Take 1 off cuz the list starts at 0, and values[52] starts at 1


        }
        
        public void OnCPUDetEvent(Object obj, ElapsedEventArgs args)
        {
            CPUPropDet();

            // Get the current load percentage
            CPULoad = values[26] + "%";

            // Get the current clock speed safely
            CPUSpeed = values[10] + "MHz";

            // Get the current base clock
            CPUBCLK = values[17] + "MHz";

            // Get the current L2 Cache speed
            // Currently is only giving either null or False on my system, needs to be tested to see what is causing this, my guess would be a AMD architecture bug, or CIM/WMI using intel only functions for L2 Cache speed
            if (values[21] == "null")
            {
                CPUL2Speed = "null";
            } else
            {
                CPUL2Speed = values[21] + "MHz";
            }

            // Get the current L3 Cache speed
            CPUL3Speed = values[23] + "MHz";

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

        // This was auto generated... its too good not to leave in XD
        // But for real.... this populates the Socket List, which is required because Im bad at using WMI....
        /// <summary>
        /// Sockets the list populate.
        /// </summary>
        /// <autogeneratedoc />
        /// TODO Edit XML Comment Template for SocketListPopulate
        void SocketListPopulate()
        {
            // Clear it in case something was in it for some unknown reason... dont want the indexes to be wrong.
            SocketList.Clear();

            // GIANT WALL OF TEXT
            // I'm guessing this is what would show up on something that isnt supported yet?
            SocketList.Add("Other");
            // Same thing as above maybe?
            SocketList.Add("Unknown");
            // Proprietary stuff?
            SocketList.Add("Daughter Board");
            // Why? Please... tell me why you are using this?
            SocketList.Add("ZIF Socket");
            // What even is this?
            SocketList.Add("Replacement/Piggy Back");
            // Finally something that makes se-wait... WHAT!?
            SocketList.Add("None");
            // Almost there....
            SocketList.Add("LIF Socket");
            // Pretty old...
            SocketList.Add("Slot 1");
            // Wake me up when we get to Lynnfield and Bloomfield stuff!
            SocketList.Add("Slot 2");
            SocketList.Add("370 Pin Socket");
            SocketList.Add("Slot A"); // Apparently CPUs can be IKEA furniture...
            SocketList.Add("Slot M");
            SocketList.Add("Socket 423");
            SocketList.Add("Socket A (Socket 462)");
            SocketList.Add("Socket 478");
            SocketList.Add("Socket 754");
            SocketList.Add("Socket 940");
            SocketList.Add("Socket 939");
            SocketList.Add("Socket mPGA604");
            SocketList.Add("Socket LGA771");
            SocketList.Add("Socket LGA775");
            SocketList.Add("Socket S1");
            SocketList.Add("Socket AM2");
            SocketList.Add("Socket F (1207)");
            // Oh cool, something made in the last 10 years!
            SocketList.Add("Socket LGA1366");
            SocketList.Add("Socket G34");
            SocketList.Add("Socket AM3");
            SocketList.Add("Socket C32");
            SocketList.Add("Socket LGA1156");
            SocketList.Add("Socket LGA1567");
            SocketList.Add("Socket PGA988A");
            SocketList.Add("Socket BGA1288");
            SocketList.Add("Socket rPGA988B");
            SocketList.Add("Socket BGA1023");
            SocketList.Add("Socket BGA1224");
            SocketList.Add("Socket LGA1155");
            SocketList.Add("Socket LGA1356");
            SocketList.Add("Socket LGA2011");
            SocketList.Add("Socket FS1");
            SocketList.Add("Socket FS2");
            SocketList.Add("Socket FM1");
            SocketList.Add("Socket FM2 or FM2+"); // My CPU uses FM2+ but showed up as FM2...
            SocketList.Add("Socket LGA2011-3");
            SocketList.Add("Socket LGA1356-3");
            SocketList.Add("Socket LGA1150");
            SocketList.Add("Socket BGA1168");
            SocketList.Add("Socket BGA1234");
            SocketList.Add("Socket BGA1364");
            SocketList.Add("Socket AM4");
            SocketList.Add("Socket LGA1151");
            SocketList.Add("Socket BGA1356");
            SocketList.Add("Socket BGA1440");
            SocketList.Add("Socket BGA1515");
            // Thats a lot of pins... probably since its for xeons... Knights Landing Xeons to be exact...
            SocketList.Add("Socket LGA3647-1");
            SocketList.Add("Socket SP3 (AMD Epyc)");
            SocketList.Add("Socket SP3r2 (TR4)");
            SocketList.Add("Socket LGA2066");
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

        // This handles Load Percentage
        string load;
        public string CPULoad
        {
            get
            {
                return load;
            }
            set
            {
                load = value;
                NotifyPropertyChanged(nameof(CPULoad));
            }
        }

        // This handles the Base Clock (Most BIOS' call it 'BCLK')
        string bClock;
        public string CPUBCLK
        {
            get
            {
                return bClock;
            }
            set
            {
                bClock = value;
                NotifyPropertyChanged(nameof(CPUBCLK));
            }
        }

        // This handles the L2 Cache speed
        string l2Speed;
        public string CPUL2Speed
        {
            get
            {
                return l2Speed;
            }
            set
            {
                l2Speed = value;
                NotifyPropertyChanged(nameof(CPUL2Speed));
            }
        }

        // This handles the L3 Cache speed
        string l3Speed;
        public string CPUL3Speed
        {
            get
            {
                return l3Speed;
            }
            set
            {
                l3Speed = value;
                NotifyPropertyChanged(nameof(CPUL3Speed));
            }
        }

        // This handles the Text Color
        Brush color;
        public Brush TextColor
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
                NotifyPropertyChanged(nameof(TextColor));
            }
        }

        // This handles the Text Color of the 64bit check boolean
        Brush otherColor;
        public Brush _64BitCheckColor
        {
            get
            {
                return otherColor;
            }
            set
            {
                otherColor = value;
                NotifyPropertyChanged(nameof(_64BitCheckColor));
            }
        }

    }

    
}
