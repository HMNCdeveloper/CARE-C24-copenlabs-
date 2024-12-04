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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MessageBox = System.Windows.Forms.MessageBox;

namespace HS5
{
    /// <summary>
    /// Interaction logic for TreatmentUpdate.xaml
    /// </summary>
    public partial class TreatmentUpdate : Window
    {
        private Database obj;
        private object id;
        private object id_paciente;
        private object idpadre;
        public Func<string, string> obtenerRecurso;
        private string paciente;

        private List<string> lista_remedios=new List<string> { };
        private List<string> lista_analisis = new List<string> { };
        private List<string> lista_rates = new List<string> { };


        public TreatmentUpdate(string patient, string nameTreatment, Database obj, Func<string, string> obtenerRecurso)
        {
            InitializeComponent();
            this.obj = obj;
            this.obtenerRecurso= obtenerRecurso;
            this.paciente = patient;
            txtNombrePaciente.Text = patient;
            txtNombreTramiento.Text = nameTreatment;
            loadContentTreatments(patient,nameTreatment);

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


        private void loadContentTreatments(string patient, string nameTreatment)
        {
            HacerConexion();
            this.id_paciente = obj.Obtener_IdPaciente_NombreCompleto(patient);
            this.id = obj.Obtener_IDTratamiento(id_paciente.ToString(), patient, nameTreatment);
            this.idpadre = obj.Obtener_IDPadre(id_paciente.ToString(), patient, nameTreatment);

            //Console.WriteLine("variable id "+id.ToString());
            //Console.WriteLine("variable id_padre"+idpadre.ToString());

            if (string.IsNullOrEmpty(this.idpadre.ToString()))
            {
                  this.idpadre= this.id;
            }


            DataTable codigos_tratamiento = obj.CodigosTratamiento(idpadre.ToString());
           
            foreach (DataRow code in codigos_tratamiento.Rows)
            {
                if (code["tipo"].ToString()=="R")
                {
                    listRemedyAdded.Items.Add(code["descripcion"].ToString());
                }else if (code["tipo"].ToString() == "A")
                {
                    listAnalisysAdded.Items.Add(code["descripcion"].ToString());
                }
                else if (code["tipo"].ToString() == "C"){
                    listRateAdded.Items.Add(code["descripcion"].ToString());
                }
               
               ;
            }

            CerrarConexion();

        }

        private void removeContentElemet(object sender, RoutedEventArgs e)
        {
          

            System.Windows.Controls.Button btnDelete = sender as System.Windows.Controls.Button;
            //Eliminar de las listas y buscar
            if (listRemedyAdded.SelectedItem != null || listAnalisysAdded.SelectedItem != null || listRateAdded.SelectedItem != null)
            {
                var data = listRemedyAdded.SelectedItem ?? listAnalisysAdded.SelectedItem ?? listRateAdded.SelectedItem;
                HacerConexion();
                obj.Eliminar_Codigo_Tratamientos(data.ToString(), this.idpadre.ToString());
                CerrarConexion();
            }

            if (btnDelete.Name == "groupDeleteRemedy")
            {
                listRemedyAdded.Items.Remove(listRemedyAdded.SelectedItem);
            }
            else if (btnDelete.Name == "groupDeleteAnalisys")
            {
                listAnalisysAdded.Items.Remove(listAnalisysAdded.SelectedItem);
            }
            else if (btnDelete.Name == "groupDeleteRate")
            {
                listRateAdded.Items.Remove(listRateAdded.SelectedItem);
            }
        }

        private void comboTipoTratamiento_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            listgenerico.Items.Clear();
            categoria.Visibility = Visibility.Hidden;
            subcategoria.Visibility = Visibility.Hidden;
            listgenerico.Width = 467;
            listgenerico.Margin=new Thickness(385, 282, 0, 0);
            ratesCutomTreatment.Visibility = Visibility.Hidden;         

            HacerConexion();
            ComboBoxItem selectedItem = (ComboBoxItem)comboTipoTratamiento.SelectedItem;

            if (selectedItem.Content.ToString() == obtenerRecurso("MenuAnalysis"))
            {
                DataTable AnalisisPaciente_Seleccionado = obj.Obtener_Analisis_Pacientes_Recientes_PorNombrePaciente(this.paciente);
                
                for (int i = 0; i <= AnalisisPaciente_Seleccionado.Rows.Count - 1; i++)
                {
                    //Agregar solo nombre del analisis
                    listgenerico.Items.Add(AnalisisPaciente_Seleccionado.Rows[i][1].ToString());
                }

                Trata.Header = obtenerRecurso("MenuAnalysis");
            }else if (selectedItem.Content.ToString() == obtenerRecurso("labelRemedy"))
            {
                string lengremedy = "";

                if (Database.table == "ingles")
                {
                    lengremedy = "EN";
                }
                else if (Database.table == "espanol")
                {
                    lengremedy = "ES";
                }
                else if (Database.table == "bulgaro")
                {
                    lengremedy = "BG";
                }


                DataTable ListaRemedios = obj.VisualizarRemedios(lengremedy);

                //Llenar el combobox con analisis relacionados
                for (int i = 0; i <= ListaRemedios.Rows.Count - 1; i++)
                {
                    //Agregar solo nombre del analisis
                    listgenerico.Items.Add(ListaRemedios.Rows[i][1].ToString());
                }

                Trata.Header = obtenerRecurso("labelRemedy");
            }else if (selectedItem.Content.ToString() == obtenerRecurso("labelRate"))
            {
                categoria.Items.Clear();
                subcategoria.Items.Clear();
                ratesCutomTreatment.Visibility = Visibility.Visible;
                categoria.Visibility= Visibility.Visible;
                subcategoria.Visibility = Visibility.Visible;
                listgenerico.Width = 229;
                listgenerico.Margin = new Thickness(634, 282, 0, 0);
              
                visualizarCategoriaCodigoParaTratamiento();

            }
            CerrarConexion();
        }



        void visualizarCategoriaCodigoParaTratamiento()
        {
            HacerConexion();
            DataTable Categorias = ratesCutomTreatment.IsChecked == true ? obj.VisualizarCategoriasCodigosPersonalizada() : obj.VisualizarCategoriasCodigos();
                //Cargar categorias
            for (int i = 0; i <= Categorias.Rows.Count - 1; i++)
            {
                if (categoria.Items.Contains(Categorias.Rows[i][1].ToString()) == false)
                {
                    categoria.Items.Add(Categorias.Rows[i][1].ToString());
                }
            }

            CerrarConexion();
        }

        private void categoria_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                subcategoria.Items.Clear(); //Limpia antes de cada uso
                listgenerico.Items.Clear();

                if (categoria.SelectedItem != null)
                {
                    HacerConexion();
                    //Buscar id_categoria para encontrar las subcategorias
                    object id_categoria = ratesCutomTreatment.IsChecked == true ? obj.BuscarCategoriasCodigosPersonalizadas(categoria.SelectedItem.ToString()) : obj.BuscarCategoriasCodigos(categoria.SelectedItem.ToString());
                    DataTable SubCategorias = ratesCutomTreatment.IsChecked == true ? obj.VisualizarSubCategoriasCodigosPerosnalizadas(id_categoria.ToString()) : obj.VisualizarSubCategoriasCodigos(id_categoria.ToString());

                    for (int y = 0; y <= SubCategorias.Rows.Count - 1; y++)
                    {
                        if (SubCategorias.Rows[y][0].ToString() != "" && categoria.SelectedItem.ToString() != SubCategorias.Rows[y][0].ToString())
                        {
                           subcategoria.Items.Add(SubCategorias.Rows[y][0].ToString());
                        }
                    }
                    

                    object id_subcategoria = ratesCutomTreatment.IsChecked == true ? obj.GetIdSubcategorieByNameCustom(categoria.SelectedItem.ToString()) : obj.GetIdSubcategorieByName(
                        categoria.SelectedItem.ToString(),
                        obj.BuscarCategoriasCodigos(categoria.SelectedItem.ToString()).ToString()
                       );

                    if (id_subcategoria != null)
                    {
                        DataTable codigos = ratesCutomTreatment.IsChecked == true ? obj.getRatesByCustomSubcategorie(id_subcategoria.ToString()) : obj.getRatesBySubcategorie(id_subcategoria.ToString());

                        //imprime los rates al que pertenenecen a la categoria en una tabla en la seccion broadcasting o tratamiento
                        for (int i = 0; i < codigos.Rows.Count; i++)
                        {
                            if (!string.IsNullOrEmpty(codigos.Rows[i][1].ToString()))
                            {
                                listgenerico.Items.Add(codigos.Rows[i][1].ToString());
                                //Categorias_Codigos.Add(codigos.Rows[i][2].ToString());
                            }

                        }
                    }

                    CerrarConexion();

                }
            }
            catch (NullReferenceException)
            {
                // MessageBox.Show("Error seleccione una categoría antes de continuar!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void subcategoria_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            try
            {

                listgenerico.Items.Clear();
                HacerConexion();
                object id_subcategoria = ratesCutomTreatment.IsChecked == true ? obj.GetIdSubcategorieByNameCustom(subcategoria.SelectedItem.ToString()) : obj.GetIdSubcategorieByName(
                                             subcategoria.SelectedItem.ToString(),
                                             obj.BuscarCategoriasCodigos(categoria.SelectedItem.ToString()).ToString()
                                             );
                DataTable codigos = ratesCutomTreatment.IsChecked == true ? obj.getRatesByCustomSubcategorie(id_subcategoria.ToString()) : obj.getRatesBySubcategorie(id_subcategoria.ToString());

                //get all rates from codigo
                for (int i = 0; i < codigos.Rows.Count; i++)
                {
                    if (!string.IsNullOrEmpty(codigos.Rows[i][1].ToString()))
                    {
                        listgenerico.Items.Add(codigos.Rows[i][1].ToString());
                        //Categorias_Codigos3.Add(codigos.Rows[i][2].ToString());
                    }

                }


                CerrarConexion();

            }
            catch (NullReferenceException)
            {
                MessageBox.Show(obtenerRecurso("messageInf1"), obtenerRecurso("messageHeadInf"),MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

        }

        private void ratesCutomTreatment_Click(object sender, RoutedEventArgs e)
        {
            categoria.Items.Clear();
            subcategoria.Items.Clear();
            listgenerico.Items.Clear();
            visualizarCategoriaCodigoParaTratamiento();
        }

        private void txtNombreTratamiento_Copy_SelectionChanged(object sender, RoutedEventArgs e)
        {
            
            if (comboTipoTratamiento.SelectedItem != null)
            {
                listgenerico.Items.Clear();

                HacerConexion();
                if (((ComboBoxItem)comboTipoTratamiento.SelectedItem).Content.ToString() == obtenerRecurso("MenuAnalysis"))
                {
                  
                    //Llama y obtiene posibles matches
                    DataTable PacientesAnalisisBuscado =  obj.Obtener_Analisis_Pacientes_Recientes_PorNombrePaciente2(paciente, txtNombreTratamiento_Copy.Text);
                    for (int j = 0; j <= PacientesAnalisisBuscado.Rows.Count - 1; j++)
                    {
                        listgenerico.Items.Add(PacientesAnalisisBuscado.Rows[j][0].ToString());
                    }
                    
                }else if (((ComboBoxItem)comboTipoTratamiento.SelectedItem).Content.ToString() == obtenerRecurso("labelRemedy"))
                {
                    DataTable Codigos = obj.BuscarCodigoRem(txtNombreTratamiento_Copy.Text);

                    for (int y = 0; y < Codigos.Rows.Count; y++)
                    {
                        if (!string.IsNullOrEmpty(Codigos.Rows[y][0].ToString()))
                        {
                            listgenerico.Items.Add(Codigos.Rows[y][0].ToString());
                        }
                    }
                }else if (((ComboBoxItem)comboTipoTratamiento.SelectedItem).Content.ToString() == obtenerRecurso("labelRate"))
                {
                    subcategoria.Items.Clear();
                    DataTable Codigos = obj.BuscarCategoriaCodigo(txtNombreTratamiento_Copy.Text);
                    for (int y = 0; y <= Codigos.Rows.Count - 1; y++)
                    {
                        if (Codigos.Rows[y][0].ToString() != "")
                        {
                            //listadoCodigos.Items.Add(new CheckBox { Content = Codigos.Rows[y][1].ToString() });
                            listgenerico.Items.Add(Codigos.Rows[y][0].ToString() + " , " + Codigos.Rows[y][1].ToString());
                        }
                    }

                }
             CerrarConexion();
            }
        }

        private void listgenerico_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string selectedItemText = listgenerico.SelectedItem.ToString();
            
            if (((ComboBoxItem)comboTipoTratamiento.SelectedItem).Content.ToString() == obtenerRecurso("MenuAnalysis"))
            {
                var findDupRemedy=listAnalisysAdded.Items.Cast<string>()
                               .Where(val => val == selectedItemText);

                if (!findDupRemedy.Any())
                {
                    listAnalisysAdded.Items.Add(selectedItemText);
                    this.lista_analisis.Add(selectedItemText);
                }
            }

            if (((ComboBoxItem)comboTipoTratamiento.SelectedItem).Content.ToString() == obtenerRecurso("labelRemedy"))
            {
                var findDupAnalisys = listRemedyAdded.Items.Cast<string>()
                               .Where(val => val == selectedItemText); ;
                
                if (!findDupAnalisys.Any())
                {
                    listRemedyAdded.Items.Add(selectedItemText);
                    this.lista_remedios.Add(selectedItemText);
                }
            }

            if (((ComboBoxItem)comboTipoTratamiento.SelectedItem).Content.ToString() == obtenerRecurso("labelRate"))
            {
                var findDupRates = listRateAdded.Items.Cast<string>()
                               .Where(val => val == selectedItemText); ;
                if (!findDupRates.Any())
                {
                    listRateAdded.Items.Add(selectedItemText);
                    this.lista_rates.Add(selectedItemText);
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            HacerConexion();
            for (int q = 0; q <= lista_remedios.Count - 1; q++)
            {
                //Registrar contenido del tratamiento sencillo
                obj.Registrar_ContenidoTratamiento(this.idpadre.ToString(), lista_remedios[q], "R");
            }

            //Para analisis
            for (int q = 0; q <= lista_analisis.Count - 1; q++)
            {
                //Registrar contenido del tratamiento sencillo
                obj.Registrar_ContenidoTratamiento(this.idpadre.ToString(), lista_analisis[q], "A");
            }

            //Para codigos individuales
            for (int q = 0; q <= lista_rates.Count - 1; q++)
            {
                //Registrar contenido del tratamiento sencillo
                obj.Registrar_ContenidoTratamiento(this.idpadre.ToString(), lista_rates[q], "C");
            }

            CerrarConexion();

            if (lista_remedios.Count > 0 || lista_analisis.Count> 0 || lista_rates.Count> 0)
            {
                MessageBox.Show(obtenerRecurso("messageInfo11"), obtenerRecurso("messageHeadInf"), MessageBoxButtons.OK,MessageBoxIcon.Information);
            }

            DialogResult = true;
            Close();

        }
    }

}
