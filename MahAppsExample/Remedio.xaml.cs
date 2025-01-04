
using System.Data;
using System.Windows;

using static MahAppsExample.MainWindow;

namespace HS5
{
    /// <summary>
    /// Interaction logic for Remedio.xaml
    /// </summary>
    public partial class Remedio : Window
    {
        public Remedio(DataTable CodigosdeRemedios)
        {
            InitializeComponent();
            for (int i = 0; i <= CodigosdeRemedios.Rows.Count - 1; i++)
            {

                if (!string.IsNullOrEmpty(CodigosdeRemedios.Rows[i][5].ToString()))
                {
                    ListaRemedios.Items.Add(new nuevoRemedio { nombrecodigo = CodigosdeRemedios.Rows[i][5].ToString(), codigo = CodigosdeRemedios.Rows[i][6].ToString(), potencia = CodigosdeRemedios.Rows[i][3].ToString(), metodo = CodigosdeRemedios.Rows[i][1].ToString(), codigocomplementario = CodigosdeRemedios.Rows[i][4].ToString(), nivel = CodigosdeRemedios.Rows[i][2].ToString() });
                }
                else if(!string.IsNullOrEmpty(CodigosdeRemedios.Rows[i][7].ToString()))
                {
                    ListaRemedios.Items.Add(new nuevoRemedio { nombrecodigo = CodigosdeRemedios.Rows[i][7].ToString(), codigo = CodigosdeRemedios.Rows[i][8].ToString(), potencia = CodigosdeRemedios.Rows[i][3].ToString(), metodo = CodigosdeRemedios.Rows[i][1].ToString(), codigocomplementario = CodigosdeRemedios.Rows[i][4].ToString(), nivel = CodigosdeRemedios.Rows[i][2].ToString() });
                }
                else
                {
                    ListaRemedios.Items.Add(new nuevoRemedio { nombrecodigo = CodigosdeRemedios.Rows[i][10].ToString(), codigo = CodigosdeRemedios.Rows[i][9].ToString(), potencia = CodigosdeRemedios.Rows[i][3].ToString(), metodo = CodigosdeRemedios.Rows[i][1].ToString(), codigocomplementario = CodigosdeRemedios.Rows[i][4].ToString(), nivel = CodigosdeRemedios.Rows[i][2].ToString() });
                }

            }
        }
    }
}
