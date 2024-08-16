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

namespace HS5
{
    /// <summary>
    /// Interaction logic for DuplicarRemedio.xaml
    /// </summary>
    public partial class DuplicarRemedio : Window
    {

        private Database obj;
        MachineP obj2;
        public Func<string, string> obtenerRecurso;
        List<string> lista_codigos = new List<string>();


        public DuplicarRemedio(Database obj, MachineP obj2, Action CargarCodigos, Func<string, string> obtenerRecurso)
        {
            InitializeComponent();

            this.obj = obj;
            this.obj2 = obj2;
            this.obtenerRecurso = obtenerRecurso;

            HacerConexion();
            DataTable categoriasPersonalizada = obj.VisualizarCategoriasCodigosPersonalizada();

            //this line of code is used to show custom categories
            for (int i = 0; i <= categoriasPersonalizada.Rows.Count - 1; i++)
            {
                Categorias.Items.Add(categoriasPersonalizada.Rows[i][1].ToString());
            }
            CerrarConexion();

            //cargar categorias
            HacerConexion();
            DataTable categorias = obj.VisualizarCategoriasCodigos();     
            //this line of code is used to show default categories
            for (int i = 0; i <= categorias.Rows.Count - 1; i++)
            {
                if (listCategorias.Items.Contains(categorias.Rows[i][1].ToString()) == false)
                {
                    listCategorias.Items.Add(categorias.Rows[i][1].ToString());
                }
            }
            CerrarConexion();
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

        private void closeWindow(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
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


        private void listCategorias_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            listSubCategorias.Items.Clear(); //Limpia antes de cada uso
            listCodigos.Items.Clear();

            try
            {
                if (listCategorias.SelectedItem != null)
                {
                    HacerConexion();
                    //Buscar id_categoria para encontrar las subcategorias
                    object id_categoria = obj.BuscarCategoriasCodigos(listCategorias.SelectedItem.ToString());
                    DataTable SubCategorias = obj.VisualizarSubCategoriasCodigos(id_categoria.ToString());

                    for (int y = 0; y <= SubCategorias.Rows.Count - 1; y++)
                    {
                        if (SubCategorias.Rows[y][0].ToString() != "" && listCategorias.SelectedItem.ToString() != SubCategorias.Rows[y][0].ToString())
                        {
                            listSubCategorias.Items.Add(SubCategorias.Rows[y][0].ToString());
                        }
                    }


                    object id_subcategoria = obj.GetIdSubcategorieByName(
                        listCategorias.SelectedItem.ToString(),
                        obj.BuscarCategoriasCodigos(listCategorias.SelectedItem.ToString()).ToString()

                        );

                    if (id_subcategoria != null)
                    {
                        DataTable codigos = obj.getRatesBySubcategorie(id_subcategoria.ToString());

                        //get all rates from codigo
                        for (int i = 0; i < codigos.Rows.Count; i++)
                        {
                            if (!string.IsNullOrEmpty(codigos.Rows[i][1].ToString()))
                            {
                                listCodigos.Items.Add(codigos.Rows[i][2].ToString() + ", " + codigos.Rows[i][1].ToString());
                                //Categorias_Codigos.Add(codigos.Rows[i][2].ToString());
                            }

                        }
                    }

                    CerrarConexion();

                }
            }
            catch (NullReferenceException)
            { }
        }

        private void listSubCategorias_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            listCodigos.Items.Clear();
            try
            {

                HacerConexion();
                object id_subcategoria = obj.GetIdSubcategorieByName(
                    listSubCategorias.SelectedItem.ToString(),
                    obj.BuscarCategoriasCodigos(listCategorias.SelectedItem.ToString()).ToString()
                 );
                DataTable codigos = obj.getRatesBySubcategorie(id_subcategoria.ToString());

                //get all rates from codigo
                for (int i = 0; i < codigos.Rows.Count; i++)
                {
                    if (!string.IsNullOrEmpty(codigos.Rows[i][1].ToString()))
                    {
                        listCodigos.Items.Add(codigos.Rows[i][2].ToString() + ", " + codigos.Rows[i][1].ToString());
                        //Categorias_Codigos3.Add(codigos.Rows[i][2].ToString());
                    }

                }


                CerrarConexion();

            }
            catch (NullReferenceException)
            { }
        }


        private void listCodigos_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (listCodigos.SelectedItem != null)
                {
                    string nameCode = listCodigos.SelectedItem.ToString();
                    var DupNameCode = lista_codigos.Where(name => name == nameCode.Split(',')[0]);

                    if (!DupNameCode.Any())
                    {
                        listelemagregados.Items.Add(nameCode);
                        lista_codigos.Add(nameCode.Split(',')[0]);
                    }
                }

            }
            catch (NullReferenceException)
            {

            }
        }

        private void cmdBorrarElemento_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (listelemagregados.SelectedItem != null)
                {
                    lista_codigos.RemoveAt(lista_codigos.IndexOf(listelemagregados.SelectedItem.ToString().Split(',')[0]));
                    listelemagregados.Items.Remove(listelemagregados.SelectedItem);
                }
                else
                {
                    listelemagregados.Items.Clear();
                    lista_codigos.Clear();
                }


            }
            catch (NullReferenceException)
            {

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                if ((Categorias.SelectedItem != null || Subcategorias.SelectedItem != null) && listelemagregados.Items.Count > 0)
                {
                  
                    HacerConexion();
                    //get id subcategorie
                    object id_subcategoria = obj.GetIdSubcategorieByNameCustom(Subcategorias.SelectedItem != null ? Subcategorias.SelectedItem.ToString() : Categorias.SelectedItem.ToString());


                    //save date from code 
                    foreach (var rate in listelemagregados.Items)
                    {

                        Guid idCodigo = Guid.NewGuid();
                        obj.Registrar_Codigo(idCodigo, rate.ToString().Split(',')[0], Guid.Parse(id_subcategoria.ToString()));
                        object codigoRate = obj.getIDCodebyFrec(rate.ToString().Split(',')[0]);

                        object name = obj.getNameByFrec(Guid.Parse(codigoRate.ToString()));
                        obj.Registrar_CodigoPersonalizado(name.ToString(),idCodigo);
                    }

                    CerrarConexion();
                    DialogResult = true;
                    Close();
                }

            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }
    }
}
