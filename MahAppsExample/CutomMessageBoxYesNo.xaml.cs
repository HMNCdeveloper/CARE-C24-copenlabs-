using System.Windows;


namespace HS5
{
    public partial class CustomMessageBoxYesNo : Window
    {
        public string Message { get; set; }
        public string Titulo { get; set; }
        public CustomMessageBoxYesNo(string message, string Titulo)
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