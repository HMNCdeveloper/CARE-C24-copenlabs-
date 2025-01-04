using System.Windows;


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
