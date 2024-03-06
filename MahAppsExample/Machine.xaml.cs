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
using System.Windows.Shapes;
using System.IO;
using System.IO.Ports;
using System.Drawing;
using System.Drawing.Imaging;
using HS5.Properties;
using System.Globalization;
using System.Threading;

namespace HS5
{
    /// <summary>
    /// Interaction logic for Machine.xaml
    /// </summary>
    public partial class Machine : Window
    {
        string[] ports; //COMs (Puertos)
        //SerialPort port; //Puerto

        public Machine()
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Settings.Default.Lenguage);
            InitializeComponent();
            //Obtiene todos los puertos COM
            ports = SerialPort.GetPortNames();

            //Muestra los puertos
            for (int i = 0; i <= ports.Length - 1; i++)
            {
                cmbPorts.Items.Add(ports[i].ToString());
            }

            BitmapImage img_logo = ToBitmapImage(HS5.Properties.Resources.HS5);
            imagelogo.Source = img_logo;
        }

        public static BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmbPorts.SelectedIndex != -1)
                {
                   MahAppsExample.MainWindow vent = new MahAppsExample.MainWindow(cmbPorts.SelectedItem.ToString());
                    vent.Show();
                    Application.Current.Windows[0].Close();
                    // this.Hide();
                }
            }
            catch (Exception)
            {

            }
        }

      

        private void CmbPorts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
