using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace HWDetCS
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page,INotifyPropertyChanged { 
        public MainPage()
        {
            InitializeComponent();

            // Doing the same Color stuff as in CPUBase.xaml.cs
            if (Environment.OSVersion.Version.Major == 10)
            {
                OSColor = SystemParameters.WindowGlassBrush;
            } else
            {
                OSColor = Brushes.DodgerBlue;
            }
        }

        private void MainPageButton1_Click(object sender, RoutedEventArgs e)
        {
            CPUBase cpuBase = new CPUBase();
            NavigationService.Navigate(cpuBase);
        }

        private void MainPageButton2_Click(object sender, RoutedEventArgs e)
        {
            GPUBase gpuBase = new GPUBase();
            NavigationService.Navigate(gpuBase);
        }

        // This makes everything able to update without throwing a fit
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

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
    }
}
