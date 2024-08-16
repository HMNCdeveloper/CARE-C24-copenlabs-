using System;

using System.Windows;

using System.Windows.Media.Imaging;

using System.IO;
using System.IO.Ports;
using System.Drawing;
using System.Drawing.Imaging;
using HS5.Properties;

using System.Globalization;
using System.Threading;
using System.Windows.Media;

namespace HS5
{
    /// <summary>
    /// Interaction logic for Machine.xaml
    /// </summary>
    public partial class Machine : Window
    {
        string[] ports; //COMs (Puertos)

        public Machine()
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Settings.Default.Lenguaje);
            InitializeComponent();
            //Obtiene todos los puertos COM
            ports = SerialPort.GetPortNames();

            //Muestra los puertos
            for (int i = 0; i <= ports.Length - 1; i++)
            {
                cmbPorts.Items.Add(ports[i].ToString());
            }
         

         
        }

        //this function is used to check if there are ports to start the Mainwindow
        private void button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmbPorts.SelectedIndex != -1)
                {
                    MahAppsExample.MainWindow vent = new MahAppsExample.MainWindow(cmbPorts.SelectedItem.ToString());
                    vent.Show();
                    Application.Current.Windows[0].Close();//this setences is used to close the recent window
                }
            }
            catch (Exception err){
                MessageBox.Show(err.ToString());
            }
        }
    }
}
