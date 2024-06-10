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

        Database obj;
        MachineP obj2 = new MachineP();
        public Func<string, string> obtenerRecurso;

        Label lblCodigosCont;
        ListBox listadoCategorias_Copy;
        ListBox listadoSubcategorias_Copy;
        ListView listadoCodigos_Copy;


        Action CargarCodigos;

        public Rates(Database obj, MachineP obj2, Action CargarCodigos, Func<string, string> obtenerRecurso, ListBox listadoCategorias_Copy, ListBox listadoSubcategorias_Copy, ListView listadoCodigos_Copy, Label lblCodigosCont)
        {
            InitializeComponent();
            this.obj = obj;
            this.obj2 = obj2;
            this.obtenerRecurso = obtenerRecurso;
            this.listadoCategorias_Copy = listadoCategorias_Copy;
            this.listadoSubcategorias_Copy = listadoSubcategorias_Copy;
            this.CargarCodigos = CargarCodigos;
            this.lblCodigosCont = lblCodigosCont;
            this.listadoCodigos_Copy = listadoCodigos_Copy;
            placeholderValues = new string[] { this.obtenerRecurso("typeText"), this.obtenerRecurso("typeDec") };
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


            if (!string.IsNullOrEmpty(nombre_codigo) && !string.IsNullOrEmpty(codigo))
            {
                try
                {

                    int codigo_num = Int32.Parse(codigo);
                    HacerConexion();

                    //Objeto
                    Radionica obj_new = new Radionica();

                    //Categoria padre
                    string id_cat_pad = obj.Obtener_IDCategoria(listadoCategorias_Copy.SelectedItem.ToString()).ToString();

                    //Subcategoria
                    string id_subcat = string.IsNullOrEmpty(listadoSubcategorias_Copy.SelectedItem?.ToString()) ? id_cat_pad : obj.Obtener_IDCategoria(listadoSubcategorias_Copy.SelectedItem.ToString()).ToString();



                    object genero_para_codigo = "T";

                    DataTable codigoRepetido = obj.ExisteCodigo(codigo);

                    if (codigoRepetido.Rows.Count == 0)
                    {
                        obj.Registrar_Codigo_Categorias(obj_new.Generar_Id(), nombre_codigo, codigo_num.ToString(), descripiton, id_subcat, id_cat_pad, genero_para_codigo.ToString());
                        CargarCodigos?.Invoke();
                        CustomMessageBox custom = new CustomMessageBox();
                        custom.Message = obtenerRecurso("rateRegistrado");
                        custom.ShowDialog();
                    }
                    else
                    {
                        HS5.CustomMessageBoxYesNo customMessageBoxYesNo = new HS5.CustomMessageBoxYesNo(obtenerRecurso("CodigoDuplicado"), obtenerRecurso("btnNewRate"));

                        bool? result = customMessageBoxYesNo.ShowDialog();

                        if (result.HasValue && result.Value)
                        {
                            obj.Registrar_Codigo_Categorias(obj_new.Generar_Id(), nombre_codigo, codigo_num.ToString(), descripiton, id_subcat, id_cat_pad, genero_para_codigo.ToString());
                            CargarCodigos?.Invoke(); ;
                            CustomMessageBox custom = new CustomMessageBox();
                            custom.Message = obtenerRecurso("rateRegistrado");
                            custom.ShowDialog();
                        }
                        else
                        {
                            CustomMessageBox custom = new CustomMessageBox();
                            custom.Message = obtenerRecurso("rateNoRegistrado");
                            custom.ShowDialog();
                        }
                    }


                    lblCodigosCont.Content = listadoCodigos_Copy.Items.Count + " Rates";
                    CerrarConexion();
                    // Lógica para el botón "Aceptar"
                    DialogResult = true;
                    Close();
                }
                catch (FormatException)
                {
                    MessageBox.Show(obtenerRecurso("messageError"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show(obtenerRecurso("messageError24"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

            HacerConexion();
            await Esperar();
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
    }

}






