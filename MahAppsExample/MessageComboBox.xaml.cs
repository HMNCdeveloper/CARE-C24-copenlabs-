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
    /// Interaction logic for MessageComboBox.xaml
    /// </summary>
    public partial class MessageComboBox : Window
    {
        public string SelectedOption { get; private set; }
        public MessageComboBox(string[] options)
        {
            InitializeComponent();
            comboBoxOptions.ItemsSource = options;
            comboBoxOptions.SelectedIndex = 0; // Selecciona el primer elemento por defecto
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (comboBoxOptions.SelectedItem != null)
            {
                SelectedOption = comboBoxOptions.SelectedItem.ToString();
                DialogResult = true; // Indica que se aceptó
            }
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false; // Indica que se canceló
            Close();
        }
    }
}
