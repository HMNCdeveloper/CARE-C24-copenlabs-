using Npgsql.PostgresTypes;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace HS5
{
    public partial class Analysis : Window
    {
        private bool x;
        public Analysis(bool x)
        {
            InitializeComponent();
            this.x = x;
            DataContext = this;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            if (x)
            {
                progressBarAnimation(9000);
            }
            else
            {
                progressBarAnimation(5300);
            }
        }


        private async void progressBarAnimation(int ms)
        {

            progressClean();
            // Valor final al que queremos llegar
            double targetValue = 100;

            // Valor inicial de la barra de progreso
            double startValue = progressBar.Value;

            // Incremento por milisegundo
            double incrementPerMillisecond = (targetValue - startValue) / ms;

            // Variable de tiempo inicial
            DateTime startTime = DateTime.Now;

            // Mientras no haya pasado el tiempo total de la animación
            while ((DateTime.Now - startTime).TotalMilliseconds < ms)
            {
                // Calcular el nuevo valor de la barra de progreso
                double elapsedTimeMilliseconds = (DateTime.Now - startTime).TotalMilliseconds;
                double newValue = startValue + (incrementPerMillisecond * elapsedTimeMilliseconds);

                // Aplicar el nuevo valor a la barra de progreso
                progressBar.Value = newValue;
                
                await Task.Delay(10);
            }

            // Ajustar el valor final a 100 en caso de que se haya excedido
            progressBar.Value = targetValue;
           
        }
        public void progressClean()
        {
            progressBar.Value = 0;

        }
    }
}
