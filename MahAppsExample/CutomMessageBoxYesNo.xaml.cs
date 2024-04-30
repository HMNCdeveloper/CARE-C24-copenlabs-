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
    public partial class CustomMessageBoxYesNo : Window
    {
        public string Message { get; set; }

        public CustomMessageBoxYesNo(string message)
        {
            InitializeComponent();
            Message = message;
            DataContext = this;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void Aceptar_Click(object sender, RoutedEventArgs e)
        {
            // Lógica para el botón "Aceptar"
            DialogResult = true;
            Close();
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            // Lógica para el botón "Cancelar"
            DialogResult = false;
            Close();
        }
    }
}