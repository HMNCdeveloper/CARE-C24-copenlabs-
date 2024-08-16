using MahAppsExample;
using System;
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
using HS5.Resources.Idiomas;
//using MathNet.Numerics.Distributions;
using System.Windows.Forms.VisualStyles;


namespace HS5
{
    /// <summary>
    /// Interaction logic for Rates.xaml
    /// </summary>
    public partial class Rates : Window
    {
        string tab = "Instrumen";
        string[] placeholderValues;
        int index = 0;

        private Database obj;
        MachineP obj2 = new MachineP();
        public Func<string, string> obtenerRecurso;

        Label lblCodigosCont;
        ListBox listadoCategorias_Copy;
        ListBox listadoSubcategorias_Copy;
        ListView listadoCodigos_Copy;


        Action CargarCodigos;

        public Rates(Database obj, MachineP obj2, Action CargarCodigos, Func<string, string> obtenerRecurso)
        {
            InitializeComponent();
            this.obj = obj;
            this.obj2 = obj2;
            this.obtenerRecurso = obtenerRecurso;
            placeholderValues = new string[] { this.obtenerRecurso("typeText"), this.obtenerRecurso("typeDec") };


            //cargar categorias
            HacerConexion();
            DataTable categorias = obj.VisualizarCategoriasCodigosPersonalizada();
            for (int i = 0; i <= categorias.Rows.Count - 1; i++)
            {
                Categorias.Items.Add(categorias.Rows[i][1].ToString());
            }
            CerrarConexion();


            //check if the user will be generate rate to enable or  disaenable abour btn generate rate id
            List<Button> list = new List<Button>
            {
                btnGenRate1,
                btnGenRate2,
                btnGenRate3,
            };
            foreach (Button button in list)
            {
                button.IsEnabled = checkBoxRate.IsChecked == true ? true : false;
            }
        }

        public bool HacerConexion()
        {
            //Credenciales y conexion a la bd
            string user = "postgres";
            string password = "radionica";
            return obj.ConexionBD(user, password); //Variable de validacion
        }



        public void CerrarConexion()
        {
            obj.CerrarBD(); //Cerrar conexion
        }


        private void saveRate(object sender, RoutedEventArgs e)
        {
            //Duplicar el remedio
            string codigo = rateId.Text;
            string nombre_codigo = rateName.Text;
            string descripiton = "Instrument";


            if (tab == obtenerRecurso("tabText"))
            {
                descripiton = "text:" + textDesc.Text;

            }
            else if (tab == obtenerRecurso("tabDec"))
            {
                descripiton = "decrete:" + textDesc.Text;
            }


            if (!string.IsNullOrEmpty(nombre_codigo) && !string.IsNullOrEmpty(codigo) && (Categorias.SelectedItem != null || Subcategorias.SelectedItem != null))
            {
                try
                {
                    registerRate();
                    // logic to btn when the user click to btn Acept 
                    DialogResult = true;
                    Close();
                }
                catch (FormatException)
                {
                    MessageBox.Show(obtenerRecurso("messageError"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            if (string.IsNullOrEmpty(nombre_codigo))
                MessageBox.Show(obtenerRecurso("messageError24"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else if (string.IsNullOrEmpty(codigo))
                MessageBox.Show(obtenerRecurso("messageErrorCode"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else if(Categorias.SelectedItem == null &&  Subcategorias.SelectedItem == null)
                MessageBox.Show(obtenerRecurso("messageErrorCatSub"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

            void registerRate()
        {      
            string codigo = rateId.Text;
            string nombre_codigo = rateName.Text;
            HacerConexion();

            // Obtiene id de la subcategoria por el nombre en la tabla categoriaCustom
            object id_subcategoria = obj.GetIdSubcategorieByNameCustom(Subcategorias.SelectedItem != null ? Subcategorias.SelectedItem.ToString() : Categorias.SelectedItem.ToString());

            if (id_subcategoria != null)
            {
                //valida si existe la frecuencia  para no duplicar
                if (!obj.validarExisteFrecuencia(codigo))
                {
                    Guid idCodigo = Guid.NewGuid();
                    //registra el rate 
                    obj.Registrar_Codigo(idCodigo, codigo, Guid.Parse(id_subcategoria.ToString()));
                    //registra el codigo
                    obj.Registrar_CodigoPersonalizado(nombre_codigo, idCodigo);
                }
                else
                {
                    MessageBox.Show("La frecuencia "+codigo+" , ya esta en uso por un rate","Advertencia",MessageBoxButton.OK,MessageBoxImage.Warning);
                }
            }
            else
            {
                //here we  defined the categories
                object id_categoria_p =obj.BuscarCategoriasCodigosPersonalizadas(Categorias.SelectedItem.ToString());
                Guid SubCategoriaID = Guid.NewGuid();
                obj.Registrar_Subcategoria(SubCategoriaID, Guid.Parse(id_categoria_p.ToString()));
                obj.Registrar_SubcategoriaPersonalizada(Categorias.SelectedItem.ToString(), SubCategoriaID);
                registerRate();

            }
            CerrarConexion();
        }


        async Task Esperar()
        {
            obj2.Diagnostic();

            // Esperar 2 segundos
            await Task.Delay(5000);

            obj2.Diagnostic();
        }
        private async void generateRate(object sender, RoutedEventArgs e)
        {


            await Esperar();
            HacerConexion();
            string randomRate = obj.Generarcodigo();
            CerrarConexion();
            rateId.Text = randomRate;
        }

        private void _generateRate(object sender, RoutedEventArgs e)
        {

            HacerConexion();
            string randomRate = obj.Generarcodigo();
            CerrarConexion();
            rateId.Text = randomRate;
        }

        private void closeWindow(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }



        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                var tabControl = sender as TabControl;
                var selectedTab = tabControl.SelectedItem as TabItem;

                if (selectedTab != null && selectedTab.Header is StackPanel stackPanel)
                {
                    var textBlock = stackPanel.Children[0] as TextBlock;
                    tab = textBlock?.Text;
                }
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            Console.WriteLine(textBox.Text);

            if (placeholderValues.Contains(textBox.Text))
            {
                index = Array.IndexOf(placeholderValues, textBox.Text);
                textBox.Text = "";
                textBox.Foreground = new SolidColorBrush(Colors.Black);
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = placeholderValues[index];
                textBox.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }

        private void selected_subcategorie(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;

            //clear subcategorie when the change the categorie 
            Subcategorias.Items.Clear();

            HacerConexion();
            object id_categoria =obj.BuscarCategoriasCodigosPersonalizadas(comboBox.SelectedItem.ToString());
            DataTable subCategorias =obj.VisualizarSubCategoriasCodigosPerosnalizadas(id_categoria.ToString());
            for (int y = 0; y <= subCategorias.Rows.Count - 1; y++)
            {
                if (subCategorias.Rows[y][0].ToString() != "" && comboBox.SelectedItem.ToString() != subCategorias.Rows[y][0].ToString())
                {
                    Subcategorias.Items.Add(subCategorias.Rows[y][0].ToString());
                }
            }
            CerrarConexion();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            List<Button> list = new List<Button>
            {
                btnGenRate1,
                btnGenRate2,
                btnGenRate3,
            };

            rateId.IsReadOnly = checkBox.IsChecked == true ? true : false;
            foreach (Button button in list)
            {
                button.IsEnabled = checkBox.IsChecked == true ? true : false;
            }
        }
    }

}





