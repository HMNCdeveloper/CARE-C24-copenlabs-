using System.Windows;


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
