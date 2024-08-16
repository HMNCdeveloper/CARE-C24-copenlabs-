using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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
using static MahAppsExample.MainWindow;

namespace HS5
{
    /// <summary>
    /// Interaction logic for TableAnalisis.xaml
    /// </summary>
    public partial class TableAnalisis : Window
    {
        public TableAnalisis(DataTable codigoAnalisis)
        {
            InitializeComponent();
            for (int p = 0; p <= codigoAnalisis.Rows.Count - 1; p++)
            {  // 3
               // idscodigos.Add(tabla_codigosanalisis.Rows[p][2].ToString()); //Guarda idcodigo del analisis
               //codigo,nombrecodigo,nivel,nivelsugerido,valor,vinicial,vfinal,decimales,tipo
                ListaCodigos.Items.Add(new nuevoCodigo { rates = codigoAnalisis.Rows[p][1].ToString(), nombre = codigoAnalisis.Rows[p][2].ToString(), niveles = codigoAnalisis.Rows[p][4].ToString(), nsugerido = codigoAnalisis.Rows[p][5].ToString(), ftester = Convert.ToInt32(codigoAnalisis.Rows[p][3].ToString()), potencia = codigoAnalisis.Rows[p][6].ToString(), potenciaSugeridad = codigoAnalisis.Rows[p][7].ToString() });

            }
        }


        //this function is used to add the handle evenr of click
        public void GridViewColumn_Loaded(object sender, RoutedEventArgs e)
        {

        }

        //this function is used to order by header grid 
        private void GridViewColumnHeader_MouseDoubleClick(object sender, RoutedEventArgs e)
        {

        }
    }    
}
