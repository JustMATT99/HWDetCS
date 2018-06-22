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
    /// Interaction logic for GPUBase.xaml
    /// </summary>
    public partial class GPUBase : Page,INotifyPropertyChanged
    {

        public List<string> names = new List<string>();
        public List<string> values = new List<string>();
        public int i = 0;

        // Set up a timer to be used later
        public Timer GPUDetRefreshTimer;



        public GPUBase()
        {
            InitializeComponent();

            // Check if we are running on Windows 10, Microsoft pls make this work again without requiring an app manifest, thanks
            if (Environment.OSVersion.Version.Major == 10)
            {
                // If we are, make the text color the same as the users Accent Color
                TextColor = SystemParameters.WindowGlassBrush;

            }
            else
            {
                // If we aren't, switch to Dodger Blue
                TextColor = Brushes.DodgerBlue;
            }

            // Run the detection stuff for the first time
            GPUDet();

            // Set up the timer
            GPUDetRefreshTimer = new Timer()
            {
                AutoReset = true,
                Interval = 500,
                Enabled = true
            };
            GPUDetRefreshTimer.Elapsed += OnGPUDetEvent;
        }

        public void GPUDet()
        {
            // First time detection
            GPUPropDet();

            // Debug Stuff
            for (int x = 0; x < names.Count; x++)
            {
                Console.WriteLine(x.ToString());
                Console.WriteLine(names[x]);
                Console.WriteLine(values[x]);
            }

            GPUNameText.Content = values[39];

            // Do some math to convert bytes to GB
            double OrigVRAMSize = Convert.ToDouble(values[3]) / 1024 / 1024 / 1024;

            GPUVRAMSizeText.Content = (Math.Round(OrigVRAMSize, 2).ToString() + "GB");

            GPUSupportedColorsText.Content = Convert.ToInt64(values[13]).ToString("N0") + " Colors";
        }

        public void OnGPUDetEvent(Object obj, ElapsedEventArgs args)
        {
            GPUPropDet();

            // Do a bit of formatting
            CurRes = String.Format("{0} x {1} @ {2}Hz", values[12], values[18], values[16]);

            CurBPP = values[11] + " Bits Per Pixel";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        public void GPUPropDet()
        {
            // Get the GPU Management class
            ManagementClass GPUClass = new ManagementClass("Win32_VideoController");
            GPUClass.Options.UseAmendedQualifiers = true;

            // Clear the lists
            names.Clear();
            values.Clear();

            // Set up data collection
            PropertyDataCollection dataCollection = GPUClass.Properties;

            // Get instance collection
            ManagementObjectCollection instanceCollection = GPUClass.GetInstances();

            foreach(PropertyData property in dataCollection)
            {
                // Add the names to the name list
                names.Add(property.Name);

                foreach(ManagementObject instance in instanceCollection)
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

            }

        }

        string curResolution;
        public string CurRes
        {
            get
            {
                return curResolution;
            }
            set
            {
                curResolution = value;
                NotifyPropertyChanged(nameof(CurRes));
            }
        }

        string curBPP;
        public string CurBPP
        {
            get
            {
                return curBPP;
            }
            set
            {
                curBPP = value;
                NotifyPropertyChanged(nameof(CurBPP));
            }
        }

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

    }

}
