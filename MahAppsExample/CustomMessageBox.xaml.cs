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

namespace HS5
{
    /// <summary>
    /// Lógica de interacción para CustomMessageBox.xaml
    /// </summary>
    public partial class CustomMessageBox : Window
    {
        // Propiedad para el mensaje
        public string Message { get; set; }

        // Constructor
        public CustomMessageBox()
        {
            InitializeComponent();
            DataContext = this;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        // Evento Click del botón OK
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            // Cierra la ventana personalizada
            this.Close();
        }
    }
}
