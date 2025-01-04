using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Interop;


namespace HS5
{
    /// <summary>
    /// Interaction logic for ResetApp.xaml
    /// </summary>
    public partial class ResetApp : Window
    {
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);


        private int contador = 20;
        private  Timer timer;

        public ResetApp()
        {
            InitializeComponent();
            CountdownText.Text = contador.ToString();
            Loaded += OnLoaded;

            timer = new Timer(1000); // Intervalo de 1 segundo
            timer.Elapsed += OnTimedEvent;
            timer.Start();

        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Obtén la ventana principal
            var mainWindow = Application.Current.Windows[0];

            if (mainWindow != null)
            {
                // Calcula la posición centrada
                var centerX = mainWindow.Left + (mainWindow.Width - this.Width) / 2;
                var centerY = mainWindow.Top + (mainWindow.Height - this.Height) / 2;

                // Asigna la posición a esta ventana
                this.Left = centerX;
                this.Top = centerY;
            }

            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            try
            {
                contador--;
                if (contador <= 0) // Ejemplo: detén el temporizador después de 5 ciclos
                {
                    timer.Stop();
                    Dispatcher.Invoke(() =>
                    {
                        // Reinicia la aplicación
                        System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                        Application.Current.Shutdown();
                    });
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        CountdownText.Text = contador.ToString();
                    });
                }
            }
            catch (TaskCanceledException)
            {}
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }


        private void Close_window(object sender, System.ComponentModel.CancelEventArgs e)
        {
           
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();

        }
    }
}
