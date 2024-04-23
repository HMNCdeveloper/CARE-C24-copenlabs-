using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls;
using System.Threading;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Microsoft.VisualBasic; //Interaction
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics; //Para ejecutar el bat file
//Libreria Qr-Codes
using iTextSharp.text;
using iTextSharp.text.pdf;
//using HS5;
using HS5.Properties;
using System.Globalization;
using HS5.Resources.Idiomas;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Application = System.Windows.Application;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using System.Windows.Media;
using System.Windows.Controls;
using System.Reflection;




namespace MahAppsExample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        Machine obj = new Machine(); //Objeto clase Machine
        Database obj2 = new Database(); //Objeto clase Database

        //Listado Heredo
        List<string> ListaHeredoTitulos = new List<string>();
        List<string> ListaHeredoDescrip = new List<string>();

        //Patologicos
        List<string> ListaPatTitulos = new List<string>();
        List<string> ListaPatDescrip = new List<string>();

        //No Patologicos
        List<string> ListaNoPatTitulos = new List<string>();
        List<string> ListaNoPatDescrip = new List<string>();

        //Comentarios
        List<string> ListaComentTitulos = new List<string>();
        List<string> ListaComentDescrip = new List<string>();

        //Telefonos
        List<string> ListaTelefonos = new List<string>();
        List<string> ListaExtensiones = new List<string>();

        //Domicilios
        List<string> ListaCalles = new List<string>();
        List<string> ListaNum = new List<string>();
        List<string> ListaColonia = new List<string>();
        List<string> ListaCP = new List<string>();
        List<string> ListaMunicipio = new List<string>();
        List<string> ListaEstado = new List<string>();
        List<string> ListaPais = new List<string>();

        //Analisis - Historial
        List<string> ListaAnalisis = new List<string>();
        List<string> ListaAnalisisFecha = new List<string>();
        DataTable Historial_Analisis_Paciente; //Datatable con historial del paciente.

        string id_paciente_global_modif;
        bool modif = false;
        List<string> Categorias_Codigos = new List<string>();
        List<string> Categorias_Codigos2 = new List<string>();

        //Analisis cualitativo y cuantitativo
        protected List<string> nombrecodigo = new List<string>();
        List<string> nivel = new List<string>();
        List<string> potencia = new List<string>();
        List<string> potenciasugeridad = new List<string>();
        List<string> ftester = new List<string>();
        List<string> codigos_rates = new List<string>();
        List<string> Sniveles = new List<string>();




        string tipo_nivel_codigo;
        string nivel_potencia;
        //string puertoCOM;

        public MainWindow(string puerto)
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Settings.Default.Lenguaje);

            //this condition is used to perform the backup loading in the db in an automated way
            if (File.Exists(RutaInstalacion() + "\\db\\code.sql"))
            {

                MessageBox.Show("You need to restore  the bckup to your local database before proceeding!.... Then it will be installed!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                HacerConexion();
                obj2.UploadBackup(RutaInstalacion() + "\\db\\code.sql", "Successful restoration!!, Right Now the System will close, you have to open the system to use it again", "Informacion");
                CerrarConexion();
                var window = Application.Current.Windows[0];
                window.Close();
            }
            else
            {
                if (Settings.Default.Lenguaje.ToString() == "es-MX")
                {
                    Database.db = "rad_es";
                }
                else if (Settings.Default.Lenguaje.ToString() == "en-US")
                {
                    Database.db = "rad_en";
                }


                InitializeComponent();
                //Condiciona al control de fechas para que solo use la fecha apartir de hoy...
                dateProg.SelectedDate = DateTime.Today;
                comboTipoProg.SelectedIndex = 0;
                Console.WriteLine(Settings.Default.Lenguaje.ToString());

                //Deteccion de la maquina o dispositivo
                string id_maquina = obj.Machine_Detection(puerto);
                IDs_maquinas_aceptados(id_maquina);

                //Oculta opciones del sistema
                OcultarDiag();
                MostrarPrincipalAnalisis();
                Cargar_TerapiaDefault();
                Mostrar_TerapiaColor();
                Desactivar_Diagnostico();

                //Solo si no existe copia la carpeta de fotos de HS 4.9
                if (!Directory.Exists(RutaInstalacion() + "//fotos"))
                {
                    Detectar_FolderEscritorio(); //Detectamos el folder de HS 4.9
                }

                //Llamada a la bd conteo de la barra superior
                //Conteo_BarraSup_ActivosPendiente();

                //Categorias Remedios y Categorias
                HacerConexion();
                DataTable Categorias = obj2.VisualizarCategoriasCodigos();
                for (int i = 0; i <= Categorias.Rows.Count - 1; i++)
                {
                    listadoCategorias_Remedios.Items.Add(Categorias.Rows[i][1].ToString());
                    listadoCategorias_Copy.Items.Add(Categorias.Rows[i][1].ToString());
                }
                lblCategoriasCont.Content = listadoCategorias_Copy.Items.Count + " " + obtenerRecurso("Categories");
                lblSubcategoriasCont.Content = "0 " + obtenerRecurso("labelSubCat");
                lblCodigosCont.Content = "0 " + obtenerRecurso("labelRate");

                CerrarConexion();

                Cargar_Tratamientos_Pendientes_Y_Activos();
                CargarListadoCompletoPacientes();
                //getVersion();
            }


        }


        //This function is used to decrypt the version is to encrypt
        string Decrypt(string encryptedText)
        {

            string ToReturn = "";
            string publickey = "16345679"; // Clave pública
            string secretkey = "97654361"; // Clave secreta

            byte[] secretkeyByte = Encoding.UTF8.GetBytes(secretkey);
            byte[] publickeybyte = Encoding.UTF8.GetBytes(publickey);

            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            {
                byte[] inputbyteArray = Convert.FromBase64String(encryptedText);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(publickeybyte, secretkeyByte), CryptoStreamMode.Write);
                cs.Write(inputbyteArray, 0, inputbyteArray.Length);
                cs.FlushFinalBlock();
                ToReturn = Encoding.UTF8.GetString(ms.ToArray());
            }

            return ToReturn;
        }

        //This function is used to get the version the program after it will be execute some functions by the version
        void getVersion()
        {
            try
            {
                string fileversion = File.ReadAllText(RutaInstalacion() + "\\version\\version.txt");
                string value = Decrypt(fileversion);
                switch (value)
                {
                    case "Basic":
                        Patients.Margin = new Thickness(2, 6, -5, -8);
                        CategoriasTab.Margin = new Thickness(19, 2, -19, -8);
                        tratamientos.Margin = new Thickness(25, 1, -29, -8);
                        setting.Margin = new Thickness(32, 0, -35, -8);


                        AnalisisTab.Visibility = Visibility.Collapsed;
                        RemediosTab.Visibility = Visibility.Collapsed;
                        ColorTab.Visibility = Visibility.Collapsed;
                        break;
                    case "Medium":
                        Patients.Margin = new Thickness(2, 6, -5, -8);
                        AnalisisTab.Margin = new Thickness(6, 4, -6, -8);
                        CategoriasTab.Margin = new Thickness(19, 2, -19, -8);
                        tratamientos.Margin = new Thickness(25, 1, -29, -8);
                        setting.Margin = new Thickness(32, 0, -35, -8);

                        RemediosTab.Visibility = Visibility.Collapsed;
                        ColorTab.Visibility = Visibility.Collapsed;
                        break;
                    case "Pro":
                        break;
                    default:
                        MessageBox.Show("happened some error, the system didn't look for the version, notify the company about the error", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        var window = Application.Current.Windows[0];
                        window.Close();
                        break;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("happened some error, notify the company about the error", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                var window = Application.Current.Windows[0];
                window.Close();
            }

        }


        //this function is used to get the langauge resources 
        private string obtenerRecurso(string nombreCadena)
        {
            string miCadena = Lenguaje.ResourceManager.GetString(nombreCadena);
            return miCadena;
        }

        //Obtiene ruta de instalacion del programa
        public string RutaInstalacion()
        {
            string programa = Environment.GetCommandLineArgs()[0];
            string directorio = System.IO.Path.GetDirectoryName(programa);
            return directorio;
        }

        //Funcion para el conteo de activos y pendientes en la barra superior
        public void Conteo_BarraSup_ActivosPendiente()
        {
            int activos = ListadoDiagActivos.Items.Count;
            HacerConexion();
            object pendientes = obj2.Tratamientos_Espera_Total();
            CerrarConexion();
            Actividad_Trata.Text = "ACT " + activos.ToString() + " | PEND " + pendientes.ToString();
        }

        //Funcion que lleva a la pestaña de Tratamiento a distancia del boton barra superior
        void IrADistancia(object sender, RoutedEventArgs e)
        {
            opcionesHomoeonic.SelectedIndex = 5;
        }

        //This function is used to load an external window
        void RegistrarVersion(object sender, RoutedEventArgs e)
        {
            //PANEL DE REGISTRO DE VERSION
            registrarGroup.Visibility = Visibility.Visible;
            lblNombreRegistro.Visibility = Visibility.Visible;
            txtNombreRegistro.Visibility = Visibility.Visible;
            lblDescripcionRegistro.Visibility = Visibility.Visible;
            txtDescripcionRegistro.Visibility = Visibility.Visible;
            lblDescripcionEjemplo.Visibility = Visibility.Visible;
            lblNombreEjemplo.Visibility = Visibility.Visible;
            cmdRegistroVersion.Visibility = Visibility.Visible;

            //BUSQUEDA DEL PACIENTE Y REGISTRO
            groupBox.Visibility = Visibility.Hidden;
            txtBuscarPaciente.Visibility = Visibility.Hidden;
            lblBusqueda.Visibility = Visibility.Hidden;
            cmdEliminar.Visibility = Visibility.Hidden;
            ListaPacientes.Visibility = Visibility.Hidden;
            lblPacientes.Visibility = Visibility.Hidden;
            PacienteGroup.Visibility = Visibility.Hidden;
            AnalisisTab.IsEnabled = false;
            RemediosTab.IsEnabled = false;
            ColorTab.IsEnabled = false;
            CategoriasTab.IsEnabled = false;
            tratamientos.IsEnabled = false;
        }


        //Funcion que detecta el folder del escritorio para ver si HS 4.9 esta instalado ahi..
        public void Detectar_FolderEscritorio()
        {
            //Obtiene la ruta del Escritorio de cada usuario..
            string strPath = Environment.GetFolderPath(
                         System.Environment.SpecialFolder.DesktopDirectory);
            //MessageBox.Show(strPath.ToString());

            //Ruta de la version HS 4.9
            string pathcompleta = strPath + "//Homoeonic Software Beta 49-4";

            //Comprueba si la path completa existe
            if (Directory.Exists(pathcompleta))
            {
                string ruta_images_HS49 = pathcompleta + "//radionica_db";

                //Comprueba si radionica_db existe sino no hace nada
                if (Directory.Exists(ruta_images_HS49))
                {
                    //Copiamos carpeta del 4.9 a nuestra ruta de instalacion HS5
                    Radionica objt = new Radionica();

                    //Copia carpeta fotos de radionica_db localizada en HS 4.9
                    objt.Copiar_Directorio(ruta_images_HS49, RutaInstalacion(), true);
                }
            }

        }

        private String CalcularTiempo_FormatoReloj(Int32 tsegundos)
        {
            Int32 horas = (tsegundos / 3600);
            Int32 minutos = ((tsegundos - horas * 3600) / 60);
            Int32 segundos = tsegundos - (horas * 3600 + minutos * 60);
            return horas.ToString() + " h, " + minutos.ToString() + " m";
        }

        //Listas de los elementos de necesarios para actualizar la duracion restante
        List<DateTime> Fechas_Diag_Activos = new List<DateTime>();
        List<bool> Banderas_Fechas_Activos = new List<bool>();

        //Paciente, Tratamiento (Nombre del tratamiento), inicio, duracion, tfaltante
        List<string> pacientes = new List<string>();
        List<string> nombre_tratamientos = new List<string>();
        List<string> inicio = new List<string>();
        List<string> duracion = new List<string>();
        List<string> tfaltantes = new List<string>();
        List<int> temitidos = new List<int>();

        static System.Windows.Forms.Timer _timer;  //Temporizador para actualizar la duracion de las fechas individuales.
        private bool temporizadorActivado = false;
        //Funcion de inicio del temporizador
        public void Empezar_Temporizador()
        {
            temporizadorActivado = true;
            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 1000;

            _timer.Enabled = true;
            _timer.Tick += new System.EventHandler(_timer_Elapsed);
        }
        private int contadorSegundos = 0;
        public void _timer_Elapsed(object sender, EventArgs e)
        {
            DateTime hora_actual = DateTime.Now;
            string nombretratamiento, tfaltante, nombretratamientoA, tfaltanteA;
            TimeSpan diff, diffA;
            int emitido;

            HacerConexion();
            if (cant_activos > 0)
            {
                ListadoDiagActivos.Items.Clear();
                DataTable Tratamientos_Activos = obj2.Tratamientos_Activos();
                for (int j = 0; j < Tratamientos_Activos.Rows.Count; j++)
                {
                    nombretratamientoA = Tratamientos_Activos.Rows[j][4].ToString();
                    diffA = Convert.ToDateTime(Tratamientos_Activos.Rows[j][7].ToString()) - hora_actual;
                    emitido = Int32.Parse(Tratamientos_Activos.Rows[j][6].ToString());

                    tfaltanteA = diffA.Days.ToString() + "d, " + diffA.Hours.ToString() + " h, " + diffA.Minutes.ToString() + " m, " + diffA.Seconds.ToString() + " s";

                    if (diffA.Minutes < 0 || diffA.Seconds < 0 || diffA.Hours < 0 || diffA.Days < 0)
                    {
                        obj2.Eliminar_TratamientoPasado(Tratamientos_Activos.Rows[j][0].ToString());
                        cant_activos--;
                        if (cant_activos == 0)
                        {
                            obj.BroadcastOFF();
                        }
                        Console.WriteLine("ELIMINADO " + Tratamientos_Activos.Rows[j][0].ToString());
                    }
                    else
                    {
                        ListadoDiagActivos.Items.Add(new nuevoTratamiento
                        {
                            paciente = Tratamientos_Activos.Rows[j][3].ToString()
                       ,
                            tratamiento = nombretratamientoA
                       ,
                            inicio = Tratamientos_Activos.Rows[j][7].ToString()
                       ,
                            duracion = CalcularTiempo_FormatoReloj(Int32.Parse(Tratamientos_Activos.Rows[j][5].ToString()))
                       ,
                            tfaltante = tfaltanteA
                        });


                        emitido = emitido + 1;

                        if (Tratamientos_Activos.Rows.Count > 0)
                        {
                            obj2.Registar_TiempoEmitido(emitido, Int32.Parse(Tratamientos_Activos.Rows[j][0].ToString()));
                        }

                    }
                }
            }
            else
            {
                obj.BroadcastOFF();
            }
            ListadoDiagNoActiv.Items.Clear();
            DataTable Tratamiento_Inactivos = obj2.Tratamientos_Inactivos();

            if (Tratamiento_Inactivos.Rows.Count > 0)
            {
                ListadoDiagInactivos.Items.Clear();
                for (int j = 0; j <= Tratamiento_Inactivos.Rows.Count - 1; j++)
                {
                    nombretratamiento = Tratamiento_Inactivos.Rows[j][4].ToString();

                    diff = Convert.ToDateTime(Tratamiento_Inactivos.Rows[j][7].ToString()) - hora_actual;
                    tfaltante = diff.Days.ToString() + "d, " + diff.Hours.ToString() + " h, " + diff.Minutes.ToString() + " m, " + diff.Seconds + " s";

                    int seg = Int32.Parse(Tratamiento_Inactivos.Rows[j][5].ToString());

                    if (diff.Minutes == 0 && diff.Seconds == 0 && diff.Hours == 0 && diff.Days == 0)
                    {
                        DateTime hora_actual2 = DateTime.Now;
                        DateTime Fecha_nueva;
                        Fecha_nueva = hora_actual2.AddSeconds(seg); //Nueva Fecha

                        obj2.ModificarFechaTratamiento(int.Parse(Tratamiento_Inactivos.Rows[j][0].ToString()), Fecha_nueva);

                        obj2.ModificarEstadoTratamientoActivo(Tratamiento_Inactivos.Rows[j][0].ToString());
                        Cargar_Tratamientos_Pendientes_Y_Activos();
                        break;
                    }
                    if (diff.Seconds > 0)
                    {
                        ListadoDiagNoActiv.Items.Add(new nuevoTratamiento
                        {
                            paciente = Tratamiento_Inactivos.Rows[j][3].ToString()
                                ,
                            tratamiento = nombretratamiento
                                ,
                            inicio = Tratamiento_Inactivos.Rows[j][7].ToString()
                                ,
                            duracion = CalcularTiempo_FormatoReloj(Int32.Parse(Tratamiento_Inactivos.Rows[j][5].ToString()))
                                ,
                            tfaltante = tfaltante
                        });
                    }
                }

            }
            CerrarConexion();
            contadorSegundos++;
        }
        int cant_activos;
        void Cargar_Tratamientos_Pendientes_Y_Activos()
        {
            try
            {
                HacerConexion();

                DataTable Tratamientos_Activos = obj2.Tratamientos_Activos();
                cant_activos = Tratamientos_Activos.Rows.Count;
                Console.WriteLine(cant_activos);
                if (cant_activos > 0)
                {
                    if (temporizadorActivado != true)
                    {
                        Empezar_Temporizador();
                    }
                    obj.BroadcastON();

                }
                ListadoDiagInactivos.Items.Clear();
                DataTable Tratamiento_Inactivos = obj2.Tratamientos_Inactivos();
                if (Tratamiento_Inactivos.Rows.Count > 0)
                {
                    if (temporizadorActivado != true)
                    {
                        Empezar_Temporizador();
                    }
                    DateTime hora_actual = DateTime.Now;
                    for (int j = 0; j < Tratamiento_Inactivos.Rows.Count; j++)
                    {
                        DateTime fechaInicioTratamientoInactivo = Convert.ToDateTime(Tratamiento_Inactivos.Rows[j][7]);
                        int resultadoComparacion = DateTime.Compare(DateTime.Now, fechaInicioTratamientoInactivo);
                        //Si la fecha ya ha pasado
                        if (resultadoComparacion > 0)
                        {
                            string idTratamiento = Tratamiento_Inactivos.Rows[j][0].ToString();
                            obj2.ModificarEstadoTratamientoVencido(idTratamiento);
                            Console.WriteLine("Entraron como pasados");
                        }
                        ////Es la misma fecha y hora
                        //else if(resultadoComparacion == 0)
                        //{
                        //    obj2.ModificarEstadoTratamientoActivo(Tratamiento_Inactivos.Rows[j][0].ToString());
                        //}
                        ////Si la fecha aun no ha pasado
                        else
                        {
                            Console.WriteLine("No estan pasados");
                            string nombretratamiento = Tratamiento_Inactivos.Rows[j][4].ToString();
                            TimeSpan diferenciaTiempoInicioActual = fechaInicioTratamientoInactivo - hora_actual;
                            string tiempoFaltanteString = diferenciaTiempoInicioActual.Days.ToString() + "d, " + diferenciaTiempoInicioActual.Hours.ToString() + " h, " + diferenciaTiempoInicioActual.Minutes.ToString() + " m";

                            ListadoDiagInactivos.Items.Add(new nuevoTratamiento
                            {
                                paciente = Tratamiento_Inactivos.Rows[j][3].ToString(),
                                tratamiento = nombretratamiento,
                                inicio = fechaInicioTratamientoInactivo.ToString(),
                                duracion = CalcularTiempo_FormatoReloj(Int32.Parse(Tratamiento_Inactivos.Rows[j][5].ToString())),
                                tfaltante = tiempoFaltanteString
                            });
                        }

                    }
                }
            }
            catch (Exception df)
            {
                MessageBox.Show(df.ToString());
            }
            finally
            {
                CerrarConexion();
            }
        }

        void IDs_maquinas_aceptados(string id)
        {
            switch (id)
            {
                case "395a-0s11-hj34-13%&-33w+W":
                    //MessageBox.Show("SOY UNA MANTRA!");
                    break;

                case "173c-ar22-hj33-18%@-21H+C":
                    break;

                case "284b-ar45-hj34-13%#-20w+Q":
                    break;

                default:
                    var window = Application.Current.Windows[0];
                    window.Close();
                    break;

            }
        }

        //Genera el documento en HS5 analisis 
        void GenerarDocumento(object sender, RoutedEventArgs e)
        {
            //Comprobamos si la version esta registrada
            HacerConexion();

            DataTable Version = obj2.Consultar_Version();
            //Campos para los datos de registro de la version
            string nombre = "";
            string descripcion = "";

            //Recorremos valores
            for (int i = 0; i <= Version.Rows.Count - 1; i++)
            {
                nombre = Version.Rows[i][1].ToString();
                descripcion = Version.Rows[i][2].ToString();
            }

            //En caso de que no este registrada
            if (Version.Rows.Count == 0)
            {
                nombre = "<REGISTRE VERSION PARA PERSONALIZAR>";
                descripcion = "<REGISTRE VERSION PARA PERSONALIZAR>";
            }

            CerrarConexion();
            CargarListadoCompletoPacientes();

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "PDF Document|*.pdf";
            saveFileDialog1.Title = "Save - Analysis Report";
            saveFileDialog1.ShowDialog();

            //Si se eligio ruta haz...
            if (saveFileDialog1.FileName != "")
            {
                Document reporte = new Document(iTextSharp.text.PageSize.LETTER, 40, 40, 80, 80);
                PdfWriter buffer = PdfWriter.GetInstance(reporte, new FileStream(saveFileDialog1.FileName.ToString(), FileMode.Create));
                //Mandamos datos del registro de version
                buffer.PageEvent = new HS5.reporte_ext("Analysis Report", nombre, descripcion); //Agrega el encabezado y pie de pagina

                reporte.Open();
                // reporte.AddTitle("Expediente del Paciente - HS5");
                //reporte.AddCreator("Homoeonic Software 5");
                // reporte.AddAuthor("HS5");

                iTextSharp.text.Font titulos = iTextSharp.text.FontFactory.GetFont("HELVETICA", 14, iTextSharp.text.Font.BOLD);
                iTextSharp.text.Font subtitulos = iTextSharp.text.FontFactory.GetFont("HELVETICA", 12, iTextSharp.text.Font.BOLD);
                iTextSharp.text.Font texto = iTextSharp.text.FontFactory.GetFont("HELVETICA", 10, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Font texto2 = iTextSharp.text.FontFactory.GetFont("HELVETICA", 10, iTextSharp.text.Font.BOLD);

                iTextSharp.text.Font LineBreak = iTextSharp.text.FontFactory.GetFont("HELVETICA", 10, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Paragraph linebreak = new iTextSharp.text.Paragraph("\n", LineBreak);

                //Documento titulo
                // iTextSharp.text.Paragraph parrafo = new iTextSharp.text.Paragraph("Expediente del Paciente", titulos);
                /// reporte.Add(parrafo);
                /// 
                reporte.Add(linebreak);
                reporte.Add(linebreak);

                //FONDO DEL DOCUMENTO
                string path = RutaInstalacion() + "//fotos//portada_hoja.png";

                //Fondo del documento
                iTextSharp.text.Image fondodoc = iTextSharp.text.Image.GetInstance(path);
                fondodoc.ScaleToFit(reporte.PageSize);
                fondodoc.Alignment = iTextSharp.text.Image.UNDERLYING;
                fondodoc.SetAbsolutePosition(0, 0);
                reporte.Add(fondodoc);

                iTextSharp.text.Paragraph parrafo2 = new iTextSharp.text.Paragraph("Personal Details", subtitulos);
                reporte.Add(parrafo2);

                Chunk parrafo3 = new Chunk("Analysis Name: ", texto2);
                Chunk parrafo3_1 = new Chunk(lblPacienteAnalisis_P1.Content.ToString(), texto);
                reporte.Add(parrafo3);
                reporte.Add(parrafo3_1);
                reporte.Add(Chunk.NEWLINE);

                Chunk parrafo4 = new Chunk("Patient's Name: ", texto2);
                Chunk parrafo4_1 = new Chunk(lblNombre_Anal1.Content.ToString(), texto);
                reporte.Add(parrafo4);
                reporte.Add(parrafo4_1);
                reporte.Add(Chunk.NEWLINE);

                Chunk parrafo5 = new Chunk("Date: ", texto2);
                Chunk parrafo5_1 = new Chunk(lblFechaAnalisis3.Content.ToString(), texto);
                reporte.Add(parrafo5);
                reporte.Add(parrafo5_1);

                /* Chunk parrafo6 = new Chunk("Fecha de impresión: ", texto2);
                 Chunk parrafo6_1 = new Chunk(DateTime.Now.ToString(), texto);
                 reporte.Add(parrafo6);
                 reporte.Add(parrafo6_1);
                 reporte.Add(linebreak);*/
                reporte.Add(linebreak);

                iTextSharp.text.Paragraph detalles = new iTextSharp.text.Paragraph("Overview", subtitulos);
                reporte.Add(detalles);
                reporte.Add(linebreak);

                //Agregar el contenido

                //Cantidad de columnas
                PdfPTable table_codigos = new PdfPTable(4);
                table_codigos.TotalWidth = 144;
                //table_codigos.AddCell(new Phrase("Código",texto2));
                table_codigos.AddCell(new Phrase("Name", texto2));
                table_codigos.AddCell(new Phrase("Value", texto2));
                table_codigos.AddCell(new Phrase("Levels", texto2));
                table_codigos.AddCell(new Phrase("S.Level", texto2));

                table_codigos.HeaderRows = 1;

                //Obtenemos valores del dataview
                IEnumerable items = this.ListaCodigos.Items;

                foreach (nuevoCodigo codigo in items)
                {
                    //ListaCodigos.Items.RemoveAt(ListaCodigos.Items.IndexOf(codigo.nombre.ToString()));
                    //table_codigos.AddCell(new Phrase(codigo.rates.ToString()));
                    table_codigos.AddCell(new Phrase(codigo.nombre.ToString()));
                    table_codigos.AddCell(new Phrase(codigo.ftester.ToString()));
                    table_codigos.AddCell(new Phrase(codigo.niveles.ToString()));
                    table_codigos.AddCell(new Phrase(codigo.nsugerido.ToString()));
                }

                reporte.Add(table_codigos);
                reporte.Close();

            }
        }

        //Funcion para eliminar un paciente del sistema
        private void cmdEliminar_Click(object sender, RoutedEventArgs e)
        {
            //Obtiene la fila elegida
            try
            {
                if (cmdEliminar.IsEnabled == false)
                {
                    MessageBox.Show(obtenerRecurso("messageInfo6"), "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {

                    MessageBoxButton buttons = MessageBoxButton.YesNo;

                    MessageBoxResult dialogResult = MessageBox.Show(obtenerRecurso("messageQuestion10"), obtenerRecurso("messageHead1"), buttons);
                    if (dialogResult == MessageBoxResult.Yes)
                    {

                        //Campo seleccionado con el mouse
                        DataRowView paciente_seleccionado = (DataRowView)ListaPacientes.SelectedItem;
                        string id_paciente = paciente_seleccionado[0].ToString();
                        HacerConexion(); //Abre conexion
                        obj2.EliminarPaciente(id_paciente); //Manda a eliminar el paciente de la bd
                        CerrarConexion(); //Cierra


                        //Borra posible foto
                        if (File.Exists(RutaInstalacion() + "\\fotos\\" + id_paciente + ".png"))
                        {
                            File.Delete(RutaInstalacion() + "\\fotos\\" + id_paciente + ".png");
                        }

                        MessageBox.Show(obtenerRecurso("messageInfo8"), "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {

                    }
                    CargarListadoCompletoPacientes();
                }
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(obtenerRecurso("messageError64"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        void Ocultar_TratamientoDirecto()
        {
            cmdCerrarBroadcast.Visibility = Visibility.Hidden;
            Broadcast.Visibility = Visibility.Hidden;
            cmdBorrarElemento.Visibility = Visibility.Hidden;
            comboPacientesTratamiento.Visibility = Visibility.Hidden;
            lblPaciente.Visibility = Visibility.Hidden;
            lblNombreTratamiento_Copy1.Visibility = Visibility.Hidden;
            comboTipoTratamiento.Visibility = Visibility.Hidden;
            lblNombreTratamiento_Copy.Visibility = Visibility.Hidden;
            listgenerico.Visibility = Visibility.Hidden;
            ProgramacionTrata.Visibility = Visibility.Hidden;
            lblNombreTratamiento_Copy2.Visibility = Visibility.Hidden;
            comboTipoProg.Visibility = Visibility.Hidden;
            lblNombreTratamiento_Copy3.Visibility = Visibility.Hidden;
            borderInfobasica_Copy1.Visibility = Visibility.Hidden;
            lblNombreTratamiento_Copy4.Visibility = Visibility.Hidden;
            dateProg.Visibility = Visibility.Hidden;
            lblNombreTratamiento_Copy5.Visibility = Visibility.Hidden;
            borderInfobasica_Copy10.Visibility = Visibility.Hidden;
            lblNombreTratamiento_Copy6.Visibility = Visibility.Hidden;
            txtHoras.Visibility = Visibility.Hidden;
            txtMinutos.Visibility = Visibility.Hidden;
            txtNombreTratamiento.Visibility = Visibility.Hidden;
            cmdIniciarTratamiento.Visibility = Visibility.Hidden;
            lblNombreTratamiento_Copy9.Visibility = Visibility.Hidden;
            txtNombreTratamiento_Copy.Visibility = Visibility.Hidden;
            Trata.Visibility = Visibility.Hidden;
            listCategoriasTrat.Visibility = Visibility.Hidden;
            listSubCategoriasTrat.Visibility = Visibility.Hidden;
            listCodigosTrat.Visibility = Visibility.Hidden;
            lblNombreTratamiento_Copy7.Visibility = Visibility.Hidden;
            txtcantidad1.Visibility = Visibility.Hidden;
            tiempo1.Visibility = Visibility.Hidden;
            lblNombreTratamiento_Copy8.Visibility = Visibility.Hidden;
            txtcantidad2.Visibility = Visibility.Hidden;
            tiempo2.Visibility = Visibility.Hidden;

            //Limpiar listas
            listgenerico.Items.Clear();
            listCategoriasTrat.Items.Clear();
            listSubCategoriasTrat.Items.Clear();
            listCodigosTrat.Items.Clear();
            cmdNuevoTratamiento.IsEnabled = true;
            cmdPausar.IsEnabled = true;
            cmdReanudar.IsEnabled = true;
            comboTipoTratamiento.SelectedIndex = -1;
            // comboTipoProg.SelectedIndex = -1;
            tiempo1.SelectedIndex = -1;

            //Panel de elementos agregados
            groupBox3_Copy2.Visibility = Visibility.Hidden;
            listelemagregados.Visibility = Visibility.Hidden;
            listelemagregados.Items.Clear(); //Limpia la lista de los elementos agregados...

            //Limpiando cajas de texto;
            txtcantidad1.Text = "";
            txtcantidad2.Text = "";
            txtHoras.Text = "";
            txtMinutos.Text = "";

        }

        void Mostrar_TratamientoDirecto()
        {
            cmdNuevoTratamiento.IsEnabled = false;
            cmdReanudar.IsEnabled = false;
            cmdPausar.IsEnabled = false;
            cmdBorrarElemento.Visibility = Visibility.Visible;
            cmdCerrarBroadcast.Visibility = Visibility.Visible;
            Broadcast.Visibility = Visibility.Visible;
            comboPacientesTratamiento.Visibility = Visibility.Visible;
            lblPaciente.Visibility = Visibility.Visible;
            lblNombreTratamiento_Copy1.Visibility = Visibility.Visible;
            comboTipoTratamiento.Visibility = Visibility.Visible;
            lblNombreTratamiento_Copy.Visibility = Visibility.Visible;
            listgenerico.Visibility = Visibility.Visible;
            ProgramacionTrata.Visibility = Visibility.Visible;
            lblNombreTratamiento_Copy2.Visibility = Visibility.Visible;
            comboTipoProg.Visibility = Visibility.Visible;
            lblNombreTratamiento_Copy3.Visibility = Visibility.Visible;
            borderInfobasica_Copy1.Visibility = Visibility.Visible;
            lblNombreTratamiento_Copy4.Visibility = Visibility.Visible;
            dateProg.Visibility = Visibility.Visible;
            lblNombreTratamiento_Copy5.Visibility = Visibility.Visible;
            borderInfobasica_Copy10.Visibility = Visibility.Visible;
            lblNombreTratamiento_Copy6.Visibility = Visibility.Visible;
            txtHoras.Visibility = Visibility.Visible;
            txtMinutos.Visibility = Visibility.Visible;
            txtNombreTratamiento.Visibility = Visibility.Visible;
            cmdIniciarTratamiento.Visibility = Visibility.Visible;
            lblNombreTratamiento_Copy9.Visibility = Visibility.Visible;
            txtNombreTratamiento_Copy.Visibility = Visibility.Visible;
            Trata.Visibility = Visibility.Visible;

            //Panel de elementos agregados
            groupBox3_Copy2.Visibility = Visibility.Visible;
            listelemagregados.Visibility = Visibility.Visible;
            //listelemagregados.Items.Add("NO HAY ELEMENTOS");

            //Llenar con pacientes el listado de pacientes
            comboPacientesTratamiento.Items.Clear();

            HacerConexion();

            //CARGA PACIENTES AL COMBOBOX
            DataTable pacientes = obj2.Mostrar_Pacientes_Listado_Sencillo();

            //Pasarlo al combo de pacientes en tratamiento directo
            for (int j = 0; j <= pacientes.Rows.Count - 1; j++)
            {
                comboPacientesTratamiento.Items.Add(pacientes.Rows[j][0].ToString() + " " + pacientes.Rows[j][1].ToString() + " " + pacientes.Rows[j][2].ToString());
            }

            CerrarConexion();

        }



        void cmdConfigurarBotonSuperior(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("HOLA");
        }

        private void cmdImagen_Click(object sender, RoutedEventArgs e)
        {
            //Seleccion
            OpenFileDialog FileSelect = new OpenFileDialog();
            FileSelect.Filter = "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";
            FileSelect.InitialDirectory = @"C:\";
            FileSelect.Title = "Select an image for your patient";
            if (FileSelect.ShowDialog() == true)
            {
                //user_img= new BitmapImage(new Uri(FileSelect.FileName)); //Usuario
                //image.Source = new BitmapImage(new Uri(FileSelect.FileName)); //Mostrar

            }
        }

        //Funcion de obtener PGR
        private void cmdObtenerPGR_Click(object sender, RoutedEventArgs e)
        {
            //Valida si se elegio un sexo del paciente
            if (optionSexoF.IsChecked == false && optionSexoM.IsChecked == false && optionSexoAn.IsChecked == false && optionSexoPl.IsChecked == false)
            {
                MessageBox.Show(obtenerRecurso("messageWarning15"), obtenerRecurso("messageHeadWarning"), MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                //Numero Random para los rangos del PGR
                Random num = new Random();
                
                //Mensaje de si hay una muestra en la maquina
                MessageBoxResult Result = MessageBox.Show(obtenerRecurso("messageQuestion7"), obtenerRecurso("messageHeadQ"), MessageBoxButton.YesNo, MessageBoxImage.Question);
        

                //Validacion
                switch (Result)
                {
                    //Respuesta SI
                    case MessageBoxResult.Yes:

                        progressBarAnimationPGR(3000, obtenerRecurso("progressBar2"));
                        new Thread((ThreadStart)delegate
                        {
                            //obj.Similie(); //DiagON
                            Thread.Sleep(3000);


                            Dispatcher.Invoke((ThreadStart)delegate
                            {
                                Radionica obj2 = new Radionica();

                                bool bandera = true; // 

                                //Validar si es humano o animal o suelo
                                if ((optionSexoM.IsChecked == true || optionSexoF.IsChecked == true) && bandera == true)//Humanos
                                {
                                    txtPGR.Text = obj2.RandomDigits(num.Next(16, 22));  //Llamada a funcion diagnostico (6 a 12 digitos)
                                }

                                if ((optionSexoAn.IsChecked == true || optionSexoPl.IsChecked == true) && bandera == true)///Animales o suelo
                                {
                                    txtPGR.Text = obj2.RandomDigits(num.Next(13, 22));  //Llamada a funcion diagnostico (3 a 7 digitos)
                                }
                            });

                        }).Start();   

                        break;

                    //Respuesta NO
                    case MessageBoxResult.No:
                        //Nothing
                        break;
                }
            }

        }

        //Funcion para registrar el paciente en la bd
        private void cmdGuardarPaciente_Click(object sender, RoutedEventArgs e)
        {
            cmdAgregarDom.IsEnabled = true;
            cmdEditarDom.Visibility = Visibility.Hidden;
            cmdEliminarDom.Visibility = Visibility.Hidden;

            //Determina el sexo del paciente y valida que se seleccione
            if (cmdModificar.IsEnabled == false)
            {
                string sexo2 = "";
                if (optionSexoM.IsChecked == true)
                {
                    sexo2 = optionSexoM.Content.ToString();
                }
                else
                {
                    if (optionSexoF.IsChecked == true)
                    {
                        sexo2 = optionSexoF.Content.ToString();
                    }
                    else
                    {
                        if (optionSexoAn.IsChecked == true)
                        {
                            sexo2 = optionSexoAn.Content.ToString();
                        }
                        else
                        {
                            if (optionSexoPl.IsChecked == true)
                            {
                                sexo2 = optionSexoPl.ToString();
                            }
                        }
                    }
                }

                //Validacion de campos nulos
                //   if (txtNombre.Text != "" && txtApellidoPat.Text != "" && txtApellidoMat.Text != "" && txtEmail.Text != "" && txtTitulo.Text != "" && txtPGR.Text != "" && combProfesion.Text != "" && txtFecha.Text != "")
                if (txtNombre.Text != "" && txtApellidoMat.Text != "" && txtPGR.Text != "")
                {
                    //Si hay conexion a la bd .. registra el paciente
                    if (HacerConexion() == true)
                    {
                        //Manda modificar el registro del paciente (Area de informacion general)
                        obj2.ModificarRegistroPaciente(id_paciente_global_modif, txtNombre.Text, txtApellidoPat.Text, txtApellidoMat.Text, txtEmail.Text, sexo2, combProfesion.Text, txtTitulo.Text, txtFecha.Text, txtPGR.Text);

                        //DOMICILIOS Y TELEFONOS SE MANEJAN INDEPENDIENTE ASI MISMO LOS ANTECEDENTES

                        try
                        {
                            //SUSTITUIR LA IMAGEN ASI SEA LA MISMA
                            //Ruta del folder "fotos" en folder de instalacion
                            string ruta = RutaInstalacion() + "//fotos//" + id_paciente_global_modif.ToString() + ".png";
                            File.Delete(ruta);
                            //Grabar en la ruta con el id_paciente como nombre y formato PNG
                            var encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create((BitmapSource)image.Source));
                            using (FileStream stream = new FileStream(ruta, FileMode.Create))
                                encoder.Save(stream);

                            image.Source = null; //Liberamos el control

                        }
                        catch (Exception)
                        {
                            // MessageBox.Show("The profile will be saved without a patient image included", "Information", MessageBoxButton.OK);
                        }

                        CerrarConexion();

                        //Limpiar todos los campos del registro
                        Limpiar_Campos();
                    }

                    MessageBox.Show(obtenerRecurso("messageWarning14"), obtenerRecurso("messageHeadWarning"), MessageBoxButton.OK, MessageBoxImage.Information);

                    //Al final de modificar
                    cmdGuardarPaciente.Content = obtenerRecurso("btnSaveP");
                    PacienteGroup.Header = obtenerRecurso("HeaderRP");
                    cmdGuardarPaciente.ToolTip = obtenerRecurso("HeaderRP");
                    SolidColorBrush brush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(60, 179, 223));
                    PacienteGroup.Background = brush;
                    tabControl.Foreground = brush;
                    cmdModificar.IsEnabled = true;
                    cmdEliminar.IsEnabled = true;

                }

            }
            else
            {                //Si se seleccione el sexo haz...

                //Validacion de campos nulos
                if (txtNombre.Text != "" && txtApellidoMat.Text != "" && txtPGR.Text != "" && txtFecha.Text != "")
                {
                    string sexo = "";
                    if (optionSexoM.IsChecked == true)
                    {
                        sexo = optionSexoM.Content.ToString();
                    }
                    else
                    {
                        if (optionSexoF.IsChecked == true)
                        {
                            sexo = optionSexoF.Content.ToString();
                        }
                        else
                        {
                            if (optionSexoAn.IsChecked == true)
                            {
                                sexo = optionSexoAn.Content.ToString();
                            }
                            else
                            {
                                if (optionSexoPl.IsChecked == true)
                                {
                                    sexo = optionSexoPl.ToString();
                                }
                                else
                                {
                                    MessageBox.Show(obtenerRecurso("mesageError63"), obtenerRecurso("messageHeadWarning"), MessageBoxButton.OK, MessageBoxImage.Warning);
                                }
                            }
                        }
                    }

                    bool Vali = HacerConexion();
                    object existe_paciente = obj2.Validar_IdPaciente(txtNombre.Text, txtApellidoPat.Text, txtApellidoMat.Text);

                    //Validar si ya esta registrado
                    if (existe_paciente.ToString() == "0")
                    {
                        //Credenciales
                        object id_paciente;

                        //Si hay conexion a la bd .. registra el paciente
                        if (Vali == true)
                        {
                            //Manda registrar el paciente (Area de informacion general)
                            id_paciente = obj2.RegistrarPacienteD(txtNombre.Text, txtApellidoPat.Text, txtApellidoMat.Text, txtEmail.Text, sexo, combProfesion.Text, txtTitulo.Text, txtFecha.Text, txtPGR.Text);

                            //Metodo de telefonos y antecedentes
                            Telefonos_y_Antecedentes(id_paciente);

                            // SALVAR IMAGEN DEL PACIENTE

                            //Ruta del folder "fotos" en folder de instalacion
                            string ruta = RutaInstalacion() + "//fotos//" + id_paciente.ToString() + ".png";

                            if (image.Source != null)
                            {
                                //Grabar en la ruta con el id_paciente como nombre y formato PNG
                                Console.WriteLine(image.Source);
                                var encoder = new PngBitmapEncoder();
                                encoder.Frames.Add(BitmapFrame.Create((BitmapSource)image.Source));
                                using (FileStream stream = new FileStream(ruta, FileMode.Create))
                                    encoder.Save(stream);

                                //Libera el control image
                                image.Source = null;
                            }
                        }
                        else
                        {
                            //Mensaje de error de conexion
                            MessageBox.Show(obtenerRecurso("messageError62"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }

                        CerrarConexion(); //Cerrar conexion

                        // MessageBox.Show("P!", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);

                        //Limpiar todos los campos del registro
                        Limpiar_Campos();

                        //Cargue listado de pacientes
                        CargarListadoCompletoPacientes();

                    }
                    else
                    {
                        //Advertencia
                        //MessageBox.Show("El paciente ya se encuentra registrado!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    }
                }
                else
                {
                    MessageBox.Show(obtenerRecurso("messageError3"), obtenerRecurso("messageHeadWarning"), MessageBoxButton.OK, MessageBoxImage.Warning);
                }

            }
        }



        //Funcion de Guardar en Memoria
        private void cmdGuardarMemoria_Click(object sender, RoutedEventArgs e)
        {
            if (txtPGR.Text == "")
            {
                MessageBox.Show(obtenerRecurso("messageError61"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {

                progressBarAnimationPGR(5000, obtenerRecurso("progressBar1"));
                new Thread((ThreadStart)delegate
                {
                    obj.Save();
                    Thread.Sleep(5000);
                    Dispatcher.Invoke((ThreadStart)delegate
                    {
                        MessageBox.Show(obtenerRecurso("messageInfo5"), "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    });

                }).Start();
            }
        }

        //Funcion de buscar el paciente
        private void cmdBuscar_Click(object sender, RoutedEventArgs e)
        {
            if (txtBuscarPaciente.Text != "")
            {
                HacerConexion();
                DataTable PacientesLista = new DataTable();
                string busqueda = txtBuscarPaciente.Text.ToUpper();
                PacientesLista = obj2.Buscar_Paciente(busqueda);
                ListaPacientes.ItemsSource = PacientesLista.DefaultView;
                CerrarConexion();
            }
            else
            {
                MessageBox.Show(obtenerRecurso("messageError36"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                txtBuscarPaciente.Focus();
            }
        }

        public bool HacerConexion()
        {
            //Credenciales y conexion a la bd
            string user = "postgres";
            string password = "radionica";
            return obj2.ConexionBD(user, password); //Variable de validacion
        }

        public void CerrarConexion()
        {
            obj2.CerrarBD(); //Cerrar conexion
        }

        private void cmdGuardarHeredo_Click(object sender, RoutedEventArgs e)
        {
            if (txtTituloHeredo.Text == "" || txtDescripcionHeredo.Text == "")
            {
                MessageBox.Show(obtenerRecurso("messageError3"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                txtTituloHeredo.Focus();
            }
            else
            {

                bool flag = false;
                //Verifica si esta ya en el heredo
                for (int j = 0; j <= ListaHeredoTitulos.Count - 1; j++)
                {
                    //Si el titulo y descripcion estan
                    if (ListaHeredoTitulos[j].ToString() == txtTituloHeredo.Text)
                    {
                        flag = true; //Si esta..
                    }
                }

                if (flag != true)
                {
                    if (cmdModificar.IsEnabled == false)
                    {
                        HacerConexion();
                        //Agrega un heredo a las listas independientes
                        ListaHeredoTitulos.Add(txtTituloHeredo.Text);
                        ListaHeredoDescrip.Add(txtDescripcionHeredo.Text);
                        ListadoHeredo.Items.Add(txtTituloHeredo.Text); //Agregar al listado
                        obj2.RegistrarAntecedentes(txtTituloHeredo.Text, txtDescripcionHeredo.Text, id_paciente_global_modif, "HF");
                        //Limpia los campos
                        txtTituloHeredo.Text = "";
                        txtDescripcionHeredo.Text = "";
                        txtTituloHeredo.Focus();

                        CerrarConexion();
                    }
                    else
                    {
                        ListadoHeredo.Items.Add(txtTituloHeredo.Text); //Agregar al listado

                        //Agrega un heredo a las listas independientes
                        ListaHeredoTitulos.Add(txtTituloHeredo.Text);
                        ListaHeredoDescrip.Add(txtDescripcionHeredo.Text);

                        //Limpia los campos
                        txtTituloHeredo.Text = "";
                        txtDescripcionHeredo.Text = "";
                        txtTituloHeredo.Focus();
                    }
                }
                else
                {
                    // MessageBox.Show("El heredo familiar ya esta incluido en el listado!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    txtTituloHeredo.Focus();
                }

            }
        }

        public void CargarListadoCompletoPacientes()
        {
            //Carga el listado de pacientes
            HacerConexion();
            DataTable ListadoPacientesCompleto = obj2.Listado_Pacientes();
            ListaPacientes.ItemsSource = ListadoPacientesCompleto.DefaultView;
            CerrarConexion();
        }

        public void CargarRegistrosPacientesRecientes()
        {
            //Carga el listado de pacientes
            HacerConexion();
            DataTable ListadoRecientesPacientes = obj2.Obtener_Analisis_Pacientes_Recientes();
            ListaPacientes_Recientes1.ItemsSource = ListadoRecientesPacientes.DefaultView;
            CerrarConexion();
        }

        private void txtBuscarPaciente_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtBuscarPaciente.Text != "")
            {
                HacerConexion();
                //Llama y obtiene posibles matches
                DataTable PacientesLista = new DataTable();
                PacientesLista = obj2.Buscar_Paciente(txtBuscarPaciente.Text);
                ListaPacientes.ItemsSource = PacientesLista.DefaultView;
                CerrarConexion();
            }
            else
            {
                //MessageBox.Show("Introduzca el nombre de un paciente a buscar", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                txtBuscarPaciente.Focus();
                CargarListadoCompletoPacientes();
            }
        }

        public void Telefonos_y_Antecedentes(object id_paciente)
        {
            //Telefonos
            //Recorrer todos los telefonos de las listas para agregar a la bd
            for (int k = 0; k <= listaTelefonos.Items.Count - 1; k++)
            {
                //Registrar telefono
                obj2.RegistrarTelefonos(ListaTelefonos[k].ToString(), ListaExtensiones[k].ToString(), id_paciente.ToString());
            }

            //Domicilios

            //Recorrer todos los domicilios del paciente y agregar a la bd
            for (int y = 0; y <= ListaCalles.Count - 1; y++)
            {
                //calle,numero,colonia,cp,municipio,estado,pai
                obj2.RegistrarDomicilios(ListaCalles[y], ListaNum[y], ListaColonia[y], ListaCP[y], ListaMunicipio[y], ListaEstado[y], ListaPais[y], id_paciente.ToString());
            }

            //Antecedentes

            //Recorrer todos los antecedentes de las listas para agregar a la bd  **Heredo familiar
            for (int j = 0; j <= ListaHeredoTitulos.Count - 1; j++)
            {
                //Registra antecedentes
                obj2.RegistrarAntecedentes(ListaHeredoTitulos[j].ToString(), ListaHeredoDescrip[j].ToString(), id_paciente.ToString(), "HF");
            }

            //Patologicos
            for (int g = 0; g <= ListaPatTitulos.Count - 1; g++)
            {
                //Registra antecedentes
                obj2.RegistrarAntecedentes(ListaPatTitulos[g].ToString(), ListaPatDescrip[g].ToString(), id_paciente.ToString(), "P");
            }

            //No Patologicos
            for (int i = 0; i <= ListaNoPatTitulos.Count - 1; i++)
            {
                //Registra antecedentes
                obj2.RegistrarAntecedentes(ListaNoPatTitulos[i].ToString(), ListaNoPatDescrip[i].ToString(), id_paciente.ToString(), "NP");
            }

            //Comentarios
            for (int m = 0; m <= ListaComentTitulos.Count - 1; m++)
            {
                //Registra antecedentes
                obj2.RegistrarAntecedentes(ListaComentTitulos[m].ToString(), ListaComentDescrip[m].ToString(), id_paciente.ToString(), "C");
            }

            //Analisis
            //Recorrer todos los analisis del paciente
            for (int k = 0; k <= ListaAnalisis.Count - 1; k++)
            {
                //Registrar telefono
                obj2.RegistrarAnalisisPaciente_Historial(id_paciente.ToString(), ListaAnalisis[k], ListaAnalisisFecha[k], "4");
            }
        }

        //Funcion para modificar el paciente
        private void cmdModificar_Click(object sender, RoutedEventArgs e)
        {

        }

        private void listaTelefonos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void cmdTelEliminar_Click(object sender, RoutedEventArgs e)
        {
            /*try
            {
                if (cmdModificar.IsEnabled == false)
                {
                    string eliminarelem = listaTelefonos.SelectedItem.ToString(); //Elemento elegido
                    HacerConexion(); //Conexion
                    for (int d = 0; d <= ListaTelefonos.Count - 1; d++)
                    {
                        if (listaTelefonos.SelectedItem.ToString() == ListaTelefonos[d].ToString() + " , Ext: " + ListaExtensiones[d].ToString())
                        {
                            //Eliminar numero de telefono
                            obj2.EliminarTelefonosPaciente(ListaTelefonos[d].ToString(), ListaExtensiones[d].ToString());

                            ListaTelefonos.RemoveAt(d);
                            ListaExtensiones.RemoveAt(d);
                            //MessageBox.Show("Llego");
                            listaTelefonos.Items.Remove(eliminarelem); //Remueve del listbox
                        }
                    }
                    CerrarConexion(); //Cerrar conexion
                }
                else
                {
                    string eliminarelem = listaTelefonos.SelectedItem.ToString(); //Elemento elegido
                    for (int d = 0; d <= ListaTelefonos.Count - 1; d++)
                    {
                        if (listaTelefonos.SelectedItem.ToString() == ListaTelefonos[d].ToString() + " , Ext: " + ListaExtensiones[d].ToString())
                        {
                            ListaTelefonos.RemoveAt(d);
                            ListaExtensiones.RemoveAt(d);
                            //MessageBox.Show("Llego");
                            listaTelefonos.Items.Remove(eliminarelem); //Remueve del listbox
                        }
                    }
                }
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("No se ha seleccionado ningun telefono a eliminar!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }*/
        }



        //Eliminar registro del heredo
        private void cmdEliminarHeredo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmdModificar.IsEnabled == false)
                {
                    HacerConexion();
                    for (int d = 0; d <= ListaHeredoTitulos.Count - 1; d++)
                    {
                        if (ListadoHeredo.SelectedItem.ToString() == ListaHeredoTitulos[d].ToString())
                        {
                            //Eliminar el elemento seleccionado (Titulo y descripcion)
                            ListadoHeredo.Items.Remove(ListadoHeredo.SelectedItem.ToString());
                            obj2.EliminarAntecedentesPaciente(ListaHeredoTitulos[d].ToString(), ListaHeredoDescrip[d].ToString(), "HF");
                            ListaHeredoTitulos.RemoveAt(d);
                            ListaHeredoDescrip.RemoveAt(d);
                        }
                    }
                    CerrarConexion();
                }
                else
                {
                    for (int d = 0; d <= ListaHeredoTitulos.Count - 1; d++)
                    {
                        if (ListadoHeredo.SelectedItem.ToString() == ListaHeredoTitulos[d].ToString())
                        {
                            //Eliminar el elemento seleccionado (Titulo y descripcion)
                            ListadoHeredo.Items.Remove(ListadoHeredo.SelectedItem.ToString());
                            ListaHeredoTitulos.RemoveAt(d);
                            ListaHeredoDescrip.RemoveAt(d);
                        }
                    }
                }
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(obtenerRecurso("messageError40"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        //Funcion de editar registro del heredo
        private void cmdEditarHeredo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmdModificar.IsEnabled == false)
                {
                    HacerConexion();
                    for (int d = 0; d <= ListaHeredoTitulos.Count - 1; d++)
                    {
                        if (ListadoHeredo.SelectedItem.ToString() == ListaHeredoTitulos[d].ToString())
                        {
                            //Eliminar el elemento seleccionado (Titulo y descripcion)
                            txtTituloHeredo.Text = ListaHeredoTitulos[d].ToString();
                            txtDescripcionHeredo.Text = ListaHeredoDescrip[d].ToString();

                            object id_antecedente = obj2.Obtener_IdAntecedente_Paciente(ListaHeredoTitulos[d].ToString(), ListaHeredoDescrip[d].ToString(), "HF");
                            obj2.EliminarAntecedentesPorIDPaciente(id_antecedente.ToString());

                            // MessageBox.Show(ListaHeredoDescrip[d].ToString());
                            ListadoHeredo.Items.Remove(ListadoHeredo.SelectedItem.ToString());
                            ListaHeredoTitulos.RemoveAt(d);
                            ListaHeredoDescrip.RemoveAt(d); //Elimina el previo
                        }
                    }
                    CerrarConexion();
                }
                else
                {
                    for (int d = 0; d <= ListaHeredoTitulos.Count - 1; d++)
                    {
                        if (ListadoHeredo.SelectedItem.ToString() == ListaHeredoTitulos[d].ToString())
                        {
                            //Eliminar el elemento seleccionado (Titulo y descripcion)
                            txtTituloHeredo.Text = ListaHeredoTitulos[d].ToString();
                            txtDescripcionHeredo.Text = ListaHeredoDescrip[d].ToString();
                            // MessageBox.Show(ListaHeredoDescrip[d].ToString());
                            ListadoHeredo.Items.Remove(ListadoHeredo.SelectedItem.ToString());
                            ListaHeredoTitulos.RemoveAt(d);
                            ListaHeredoDescrip.RemoveAt(d); //Elimina el previo
                        }
                    }
                }
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(obtenerRecurso("messageError60"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        private void cmdEliminarPatologicos_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmdModificar.IsEnabled == false)
                {
                    HacerConexion();
                    for (int d = 0; d <= ListaPatTitulos.Count - 1; d++)
                    {
                        if (listadoPatologicos.SelectedItem.ToString() == ListaPatTitulos[d].ToString())
                        {
                            //Eliminar el elemento seleccionado (Titulo y descripcion)
                            listadoPatologicos.Items.Remove(listadoPatologicos.SelectedItem.ToString());
                            obj2.EliminarAntecedentesPaciente(ListaPatTitulos[d].ToString(), ListaPatDescrip[d].ToString(), "P");
                            ListaPatTitulos.RemoveAt(d);
                            ListaPatDescrip.RemoveAt(d);
                        }
                    }
                    CerrarConexion();
                }
                else
                {
                    for (int d = 0; d <= ListaPatTitulos.Count - 1; d++)
                    {
                        if (listadoPatologicos.SelectedItem.ToString() == ListaPatTitulos[d].ToString())
                        {
                            //Eliminar el elemento seleccionado (Titulo y descripcion)
                            listadoPatologicos.Items.Remove(listadoPatologicos.SelectedItem.ToString());
                            ListaPatTitulos.RemoveAt(d);
                            ListaPatDescrip.RemoveAt(d);
                        }
                    }
                }
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(obtenerRecurso("messageError40"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        private void cmdEditarPatologico_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmdModificar.IsEnabled == false)
                {
                    HacerConexion();
                    for (int d = 0; d <= ListaPatTitulos.Count - 1; d++)
                    {
                        if (listadoPatologicos.SelectedItem.ToString() == ListaPatTitulos[d].ToString())
                        {
                            //Eliminar el elemento seleccionado (Titulo y descripcion)
                            txtTituloPat.Text = ListaPatTitulos[d].ToString();
                            txtDescripPat.Text = ListaPatDescrip[d].ToString();
                            // MessageBox.Show(ListaHeredoDescrip[d].ToString());
                            object id_antecendente = obj2.Obtener_IdAntecedente_Paciente(ListaPatTitulos[d].ToString(), ListaPatDescrip[d].ToString(), "P");
                            obj2.EliminarAntecedentesPorIDPaciente(id_antecendente.ToString());
                            listadoPatologicos.Items.Remove(listadoPatologicos.SelectedItem.ToString());
                            ListaPatTitulos.RemoveAt(d);
                            ListaPatDescrip.RemoveAt(d); //Elimina el previo
                        }
                    }
                    CerrarConexion();
                }
                else
                {
                    for (int d = 0; d <= ListaPatTitulos.Count - 1; d++)
                    {
                        if (listadoPatologicos.SelectedItem.ToString() == ListaPatTitulos[d].ToString())
                        {
                            //Eliminar el elemento seleccionado (Titulo y descripcion)
                            txtTituloPat.Text = ListaPatTitulos[d].ToString();
                            txtDescripPat.Text = ListaPatDescrip[d].ToString();
                            // MessageBox.Show(ListaHeredoDescrip[d].ToString());
                            listadoPatologicos.Items.Remove(listadoPatologicos.SelectedItem.ToString());
                            ListaPatTitulos.RemoveAt(d);
                            ListaPatDescrip.RemoveAt(d); //Elimina el previo
                        }
                    }
                }
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(obtenerRecurso("messageError60"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        private void cmdGuardarPat_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cmdEliminarNoPat_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmdModificar.IsEnabled == false)
                {
                    HacerConexion();
                    for (int d = 0; d <= ListaNoPatTitulos.Count - 1; d++)
                    {
                        if (listadoNoPatologicos.SelectedItem.ToString() == ListaNoPatTitulos[d].ToString())
                        {
                            //Eliminar el elemento seleccionado (Titulo y descripcion)
                            listadoNoPatologicos.Items.Remove(listadoNoPatologicos.SelectedItem.ToString());
                            obj2.EliminarAntecedentesPaciente(ListaNoPatTitulos[d].ToString(), ListaNoPatDescrip[d].ToString(), "NP");
                            ListaNoPatTitulos.RemoveAt(d);
                            ListaNoPatDescrip.RemoveAt(d);
                        }
                    }
                    CerrarConexion();
                }
                else
                {
                    for (int d = 0; d <= ListaNoPatTitulos.Count - 1; d++)
                    {
                        if (listadoNoPatologicos.SelectedItem.ToString() == ListaNoPatTitulos[d].ToString())
                        {
                            //Eliminar el elemento seleccionado (Titulo y descripcion)
                            listadoNoPatologicos.Items.Remove(listadoNoPatologicos.SelectedItem.ToString());
                            ListaNoPatTitulos.RemoveAt(d);
                            ListaNoPatDescrip.RemoveAt(d);
                        }
                    }
                }
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(obtenerRecurso("messageError40"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        private void cmdEditarNoPat_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmdModificar.IsEnabled == false)
                {
                    HacerConexion();
                    for (int d = 0; d <= ListaNoPatTitulos.Count - 1; d++)
                    {
                        if (listadoNoPatologicos.SelectedItem.ToString() == ListaNoPatTitulos[d].ToString())
                        {
                            //Eliminar el elemento seleccionado (Titulo y descripcion)
                            txtTituloNoPat.Text = ListaNoPatTitulos[d].ToString();
                            txtDescripNoPat.Text = ListaNoPatDescrip[d].ToString();
                            // MessageBox.Show(ListaHeredoDescrip[d].ToString());
                            listadoNoPatologicos.Items.Remove(listadoNoPatologicos.SelectedItem.ToString());
                            object id_antecedente = obj2.Obtener_IdAntecedente_Paciente(ListaNoPatTitulos[d].ToString(), ListaNoPatDescrip[d].ToString(), "NP");
                            obj2.EliminarAntecedentesPorIDPaciente(id_antecedente.ToString());
                            ListaNoPatTitulos.RemoveAt(d);
                            ListaNoPatDescrip.RemoveAt(d); //Elimina el previo
                        }
                    }
                    CerrarConexion();
                }
                else
                {
                    for (int d = 0; d <= ListaNoPatTitulos.Count - 1; d++)
                    {
                        if (listadoNoPatologicos.SelectedItem.ToString() == ListaNoPatTitulos[d].ToString())
                        {
                            //Eliminar el elemento seleccionado (Titulo y descripcion)
                            txtTituloNoPat.Text = ListaNoPatTitulos[d].ToString();
                            txtDescripNoPat.Text = ListaNoPatDescrip[d].ToString();
                            // MessageBox.Show(ListaHeredoDescrip[d].ToString());
                            listadoNoPatologicos.Items.Remove(listadoNoPatologicos.SelectedItem.ToString());
                            ListaNoPatTitulos.RemoveAt(d);
                            ListaNoPatDescrip.RemoveAt(d); //Elimina el previo
                        }
                    }
                }
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(obtenerRecurso("messageError60"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        private void cmdEliminarComent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmdModificar.IsEnabled == false)
                {
                    HacerConexion();
                    for (int d = 0; d <= ListaComentTitulos.Count - 1; d++)
                    {
                        if (listadoComentarios.SelectedItem.ToString() == ListaComentTitulos[d].ToString())
                        {
                            //Eliminar el elemento seleccionado (Titulo y descripcion)
                            listadoComentarios.Items.Remove(listadoComentarios.SelectedItem.ToString());
                            obj2.EliminarAntecedentesPaciente(ListaComentTitulos[d].ToString(), ListaComentDescrip[d].ToString(), "C");
                            ListaComentTitulos.RemoveAt(d);
                            ListaComentDescrip.RemoveAt(d);
                        }
                    }
                    CerrarConexion();
                }
                else
                {
                    for (int d = 0; d <= ListaComentTitulos.Count - 1; d++)
                    {
                        if (listadoComentarios.SelectedItem.ToString() == ListaComentTitulos[d].ToString())
                        {
                            //Eliminar el elemento seleccionado (Titulo y descripcion)
                            listadoComentarios.Items.Remove(listadoComentarios.SelectedItem.ToString());
                            ListaComentTitulos.RemoveAt(d);
                            ListaComentDescrip.RemoveAt(d);
                        }
                    }
                }
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(obtenerRecurso("messageError40"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        private void cmdEditarComent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmdModificar.IsEnabled == false)
                {
                    HacerConexion();
                    for (int d = 0; d <= ListaComentTitulos.Count - 1; d++)
                    {
                        if (listadoComentarios.SelectedItem.ToString() == ListaComentTitulos[d].ToString())
                        {
                            //Eliminar el elemento seleccionado (Titulo y descripcion)
                            txtTituloComent.Text = ListaComentTitulos[d].ToString();
                            txtDescripComent.Text = ListaComentDescrip[d].ToString();
                            // MessageBox.Show(ListaHeredoDescrip[d].ToString());

                            object id_antecedente = obj2.Obtener_IdAntecedente_Paciente(ListaComentTitulos[d].ToString(), ListaComentDescrip[d].ToString(), "C");
                            obj2.EliminarAntecedentesPorIDPaciente(id_antecedente.ToString());

                            listadoComentarios.Items.Remove(listadoComentarios.SelectedItem.ToString());
                            ListaComentTitulos.RemoveAt(d);
                            ListaComentDescrip.RemoveAt(d); //Elimina el previo
                        }
                    }
                    CerrarConexion();
                }
                else
                {
                    for (int d = 0; d <= ListaComentTitulos.Count - 1; d++)
                    {
                        if (listadoComentarios.SelectedItem.ToString() == ListaComentTitulos[d].ToString())
                        {
                            //Eliminar el elemento seleccionado (Titulo y descripcion)
                            txtTituloComent.Text = ListaComentTitulos[d].ToString();
                            txtDescripComent.Text = ListaComentDescrip[d].ToString();
                            // MessageBox.Show(ListaHeredoDescrip[d].ToString());
                            listadoComentarios.Items.Remove(listadoComentarios.SelectedItem.ToString());
                            ListaComentTitulos.RemoveAt(d);
                            ListaComentDescrip.RemoveAt(d); //Elimina el previo
                        }
                    }
                }
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(obtenerRecurso("messageError60"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        private void cmdGuardarPat_Click_1(object sender, RoutedEventArgs e)
        {
            if (txtTituloPat.Text == "" || txtDescripPat.Text == "")
            {
                MessageBox.Show(obtenerRecurso("messageError3"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                txtTituloPat.Focus();
            }
            else
            {
                bool flag = false;
                //Verifica si esta ya en el patologicos
                for (int j = 0; j <= ListaPatTitulos.Count - 1; j++)
                {
                    //Si el titulo y descripcion estan
                    if (ListaPatTitulos[j].ToString() == txtTituloPat.Text)
                    {
                        flag = true; //Si esta..
                    }
                }

                if (flag != true)
                {
                    if (cmdModificar.IsEnabled == false)
                    {
                        HacerConexion();
                        listadoPatologicos.Items.Add(txtTituloPat.Text); //Agregar al listado

                        //Agrega un heredo a las listas independientes
                        ListaPatTitulos.Add(txtTituloPat.Text);
                        ListaPatDescrip.Add(txtDescripPat.Text);

                        //Agregar a la bd
                        obj2.RegistrarAntecedentes(txtTituloPat.Text, txtDescripPat.Text, id_paciente_global_modif, "P");

                        //Limpia los campos
                        txtTituloPat.Text = "";
                        txtDescripPat.Text = "";
                        txtTituloPat.Focus();
                        CerrarConexion();
                    }
                    else
                    {
                        listadoPatologicos.Items.Add(txtTituloPat.Text); //Agregar al listado

                        //Agrega un heredo a las listas independientes
                        ListaPatTitulos.Add(txtTituloPat.Text);
                        ListaPatDescrip.Add(txtDescripPat.Text);

                        //Limpia los campos
                        txtTituloPat.Text = "";
                        txtDescripPat.Text = "";
                        txtTituloPat.Focus();
                    }
                }
                else
                {
                    MessageBox.Show(obtenerRecurso("messageError58"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    txtTituloPat.Focus();
                }
            }
        }

        private void cmdGuardarNoPat_Click(object sender, RoutedEventArgs e)
        {
            if (txtTituloNoPat.Text == "" || txtDescripNoPat.Text == "")
            {
                MessageBox.Show(obtenerRecurso("messageError3"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                txtTituloNoPat.Focus();
            }
            else
            {

                bool flag = false;
                //Verifica si esta ya en el patologicos
                for (int j = 0; j <= ListaNoPatTitulos.Count - 1; j++)
                {
                    //Si el titulo y descripcion estan
                    if (ListaNoPatTitulos[j].ToString() == txtTituloNoPat.Text)
                    {
                        flag = true; //Si esta..
                    }
                }

                if (flag != true)
                {
                    if (cmdModificar.IsEnabled == false)
                    {
                        HacerConexion();
                        listadoNoPatologicos.Items.Add(txtTituloNoPat.Text); //Agregar al listado

                        //Agrega un heredo a las listas independientes
                        ListaNoPatTitulos.Add(txtTituloNoPat.Text);
                        ListaNoPatDescrip.Add(txtDescripNoPat.Text);

                        obj2.RegistrarAntecedentes(txtTituloNoPat.Text, txtDescripNoPat.Text, id_paciente_global_modif, "NP");

                        //Limpia los campos
                        txtTituloNoPat.Text = "";
                        txtDescripNoPat.Text = "";
                        txtTituloNoPat.Focus();
                        CerrarConexion();
                    }
                    else
                    {
                        listadoNoPatologicos.Items.Add(txtTituloNoPat.Text); //Agregar al listado

                        //Agrega un heredo a las listas independientes
                        ListaNoPatTitulos.Add(txtTituloNoPat.Text);
                        ListaNoPatDescrip.Add(txtDescripNoPat.Text);

                        //Limpia los campos
                        txtTituloNoPat.Text = "";
                        txtDescripNoPat.Text = "";
                        txtTituloNoPat.Focus();
                    }
                }
                else
                {
                    MessageBox.Show("El registro no patologico ya esta incluido en el listado!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    txtTituloNoPat.Focus();
                }
            }
        }

        private void cmdGuardarComent_Click(object sender, RoutedEventArgs e)
        {
            if (txtTituloComent.Text == "" || txtDescripComent.Text == "")
            {
                //obtenerRecurso("messagerError58")
                MessageBox.Show(obtenerRecurso("messageError58"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                txtTituloComent.Focus();
            }
            else
            {

                bool flag = false;
                //Verifica si esta ya en el patologicos
                for (int j = 0; j <= ListaComentTitulos.Count - 1; j++)
                {
                    //Si el titulo y descripcion estan
                    if (ListaComentTitulos[j].ToString() == txtTituloComent.Text)// && ListaHeredoDescrip[j].ToString() == txtDescripcionHeredo.Text)
                    {
                        flag = true; //Si esta..
                    }
                }

                if (flag != true)
                {
                    if (cmdModificar.IsEnabled == false)
                    {
                        HacerConexion();
                        listadoComentarios.Items.Add(txtTituloComent.Text); //Agregar al listado

                        //Agrega un heredo a las listas independientes
                        ListaComentTitulos.Add(txtTituloComent.Text);
                        ListaComentDescrip.Add(txtDescripComent.Text);

                        obj2.RegistrarAntecedentes(txtTituloComent.Text, txtDescripComent.Text, id_paciente_global_modif, "C");

                        //Limpia los campos
                        txtTituloComent.Text = "";
                        txtDescripComent.Text = "";
                        txtTituloComent.Focus();
                        CerrarConexion();
                    }
                    else
                    {
                        listadoComentarios.Items.Add(txtTituloComent.Text); //Agregar al listado

                        //Agrega un heredo a las listas independientes
                        ListaComentTitulos.Add(txtTituloComent.Text);
                        ListaComentDescrip.Add(txtDescripComent.Text);

                        //Limpia los campos
                        txtTituloComent.Text = "";
                        txtDescripComent.Text = "";
                        txtTituloComent.Focus();
                    }
                }
                else
                {
                    MessageBox.Show("El comentario ya esta incluido en el listado!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    txtTituloComent.Focus();
                }
            }
        }

        //Funcion de crear un nuevo analisis
        private void cmdAnalisis_Click(object sender, RoutedEventArgs e)
        {
            //Valor de la caja
            string nombre_analisis;

            if (txtNombre.Text == "" || txtApellidoPat.Text == "")
            {
                MessageBox.Show(obtenerRecurso("messageError57"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                tabControl.SelectedIndex = 0; //Va a tab info general del paciente
                txtNombre.Focus();
            }
            else
            {

                nombre_analisis = Interaction.InputBox(obtenerRecurso("messageQuestion6"), obtenerRecurso("messageHeadQ5"), "", 300, 300);

                if (nombre_analisis == "")
                {
                    MessageBox.Show(obtenerRecurso("messageError9"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    //Comprobar si el analisis ya esta agregado para elegir otro nombre
                    bool flag = false;

                    //Ciclo de la lista de analisis
                    for (int q = 0; q <= ListaAnalisis.Count - 1; q++)
                    {
                        if (nombre_analisis == ListaAnalisis[q]) //Nombre es igual al nombre del listado
                        {
                            flag = true; //Si esta en lista
                            break;
                        }
                    }

                    if (flag != true)
                    {
                        bool val = HacerConexion(); //Variable de validacion

                        if (val == true)
                        {
                            string date;
                            date = DateTime.Now.ToString();
                            if (cmdModificar.IsEnabled == false)
                            {
                                obj2.RegistrarAnalisisPaciente_Historial(id_paciente_global_modif, nombre_analisis, date, "4");
                                //Agregar al listbox
                                listadoAnalisis.Items.Add(nombre_analisis + " , " + date); //listbox
                            }
                        }
                        else
                        {
                            MessageBox.Show("Error al abrir la BD!", "Error de comunicacion", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        CerrarConexion();
                    }
                    else
                    {
                        MessageBox.Show(obtenerRecurso("messageError56"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        //Funcion para buscar analisis dentro del paciente seccion
        private void cmdBuscarAnalisis_Click(object sender, RoutedEventArgs e)
        {
            if (cmdModificar.IsEnabled == false)
            {
                //Checar si la caja esta vacia
                if (txtBusquedaAnalisis.Text != "")
                {
                    bool val = HacerConexion(); //Variable de validacion
                    object validar_analisis = obj2.Validar_Analisis(id_paciente_global_modif, txtBusquedaAnalisis.Text);

                    if (val == true)
                    {
                        if (validar_analisis.ToString() != "0") //No esta en base
                        {
                            DataTable Analisis_Paciente = obj2.BuscarAnalisisPaciente(id_paciente_global_modif, txtBusquedaAnalisis.Text);

                            //mostrar en el listbox la busqueda
                            listadoAnalisis.Items.Add(Analisis_Paciente.Rows[0][2].ToString() + " , " + Analisis_Paciente.Rows[0][3].ToString()); //Agarra el nombre del analisis buscado

                            //Para listview
                            DataTable BusquedaAnalisisCompleto = obj2.HistorialAnalisisPacienteBuscado(Analisis_Paciente.Rows[0][1].ToString()); //Manda id del analisis para traer info completa
                            historialAnalisis.ItemsSource = BusquedaAnalisisCompleto.DefaultView;
                        }
                        else
                        {
                            MessageBox.Show("El analisis - " + txtBusquedaAnalisis.Text + " - del paciente " + txtNombre.Text + " no esta registrado!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            txtBusquedaAnalisis.Focus();
                        }

                    }
                    else
                    {
                        MessageBox.Show("Error al abrir la BD!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    CerrarConexion();
                }
                else
                {
                    //Mostrar mensaje
                    MessageBox.Show(obtenerRecurso("messageError55"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    txtBusquedaAnalisis.Focus();
                }
            }
            else
            {
                MessageBox.Show(obtenerRecurso("messageError54"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        private void ListadoHeredo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void listadoAnalisis_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (modif != false)
            {
                //Para listview agrega los valores de acuerdo al elemento seleccionado del listbox
                for (int k = 0; k <= Historial_Analisis_Paciente.Rows.Count - 1; k++) //Para encontrar el id dentro del historial analisis paciente datatable
                {
                    if (listadoAnalisis.SelectedItem.ToString() == Historial_Analisis_Paciente.Rows[k][2].ToString() + " , " + Historial_Analisis_Paciente.Rows[k][3].ToString())
                    {
                        // Para listview
                        DataTable BusquedaAnalisisCompleto = obj2.HistorialAnalisisPacienteBuscado(Historial_Analisis_Paciente.Rows[k][1].ToString()); //Manda id del analisis para traer info completa
                        historialAnalisis.ItemsSource = BusquedaAnalisisCompleto.DefaultView;
                    }
                }
            }
        }

        //Funcion limpiar campos en el programa
        public void Limpiar_Campos()
        {
            //Info general
            txtNombre.Clear();
            txtApellidoPat.Clear();
            txtApellidoMat.Clear();
            txtFecha.Text = "";
            txtEmail.Clear();
            txtTitulo.Clear();
            combProfesion.SelectedIndex = -1;
            optionSexoAn.IsChecked = false;
            optionSexoF.IsChecked = false;
            optionSexoM.IsChecked = false;
            optionSexoPl.IsChecked = false;
            txtPGR.Clear();
            txtTelefonos.Clear();
            txtExtensiones.Clear();
            listaTelefonos.Items.Clear();
            ListaTelefonos.Clear();

            //Domicilios
            txtCalle.Clear();
            txtNum.Clear();
            txtColonia.Clear();
            txtCP.Clear();
            txtMunicipio.Clear();
            txtEstado.Clear();
            txtCountry.Clear();
            ListaCalles.Clear();
            ListaNum.Clear();
            ListaColonia.Clear();
            ListaMunicipio.Clear();
            ListaCP.Clear();
            ListaEstado.Clear();
            ListaPais.Clear();
            listadodomicilios.Items.Clear();

            //Antecedentes
            txtTituloHeredo.Clear();
            txtDescripcionHeredo.Clear();
            ListaHeredoTitulos.Clear();
            ListaHeredoDescrip.Clear();
            ListadoHeredo.Items.Clear();

            txtTituloPat.Clear();
            txtDescripPat.Clear();
            ListaPatTitulos.Clear();
            ListaPatDescrip.Clear();
            listadoPatologicos.Items.Clear();

            txtTituloNoPat.Clear();
            txtDescripNoPat.Clear();
            ListaNoPatTitulos.Clear();
            ListaNoPatDescrip.Clear();
            listadoNoPatologicos.Items.Clear();

            txtTituloComent.Clear();
            txtDescripComent.Clear();
            ListaComentTitulos.Clear();
            ListaComentDescrip.Clear();
            listadoComentarios.Items.Clear();

            //Historial
            listadoAnalisis.Items.Clear();
            historialAnalisis.Items.Clear();

        }

        private void cmdDomiclioAgregar_Click(object sender, RoutedEventArgs e)
        {
        }

        private void cmdAgregarTelef_Click(object sender, RoutedEventArgs e)
        {
        }

        private void ListaPacientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Si modificar esta activado mostrar mensaje
            if (cmdModificar.IsEnabled == false && txtNombre.Text != "")
            {
                MessageBox.Show(obtenerRecurso("messageWarning13"), obtenerRecurso("messageHeadWarning"), MessageBoxButton.OK, MessageBoxImage.Exclamation);

            }
        }


        private void cmdMasDomicilios_Click_1(object sender, RoutedEventArgs e)
        {
            /* if (txtCalle.Text == "" || txtNum.Text == "" || txtColonia.Text == "" || txtCP.Text == "" || txtMunicipio.Text == "" || txtEstado.Text == "" || txtCountry.Text == "")
             {
                 MessageBox.Show("Primero complete el domicilio principal, antes de agregar mas domicilios!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                 txtCalle.Focus();
             }
             else
             {
                 domicilios.Visibility = Visibility.Visible;
                 domicilios.IsSelected = true;
             }*/
        }

        private void optionSexoM_Checked(object sender, RoutedEventArgs e)
        {

        }


        private void cmdAgregarTelef_Click_1(object sender, RoutedEventArgs e)
        {
            bool Vali = false;
            if (cmdModificar.IsEnabled == false)
            {
                //Comprueba si ya esta
                for (int d = 0; d <= ListaTelefonos.Count - 1; d++)
                {
                    if (txtTelefonos.Text + " , Ext: " + txtExtensiones.Text == ListaTelefonos[d].ToString() + " , Ext: " + ListaExtensiones[d].ToString())
                    {
                        MessageBox.Show(obtenerRecurso("messageWarning12"), obtenerRecurso("messageHeadWarning"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        txtTelefonos.Focus();
                        Vali = true;
                    }
                }

                if (Vali != true)
                {
                    if (txtTelefonos.Text != "")
                    {
                        HacerConexion();
                        if (txtExtensiones.Text == "")
                        {
                            listaTelefonos.Items.Add(txtTelefonos.Text + " , Ext: NA");
                            // ListaTelefonos.Add(txtTelefonos.Text);
                            //  ListaExtensiones.Add("NA");
                            //Agregar un telefono al paciente
                            obj2.RegistrarTelefonos(txtTelefonos.Text, "NA", id_paciente_global_modif);
                            txtTelefonos.Text = "";
                            txtExtensiones.Text = "";
                            txtTelefonos.Focus();
                        }
                        else
                        {
                            listaTelefonos.Items.Add(txtTelefonos.Text + " , Ext: " + txtExtensiones.Text);
                            //Agregar un telefono al paciente

                            obj2.RegistrarTelefonos(txtTelefonos.Text, txtExtensiones.Text, id_paciente_global_modif);
                            txtTelefonos.Text = "";
                            txtExtensiones.Text = "";
                            txtTelefonos.Focus();
                        }
                        CerrarConexion();
                    }
                    else
                    {
                        MessageBox.Show(obtenerRecurso("messageError53"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        txtTelefonos.Focus();
                    }
                }
                else
                {
                    MessageBox.Show(obtenerRecurso("messageWarning12"), obtenerRecurso("messageHeadWarning"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    txtTelefonos.Focus();
                }
            }
            else
            {
                if (txtTelefonos.Text != "")
                {
                    //Comprueba si ya esta
                    for (int d = 0; d <= ListaTelefonos.Count - 1; d++)
                    {
                        if (txtTelefonos.Text + " , Ext: " + txtExtensiones.Text == ListaTelefonos[d].ToString() + " , Ext: " + ListaExtensiones[d].ToString())
                        {
                            MessageBox.Show(obtenerRecurso("messageWarning12"), obtenerRecurso("messageHeadWarning"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            txtTelefonos.Focus();
                            Vali = true;
                        }
                    }

                    if (txtExtensiones.Text == "")
                    {
                        listaTelefonos.Items.Add(txtTelefonos.Text + " , Ext: NA");
                        ListaTelefonos.Add(txtTelefonos.Text);
                        ListaExtensiones.Add("NA");
                        txtTelefonos.Text = "";
                        txtExtensiones.Text = "";
                        txtTelefonos.Focus();
                    }
                    else
                    {

                        if (Vali == false)
                        {
                            listaTelefonos.Items.Add(txtTelefonos.Text + " , Ext: " + txtExtensiones.Text);
                            ListaTelefonos.Add(txtTelefonos.Text);
                            ListaExtensiones.Add(txtExtensiones.Text);
                            txtTelefonos.Text = "";
                            txtExtensiones.Text = "";
                            txtTelefonos.Focus();
                        }
                    }
                }
                else
                {
                    MessageBox.Show(obtenerRecurso("messageError53"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    txtTelefonos.Focus();
                }
            }
        }

        private void cmdEliminarTelef_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmdModificar.IsEnabled == false)
                {
                    HacerConexion(); //Conexion
                    for (int d = 0; d <= ListaTelefonos.Count - 1; d++)
                    {
                        if (listaTelefonos.SelectedItem.ToString() == ListaTelefonos[d].ToString() + " , Ext: " + ListaExtensiones[d].ToString())
                        {
                            //Cargar en cajas de texto
                            object telefono_editar = obj2.Obtener_IdTelefono_Paciente(ListaTelefonos[d].ToString(), ListaExtensiones[d].ToString());
                            obj2.EliminarTelefonosPacientePorID(telefono_editar.ToString()); //Elimina por ID

                            listaTelefonos.Items.Remove(listaTelefonos.SelectedItem.ToString());
                            ListaTelefonos.RemoveAt(d);
                            ListaExtensiones.RemoveAt(d);
                        }
                    }
                    CerrarConexion();//Cerrar conexion
                }
                else
                {
                    for (int d = 0; d <= ListaTelefonos.Count - 1; d++)
                    {
                        if (listaTelefonos.SelectedItem.ToString() == ListaTelefonos[d].ToString() + " , Ext: " + ListaExtensiones[d].ToString())
                        {
                            listaTelefonos.Items.Remove(listaTelefonos.SelectedItem.ToString());
                            ListaTelefonos.RemoveAt(d);
                            ListaExtensiones.RemoveAt(d);
                        }
                    }
                }
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(obtenerRecurso("messageError52"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmdEditarTelef_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmdModificar.IsEnabled == false)
                {
                    HacerConexion();
                    for (int d = 0; d <= ListaTelefonos.Count - 1; d++)
                    {
                        if (listaTelefonos.SelectedItem.ToString() == ListaTelefonos[d].ToString() + " , Ext: " + ListaExtensiones[d].ToString())
                        {
                            //Eliminar el elemento seleccionado (Titulo y descripcion)
                            txtTelefonos.Text = ListaTelefonos[d].ToString();
                            txtExtensiones.Text = ListaExtensiones[d].ToString();

                            object id_telefono = obj2.Obtener_IdTelefono_Paciente(ListaTelefonos[d].ToString(), ListaExtensiones[d].ToString());
                            obj2.EliminarTelefonosPacientePorID(id_telefono.ToString());

                            listaTelefonos.Items.Remove(listaTelefonos.SelectedItem.ToString());
                            ListaTelefonos.RemoveAt(d);
                            ListaExtensiones.RemoveAt(d); //Elimina el previo
                        }
                    }
                    CerrarConexion();
                }
                else
                {
                    for (int d = 0; d <= ListaTelefonos.Count - 1; d++)
                    {
                        if (listaTelefonos.SelectedItem.ToString() == ListaTelefonos[d].ToString())
                        {
                            //Eliminar el elemento seleccionado (Titulo y descripcion)
                            txtTelefonos.Text = ListaTelefonos[d].ToString();
                            txtExtensiones.Text = ListaExtensiones[d].ToString();
                            // MessageBox.Show(ListaHeredoDescrip[d].ToString());
                            listaTelefonos.Items.Remove(listaTelefonos.SelectedItem.ToString());
                            ListaTelefonos.RemoveAt(d);
                            ListaExtensiones.RemoveAt(d); //Elimina el previo
                        }
                    }
                }
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(obtenerRecurso("messageError51"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        void Desactivar_Diagnostico()
        {
            BusquedaReanalisis.Visibility = Visibility.Hidden;
            lblPacienteAnalisis2.Visibility = Visibility.Hidden;
            cmdAnalisisPaciente.Visibility = Visibility.Hidden;
            listadoPacientes.Visibility = Visibility.Hidden;
            lblPacienteAnalisis.Visibility = Visibility.Hidden;
            cmdAnalisisPaciente_Copy.Visibility = Visibility.Hidden;
            cmdAnalizarr.Visibility = Visibility.Hidden;
            cmdReanalizarr.Visibility = Visibility.Hidden;
            lblPacienteAnalisis_Copy.Visibility = Visibility.Hidden;
            comboOtrosAnal.Visibility = Visibility.Hidden;
            cmdAnalisisReanalisisCerrar.Visibility = Visibility.Hidden;
        }

        /*
          This function is  used to save a address from the patient
         */
        private void cmdAgregarDom_Click(object sender, RoutedEventArgs e)
        {
            bool valflag = obj2.ComprobarDomicilios(txtCalle.Text, txtColonia.Text, txtNum.Text, txtCP.Text, txtMunicipio.Text, txtEstado.Text, txtCountry.Text);

            if (valflag != true)
            {
                if (cmdEditarDom.IsEnabled == false && cmdAgregarDom.IsEnabled == true)
                {
                    HacerConexion();
                    obj2.RegistrarDomicilios(txtCalle.Text, txtNum.Text, txtColonia.Text, txtCP.Text, txtMunicipio.Text, txtEstado.Text, txtCountry.Text, id_paciente_global_modif);

                    listadodomicilios.Items.Add(obtenerRecurso("labelStreet") + " " + txtCalle.Text + ", " + obtenerRecurso("labelNum") + " " + txtNum.Text + ", " + obtenerRecurso("labelAvenue") + " " + txtColonia.Text + ", " + obtenerRecurso("labelZpCode") + " " + txtCP.Text + ", " + obtenerRecurso("labelCS") + " " + txtMunicipio.Text + ", " + obtenerRecurso("labelSatet") + " " + txtEstado.Text + ", " + obtenerRecurso("labelCountry") + " " + txtCountry.Text);
                    ListaCalles.Add(txtCalle.Text);
                    ListaColonia.Add(txtColonia.Text);
                    ListaNum.Add(txtNum.Text);
                    ListaCP.Add(txtCP.Text);
                    ListaMunicipio.Add(txtMunicipio.Text);
                    ListaEstado.Add(txtEstado.Text);
                    ListaPais.Add(txtCountry.Text);

                    //Limpiar campos
                    txtCalle.Clear();
                    txtNum.Clear();
                    txtColonia.Clear();
                    txtCP.Clear();
                    txtMunicipio.Clear();
                    txtEstado.Clear();
                    txtCountry.Clear();

                    cmdAgregarDom.IsEnabled = false;
                    cmdEditarDom.IsEnabled = true;
                    cmdEliminarDom.IsEnabled = true;
                    CerrarConexion();
                }
                else
                {
                    listadodomicilios.Items.Add(obtenerRecurso("labelStreet") + " " + txtCalle.Text + ", " + obtenerRecurso("labelNum") + " " + txtNum.Text + ", " + obtenerRecurso("labelAvenue") + " " + txtColonia.Text + ", " + obtenerRecurso("labelZpCode") + " " + txtCP.Text + ", " + obtenerRecurso("labelCS") + " " + txtMunicipio.Text + ", " + obtenerRecurso("labelSatet") + " " + txtEstado.Text + ", " + obtenerRecurso("labelCountry") + " " + txtCountry.Text);
                    ListaCalles.Add(txtCalle.Text);
                    ListaColonia.Add(txtColonia.Text);
                    ListaNum.Add(txtNum.Text);
                    ListaCP.Add(txtCP.Text);
                    ListaMunicipio.Add(txtMunicipio.Text);
                    ListaEstado.Add(txtEstado.Text);
                    ListaPais.Add(txtCountry.Text);

                    //Limpiar campos
                    txtCalle.Clear();
                    txtNum.Clear();
                    txtColonia.Clear();
                    txtCP.Clear();
                    txtMunicipio.Clear();
                    txtEstado.Clear();
                    txtCountry.Clear();

                }
            }
            else
            {
                MessageBox.Show(obtenerRecurso("messageError3"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /*
          This method is used to edit the pataint's address
         */
        private void cmdEditarDom_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (listadodomicilios.SelectedItem.ToString() != null)
                {
                    cmdEditarDom.IsEnabled = false;
                    cmdEliminarDom.IsEnabled = false;
                    cmdAgregarDom.IsEnabled = true;


                    HacerConexion();
                    for (int d = 0; d <= ListaCalles.Count - 1; d++)
                    {
                        if (listadodomicilios.SelectedItem.ToString() == obtenerRecurso("labelStreet") + " " + ListaCalles[d].ToString() + ", " + obtenerRecurso("labelNum") + " " + ListaNum[d].ToString() + ", " + obtenerRecurso("labelAvenue") + " " + ListaColonia[d].ToString() + ", " + obtenerRecurso("labelZpCode") + " " + ListaCP[d].ToString() + ", " + obtenerRecurso("labelCS") + " " + ListaMunicipio[d].ToString() + ", " + obtenerRecurso("labelSatet") + " " + ListaEstado[d].ToString() + ", " + obtenerRecurso("labelCountry") + " " + ListaPais[d].ToString())
                        {
                            //Eliminar el elemento seleccionado 
                            txtCalle.Text = ListaCalles[d].ToString();
                            txtNum.Text = ListaNum[d].ToString();
                            txtColonia.Text = ListaColonia[d].ToString();
                            txtCP.Text = ListaCP[d].ToString();
                            txtMunicipio.Text = ListaMunicipio[d].ToString();
                            txtEstado.Text = ListaEstado[d].ToString();
                            txtCountry.Text = ListaPais[d].ToString();



                            listadodomicilios.Items.Remove(listadodomicilios.SelectedItem.ToString());

                            object id_domicilio_modif = obj2.Obtener_IdDomicilio_Paciente(ListaCalles[d].ToString(), ListaNum[d].ToString(), ListaColonia[d].ToString(), ListaCP[d].ToString(), ListaMunicipio[d].ToString(), ListaEstado[d].ToString(), ListaPais[d].ToString(), id_paciente_global_modif);

                            obj2.EliminarDomicilioPaciente(id_domicilio_modif.ToString());
                            ListaCalles.RemoveAt(d);
                            ListaNum.RemoveAt(d);
                            ListaColonia.RemoveAt(d);
                            ListaCP.RemoveAt(d);
                            ListaMunicipio.RemoveAt(d);
                            ListaEstado.RemoveAt(d);
                            ListaPais.RemoveAt(d);

                        }
                    }
                    CerrarConexion();
                }
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(obtenerRecurso("messageError68"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        /*
          This method is used to delete the patient's address
         */
        private void cmdEliminarDom_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (listadodomicilios.SelectedItem.ToString() != null)
                {
                    HacerConexion();
                    for (int d = 0; d <= ListaCalles.Count - 1; d++)
                    {
                        if (listadodomicilios.SelectedItem.ToString() == obtenerRecurso("labelStreet") + " " + ListaCalles[d].ToString() + ", " + obtenerRecurso("labelNum") + " " + ListaNum[d].ToString() + ", " + obtenerRecurso("labelAvenue") + " " + ListaColonia[d].ToString() + ", " + obtenerRecurso("labelZpCode") + " " + ListaCP[d].ToString() + ", " + obtenerRecurso("labelCS") + " " + ListaMunicipio[d].ToString() + ", " + obtenerRecurso("labelSatet") + " " + ListaEstado[d].ToString() + ", " + obtenerRecurso("labelCountry") + " " + ListaPais[d].ToString())
                        {
                            object id_domicilio_modif = obj2.Obtener_IdDomicilio_Paciente(ListaCalles[d].ToString(), ListaNum[d].ToString(), ListaColonia[d].ToString(), ListaCP[d].ToString(), ListaMunicipio[d].ToString(), ListaEstado[d].ToString(), ListaPais[d].ToString(), id_paciente_global_modif);

                            if (id_domicilio_modif != null)
                            {
                                obj2.EliminarDomicilioPaciente(id_domicilio_modif.ToString());
                            }


                            listadodomicilios.Items.Remove(listadodomicilios.SelectedItem.ToString());
                            ListaCalles.RemoveAt(d);
                            ListaNum.RemoveAt(d);
                            ListaColonia.RemoveAt(d);
                            ListaCP.RemoveAt(d);
                            ListaMunicipio.RemoveAt(d);
                            ListaEstado.RemoveAt(d);
                            ListaPais.RemoveAt(d);
                        }
                    }

                    if (listadodomicilios.Items.Count == 0)
                    {
                        cmdEliminarDom.IsEnabled = false;
                        cmdEditarDom.IsEnabled = false;
                    }

                    cmdAgregarDom.IsEnabled = true;
                    CerrarConexion();//Cerrar conexion
                }
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(obtenerRecurso("messageError69"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmdBuscar_Analisis_Click(object sender, RoutedEventArgs e)
        {

        }

        private void txtBuscarPaciente1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtBuscarPaciente2.Text != "")
            {
                HacerConexion();
                //Llama y obtiene posibles matches
                DataTable PacientesAnalisisBuscado = new DataTable();
                PacientesAnalisisBuscado = obj2.Buscar_Analisis(txtBuscarPaciente2.Text);
                ListaPacientes_Recientes1.ItemsSource = PacientesAnalisisBuscado.DefaultView;
                CerrarConexion();
            }
            else
            {
                if (txtBuscarPaciente2.Text == "")
                {
                    CargarRegistrosPacientesRecientes();
                }
                else
                {
                    // MessageBox.Show("Introduzca el nombre del paciente a buscar", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    txtBuscarPaciente2.Focus();
                }
            }
        }



        //Guarda en listas
        /*List<string> Pac_nombre = new List<string>();
        List<string> Pac_apell1 = new List<string>();
        List<string> Pac_apell2 = new List<string>();
        List<string> Pac_sexo = new List<string>();*/

        void Cargar_Pacientes_Diagnostico()
        {
            //Limpia la lista antes de continuar
            listadoPacientes.Items.Clear();

            DataTable pacientes = obj2.Mostrar_Pacientes_Listado_Sencillo();

            //Pasarlo al listado de pacientes
            for (int j = 0; j <= pacientes.Rows.Count - 1; j++)
            {
                /* Pac_nombre.Add(pacientes.Rows[j][0].ToString());
                  Pac_apell1.Add(pacientes.Rows[j][1].ToString());
                  Pac_apell2.Add(pacientes.Rows[j][2].ToString());
                  Pac_sexo.Add(pacientes.Rows[j][3].ToString()); */
                listadoPacientes.Items.Add(pacientes.Rows[j][0].ToString() + " " + pacientes.Rows[j][1].ToString() + " " + pacientes.Rows[j][2].ToString());
            }

        }

        private void cmdNuevoAnalisis_Click(object sender, RoutedEventArgs e)
        {
            //Mostrar elementos del panel analisis/reanalisis
            listadoPacientes.Visibility = Visibility.Visible;
            BusquedaReanalisis.Visibility = Visibility.Visible;
            lblPacienteAnalisis2.Visibility = Visibility.Visible;
            cmdAnalisisPaciente.Visibility = Visibility.Visible;
            cmdAnalisisPaciente_Copy.Visibility = Visibility.Visible;
            cmdAnalizarr.Visibility = Visibility.Visible;
            cmdReanalizarr.Visibility = Visibility.Visible;
            lblPacienteAnalisis_Copy.Visibility = Visibility.Visible;
            comboOtrosAnal.Visibility = Visibility.Visible;
            lblPacienteAnalisis.Visibility = Visibility.Visible;
            cmdAnalisisReanalisisCerrar.Visibility = Visibility.Visible;

            //Desactivar
            cmdNuevoAnalisis1.IsEnabled = false;
            cmdAnalisisPaciente.Focus();
            comboOtrosAnal.Items.Clear();

            Cargar_Pacientes_Diagnostico();
        }

        private void cmdListaDiagnos_Click(object sender, RoutedEventArgs e)
        {
            HacerConexion();
            object id_analisis = obj2.Buscar_IdAnalisis_Nombre(lblPacienteAnalisis_P1.Content.ToString());
            object cant_codigos_analisis = obj2.Obtener_Codigos_Cantidad_Analisis(id_analisis.ToString());
            int cant_num_codigo = Convert.ToInt32(cant_codigos_analisis);
            CerrarConexion();

            if ((ListaCodigos.Items.Count != 0 || cant_num_codigo != 0) || ListaCodigos.Items.Count == 0 || cant_num_codigo == 0)
            {
                BusquedaAnalisisGroup1.Visibility = Visibility.Visible;
                Lista_Analisis_Group1.Visibility = Visibility.Visible;
                lblBusqueda_Copy1.Visibility = Visibility.Visible;
                txtBuscarPaciente2.Visibility = Visibility.Visible;
                cmdNuevoAnalisis1.Visibility = Visibility.Visible;
                cmdEliminar_Copy1.Visibility = Visibility.Visible;
                ListaPacientes_Recientes1.Visibility = Visibility.Visible;

                OcultarDiag();
                //Autoguardar
                Guarda_Diagnostico();


                ListaCodigos.Items.Clear();
                txtEstatura.Text = "";
                txtPresionSist.Text = "";
                txtIMC.Text = "";
                txtFR.Text = "";
                txtTA.Text = "";
                txtPeso.Text = "";
                txtPresionDistolica.Text = "";
                txtFC.Text = "";
                txtTemp.Text = "";

                txtPadecimiento.Text = "";
                txtInterrogatorio.Text = "";

                //optionProbabilidad.IsChecked = false;
                optionPorcentaje.IsChecked = false;
                option100.IsChecked = false;
                //optionPolaridad.IsChecked = false;
                //optionPronunciamiento.IsChecked = false;
                optionradionico.IsChecked = false;
                optionSugerirNiv.IsChecked = false;
                optionSugerirPot.IsChecked=false;

                comboNiveles.SelectedIndex = -1;
                comboP.SelectedIndex = -1;

                OcultarDiag2();
                CargarRegistrosPacientesRecientes();
            }
            else
            {

                //MessageBox.Show("Perform an analysis first in order to save it", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        void OcultarDiag2()
        {
            cmdProcesarAnalisis.IsEnabled = true;

            cmdIniciarDiag.IsEnabled = true;
            cmdAgregarCodigos.IsEnabled = true;
            cmdHacerRemedios.IsEnabled = true;
            cmdGuardarTarjeta.IsEnabled = true;
            cmdEnviarFrecuencia.IsEnabled = true;
            cmdDocumento.IsEnabled = true;
            cmdEliminarCodigosNoSensados.IsEnabled = true;

            //SECCION DE LOS CODIGOS
            lblCategorias.Visibility = Visibility.Hidden;
            listadoCategorias.Visibility = Visibility.Hidden;
            lblSubCategorias.Visibility = Visibility.Hidden;
            listadoSubcategorias.Visibility = Visibility.Hidden;
            listadoCodigos.Visibility = Visibility.Hidden;
            cmdTodos.Visibility = Visibility.Hidden;
            cmdNinguno.Visibility = Visibility.Hidden;
            lblCodigoBuscar.Visibility = Visibility.Hidden;
            txtCodigoBuscar.Visibility = Visibility.Hidden; ;


            //SECCION DEL METODO DE RESULTADOS
            TipoAnalisis_Group.Visibility = Visibility.Hidden;
            //optionProbabilidad.Visibility = Visibility.Hidden;
            optionPorcentaje.Visibility = Visibility.Hidden;
            option100.Visibility = Visibility.Hidden;
            //optionPolaridad.Visibility = Visibility.Hidden;
            //optionPronunciamiento.Visibility = Visibility.Hidden;
            optionSugerirNiv.Visibility = Visibility.Hidden;
            optionSugerirPot.Visibility = Visibility.Hidden;
            optionradionico.Visibility = Visibility.Hidden;
            nivellabel.Visibility = Visibility.Hidden;
            comboNiveles.Visibility = Visibility.Hidden;
            nivelP.Visibility = Visibility.Hidden;
            comboP.Visibility = Visibility.Hidden;
            cmdIniciarDiag.Visibility = Visibility.Hidden;
            cmdAgregarCodigos.Visibility = Visibility.Hidden;
            cmdHacerRemedios.Visibility = Visibility.Hidden;
            cmdGuardarTarjeta.Visibility = Visibility.Hidden;
            cmdEnviarFrecuencia.Visibility = Visibility.Hidden;
            cmdDocumento.Visibility = Visibility.Hidden;
            cmdEliminarCodigosNoSensados.Visibility = Visibility.Hidden;

            listadoCodigos.Items.Clear();
            listadoSubcategorias.Items.Clear();
            listadoCategorias.SelectedIndex = -1;
        }

        void OcultarDiag()
        {
            lblPacienteAnalisis1.Visibility = Visibility.Hidden;
            lblPacienteAnalisis_P1.Visibility = Visibility.Hidden;
            lblNombreAnalisis1.Visibility = Visibility.Hidden;
            lblNombre_Anal1.Visibility = Visibility.Hidden;
            lblFechaAnalisis2.Visibility = Visibility.Hidden;
            lblFechaAnalisis3.Visibility = Visibility.Hidden;
            cmdGuardarDiagnostico.Visibility = Visibility.Hidden;
            cmdListaDiagnos.Visibility = Visibility.Hidden;
            Diagnosticos_Tabs.Visibility = Visibility.Hidden;
        }

        void MostrarPrincipalAnalisis()
        {
            //Controles al elegir un analisis
            BusquedaAnalisisGroup1.Visibility = Visibility.Visible;
            Lista_Analisis_Group1.Visibility = Visibility.Visible;
            lblBusqueda_Copy1.Visibility = Visibility.Visible;
            txtBuscarPaciente2.Visibility = Visibility.Visible;
            cmdNuevoAnalisis1.Visibility = Visibility.Visible;
            cmdEliminar_Copy1.Visibility = Visibility.Visible;
            ListaPacientes_Recientes1.Visibility = Visibility.Visible;

        }

        private void listadoCategorias_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void listadoCategorias_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            listadoSubcategorias.Items.Clear(); //Limpia antes de cada uso
            listadoCodigos.Items.Clear();
            Categorias_Codigos.Clear();

            if (listadoCategorias.SelectedItem != null)
            {

                HacerConexion();
                DataTable CategoriasCodigos;

                //Buscar id_categoria para encontrar las subcategorias
                object id_categoria = obj2.BuscarCategoriasCodigos(listadoCategorias.SelectedItem.ToString());

                DataTable SubCategorias = obj2.VisualizarSubCategoriasCodigos(id_categoria.ToString());

                //Si no tiene subcategorias mostrarlos como codigos ya
                if (SubCategorias.Rows.Count == 0)
                {
                    CategoriasCodigos = obj2.VisualizarSubCategoriasCodigos2(id_categoria.ToString());

                    for (int y = 0; y <= CategoriasCodigos.Rows.Count - 1; y++)
                    {
                        if (CategoriasCodigos.Rows[y][1].ToString() != "")
                        {

                            listadoCodigos.Items.Add(CategoriasCodigos.Rows[y][1].ToString());
                            Categorias_Codigos.Add(CategoriasCodigos.Rows[y][2].ToString());
                        }

                    }
                }
                else
                {
                    //Agrega las categorias
                    for (int y = 0; y <= SubCategorias.Rows.Count - 1; y++)
                    {
                        if (SubCategorias.Rows[y][0].ToString() != "")
                        {
                            listadoSubcategorias.Items.Add(SubCategorias.Rows[y][0].ToString());
                        }
                    }

                    // IMPORTANTE: categorias con subcategorias y codigos en su categoria

                    //Tambien puede haber categorias con subcategorias y codigos en su categoria...
                    CategoriasCodigos = obj2.VisualizarSubCategoriasCodigos2(id_categoria.ToString());

                    for (int y = 0; y <= CategoriasCodigos.Rows.Count - 1; y++)
                    {
                        if (CategoriasCodigos.Rows[y][1].ToString() != "")
                        {
                            listadoCodigos.Items.Add(CategoriasCodigos.Rows[y][1].ToString());
                            Categorias_Codigos.Add(CategoriasCodigos.Rows[y][2].ToString()); //Guarda el codigo
                        }

                    }
                }
                CerrarConexion();
            }
        }

        private void listadoCodigos_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (listadoCodigos.SelectedItem != null)
            {
                try
                {
                    int index = listadoCodigos.Items.IndexOf(listadoCodigos.SelectedItem.ToString());
                    IEnumerable items = this.ListaCodigos.Items;
                    nombrecodigo.Clear();
                    codigos_rates.Clear();

                    //Comprobar si ya estan agregados
                    foreach (nuevoCodigo codigo in items)
                    {
                        nombrecodigo.Add(codigo.nombre.ToString());
                        codigos_rates.Add(codigo.rates.ToString());
                    }

                    if (ListaCodigos.Items.Count != 0)
                    {

                        if (nombrecodigo.Contains(listadoCodigos.SelectedItem.ToString()) == false)
                        {
                            ListaCodigos.Items.Add(new nuevoCodigo { nombre = listadoCodigos.SelectedItem.ToString(), rates = Categorias_Codigos[index], niveles = "-",potencia="-",potenciaSugeridad="-", ftester = Convert.ToInt32(0), nsugerido = "-" });
                        }
                    }
                    else
                    {

                        if (busqueda == true)
                        {
                            HacerConexion();
                            ListaCodigos.Items.Add(new nuevoCodigo { nombre = listadoCodigos.SelectedItem.ToString(), rates = obj2.BuscarCodigoPorNombreCategoria(listadoCodigos.SelectedItem.ToString()).ToString() });
                            busqueda = false;
                            CerrarConexion();

                        }
                        else
                        {
                            ListaCodigos.Items.Add(new nuevoCodigo { nombre = listadoCodigos.SelectedItem.ToString(), rates = Categorias_Codigos[index], niveles = "-", potencia="-",potenciaSugeridad="-",ftester = Convert.ToInt32(0), nsugerido = "-" });
                        }

                    }
                    lblContCodigos.Content = ListaCodigos.Items.Count;

                }
                catch (Exception)
                {
                    //MessageBox.Show("Por favor seleccione una categoría o subcategoría primero!", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }


        //this function is importan on xaml but it doesn't have any functionality
        private void part1_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void listadoSubcategorias_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            listadoCodigos.Items.Clear();
            Categorias_Codigos.Clear(); //Limpia los codigos guardados

            if (listadoSubcategorias.SelectedItem != null)
            {
                try
                {
                    HacerConexion();
                    object id_categoria = obj2.BuscarCategoriasCodigos(listadoCategorias.SelectedItem.ToString());

                    object id_subcategoria = obj2.BuscarCategoriasCodigosSub(listadoSubcategorias.SelectedItem.ToString(), id_categoria.ToString());

                    /// AGREGADO
                    /// 
                    //Buscar el sexo del paciente
                    DataTable paciente_sexo_tabla = obj2.VisualizarAnalisisPorGenero2(lblNombre_Anal1.Content.ToString());
                    string sexo = "";

                    for (int a = 0; a <= paciente_sexo_tabla.Rows.Count - 1; a++)
                    {

                        if (paciente_sexo_tabla.Rows[a][0].ToString() == "Masculino")
                        {
                            sexo = "M";
                        }

                        if (paciente_sexo_tabla.Rows[a][0].ToString() == "Femenino")
                        {
                            sexo = "F";
                        }

                        if (paciente_sexo_tabla.Rows[a][0].ToString() == "Animal")
                        {
                            sexo = "A";
                        }

                        if (paciente_sexo_tabla.Rows[a][0].ToString() == "Plantas y suelo")
                        {
                            sexo = "P";
                        }
                    }


                    DataTable Codigos = obj2.VisualizarSubCategoriasCodigosListado(id_subcategoria.ToString(), sexo);


                    if (Codigos.Rows.Count == 0) //Sino hay por genero pues utiliza el de todos...
                    {
                        Codigos = obj2.VisualizarSubCategoriasCodigosListadoGenero_Todos(id_subcategoria.ToString(), sexo);
                    }


                    //AGREGADO HASTA AQUI

                    for (int y = 0; y <= Codigos.Rows.Count - 1; y++)
                    {
                        if (Codigos.Rows[y][1].ToString() != "")
                        {
                            listadoCodigos.Items.Add(Codigos.Rows[y][1].ToString());
                            Categorias_Codigos.Add(Codigos.Rows[y][2].ToString()); //Guarda el codigo
                        }
                    }


                    CerrarConexion();
                }
                catch (NullReferenceException)
                {
                    MessageBox.Show(obtenerRecurso("messageError50"), obtenerRecurso("messageHeadWarning"), MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }


        public class nuevoCodigo
        {

            public string nombre { get; set; }

            public string rates { get; set; }
            public string niveles { get; set; }
            public int ftester { get; set; }

            public string potencia { get; set; }

            public string potenciaSugeridad { get; set; }

            public string nsugerido { get; set; }

        }

        public class nuevaTerapia
        {

            public string nombre { get; set; }
            public string fecha { get; set; }

        }

        private void listadoCategorias_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {


        }

        void GuardarPadecimiento(object sender, RoutedEventArgs e)
        {
            if (txtPadecimiento.Text != "" && txtInterrogatorio.Text != "")
            {
                HacerConexion();

                //Registra los datos de padecimiento
                obj2.Registrar_Padecimiento_Analisis(txtPadecimiento.Text, txtInterrogatorio.Text, lblPacienteAnalisis_P1.Content.ToString());

                CerrarConexion();
                //Cambia el nivel del default
                Diagnosticos_Tabs.SelectedIndex = 0;
            }
            else
            {
                MessageBox.Show(obtenerRecurso("mssageError49"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        void GuardarInfoBiologica(object sender, RoutedEventArgs e)
        {
            if (txtEstatura.Text != "" && txtPresionSist.Text != "" && txtIMC.Text != "" && txtFR.Text != "" && txtTA.Text != "" && txtPeso.Text != "" && txtPresionDistolica.Text != "" && txtFC.Text != "" && txtTemp.Text != "")
            {
                double[] infobiologica = new double[9];

                try
                {
                    //Recorrer informacion
                    infobiologica[0] = Double.Parse(txtEstatura.Text);
                    infobiologica[1] = Double.Parse(txtPresionSist.Text);
                    infobiologica[2] = Double.Parse(txtIMC.Text);
                    infobiologica[3] = Double.Parse(txtFR.Text);
                    infobiologica[4] = Double.Parse(txtTA.Text);
                    infobiologica[5] = Double.Parse(txtPeso.Text);
                    infobiologica[6] = Double.Parse(txtPresionDistolica.Text);
                    infobiologica[7] = Double.Parse(txtFC.Text);
                    infobiologica[8] = Double.Parse(txtTemp.Text);

                    HacerConexion();

                    //Registra la info biologica
                    obj2.Registrar_Informacion_Biologica(infobiologica, lblPacienteAnalisis_P1.Content.ToString());
                    CerrarConexion();

                    //Cambia el nivel del default
                    Diagnosticos_Tabs.SelectedIndex = 0;
                }
                catch (FormatException)
                {
                    MessageBox.Show(obtenerRecurso("messageError48"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                }
            }
            else
            {
                MessageBox.Show(obtenerRecurso("messageError47"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        private void cmdGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (txtEstatura.Text != "" && txtPresionSist.Text != "" && txtIMC.Text != "" && txtFR.Text != "" && txtTA.Text != "" && txtPeso.Text != "" && txtPresionDistolica.Text != "" && txtFC.Text != "" && txtTemp.Text != "")
            {
                double[] infobiologica = new double[9];

                try
                {
                    //Recorrer informacion
                    infobiologica[0] = Double.Parse(txtEstatura.Text);
                    infobiologica[1] = Double.Parse(txtPresionSist.Text);
                    infobiologica[2] = Double.Parse(txtIMC.Text);
                    infobiologica[3] = Double.Parse(txtFR.Text);
                    infobiologica[4] = Double.Parse(txtTA.Text);
                    infobiologica[5] = Double.Parse(txtPeso.Text);
                    infobiologica[6] = Double.Parse(txtPresionDistolica.Text);
                    infobiologica[7] = Double.Parse(txtFC.Text);
                    infobiologica[8] = Double.Parse(txtTemp.Text);

                    HacerConexion();

                    //Registra la info biologica
                    obj2.Registrar_Informacion_Biologica(infobiologica, lblPacienteAnalisis_P1.Content.ToString());
                    CerrarConexion();

                    //Cambia el nivel del default
                    Diagnosticos_Tabs.SelectedIndex = 0;
                }
                catch (FormatException)
                {
                    MessageBox.Show(obtenerRecurso("messageError48"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                }
            }
            else
            {
                MessageBox.Show(obtenerRecurso("messageError47"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }

        }

        private void cmdBorrar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ListaCodigos.SelectedItems.Count == 1)
                {
                    dynamic selectedItem = ListaCodigos.SelectedItem;
                    var nombre_selected = selectedItem.nombre;
                    int inde_var = nombrecodigo.IndexOf(nombre_selected);
                    ListaCodigos.Items.RemoveAt(ListaCodigos.Items.IndexOf(ListaCodigos.SelectedItem));

                }
                else
                {

                    for (int q = 0; q <= ListaCodigos.SelectedItems.Count - 1; q++)
                    {

                        dynamic selectedItem2 = ListaCodigos.SelectedItems[q];
                        var nombre_selected2 = selectedItem2.nombre;
                        int inde_var2 = nombrecodigo.IndexOf(nombre_selected2);
                        ListaCodigos.Items.RemoveAt(ListaCodigos.Items.IndexOf(ListaCodigos.SelectedItems[q]));
                    }
                }

                lblContCodigos.Content = ListaCodigos.Items.Count;
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(obtenerRecurso("messageError40"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        private void cmdTodos_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                //Siempre visible
                borderInfobasica_Copy2.Visibility = Visibility.Visible;
                cmdProcesarAnalisis.Visibility = Visibility.Visible;
                cmdBorrar.Visibility = Visibility.Visible;
                cmdRango.Visibility = Visibility.Visible;

                if (listadoCodigos.Items.Count == 0)
                {
                    MessageBox.Show(obtenerRecurso("messageWarning11"), obtenerRecurso("messageHeadWarning"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                else
                {
                    //Elementos
                    IEnumerable items = this.ListaCodigos.Items;
                    nombrecodigo.Clear();
                    codigos_rates.Clear();

                    //Comprobar si ya estan agregados
                    foreach (nuevoCodigo codigo in items)
                    {
                        nombrecodigo.Add(codigo.nombre.ToString());
                        codigos_rates.Add(codigo.rates.ToString());
                    }

                    if (ListaCodigos.Items.Count != 0)
                    {
                        //Agregar todos los existentes en la listadecodigos
                        for (int w = 0; w <= listadoCodigos.Items.Count - 1; w++)
                        {

                            if (nombrecodigo.Contains(listadoCodigos.Items[w].ToString()) == false)
                            {
                                //Buscar codigo
                                ListaCodigos.Items.Add(new nuevoCodigo { nombre = listadoCodigos.Items[w].ToString(), rates = Categorias_Codigos[w], niveles = "-", ftester = Convert.ToInt32(0),potencia="-",potenciaSugeridad="-", nsugerido = "-" });
                            }


                        }
                        lblContCodigos.Content = ListaCodigos.Items.Count;

                    }
                    else
                    {

                        //Agregar todos los existentes en la listadecodigos
                        for (int w = 0; w <= listadoCodigos.Items.Count - 1; w++)
                        {
                            
                            ListaCodigos.Items.Add(new nuevoCodigo { nombre = listadoCodigos.Items[w].ToString(), rates = Categorias_Codigos[w], niveles = "-", ftester = Convert.ToInt32(0), potencia="-",potenciaSugeridad="-",nsugerido = "-" });
                        }
                        lblContCodigos.Content = ListaCodigos.Items.Count;

                    }
                }
            }
            catch (Exception) { }
        }


        private void cmdNinguno_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                //Siempre visible
                borderInfobasica_Copy2.Visibility = Visibility.Visible;
                cmdProcesarAnalisis.Visibility = Visibility.Visible;
                cmdBorrar.Visibility = Visibility.Visible;
                cmdRango.Visibility = Visibility.Visible;

                IEnumerable items = this.ListaCodigos.Items;
                nombrecodigo.Clear();
                codigos_rates.Clear();

                foreach (nuevoCodigo codigo in items)
                {

                    nombrecodigo.Add(codigo.nombre.ToString());
                    codigos_rates.Add(codigo.rates.ToString());

                }

                for (int i = 0; i <= listadoCodigos.Items.Count - 1; i++)
                {
                    if (nombrecodigo.Contains(listadoCodigos.Items[i].ToString()) == true)
                    {
                        int index = nombrecodigo.IndexOf(listadoCodigos.Items[i].ToString());
                        nombrecodigo.RemoveAt(index);
                        codigos_rates.RemoveAt(index);

                    }

                }

                ListaCodigos.Items.Clear();

                for (int w = 0; w <= nombrecodigo.Count - 1; w++)
                {
                    ListaCodigos.Items.Add(new nuevoCodigo { nombre = nombrecodigo[w], rates = codigos_rates[w], niveles = "-", ftester = Convert.ToInt32(0), nsugerido = "-" });


                }

                lblContCodigos.Content = ListaCodigos.Items.Count;

            }
            catch (Exception) { }
        }

        private void cmdCodigoBuscar_Click(object sender, RoutedEventArgs e)
        {
            if (txtCodigoBuscar.Text != "")
            {
                listadoCodigos.Items.Clear();

                HacerConexion();

                DataTable Codigos = obj2.BuscarCodigo2(txtCodigoBuscar.Text);

                for (int y = 0; y <= Codigos.Rows.Count - 1; y++)
                {
                    if (Codigos.Rows[y][0].ToString() != "")
                    {
                        //listadoCodigos.Items.Add(new CheckBox { Content = Codigos.Rows[y][1].ToString() });
                        listadoCodigos.Items.Add(Codigos.Rows[y][0].ToString());
                    }
                }

                CerrarConexion();
            }
            else
            {
                MessageBox.Show(obtenerRecurso("messageWarning10"), obtenerRecurso("messageHeadWarning"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
        bool busqueda;

        private void txtCodigoBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            listadoCategorias.SelectedIndex = -1;
            listadoSubcategorias.SelectedIndex = -1;
            listadoCodigos.SelectedIndex = -1;

            if (txtCodigoBuscar.Text != "")
            {
                //Busqueda on
                busqueda = true;

                listadoCodigos.Items.Clear();

                HacerConexion();

                DataTable Codigos = obj2.BuscarCodigo(txtCodigoBuscar.Text);

                for (int y = 0; y <= Codigos.Rows.Count - 1; y++)
                {
                    if (Codigos.Rows[y][0].ToString() != "")
                    {
                        //listadoCodigos.Items.Add(new CheckBox { Content = Codigos.Rows[y][1].ToString() });
                        listadoCodigos.Items.Add(Codigos.Rows[y][0].ToString());
                    }
                }

                CerrarConexion();
            }
            else
            {
                listadoCodigos.Items.Clear(); //limpiar la lista de codigos
                                              // lblCodigosCont.Content = listadoCodigos.Items.Count + " Códigos";
            }
        }

        private void hideProgressBar()
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                progreso1.Visibility = Visibility.Hidden;
            }));
        }
        private void showProgressBar()
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                progreso1.Visibility = Visibility.Visible;
            }));
        }

        private void cmdProcesarAnalisis_Click(object sender, RoutedEventArgs e)
        {
            if (ListaCodigos.Items.Count != 0)
            {
                cmdProcesarAnalisis.IsEnabled = false;

                //SECCION DE LOS CODIGOS
                lblCategorias.Visibility = Visibility.Hidden;
                listadoCategorias.Visibility = Visibility.Hidden;
                lblSubCategorias.Visibility = Visibility.Hidden;
                listadoSubcategorias.Visibility = Visibility.Hidden;
                listadoCodigos.Visibility = Visibility.Hidden;
                cmdTodos.Visibility = Visibility.Hidden;
                cmdNinguno.Visibility = Visibility.Hidden;
                lblCodigoBuscar.Visibility = Visibility.Hidden;
                txtCodigoBuscar.Visibility = Visibility.Hidden;

                //SECCION DEL METODO DE RESULTADOS
                TipoAnalisis_Group.Visibility = Visibility.Visible;
                optionPorcentaje.Visibility = Visibility.Visible;
                option100.Visibility = Visibility.Visible;
                cmdIniciarDiag.Visibility = Visibility.Visible;
                optionradionico.Visibility = Visibility.Visible;
                cmdAgregarCodigos.Visibility = Visibility.Visible;
                nivellabel.Visibility = Visibility.Visible;
                comboNiveles.Visibility = Visibility.Visible;
                nivelP.Visibility = Visibility.Visible;
                comboP.Visibility = Visibility.Visible;
                cmdHacerRemedios.Visibility = Visibility.Visible;
                cmdGuardarTarjeta.Visibility = Visibility.Visible;
                cmdEnviarFrecuencia.Visibility = Visibility.Visible;
                cmdDocumento.Visibility = Visibility.Visible;
                cmdEliminarCodigosNoSensados.Visibility = Visibility.Visible;
                optionSugerirNiv.Visibility = Visibility.Visible;
                optionSugerirPot.Visibility=Visibility.Visible;

            }

        }

        private void cmdRango_Click(object sender, RoutedEventArgs e)
        {
            /*if (option100.IsChecked != true)
          {*/
            string rangomin, rangomax;

            rangomin = Interaction.InputBox(obtenerRecurso("messageQuestion5"), obtenerRecurso("messageHeadQ4"), "", 300, 300);

            rangomax = Interaction.InputBox(obtenerRecurso("messageQuestion4"), obtenerRecurso("messageHeadQ4"), "", 300, 300);

            if (rangomin == "" || rangomax == "")
            {
                MessageBox.Show(obtenerRecurso("messageError46"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
            else
            {
                double rangominvalor = Convert.ToDouble(rangomin);
                double rangomaxvalor = Convert.ToDouble(rangomax);

                IEnumerable itemsCodigos = this.ListaCodigos.Items;

                List<nuevoCodigo> objetos_Codigos = new List<nuevoCodigo>();

                //Listas
                List<string> codigos_ord = new List<string>();
                List<string> nombres_ord = new List<string>();
                List<int> valor_ord = new List<int>();
                List<string> niveles_ord = new List<string>();
                List<string> sugeridos_ord = new List<string>();

                //De objetos los pasamos a listas
                foreach (nuevoCodigo codigo in itemsCodigos)
                {
                    objetos_Codigos.Add(codigo);
                }

                //Elimina valores de acuerdo al rango
                objetos_Codigos.RemoveAll(codigo => (codigo.ftester >= rangominvalor && codigo.ftester <= rangomaxvalor));
                //Limpia la lista de valores
                ListaCodigos.Items.Clear();

                //Pasa de nuevo a la ListaCodigos
                foreach (nuevoCodigo codigo in objetos_Codigos)
                {
                    ListaCodigos.Items.Add(
                        new nuevoCodigo
                        {
                            nombre = codigo.nombre,
                            rates = codigo.rates,
                            niveles = codigo.niveles,
                            ftester = codigo.ftester,
                            nsugerido = codigo.nsugerido
                        });
                }

                //Actualiza el contador de los codigos.
                lblContCodigos.Content = ListaCodigos.Items.Count;
            }
        }

        private void listadoCodigos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void listadoSubcategorias_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        //this function is used to active and enable  when the diagnostic  had been finished
        void Panel_opciones()
        {
            //Desactivar boton de (Codigos no sensados)
            cmdEliminarCodigosNoSensados.IsEnabled = true;
            listadoCategorias.SelectedIndex = -1;
            listadoSubcategorias.SelectedIndex = -1;
            listadoCodigos.SelectedIndex = -1;
            Desactivar_Iniciar_AgregarCodigo();
        }

        void Panel_opcion2()
        {
            progreso1.Visibility = Visibility.Hidden;
            ListaCodigos.Items.Clear();
        }

        void NivelSugerido()
        {
            HacerConexion();
            Radionica objF = new Radionica();
            for (int i = 0; i <= nombrecodigo.Count - 1; i++)
            {
                Sniveles.Add(objF.RadionicoSugerirNiveles());
            }
            CerrarConexion();
        }

        void NivelSugerirPotencia()
        {
            HacerConexion();
            Radionica objF = new Radionica();
            for (int i = 0; i <= nombrecodigo.Count - 1; i++)
            {
                potenciasugeridad.Add(objF.RadionicaSurgerirPotencia());
            }
            CerrarConexion();
        }

        void Limpiar_Listas()
        {
            nombrecodigo.Clear();
            nivel.Clear();
            ftester.Clear();
            codigos_rates.Clear();
            Sniveles.Clear();
        }

        private void cmdIniciarDiag_Click(object sender, RoutedEventArgs e)
        {
            //Tipo de opciones
            string tipo = "";


            if (option100.IsChecked == true)
            {
                tipo = "100";
            } else if (optionPorcentaje.IsChecked == true)
            {
                tipo = "Porcentaje";
            } else if (optionradionico.IsChecked == true)
            {
                tipo = "Radionico";
            }


            //Sino hay seleccion de nivel del codigo agarra por default el 01 - FISICO

            /*this line of code should be rewied  in a future moment*/
            if (comboNiveles.SelectedIndex == -1)
            {
                comboNiveles.SelectedIndex = 0;
            }

            if (comboNiveles.SelectedIndex == -1)
            {
                comboP.SelectedIndex = 0;
            }

            //Selectiva con el tipo de analisis
            switch (tipo)
            {
                // PRO
                case "100":
                    cmdRango.IsEnabled = true;
                    optionSugerirNiv.IsChecked = true;
                    optionSugerirPot.IsChecked = true;
                    IEnumerable items1 = this.ListaCodigos.Items;
                    Limpiar_Listas();

                    progreso1.Visibility = Visibility.Visible;

                    new Thread((ThreadStart)delegate
                    {
                        obj.Diagnostic();
                        Thread.Sleep(4000); //Tiempo

                        //Realizar Diagnostico
                        foreach (nuevoCodigo codigo in items1)
                        {
                            nombrecodigo.Add(codigo.nombre.ToString());
                            codigos_rates.Add(codigo.rates.ToString());
                        }

                        Radionica obj1 = new Radionica(); //Utilizando numeros random para el valor
                        for (int i = 0; i <= nombrecodigo.Count - 1; i++)
                        {
                            ftester.Add(obj1.ValorSugerido());
                            nivel.Add(obj1.SugerirNiveles());
                        }

                        Dispatcher.Invoke((ThreadStart)delegate
                        {
                            obj.Diagnostic();
                            Panel_opcion2();

                            //Agrega valores random a la columna de valores
                            for (int w = 0; w <= nombrecodigo.Count - 1; w++)
                            {
                                ListaCodigos.Items.Add(new nuevoCodigo { nombre = nombrecodigo[w], rates = codigos_rates[w], potencia = "-", potenciaSugeridad = "-", niveles = "-", ftester = Convert.ToInt32(ftester[w]), nsugerido = nivel[w] });
                            }

                            Panel_opciones();
                        });

                    }).Start();

                    break;

                case "Radionico":
                    cmdRango.IsEnabled = true;
                    IEnumerable items7 = this.ListaCodigos.Items;
                    Limpiar_Listas();

                    progreso1.Visibility = Visibility.Visible;

                    new Thread((ThreadStart)delegate
                    {
                        obj.Diagnostic();

                        Thread.Sleep(4000); //Tiempo

                        //Realizar Diagnostico
                        foreach (nuevoCodigo codigo in items7)
                        {
                            nombrecodigo.Add(codigo.nombre.ToString());
                            codigos_rates.Add(codigo.rates.ToString());
                        }

                        Radionica obj1 = new Radionica(); //Utilizando numeros random para el valor
                        for (int i = 0; i <= nombrecodigo.Count - 1; i++)
                        {
                            ftester.Add(obj1.ValorSugerido());
                            nivel.Add(tipo_nivel_codigo);
                            potencia.Add(nivel_potencia);
                        }

                        Dispatcher.Invoke((ThreadStart)delegate
                        {
                            obj.Diagnostic();
                            Panel_opcion2();

                            if (optionSugerirNiv.IsChecked == true)
                            {
                                NivelSugerido();
                            }

                            if (optionSugerirPot.IsChecked == true)
                            {
                                NivelSugerirPotencia();
                            }


                            for (int w = 0; w <= nombrecodigo.Count - 1; w++)
                            {
                                ListaCodigos.Items.Add(new nuevoCodigo {
                                    nombre = nombrecodigo[w],
                                    rates = codigos_rates[w],
                                    potencia = potencia[w],
                                    potenciaSugeridad = potenciasugeridad.Count > 0 ? potenciasugeridad[w] : "-",
                                    niveles = nivel[w], ftester = Convert.ToInt32(ftester[w]),
                                    nsugerido = Sniveles.Count > 0 ? Sniveles[w] : "-"
                                });
                            }

                            Panel_opciones();
                        });

                    }).Start();

                    break;

                case "Porcentaje":
                    IEnumerable items5 = this.ListaCodigos.Items;
                    Limpiar_Listas();
                    progreso1.Visibility = Visibility.Visible;

                    new Thread((ThreadStart)delegate
                    {
                        obj.Diagnostic();
                        Thread.Sleep(6000); //Tiempo
                        foreach (nuevoCodigo codigo in items5)
                        {
                            nombrecodigo.Add(codigo.nombre.ToString());
                            codigos_rates.Add(codigo.rates.ToString());
                        }

                        Radionica obj6 = new Radionica(); //Utilizando numeros random para el valor
                        for (int i = 0; i <= nombrecodigo.Count - 1; i++)
                        {
                            ftester.Add(obj6.Porcentaje());
                            nivel.Add(tipo_nivel_codigo);
                            potencia.Add(nivel_potencia);
                        }
                        //then dispatch back to the UI thread to update the progress bar
                        Dispatcher.Invoke((ThreadStart)delegate
                        {
                            obj.Diagnostic();
                            Panel_opcion2();

                            if (optionSugerirNiv.IsChecked == true)
                            {
                                NivelSugerido();
                            }

                            if (optionSugerirPot.IsChecked == true)
                            {
                                NivelSugerirPotencia();
                            }


                            for (int w = 0; w <= nombrecodigo.Count - 1; w++)
                            {
                                ListaCodigos.Items.Add(new nuevoCodigo {
                                    nombre = nombrecodigo[w],
                                    rates = codigos_rates[w],
                                    potencia = potencia[w],
                                    potenciaSugeridad = potenciasugeridad.Count > 0 ? potenciasugeridad[w] : "-",
                                    niveles = nivel[w],
                                    ftester = Convert.ToInt32(ftester[w]),
                                    nsugerido = Sniveles.Count > 0 ? Sniveles[w] : "-"
                                });
                            }

                            Panel_opciones();
                        });

                    }).Start();
                    break;
            }
        }

        //this function is used to add the handle evenr of click
        private void GridViewColumn_Loaded(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader header = sender as GridViewColumnHeader;
            if (header != null)
            {
                header.Click += GridViewColumnHeader_MouseDoubleClick;
            }
        }

        //this function is used to order by header grid 
        private void GridViewColumnHeader_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader header = sender as GridViewColumnHeader;
            if (header != null)
            {
                GridViewColumn column = header.Column;
                string nameTable = column.Header.ToString();

                IEnumerable itemsCodigos = this.ListaCodigos.Items;
                List<nuevoCodigo> objetos_Codigos = new List<nuevoCodigo>();

                //Listas
                List<string> codigos_ord = new List<string>();
                List<string> nombres_ord = new List<string>();
                List<string> valor_ord = new List<string>();
                List<string> niveles_ord = new List<string>();
                List<string> sugeridos_ord = new List<string>();
                List<string> potencia_ord = new List<string>();
                List<string> potenciasugeridad_ord = new List<string>();
                IEnumerable<nuevoCodigo> query = null;

                foreach (nuevoCodigo codigo in itemsCodigos)
                {
                    objetos_Codigos.Add(codigo);
                }


                if (nameTable == obtenerRecurso("tableName"))
                    query = objetos_Codigos.OrderBy(codigo => codigo.nombre);

                else if (nameTable == obtenerRecurso("tableRate"))
                    query = objetos_Codigos.OrderBy(codigo => codigo.rates);

                else if (nameTable == obtenerRecurso("tableValue"))
                    query = objetos_Codigos.OrderBy(codigo => codigo.ftester);

                if (query != null)
                {
                    ListaCodigos.Items.Clear();

                    //Lectura
                    foreach (nuevoCodigo codigo in query)
                    {
                        
                        codigos_ord.Add(codigo.rates.ToString());
                        nombres_ord.Add(codigo.nombre.ToString());
                        valor_ord.Add(codigo.ftester.ToString());
                        niveles_ord.Add(codigo.niveles.ToString());
                        sugeridos_ord.Add(codigo.nsugerido.ToString());
                        potencia_ord.Add(codigo.potencia.ToString());
                        potenciasugeridad_ord.Add(codigo.potenciaSugeridad.ToString());
                    }

                    for (int p = 0; p <= codigos_ord.Count - 1; p++)
                    {
                        ListaCodigos.Items.Add(new nuevoCodigo { nombre = nombres_ord[p], rates = codigos_ord[p], potencia = potencia_ord[p], potenciaSugeridad = potenciasugeridad_ord[p], niveles = niveles_ord[p], ftester = Convert.ToInt32(valor_ord[p]), nsugerido = sugeridos_ord[p] });
                    }
                }

            }
        }

        void Esconder_Controles_tipos_analisis()
        {
            TipoAnalisis_Group.Visibility = Visibility.Hidden;
            //optionProbabilidad.Visibility = Visibility.Hidden;
            optionPorcentaje.Visibility = Visibility.Hidden;
            option100.Visibility = Visibility.Hidden;
            //optionPolaridad.Visibility = Visibility.Hidden;
            //optionPronunciamiento.Visibility = Visibility.Hidden;
            cmdIniciarDiag.Visibility = Visibility.Hidden;
            cmdAgregarCodigos.Visibility = Visibility.Hidden;
        }

        void Mostrar_Controles_tipos_analisis()
        {
            TipoAnalisis_Group.Visibility = Visibility.Visible;
            //optionProbabilidad.Visibility = Visibility.Visible;
            optionPorcentaje.Visibility = Visibility.Visible;
            option100.Visibility = Visibility.Visible;
            //optionPolaridad.Visibility = Visibility.Visible;
            //optionPronunciamiento.Visibility = Visibility.Visible;
            cmdIniciarDiag.Visibility = Visibility.Visible;
            cmdAgregarCodigos.Visibility = Visibility.Visible;
        }

        void Hombre()
        {
            myGif.Visibility = Visibility.Visible;
            //progreso.Visibility = Visibility.Visible;
        }

        void Hombre_Esconder()
        {
            myGif.Visibility = Visibility.Hidden;
            //progreso.Visibility = Visibility.Hidden;
        }

        void Mujer()
        {
            myGif2.Visibility = Visibility.Visible;
            //progreso.Visibility = Visibility.Visible;
        }

        void Mujer_Esconder()
        {
            myGif2.Visibility = Visibility.Hidden;
            //progreso.Visibility = Visibility.Hidden;
        }

        private void cmdAgregarCodigos_Click(object sender, RoutedEventArgs e)
        {
            cmdProcesarAnalisis.IsEnabled = true;

            //SECCION DE LOS CODIGOS
            lblCategorias.Visibility = Visibility.Visible;
            listadoCategorias.Visibility = Visibility.Visible;
            lblSubCategorias.Visibility = Visibility.Visible;
            listadoSubcategorias.Visibility = Visibility.Visible;
            //lblCodigos.Visibility = Visibility.Visible;
            listadoCodigos.Visibility = Visibility.Visible;
            cmdTodos.Visibility = Visibility.Visible;
            cmdNinguno.Visibility = Visibility.Visible;
            lblCodigoBuscar.Visibility = Visibility.Visible;
            txtCodigoBuscar.Visibility = Visibility.Visible;
            //borderInfobasica_Copy5.Visibility = Visibility.Visible;            


            //SECCION DEL METODO DE RESULTADOS
            TipoAnalisis_Group.Visibility = Visibility.Hidden;
            //optionProbabilidad.Visibility = Visibility.Hidden;
            optionPorcentaje.Visibility = Visibility.Hidden;
            option100.Visibility = Visibility.Hidden;
            //optionPolaridad.Visibility = Visibility.Hidden;
            //optionPronunciamiento.Visibility = Visibility.Hidden;
            optionSugerirNiv.Visibility = Visibility.Hidden;
            optionSugerirPot.Visibility = Visibility.Hidden;
            optionradionico.Visibility = Visibility.Hidden;
            nivellabel.Visibility = Visibility.Hidden;
            comboNiveles.Visibility = Visibility.Hidden;
            nivelP.Visibility = Visibility.Hidden;
            comboP.Visibility = Visibility.Hidden;
            cmdIniciarDiag.Visibility = Visibility.Hidden;
            cmdAgregarCodigos.Visibility = Visibility.Hidden;
            cmdHacerRemedios.Visibility = Visibility.Hidden;
            cmdGuardarTarjeta.Visibility = Visibility.Hidden;
            cmdEnviarFrecuencia.Visibility = Visibility.Hidden;
            cmdDocumento.Visibility = Visibility.Hidden;
            cmdEliminarCodigosNoSensados.Visibility = Visibility.Hidden;

            listadoCodigos.Items.Clear();
            listadoSubcategorias.Items.Clear();
            listadoCategorias.SelectedIndex = -1;
        }

        //this function is used to enable some btns
        void Desactivar_Iniciar_AgregarCodigo()
        {
            cmdIniciarDiag.IsEnabled = false;
            cmdAgregarCodigos.IsEnabled = false;
            cmdHacerRemedios.IsEnabled = true;
            cmdGuardarTarjeta.IsEnabled = true;
            cmdEnviarFrecuencia.IsEnabled = true;
            cmdDocumento.IsEnabled = true;
        }


        //thid method is used to  show the patient's data when the user select on the list 

        private void ListaPacientes_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //Funcion desactivada mientras modificar un perfil
                cmdEliminarDom.Visibility = Visibility.Visible;
                cmdEditarDom.Visibility = Visibility.Visible;


                PacienteGroup.Visibility = Visibility.Hidden;
                PacienteGroup_Copy.Visibility = Visibility.Visible;

                //Campo seleccionado con el mouse
                DataRowView paciente_seleccionado = (DataRowView)ListaPacientes.SelectedItem;
                if (paciente_seleccionado != null)
                {
                    string id_paciente = paciente_seleccionado[0].ToString();
                    id_paciente_global_modif = id_paciente;
                    //MessageBox.Show(id_paciente);

                    //INFORMACION DEL PACIENTE

                    //Traer informacion del paciente (Tabla rad_pacientes)
                    DataTable Paciente_InfoPrincipal = obj2.Modificar_Paciente(id_paciente);

                    //Extraer (Nombre, apellido1, apellido2, email, sexo, profesion, titulo, fecha de nacimiento, fpg)
                    txtNombre1.Text = Paciente_InfoPrincipal.Rows[0][1].ToString();
                    txtApellidoPat1.Text = Paciente_InfoPrincipal.Rows[0][2].ToString();
                    txtApellidoMat1.Text = Paciente_InfoPrincipal.Rows[0][3].ToString();
                    txtEmail1.Text = Paciente_InfoPrincipal.Rows[0][4].ToString();
                    //lblNombreGrande.Content = txtNombre.Text +" "+ txtApellidoPat.Text + " "+txtApellidoMat.Text;
                    txtSexo1.Text = Paciente_InfoPrincipal.Rows[0][5].ToString();
                    txtProfesion1.Text = Paciente_InfoPrincipal.Rows[0][6].ToString();
                    txtTitulo1.Text = Paciente_InfoPrincipal.Rows[0][7].ToString();
                    txtFecha1.Text = Paciente_InfoPrincipal.Rows[0][8].ToString();
                    txtPGR1.Text = Paciente_InfoPrincipal.Rows[0][9].ToString();

                    //DOMICILIOS
                    DataTable Paciente_Domicilios = obj2.ListadoPacienteDomicilios(id_paciente);
                    //MessageBox.Show(Paciente_Domicilios.Rows[0][0].ToString());
                    //listadoDomicilios.ItemsSource = Paciente_Domicilios.DefaultView; //Carga los domicilios del paciente
                    listadodomicilios1_Copy.Items.Clear();

                    if (Paciente_Domicilios.Rows.Count > 0)
                    {
                        cmdAgregarDom.IsEnabled = false;
                        cmdEliminarDom.IsEnabled = true;
                        cmdEditarDom.IsEnabled = true;
                    }
                    else
                    {
                        cmdAgregarDom.IsEnabled = true;
                        cmdEliminarDom.IsEnabled = false;
                        cmdEditarDom.IsEnabled = false;
                    }

                    //calle,numero,colonia,cp,municipio,estado,pais
                    for (int q = 0; q <= Paciente_Domicilios.Rows.Count - 1; q++)
                    {
                        listadodomicilios1_Copy.Items.Add(obtenerRecurso("labelStreet") + " " + Paciente_Domicilios.Rows[q][0].ToString() + ", " + obtenerRecurso("labelNum") + " " + Paciente_Domicilios.Rows[q][1].ToString() + ", " + obtenerRecurso("labelAvenue") + " " + Paciente_Domicilios.Rows[q][2].ToString() + ", " + obtenerRecurso("labelZpCode") + " " + Paciente_Domicilios.Rows[q][3].ToString() + ", " + obtenerRecurso("labelCS") + " " + Paciente_Domicilios.Rows[q][4].ToString() + ", " + obtenerRecurso("labelSatet") + " " + Paciente_Domicilios.Rows[q][5].ToString() + ", " + obtenerRecurso("labelCountry") + " " + Paciente_Domicilios.Rows[q][6].ToString());

                    }

                    //TELEFONOS Y ANTECEDENTES

                    //Traer informacion de los telefonos (rad_telefonos)
                    DataTable Paciente_Telefonos = obj2.Modificar_PacienteTelefonos(id_paciente);

                    listaTelefonos1.Items.Clear();

                    //Extraer del datatable los telefonos
                    for (int y = 0; y <= Paciente_Telefonos.Rows.Count - 1; y++)
                    {
                        listaTelefonos1.Items.Add(Paciente_Telefonos.Rows[y][0].ToString() + " , Ext: " + Paciente_Telefonos.Rows[y][1].ToString());

                    }

                    //Traer informacion de los antecedentes (rad_antecedentes)
                    DataTable Paciente_Antecedentes = obj2.Modificar_PacienteAntecedentes(id_paciente);

                    ListadoHeredo1_Copy.Items.Clear();
                    listadoPatologicos1_Copy.Items.Clear();
                    listadoNoPatologicos1_Copy.Items.Clear();
                    listadoComentarios1_Copy.Items.Clear();
                    listadoAnalisis1_Copy.Items.Clear();

                    //Extraer del datatable
                    for (int w = 0; w <= Paciente_Antecedentes.Rows.Count - 1; w++)
                    {
                        //Selectiva para los diferentes tipos de antecedentes
                        switch (Paciente_Antecedentes.Rows[w][2].ToString())
                        {
                            case "HF":

                                ListadoHeredo1_Copy.Items.Add(Paciente_Antecedentes.Rows[w][0].ToString());
                                break;

                            case "P":

                                listadoPatologicos1_Copy.Items.Add(Paciente_Antecedentes.Rows[w][0].ToString());
                                break;

                            case "NP":

                                listadoNoPatologicos1_Copy.Items.Add(Paciente_Antecedentes.Rows[w][0].ToString());
                                break;

                            case "C":

                                listadoComentarios1_Copy.Items.Add(Paciente_Antecedentes.Rows[w][0].ToString());
                                break;
                        }

                        //Agrega al conglomerado del paciente antecedentes
                        // Paciente_Conglomerado.Add(Paciente_Telefonos.Rows[w][0].ToString()); 
                        //  Paciente_Conglomerado.Add(Paciente_Telefonos.Rows[w][1].ToString()); 
                    }

                    //ANALISIS

                    Historial_Analisis_Paciente = obj2.HistorialAnalisisPacienteCompleto(id_paciente.ToString());

                    //Agrega a listbox los nombres de los analisis
                    for (int p = 0; p <= Historial_Analisis_Paciente.Rows.Count - 1; p++)
                    {
                        listadoAnalisis1_Copy.Items.Add(Historial_Analisis_Paciente.Rows[p][2].ToString() + " , " + Historial_Analisis_Paciente.Rows[p][3].ToString()); //Agarra el nombre del analisis buscado
                    }

                    //IMAGEN DEL PACIENTE

                    string ruta_imagen = RutaInstalacion() + "//fotos//" + id_paciente.ToString() + ".png";

                    //Guarda URL de la imagen del paciente..
                    if (File.Exists(ruta_imagen) == true)
                    {
                        //Guarda la ruta
                        id_ppaciente.Content = ruta_imagen;

                        byte[] imageInfo = File.ReadAllBytes(ruta_imagen);

                        BitmapImage image;

                        using (MemoryStream imageStream = new MemoryStream(imageInfo))
                        {
                            image = new BitmapImage();
                            image.BeginInit();
                            image.CacheOption = BitmapCacheOption.OnLoad;
                            image.StreamSource = imageStream;
                            image.EndInit();
                        }
                        //Se almacena en memoria y evita hacer uso directo de ella.
                        /*   BitmapImage imageb = new BitmapImage();
                           imageb.BeginInit();
                           Uri imageSource = new Uri(ruta_imagen);
                           imageb.UriSource = imageSource;
                           imageb.EndInit();*/
                        image1.Source = image;
                        //image1.Source= new BitmapImage(new Uri(ruta_imagen));
                    }
                    else
                    {
                        id_ppaciente.Content = "NA";
                    }
                    // id_ppaciente.Content = ruta_imagen;               

                    //Mostrar Listado completo de pacientes
                    CargarListadoCompletoPacientes();

                }

            }
            catch (NullReferenceException)
            {
                MessageBox.Show(obtenerRecurso("messageError45"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                PacienteGroup_Copy.Visibility = Visibility.Hidden;
                PacienteGroup.Visibility = Visibility.Visible;

            }
        }

        private void cmdGuardarPaciente1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PacienteGroup.Visibility = Visibility.Visible;
                //      historial.Visibility = Visibility.H;
                PacienteGroup_Copy.Visibility = Visibility.Hidden;

                cmdModificar.IsEnabled = false;
                //INFO GENERAL

                //Campo seleccionado con el mouse
                string id_paciente = id_paciente_global_modif;
                //MessageBox.Show(id_paciente);

                //Traer informacion del paciente (Tabla rad_pacientes)
                DataTable Paciente_InfoPrincipal = obj2.Modificar_Paciente(id_paciente);

                //Extraer (Nombre, apellido1, apellido2, email, sexo, profesion, titulo, fecha de nacimiento, fpg)
                txtNombre.Text = Paciente_InfoPrincipal.Rows[0][1].ToString();
                txtApellidoPat.Text = Paciente_InfoPrincipal.Rows[0][2].ToString();
                txtApellidoMat.Text = Paciente_InfoPrincipal.Rows[0][3].ToString();
                txtEmail.Text = Paciente_InfoPrincipal.Rows[0][4].ToString();
                //lblNombreGrande.Content = txtNombre.Text +" "+ txtApellidoPat.Text + " "+txtApellidoMat.Text;
                switch (Paciente_InfoPrincipal.Rows[0][5].ToString()) //Sexo
                {
                    case "Male":
                        optionSexoM.IsChecked = true;

                        break;

                    case "Female":
                        optionSexoF.IsChecked = true;

                        break;

                    case "Animal":
                        optionSexoAn.IsChecked = true;

                        break;

                    case "Plants and soil":
                        optionSexoPl.IsChecked = true;

                        break;
                }
                combProfesion.Text = Paciente_InfoPrincipal.Rows[0][6].ToString();
                txtTitulo.Text = Paciente_InfoPrincipal.Rows[0][7].ToString();
                txtFecha.Text = Paciente_InfoPrincipal.Rows[0][8].ToString();
                txtPGR.Text = Paciente_InfoPrincipal.Rows[0][9].ToString();

                //Agrega los elementos al conglomerado de la pestaña de info general.
                /*for (int w = 1; w <= 9; w++)
                {
                    Paciente_Conglomerado.Add(Paciente_InfoPrincipal.Rows[0][w].ToString());
                }*/

                //DOMICILIOS
                DataTable Paciente_Domicilios = obj2.ListadoPacienteDomicilios(id_paciente);
                //MessageBox.Show(Paciente_Domicilios.Rows[0][0].ToString());
                //listadoDomicilios.ItemsSource = Paciente_Domicilios.DefaultView; //Carga los domicilios del paciente

                //calle,numero,colonia,cp,municipio,estado,pais
                for (int q = 0; q <= Paciente_Domicilios.Rows.Count - 1; q++)
                {

                    listadodomicilios.Items.Add(obtenerRecurso("labelStreet") + " " + Paciente_Domicilios.Rows[q][0].ToString() + ", " + obtenerRecurso("labelNum") + " " + Paciente_Domicilios.Rows[q][1].ToString() + ", " + obtenerRecurso("labelAvenue") + " " + Paciente_Domicilios.Rows[q][2].ToString() + ", " + obtenerRecurso("labelZpCode") + " " + Paciente_Domicilios.Rows[q][3].ToString() + ", " + obtenerRecurso("labelCS") + " " + Paciente_Domicilios.Rows[q][4].ToString() + ", " + obtenerRecurso("labelSatet") + " " + Paciente_Domicilios.Rows[q][5].ToString() + ", " + obtenerRecurso("labelCountry") + " " + Paciente_Domicilios.Rows[q][6].ToString());
                    ListaCalles.Add(Paciente_Domicilios.Rows[q][0].ToString());
                    ListaNum.Add(Paciente_Domicilios.Rows[q][1].ToString());
                    ListaColonia.Add(Paciente_Domicilios.Rows[q][2].ToString());
                    ListaCP.Add(Paciente_Domicilios.Rows[q][3].ToString());
                    ListaMunicipio.Add(Paciente_Domicilios.Rows[q][4].ToString());
                    ListaEstado.Add(Paciente_Domicilios.Rows[q][5].ToString());
                    ListaPais.Add(Paciente_Domicilios.Rows[q][6].ToString());
                }

                //TELEFONOS Y ANTECEDENTES

                //Traer informacion de los telefonos (rad_telefonos)
                DataTable Paciente_Telefonos = obj2.Modificar_PacienteTelefonos(id_paciente);

                //Limpiamos listas con telefonos y extensiones
                ListaExtensiones.Clear();
                ListaTelefonos.Clear();

                //Extraer del datatable los telefonos
                for (int y = 0; y <= Paciente_Telefonos.Rows.Count - 1; y++)
                {
                    ListaTelefonos.Add(Paciente_Telefonos.Rows[y][0].ToString()); //Telefonos
                    ListaExtensiones.Add(Paciente_Telefonos.Rows[y][1].ToString()); //Extensiones
                    listaTelefonos.Items.Add(Paciente_Telefonos.Rows[y][0].ToString() + " , Ext: " + Paciente_Telefonos.Rows[y][1].ToString());
                }


                //Traer informacion de los antecedentes (rad_antecedentes)
                DataTable Paciente_Antecedentes = obj2.Modificar_PacienteAntecedentes(id_paciente);

                //Limpiar las listas
                ListaHeredoTitulos.Clear(); ListaHeredoDescrip.Clear();
                ListaPatTitulos.Clear(); ListaPatDescrip.Clear();
                ListaNoPatTitulos.Clear(); ListaNoPatDescrip.Clear();
                ListaComentTitulos.Clear(); ListaComentDescrip.Clear();

                //Extraer del datatable
                for (int w = 0; w <= Paciente_Antecedentes.Rows.Count - 1; w++)
                {
                    //Selectiva para los diferentes tipos de antecedentes
                    switch (Paciente_Antecedentes.Rows[w][2].ToString())
                    {
                        case "HF":
                            ListaHeredoTitulos.Add(Paciente_Antecedentes.Rows[w][0].ToString());
                            ListaHeredoDescrip.Add(Paciente_Antecedentes.Rows[w][1].ToString());
                            ListadoHeredo.Items.Add(Paciente_Antecedentes.Rows[w][0].ToString());
                            break;

                        case "P":
                            ListaPatTitulos.Add(Paciente_Antecedentes.Rows[w][0].ToString());
                            ListaPatDescrip.Add(Paciente_Antecedentes.Rows[w][1].ToString());
                            listadoPatologicos.Items.Add(Paciente_Antecedentes.Rows[w][0].ToString());
                            break;

                        case "NP":
                            ListaNoPatTitulos.Add(Paciente_Antecedentes.Rows[w][0].ToString());
                            ListaNoPatDescrip.Add(Paciente_Antecedentes.Rows[w][1].ToString());
                            listadoNoPatologicos.Items.Add(Paciente_Antecedentes.Rows[w][0].ToString());
                            break;

                        case "C":
                            ListaComentTitulos.Add(Paciente_Antecedentes.Rows[w][0].ToString());
                            ListaComentDescrip.Add(Paciente_Antecedentes.Rows[w][1].ToString());
                            listadoComentarios.Items.Add(Paciente_Antecedentes.Rows[w][0].ToString());
                            break;
                    }
                }

                //ANALISIS

                Historial_Analisis_Paciente = obj2.HistorialAnalisisPacienteCompleto(id_paciente.ToString());

                //Agrega a listbox los nombres de los analisis
                for (int p = 0; p <= Historial_Analisis_Paciente.Rows.Count - 1; p++)
                {
                    listadoAnalisis.Items.Add(Historial_Analisis_Paciente.Rows[p][2].ToString() + " , " + Historial_Analisis_Paciente.Rows[p][3].ToString()); //Agarra el nombre del analisis buscado
                }

                //Cambiar el nombre header paciente registro
                PacienteGroup.Header = obtenerRecurso("labelEditP");
                cmdGuardarPaciente.Content = obtenerRecurso("brnSC");
                //cmdGuardarPaciente.ToolTip = "Modificar el registro del paciente";
                SolidColorBrush brush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(105, 185, 126));
                PacienteGroup.Background = brush;
                tabControl.Foreground = brush;


                string ruta_imagen = RutaInstalacion() + "//fotos//" + id_paciente.ToString() + ".png";


                //Si existe imagen haz...
                if (File.Exists(ruta_imagen))
                {
                    byte[] imageInfo = File.ReadAllBytes(ruta_imagen);

                    BitmapImage imageb;

                    using (MemoryStream imageStream = new MemoryStream(imageInfo))
                    {
                        imageb = new BitmapImage();
                        imageb.BeginInit();
                        imageb.CacheOption = BitmapCacheOption.OnLoad;
                        imageb.StreamSource = imageStream;
                        imageb.EndInit();
                    }

                    image.Source = imageb;
                    //Liberamos el image control de vista del paciente
                    image1.Source = null;
                }

                cmdEliminar.IsEnabled = true;  //Activar control de borrado

                //Mostrar
                CargarListadoCompletoPacientes();
            }
            catch (Exception ex)
            {
                MessageBox.Show("We haven't been able to modify the profile, please contact your provider!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                MessageBox.Show("Error: " + ex.ToString());
                cmdModificar.IsEnabled = true;
                cmdEliminar.IsEnabled = true;
                //cmdRespaldar.IsEnabled = true;
            }
        }

        private void listadoSubcategorias_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }

        private void myGif_MediaEnded(object sender, RoutedEventArgs e)
        {
            myGif.Position = new TimeSpan(0, 0, 1);
            myGif.Play();
        }

        private void myGif2_MediaEnded(object sender, RoutedEventArgs e)
        {
            myGif2.Position = new TimeSpan(0, 0, 1);
            myGif2.Play();
        }

        private void cmdGuardarTarjeta_Click(object sender, RoutedEventArgs e)
        {
            obj.Save(); //Llama a la maquina
            Thread.Sleep(5000);
            MessageBox.Show(obtenerRecurso("messageInfo4"), "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void cmdEnviarFrecuencia_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string prueba = ListaCodigos.SelectedItem.ToString();
                obj.Diagnostic(); //Llama a la maquina
                Thread.Sleep(5000);
                MessageBox.Show(obtenerRecurso("messageWarning9"), obtenerRecurso("messageHeadWarning"), MessageBoxButton.OK, MessageBoxImage.Information);
                obj.Diagnostic();
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(obtenerRecurso("messageError44"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //Guarda el diagnostico en la base de datos
        private void cmdGuardarDiagnostico_Click(object sender, RoutedEventArgs e)
        {

        }

        void Guarda_Diagnostico()
        {
            try
            {
                HacerConexion();

                //Listas para los objetos
                List<string> codigos_s = new List<string>();
                List<string> nombres_s = new List<string>();
                List<string> valor_s = new List<string>();
                List<string> niveles_s = new List<string>();
                List<string> sugeridos = new List<string>();
                List<string> potencia = new List<string>();
                List<string> potenciaSugeridad = new List<string>();

                object id_codigo;
                object id_analisis;

                //Obtenemos elementos del analisis en base a lo que se eligio
                IEnumerable items6 = this.ListaCodigos.Items;

                //De objetos los pasamos a listas
                foreach (nuevoCodigo codigo in items6)
                {
                    nombres_s.Add(codigo.nombre.ToString());
                    codigos_s.Add(codigo.rates.ToString());
                    valor_s.Add(codigo.ftester.ToString());
                    niveles_s.Add(codigo.niveles.ToString());
                    sugeridos.Add(codigo.nsugerido.ToString());
                    potencia.Add(codigo.potencia.ToString());
                    potenciaSugeridad.Add(codigo.potenciaSugeridad.ToString());

                }

                if (valor_s.Count == 0 || ListaCodigos.Items.Count == 0)
                {
                    MessageBox.Show(obtenerRecurso("messageError43"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                if (valor_s.Count != 0 || ListaCodigos.Items.Count != 0)
                {
                    //Buscar el id_analisis
                    id_analisis = obj2.Buscar_IdAnalisis_Nombre(lblPacienteAnalisis_P1.Content.ToString());

                    // MessageBox.Show(id_analisis.ToString());
                    //Actualizar estado del analisis ha analizado
                    obj2.Modificar_Estado_Analisis_Analizado(id_analisis.ToString());

                    //Guardar cada uno de los codigos de analisis
                    for (int i = 0; i <= nombres_s.Count - 1; i++)
                    {
                        //Buscar el id_codigo
                        id_codigo = obj2.Buscar_IdCodigo_Codigo(codigos_s[i]);

                        //MessageBox.Show(id_codigo.ToString());
                        //Guarda cada uno de los codigos analizados en base
                        obj2.Registrar_Codigo_de_Analisis(id_analisis.ToString(), id_codigo.ToString(), codigos_s[i], nombres_s[i], valor_s[i], niveles_s[i], sugeridos[i], potencia[i], potenciaSugeridad[i]);
                    }

                    //MessageBox.Show("Analysis Saved!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                CerrarConexion();
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(obtenerRecurso("messageError42"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void txtEmail_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        void Desbloquear_OpcionesNoPRO()
        {
            optionSugerirNiv.IsEnabled = true;
            optionSugerirPot.IsEnabled = true;
            comboNiveles.IsEnabled = true;
            comboP.IsEnabled = true;
        }

        private void option100_Checked(object sender, RoutedEventArgs e)
        {
            optionSugerirNiv.IsEnabled = false;
            optionSugerirPot.IsEnabled=false;
            comboNiveles.IsEnabled = false;
            comboP.IsEnabled = false;
        }

        private void comboNiveles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboNiveles.SelectedIndex != -1)
            {
                tipo_nivel_codigo = ((ComboBoxItem)comboNiveles.SelectedItem).Content.ToString();

            }
        }

        private void comboPontency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboP.SelectedIndex != -1)
            {
                nivel_potencia = ((ComboBoxItem)comboP.SelectedItem).Content.ToString();
            }

        }




        private void optionradionico_Checked_1(object sender, RoutedEventArgs e)
        {
            Desbloquear_OpcionesNoPRO();
        }

        private void optionPolaridad_Checked(object sender, RoutedEventArgs e)
        {
            Desbloquear_OpcionesNoPRO();
        }

        private void optionPorcentaje_Checked(object sender, RoutedEventArgs e)
        {
            Desbloquear_OpcionesNoPRO();
        }

        private void optionProbabilidad_Checked(object sender, RoutedEventArgs e)
        {
            Desbloquear_OpcionesNoPRO();
        }

        private void optionPronunciamiento_Checked(object sender, RoutedEventArgs e)
        {
            Desbloquear_OpcionesNoPRO();
        }

        private void cmdEliminarCodigosNoSensados_Click(object sender, RoutedEventArgs e)
        {
            //Obtiene lo que ahi en el cuadro
            IEnumerable itemsCodigos = this.ListaCodigos.Items;
            List<string> codigos_s = new List<string>();
            List<string> nombres_s = new List<string>();
            List<string> valor_s = new List<string>();
            List<string> niveles_s = new List<string>();
            List<string> sugeridos = new List<string>();

            //De objetos los pasamos a listas
            foreach (nuevoCodigo codigo in itemsCodigos)
            {
                nombres_s.Add(codigo.nombre.ToString());
                codigos_s.Add(codigo.rates.ToString());
                valor_s.Add(codigo.ftester.ToString());
                niveles_s.Add(codigo.niveles.ToString());
                sugeridos.Add(codigo.nsugerido.ToString());
            }

            //Limpiamos la lista
            ListaCodigos.Items.Clear();

            //Elimina los que esten con - en niveles utilizando PRO o otro metodo
            for (int i = 0; i <= nombres_s.Count - 1; i++)
            {
                if (niveles_s[i] == "-")
                {
                    nombres_s.RemoveAt(i);
                    codigos_s.RemoveAt(i);
                    valor_s.RemoveAt(i);
                    niveles_s.RemoveAt(i);
                    sugeridos.RemoveAt(i);
                }
            }

            //Volver a meterlos en la lista
            for (int w = 0; w <= nombres_s.Count - 1; w++)
            {
                ListaCodigos.Items.Add(new nuevoCodigo { nombre = nombres_s[w], rates = codigos_s[w], niveles = niveles_s[w], ftester = Convert.ToInt32(valor_s[w]), nsugerido = sugeridos[w] });
            }

            lblContCodigos.Content = ListaCodigos.Items.Count.ToString(); //Actualizamos el contador

            cmdEliminarCodigosNoSensados.IsEnabled = false; //Desactivar el boton
        }

        //Cargar los remedios
        void CargarListadoRemedios()
        {
            try
            {
                HacerConexion();
                string letra; //Letra para el filtrado de los remedios
                DataTable RemediosLista = obj2.VisualizarRemedios();
                listadoRemedios.Items.Clear(); //Limpiar la lista
                CerrarConexion();

            
                if (comboCategoriasRemedios.SelectedIndex == -1)
                {



                    //Agrega elementos al listbox
                    for (int i = 0; i <= RemediosLista.Rows.Count - 1; i++)
                    {
                        listadoRemedios.Items.Add(RemediosLista.Rows[i][1].ToString());
                    }
                    //listadoRemedios.ItemsSource = RemediosLista.DefaultView;
                }
                else 
                {
                      checkOpcion1.IsChecked = false;
                      checkOpcion2.IsChecked = false;
                      checkOpcion3.IsChecked = false;
                       letra = ((ComboBoxItem)comboCategoriasRemedios.SelectedItem).Content.ToString();
                        var FilterRemedy =RemediosLista.Clone();

                        //filter the list of remedy by the letter
                        foreach (DataRow row in RemediosLista.Rows)
                        {
                            if (row["nombre"].ToString().StartsWith(letra))
                            {
                                FilterRemedy.ImportRow(row);
                            }
                        }

                        //Agrega elementos al listbox
                        for (int i = 0; i <= FilterRemedy.Rows.Count - 1; i++)
                        {
                            listadoRemedios.Items.Add(FilterRemedy.Rows[i][1].ToString());
                        }
                    
                }
            }
            catch (Npgsql.NpgsqlException ex)
            {
                ex.ToString();
            }
           
        }



        private void comboCategoriasRemedios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            CargarListadoRemedios();
        }

        private void checkOpcion1_Checked(object sender, RoutedEventArgs e)
        {
            DataTable RemediosLista = new DataTable();
            listadoRemedios.Items.Clear(); //Limpiar la lista

            HacerConexion();
            RemediosLista = obj2.VisualizarRemediosPorDiagnostico();

            //Agrega elementos al listbox
            for (int i = 0; i <= RemediosLista.Rows.Count - 1; i++)
            {
                listadoRemedios.Items.Add(RemediosLista.Rows[i][1].ToString());
            }
            comboCategoriasRemedios.SelectedIndex = -1;
            CerrarConexion();
        }

        private void checkOpcion2_Checked(object sender, RoutedEventArgs e)
        {
            DataTable RemediosLista = new DataTable();
            listadoRemedios.Items.Clear(); //Limpiar la lista

            HacerConexion();
            RemediosLista = obj2.VisualizarRemediosGenerales_Usuarios();
            comboCategoriasRemedios.SelectedIndex = -1;
            //Agrega elementos al listbox
            for (int i = 0; i <= RemediosLista.Rows.Count - 1; i++)
            {
                listadoRemedios.Items.Add(RemediosLista.Rows[i][1].ToString());
            }
            CerrarConexion();
        }

        object verificacion_remedio_de_analisis;

        private void listadoRemedios_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListaRemedios.Items.Clear(); //Limpiar lista

            try
            {
                //Aparecer controles
                ControlRemedios.Visibility = Visibility.Visible;
                lblNombreRemedioResp.Visibility = Visibility.Visible;
                lblFechaRemedioResp.Visibility = Visibility.Visible;
                lblFechaRemedio.Visibility = Visibility.Visible;
                lblNombreRemedio.Visibility = Visibility.Visible;
                cmdDuplicarRemedio.Visibility = Visibility.Visible;
                cmdCerrarRemedio.Visibility = Visibility.Visible;

                HacerConexion();
                string nombre = listadoRemedios.SelectedItem.ToString(); //Nombre seleccionado

                object id_remedio = obj2.Obtener_IdRemedio(nombre); //Id remedio

                //Verifica si el remedio no proviene de un analisis

                verificacion_remedio_de_analisis = obj2.Obtener_IdRemediosCodigos(id_remedio.ToString());

                if (verificacion_remedio_de_analisis.ToString() != "")
                {
                    DataTable CodigosdeRemedio_Analisis = obj2.VisualizarCodigos_Remedios_de_Analisis(verificacion_remedio_de_analisis.ToString());
                    for (int i = 0; i <= CodigosdeRemedio_Analisis.Rows.Count - 1; i++)
                    {
                        ListaRemedios.Items.Add(new nuevoRemedio { nombrecodigo = CodigosdeRemedio_Analisis.Rows[i][1].ToString(), codigo = CodigosdeRemedio_Analisis.Rows[i][0].ToString(), nivel = CodigosdeRemedio_Analisis.Rows[i][2].ToString(), metodo = "R", potencia = "1", codigocomplementario = "-" });

                    }

                    //Borramos de codigos de analisis
                    //obj2.Eliminar_CodigosAnalisis(verificacion_remedio_de_analisis.ToString());
                    obj2.Alterar_IdAnalisis("", nombre);
                    Salvar_Remedio(); //Salvar remedio
                }
                else
                {
                    //Obtener codigos de remedios en base a id remedio
                    DataTable CodigosdeRemedios = obj2.VisualizarCodigos_Remedios_IdRemedio(id_remedio.ToString());

                    for (int i = 0; i <= CodigosdeRemedios.Rows.Count - 1; i++)
                    {
                        ListaRemedios.Items.Add(new nuevoRemedio { nombrecodigo = CodigosdeRemedios.Rows[i][1].ToString(), codigo = CodigosdeRemedios.Rows[i][0].ToString(), potencia = CodigosdeRemedios.Rows[i][2].ToString(), metodo = CodigosdeRemedios.Rows[i][3].ToString(), codigocomplementario = CodigosdeRemedios.Rows[i][4].ToString(), nivel = CodigosdeRemedios.Rows[i][5].ToString() });

                    }

                    // ListaRemedios.ItemsSource = CodigosdeRemedios.DefaultView; //Carga los codigos
                }
                //Introduce detalles de los remedios
                lblNombreRemedioResp.Content = listadoRemedios.SelectedItem.ToString();
                lblFechaRemedioResp.Content = obj2.Buscar_Fecha_Remedio(listadoRemedios.SelectedItem.ToString()).ToString(); //Introduce fecha del remedio

                lblContCodigosRemedios.Content = ListaRemedios.Items.Count; //Contador de codigos de remedios
                comboOrdenarRemedios.SelectedIndex = -1;
                CerrarConexion();
            }
            catch (Exception)
            {
                MessageBox.Show(obtenerRecurso("messageWarning7"), obtenerRecurso("messageHeadWarning"), MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        //Salvado
        public void Salvar_Remedio()
        {
            //Antes de cambiar salvar el contenido - SALVAMOS
            //HacerConexion(database_name);

            //Borramos los resultados de un analisis en caso de venir de ahi
            //  obj2.Eliminar_CodigosAnalisis(verificacion_remedio_de_analisis.ToString());

            string nombre_s = listadoRemedios.SelectedItem.ToString(); //Nombre seleccionado

            object id_remedio_s = obj2.Obtener_IdRemedio(nombre_s); //Id remedio

            //Obtener los elementos
            IEnumerable itemsCodigos = this.ListaRemedios.Items;

            //Listas
            List<string> codigos_ord = new List<string>();
            List<string> nombres_ord = new List<string>();
            List<string> potencia_ord = new List<string>();
            List<string> metodo_ord = new List<string>();
            List<string> complementario_ord = new List<string>();
            List<string> nivel_ord = new List<string>();

            //De objetos los pasamos a listas
            foreach (nuevoRemedio codigo in itemsCodigos)
            {
                //Guardamos en listas todos los remedios de las listas
                codigos_ord.Add(codigo.codigo);
                nombres_ord.Add(codigo.nombrecodigo);
                potencia_ord.Add(codigo.potencia);
                metodo_ord.Add(codigo.metodo);
                complementario_ord.Add(codigo.codigocomplementario);
                nivel_ord.Add(codigo.nivel);
            }



            //Antes borramos todo lo de un remedio anterior para evitar duplicados
            obj2.Eliminar_codigos_remedio(id_remedio_s.ToString());



            Random rdm = new Random();
            //Copiamos los nuevos en la bd
            for (int i = 0; i <= codigos_ord.Count - 1; i++)
            {
                //Pasarlos a la bd los codigos del remedio
                Radionica obj_1 = new Radionica();
                string id_generado = rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() +
                     rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() +
                      rdm.Next(0, 9).ToString() + "-" + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() +
                     rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() +
                      rdm.Next(0, 9).ToString();

                string id_codigo_buscado = obj2.Buscar_IdCodigo_Codigo(codigos_ord[i]).ToString();

                obj2.Registrar_CodigosdeRemedios(id_generado + "-CR", id_remedio_s.ToString(), codigos_ord[i], complementario_ord[i], nombres_ord[i], id_codigo_buscado, potencia_ord[i], metodo_ord[i], nivel_ord[i]);

                id_generado = obj_1.Generar_Id();
            }
            //CerrarConexion();
        }

        //codigo,nombrecodigo,potencia,metodo,codigocomplementario,nivel
        public class nuevoRemedio
        {
            public string codigo { get; set; }
            public string nombrecodigo { get; set; }
            public string potencia { get; set; }
            public string metodo { get; set; }
            public string codigocomplementario { get; set; }
            public string nivel { get; set; }
        }

        private void listadoRemedios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        string id_paciente_global;
        //Crear remedio en base a diagnostico
        private void cmdHacerRemedios_Click(object sender, RoutedEventArgs e)
        {
            //Mandamos salvar lo de la ListaCodigos
            Guarda_Diagnostico();

            //Nombre del remedio
            string nombre_remedio_diagnostico;

            nombre_remedio_diagnostico = Interaction.InputBox(obtenerRecurso("messageQuestion3"), "Name", obtenerRecurso("messageHeadQ3") + lblPacienteAnalisis_P1.Content.ToString(), 300, 300);

            if (nombre_remedio_diagnostico == "")
            {
                MessageBox.Show(obtenerRecurso("messageError39"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                //Grabar registro en bd del inicio del remedio va a tabla rad_remedios
                HacerConexion();

                object val_remedio = obj2.Obtener_Id_Remedio(nombre_remedio_diagnostico);

                //Si no esta continuar..
                if (val_remedio == null)
                {
                    Random rdm = new Random();

                    object id_analisis = obj2.Buscar_IdAnalisis_Nombre(lblPacienteAnalisis_P1.Content.ToString());
                    object id_paciente_nombrecompleto = obj2.Obtener_IdPaciente_NombreCompleto(lblNombre_Anal1.Content.ToString());

                    string id_remedio_generado = rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() +
                         rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() +
                          rdm.Next(0, 9).ToString() + "-" + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() +
                         rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() +
                          rdm.Next(0, 9).ToString();

                    //Crear registro de remedio proveniente de diagnostico
                    obj2.Registrar_Remedio_Diagnostico(id_remedio_generado, nombre_remedio_diagnostico, id_paciente_nombrecompleto.ToString(), lblPacienteAnalisis_P1.Content.ToString(), id_analisis.ToString(), lblNombre_Anal1.Content.ToString(), DateTime.Now, "");

                    //Ir a remedios y homeopatia
                    CargarListadoRemedios(); //Actualiza el listado de remedios por aquello que se agregarara uno

                    //opcionesHomoeonic.SelectedIndex = 2; 
                    CerrarConexion();
                }
                else
                {
                    MessageBox.Show(string.Join(nombre_remedio_diagnostico, obtenerRecurso("messageError41").Split('-')), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void txtbusquedaremedio_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtbusquedaremedio.Text != "")
            {
                listadoRemedios.Items.Clear();//Limpiar la caja de remedios

                HacerConexion();
                DataTable RemediosBuscados;
                RemediosBuscados = obj2.CodigoRemedioBuscado(txtbusquedaremedio.Text);

                //Agrega elementos al listbox
                for (int i = 0; i <= RemediosBuscados.Rows.Count - 1; i++)
                {
                    listadoRemedios.Items.Add(RemediosBuscados.Rows[i][1].ToString());
                }

                CerrarConexion();
            }
            else
            {
                CargarListadoRemedios();
            }
        }

        private void listadoCategorias_Remedios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void listadoCategorias_Remedios_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            listadoSubcategorias_Remedios.Items.Clear(); //Limpia antes de cada uso
            listadoCodigos_Remedios.Items.Clear();
            Categorias_Codigos.Clear();

            if (listadoCategorias_Remedios.SelectedItem != null)
            {

                try
                {
                    HacerConexion();
                    DataTable CategoriasCodigos;

                    //Buscar id_categoria para encontrar las subcategorias
                    object id_categoria = obj2.BuscarCategoriasCodigos(listadoCategorias_Remedios.SelectedItem.ToString());

                    DataTable SubCategorias = obj2.VisualizarSubCategoriasCodigos(id_categoria.ToString());

                    //Si no tienen subcategoria mostrarlos como codigos ya
                    if (SubCategorias.Rows.Count == 0)
                    {
                        CategoriasCodigos = obj2.VisualizarSubCategoriasCodigos2(id_categoria.ToString());

                        for (int y = 0; y <= CategoriasCodigos.Rows.Count - 1; y++)
                        {
                            if (CategoriasCodigos.Rows[y][1].ToString() != "")
                            {
                                //listadoCodigos.Items.Add(new CheckBox { Content = SubCategorias.Rows[y][1].ToString() });
                                listadoCodigos_Remedios.Items.Add(CategoriasCodigos.Rows[y][1].ToString());

                                Categorias_Codigos.Add(CategoriasCodigos.Rows[y][2].ToString()); //Guarda el codigo
                            }

                        }
                    }
                    else
                    {
                        //Agrega las categorias
                        for (int y = 0; y <= SubCategorias.Rows.Count - 1; y++)
                        {
                            if (SubCategorias.Rows[y][0].ToString() != "")
                            {
                                listadoSubcategorias_Remedios.Items.Add(SubCategorias.Rows[y][0].ToString());
                            }
                        }

                        // IMPORTANTE: categorias con subcategorias y codigos en su categoria

                        //Tambien puede haber categorias con subcategorias y codigos en su categoria...
                        CategoriasCodigos = obj2.VisualizarSubCategoriasCodigos2(id_categoria.ToString());

                        for (int y = 0; y <= CategoriasCodigos.Rows.Count - 1; y++)
                        {
                            if (CategoriasCodigos.Rows[y][1].ToString() != "")
                            {
                                //listadoCodigos.Items.Add(new CheckBox { Content = SubCategorias.Rows[y][1].ToString() });
                                listadoCodigos_Remedios.Items.Add(CategoriasCodigos.Rows[y][1].ToString());

                                Categorias_Codigos.Add(CategoriasCodigos.Rows[y][2].ToString()); //Guarda el codigo
                            }

                        }
                    }
                    CerrarConexion();
                }
                catch (NullReferenceException)
                {
                    //   MessageBox.Show("Por favor seleccione una categoría primero!", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void listadoSubcategorias_Remedios_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            listadoCodigos_Remedios.Items.Clear();
            Categorias_Codigos.Clear(); //Limpia los codigos guardados

            if (listadoSubcategorias_Remedios.SelectedItem != null)
            {
                try
                {
                    HacerConexion();
                    object id_categoria = obj2.BuscarCategoriasCodigos(listadoCategorias_Remedios.SelectedItem.ToString());

                    object id_subcategoria = obj2.BuscarCategoriasCodigosSub(listadoSubcategorias_Remedios.SelectedItem.ToString(), id_categoria.ToString());

                    /// AGREGADO
                    /// 
                    //Buscar el sexo del paciente
                    DataTable paciente_sexo_tabla = obj2.VisualizarAnalisisPorGenero();
                    string sexo = "";

                    for (int a = 0; a <= paciente_sexo_tabla.Rows.Count - 1; a++)
                    {
                        //Si es igual el nombre obtener el sexo
                        if (paciente_sexo_tabla.Rows[a][1].ToString() == lblPacienteAnalisis_P1.Content.ToString())
                        {
                            //MessageBox.Show(paciente_sexo_tabla.Rows[a][0].ToString());
                            if (paciente_sexo_tabla.Rows[a][0].ToString() == "Masculino")
                            {
                                sexo = "M";
                            }

                            if (paciente_sexo_tabla.Rows[a][0].ToString() == "Femenino")
                            {
                                sexo = "F";
                            }

                            if (paciente_sexo_tabla.Rows[a][0].ToString() == "Animal")
                            {
                                sexo = "A";
                            }

                            if (paciente_sexo_tabla.Rows[a][0].ToString() == "Plantas y suelo")
                            {
                                sexo = "P";
                            }

                        }
                    }
                    if (id_subcategoria.ToString() != null)
                    {

                        DataTable Codigos = obj2.VisualizarSubCategoriasCodigosListado(id_subcategoria.ToString(), sexo);

                        //MessageBox.Show(Codigos.Rows.Count.ToString());
                        //MessageBox.Show(id_subcategoria.ToString());

                        if (Codigos.Rows.Count == 0) //Sino hay por genero pues utiliza el de todos...
                        {
                            Codigos = obj2.VisualizarSubCategoriasCodigosListado(id_subcategoria.ToString(), "T");
                        }
                        else // De lo contrario si existen codigos por genero entonces.. que seria genero mas genero=T
                        {
                            Codigos = obj2.VisualizarSubCategoriasCodigosListadoGenero_Todos(id_subcategoria.ToString(), sexo);
                        }

                        //AGREGADO HASTA AQUI

                        for (int y = 0; y <= Codigos.Rows.Count - 1; y++)
                        {
                            if (Codigos.Rows[y][1].ToString() != "")
                            {
                                //listadoCodigos.Items.Add(new CheckBox { Content = Codigos.Rows[y][1].ToString() });
                                listadoCodigos_Remedios.Items.Add(Codigos.Rows[y][1].ToString());
                                Categorias_Codigos.Add(Codigos.Rows[y][2].ToString()); //Guarda el codigo
                            }
                        }

                    }
                    CerrarConexion();
                }
                catch (NullReferenceException)
                {
                    //  MessageBox.Show("Por favor seleccione una categoría primero!", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void listadoCodigos_Remedios_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (listadoCodigos_Remedios.SelectedItem != null)
            {
                try
                {
                    int index = listadoCodigos_Remedios.Items.IndexOf(listadoCodigos_Remedios.SelectedItem.ToString());
                    IEnumerable items = this.ListaRemedios.Items; //Remedios
                    nombrecodigo.Clear();
                    codigos_rates.Clear();

                    //Comprobar si ya estan agregados
                    foreach (nuevoRemedio codigo in items)
                    {
                        nombrecodigo.Add(codigo.nombrecodigo.ToString());
                        codigos_rates.Add(codigo.codigo.ToString());
                    }

                    if (ListaRemedios.Items.Count != 0)
                    {
                        //if (comboNiveles.SelectedIndex != -1)
                        //{
                        if (nombrecodigo.Contains(listadoCodigos_Remedios.SelectedItem.ToString()) == false)
                        {
                            // ListaCodigos.Items.Add(new nuevoCodigo { nombre = listadoCodigos.SelectedItem.ToString(), nivel="22", valor="22", nivelsugerido="22" });
                            ListaRemedios.Items.Add(new nuevoRemedio { nombrecodigo = listadoCodigos_Remedios.SelectedItem.ToString(), codigo = Categorias_Codigos[index], potencia = "-", metodo = "R", nivel = "1" });

                        }
                        //}
                        //else
                        // {
                        //  MessageBox.Show("Seleccione un nivel para los códigos!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                        // }
                    }
                    else
                    {
                        //if (comboNiveles.SelectedIndex != -1)
                        // {

                        if (lblFechaRemedioResp.Content.ToString() != "" && lblNombreRemedioResp.ToString() != "")
                        {
                            if (busqueda == true)
                            {
                                HacerConexion();
                                ListaRemedios.Items.Add(new nuevoRemedio { nombrecodigo = listadoCodigos_Remedios.SelectedItem.ToString(), codigo = obj2.BuscarCodigoPorNombreCategoria(listadoCodigos_Remedios.SelectedItem.ToString()).ToString(), potencia = "-", metodo = "R", nivel = "1" });
                                busqueda = false;
                                CerrarConexion();

                            }
                            else
                            {
                                ListaRemedios.Items.Add(new nuevoRemedio { nombrecodigo = listadoCodigos_Remedios.SelectedItem.ToString(), codigo = Categorias_Codigos[index], potencia = "-", metodo = "R", nivel = "1" });


                            }

                        }
                        else
                        {
                            MessageBox.Show(obtenerRecurso("messageInfo3"), "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                        }
                        // }
                        // else
                        //{
                        //     MessageBox.Show("Seleccione un nivel para los códigos!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        //}
                    }
                    lblContCodigosRemedios.Content = ListaRemedios.Items.Count;

                }
                catch (NullReferenceException)
                {
                    // MessageBox.Show("Please select a category an!", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void Grid_GotFocus(object sender, RoutedEventArgs e)
        {

            //Lista de pacientes
            CargarListadoCompletoPacientes();
        }

        private void listadoSubcategorias_Remedios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void listadoCodigos_Remedios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void txtCodigoBuscar_Remedios_TextChanged(object sender, TextChangedEventArgs e)
        {
            listadoCategorias_Remedios.SelectedIndex = -1;
            listadoSubcategorias_Remedios.SelectedIndex = -1;
            listadoCodigos_Remedios.SelectedIndex = -1;

            if (txtCodigoBuscar_Remedios.Text != "")
            {
                //Busqueda on
                busqueda = true;

                listadoCodigos_Remedios.Items.Clear();

                HacerConexion();

                DataTable Codigos = obj2.BuscarCodigo(txtCodigoBuscar_Remedios.Text);

                for (int y = 0; y <= Codigos.Rows.Count - 1; y++)
                {
                    if (Codigos.Rows[y][0].ToString() != "")
                    {
                        //listadoCodigos.Items.Add(new CheckBox { Content = Codigos.Rows[y][1].ToString() });
                        listadoCodigos_Remedios.Items.Add(Codigos.Rows[y][0].ToString());
                    }
                }

                CerrarConexion();
            }
        }

        private void cmdTodos_Remedios_Click(object sender, RoutedEventArgs e)
        {
            if (listadoCodigos_Remedios.Items.Count == 0)
            {
                MessageBox.Show(obtenerRecurso("messageWarning6"), obtenerRecurso("messageHeadWarning"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            else
            {
                //Elementos
                IEnumerable items = this.ListaRemedios.Items;
                nombrecodigo.Clear();
                codigos_rates.Clear();

                //Comprobar si ya estan agregados
                foreach (nuevoRemedio codigo in items)
                {
                    nombrecodigo.Add(codigo.nombrecodigo.ToString());
                    codigos_rates.Add(codigo.codigo.ToString());
                }

                if (ListaRemedios.Items.Count != 0)
                {
                    //Agregar todos los existentes en la listadecodigos
                    for (int w = 0; w <= listadoCodigos_Remedios.Items.Count - 1; w++)
                    {
                        if (nombrecodigo.Contains(listadoCodigos_Remedios.Items[w].ToString()) == false)
                        {
                            //Buscar codigo
                            ListaRemedios.Items.Add(new nuevoRemedio { nombrecodigo = listadoCodigos_Remedios.Items[w].ToString(), codigo = Categorias_Codigos[w], potencia = "1", metodo = "R", nivel = "1" });
                        }
                    }
                    lblContCodigosRemedios.Content = ListaRemedios.Items.Count;

                }
                else
                {

                    //Agregar todos los existentes en la listadecodigos
                    for (int w = 0; w <= listadoCodigos_Remedios.Items.Count - 1; w++)
                    {
                        //ListaCodigos.Items.Add(new nuevoCodigo { nombre = listadoCodigos.Items[w].ToString(), rates = obj2.BuscarCodigoPorNombreCategoria(listadoCodigos.Items[w].ToString()).ToString() });
                        ListaRemedios.Items.Add(new nuevoRemedio { nombrecodigo = listadoCodigos_Remedios.Items[w].ToString(), codigo = Categorias_Codigos[w], potencia = "1", metodo = "R", nivel = "1" });

                    }
                    lblContCodigosRemedios.Content = ListaRemedios.Items.Count;

                }
            }
        }

        private void cmdNinguno_Remedios_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable items = this.ListaRemedios.Items;
            nombrecodigo.Clear();
            codigos_rates.Clear();
            List<string> potencia_ord = new List<string>();
            List<string> metodo_ord = new List<string>();
            List<string> nivel_ord = new List<string>();

            foreach (nuevoRemedio codigo in items)
            {
                nombrecodigo.Add(codigo.nombrecodigo.ToString());
                codigos_rates.Add(codigo.codigo.ToString());
                potencia_ord.Add(codigo.potencia.ToString());
                metodo_ord.Add(codigo.metodo.ToString());
                nivel_ord.Add(codigo.nivel.ToString());
            }

            for (int i = 0; i <= listadoCodigos_Remedios.Items.Count - 1; i++)
            {
                if (nombrecodigo.Contains(listadoCodigos_Remedios.Items[i].ToString()) == true)
                {
                    int index = nombrecodigo.IndexOf(listadoCodigos_Remedios.Items[i].ToString());
                    nombrecodigo.RemoveAt(index);
                    codigos_rates.RemoveAt(index);
                    potencia_ord.RemoveAt(index);
                    metodo_ord.RemoveAt(index);
                    nivel_ord.RemoveAt(index);

                }

            }

            ListaRemedios.Items.Clear();

            for (int w = 0; w <= nombrecodigo.Count - 1; w++)
            {
                ListaRemedios.Items.Add(new nuevoRemedio { nombrecodigo = nombrecodigo[w], codigo = codigos_rates[w], potencia = potencia_ord[w], metodo = metodo_ord[w], nivel = nivel_ord[w] });
            }

            lblContCodigosRemedios.Content = ListaRemedios.Items.Count;

        }

        private void cmdQuitar_Click(object sender, RoutedEventArgs e)
        {
            //Elemento elegido
            //ListaCodigos.Items.IndexOf(ListaCodigos.SelectedItem);
            if (ListaRemedios.Items.Count != 0)
            {
                try
                {
                    if (ListaRemedios.SelectedItems.Count == 1)
                    {
                        ListaRemedios.Items.RemoveAt(ListaRemedios.Items.IndexOf(ListaRemedios.SelectedItem));
                    }
                    else
                    {
                        if (ListaRemedios.SelectedItems.Count != 0)
                        {
                            for (int q = 0; q <= ListaRemedios.SelectedItems.Count - 1; q++)
                            {
                                // MessageBox.Show(ListaCodigos.SelectedItems[q].ToString());
                                ListaRemedios.Items.Remove(ListaRemedios.SelectedItems[q]);
                            }
                            ListaRemedios.Items.RemoveAt(ListaRemedios.Items.IndexOf(ListaRemedios.SelectedItem));
                        }
                    }
                    // MessageBox.Show(ListaCodigos.SelectedItems.Count.ToString());
                    lblContCodigosRemedios.Content = ListaRemedios.Items.Count.ToString();
                }
                catch (NullReferenceException)
                {
                    MessageBox.Show(obtenerRecurso("messageError40"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                }
            }
        }

        private void comboOrdenarRemedios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListaRemedios.Items.Count == 0)
            {
                MessageBox.Show(obtenerRecurso("messageWarning5"), obtenerRecurso("messageHeadWarning"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            else
            {
                //Obtiene lo que ahi en el cuadro
                IEnumerable itemsCodigos = this.ListaRemedios.Items;

                List<nuevoRemedio> objetos_Codigos = new List<nuevoRemedio>();

                //Listas
                List<string> codigos_ord = new List<string>();
                List<string> nombres_ord = new List<string>();
                List<string> potencia_ord = new List<string>();
                List<string> metodo_ord = new List<string>();
                List<string> complementario_ord = new List<string>();
                List<string> nivel_ord = new List<string>();

                IEnumerable<nuevoRemedio> query;

                //De objetos los pasamos a listas
                foreach (nuevoRemedio codigo in itemsCodigos)
                {
                    objetos_Codigos.Add(codigo);
                }

                if (comboOrdenarRemedios.SelectedIndex != -1)
                {
                    string valor_combo = ((ComboBoxItem)comboOrdenarRemedios.SelectedItem).Content.ToString();

                    //Name - option
                    if (valor_combo == obtenerRecurso("tableName"))
                    {

                        //order by name
                        query = objetos_Codigos.OrderBy(codigo => codigo.nombrecodigo);

                        ListaRemedios.Items.Clear();

                        //reading
                        foreach (nuevoRemedio codigo in query)
                        {
                            codigos_ord.Add(codigo.codigo.ToString());
                            nombres_ord.Add(codigo.nombrecodigo.ToString());
                            potencia_ord.Add(codigo.potencia.ToString());
                            metodo_ord.Add(codigo.metodo.ToString());
                            complementario_ord.Add(codigo.codigocomplementario.ToString());
                            nivel_ord.Add(codigo.nivel.ToString());
                        }

                        for (int p = 0; p <= codigos_ord.Count - 1; p++)
                        {
                            ListaRemedios.Items.Add(new nuevoRemedio { nombrecodigo = nombres_ord[p], codigo = codigos_ord[p], potencia = potencia_ord[p], metodo = metodo_ord[p], codigocomplementario = complementario_ord[p], nivel = nivel_ord[p] });
                        }
                    }


                    if (valor_combo == obtenerRecurso("tablePotency"))
                    {
                        try
                        {
                            query = objetos_Codigos.OrderBy(codigo => double.Parse(codigo.potencia));

                            ListaRemedios.Items.Clear();

                            //reading
                            foreach (nuevoRemedio codigo in query)
                            {
                                codigos_ord.Add(codigo.codigo.ToString());
                                nombres_ord.Add(codigo.nombrecodigo.ToString());
                                potencia_ord.Add(codigo.potencia.ToString());
                                metodo_ord.Add(codigo.metodo.ToString());
                                complementario_ord.Add(codigo.codigocomplementario.ToString());
                                nivel_ord.Add(codigo.nivel.ToString());
                            }

                            for (int p = 0; p <= codigos_ord.Count - 1; p++)
                            {
                                ListaRemedios.Items.Add(new nuevoRemedio { nombrecodigo = nombres_ord[p], codigo = codigos_ord[p], potencia = potencia_ord[p], metodo = metodo_ord[p], codigocomplementario = complementario_ord[p], nivel = nivel_ord[p] });
                            }
                        }
                        catch (FormatException)
                        {
                            query = objetos_Codigos.OrderBy(codigo => codigo.potencia);

                            ListaRemedios.Items.Clear();

                            //reading
                            foreach (nuevoRemedio codigo in query)
                            {
                                codigos_ord.Add(codigo.codigo.ToString());
                                nombres_ord.Add(codigo.nombrecodigo.ToString());
                                potencia_ord.Add(codigo.potencia.ToString());
                                metodo_ord.Add(codigo.metodo.ToString());
                                complementario_ord.Add(codigo.codigocomplementario.ToString());
                                nivel_ord.Add(codigo.nivel.ToString());
                            }

                            for (int p = 0; p <= codigos_ord.Count - 1; p++)
                            {
                                ListaRemedios.Items.Add(new nuevoRemedio { nombrecodigo = nombres_ord[p], codigo = codigos_ord[p], potencia = potencia_ord[p], metodo = metodo_ord[p], codigocomplementario = complementario_ord[p], nivel = nivel_ord[p] });
                            }
                        }
                    }

                    if (valor_combo == obtenerRecurso("tableMethod"))
                    {
                        //Ordenar por nombre
                        query = objetos_Codigos.OrderBy(codigo => codigo.metodo);

                        ListaRemedios.Items.Clear();

                        //Lectura
                        foreach (nuevoRemedio codigo in query)
                        {
                            codigos_ord.Add(codigo.codigo.ToString());
                            nombres_ord.Add(codigo.nombrecodigo.ToString());
                            potencia_ord.Add(codigo.potencia.ToString());
                            metodo_ord.Add(codigo.metodo.ToString());
                            complementario_ord.Add(codigo.codigocomplementario.ToString());
                            nivel_ord.Add(codigo.nivel.ToString());
                        }

                        for (int p = 0; p <= codigos_ord.Count - 1; p++)
                        {
                            ListaRemedios.Items.Add(new nuevoRemedio { nombrecodigo = nombres_ord[p], codigo = codigos_ord[p], potencia = potencia_ord[p], metodo = metodo_ord[p], codigocomplementario = complementario_ord[p], nivel = nivel_ord[p] });
                        }
                    }

                }
            }
        }

        private void ListaRemedios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        //Exporta el codigo QR del remedio
        private void cmdQuitar_Click_1(object sender, RoutedEventArgs e)
        {
            //Manda id_remedio para generar el codigo QR
            HacerConexion();
            object id_remedio_s = obj2.Obtener_IdRemedio(listadoRemedios.SelectedItem.ToString()); //Id remedio

            //MessageBox.Show(id_remedio_s.ToString());
            HS5.Codigo vent = new HS5.Codigo(id_remedio_s.ToString());
            vent.Show();
            CerrarConexion();
        }

        /*
         * Seccion para generar codigo QR
         * */



        private void cmdDuplicar_Click(object sender, RoutedEventArgs e)
        {
            if (ListaRemedios.Items.Count != 0)
            {
                //Duplicar elemento
                //for (int i = 0; i <= ListaRemedios.SelectedItems.Count - 1; i++)
                IEnumerable itemsCodigosDup = this.ListaRemedios.SelectedItems;
                foreach (nuevoRemedio codi in itemsCodigosDup)
                {

                    ListaRemedios.Items.Add(new nuevoRemedio
                    {
                        nombrecodigo = codi.nombrecodigo + "- Dupl",
                        codigo = codi.codigo,
                        potencia = codi.potencia,
                        metodo = codi.metodo,
                        codigocomplementario = codi.codigocomplementario,
                        nivel = codi.nivel
                    });
                }

                //Ordena por nombre en automatico
                //Obtiene lo que ahi en el cuadro
                IEnumerable itemsCodigos = this.ListaRemedios.Items;
                List<nuevoRemedio> objetos_Codigos = new List<nuevoRemedio>();

                IEnumerable<nuevoRemedio> query;

                //De objetos los pasamos a listas
                foreach (nuevoRemedio codigo in itemsCodigos)
                {
                    objetos_Codigos.Add(codigo);
                }

                //Ordenar por nombre
                query = objetos_Codigos.OrderBy(codigo => codigo.nombrecodigo);

                ListaRemedios.Items.Clear();

                //De objetos los pasamos a listas
                foreach (nuevoRemedio remedio in query)
                {
                    ListaRemedios.Items.Add(remedio);
                }
            }

            lblContCodigosRemedios.Content = ListaRemedios.Items.Count;
        }

        private void txtBuscarCodigo1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ListaRemedios.Items.Count != 0)
            {
                IEnumerable itemsCodigos = this.ListaRemedios.Items;
                List<nuevoRemedio> lista_objetos = new List<nuevoRemedio>();

                //De objetos los pasamos a listas
                foreach (nuevoRemedio codigo in itemsCodigos)
                {
                    lista_objetos.Add(codigo);
                }

                //Sino esta vacio entonces hacemos...
                if (txtBuscarCodigo1.Text != "")
                {
                    ListaRemedios.Items.Clear();

                    string search = txtBuscarCodigo1.Text;
                    //Obtiene lista de objetos
                    IEnumerable<nuevoRemedio> results = lista_objetos.Where(codigo => codigo.nombrecodigo.Contains(txtBuscarCodigo1.Text));

                    foreach (nuevoRemedio codigo in results)
                    {
                        ListaRemedios.Items.Add(codigo);
                    }
                    lblContCodigosRemedios.Content = ListaRemedios.Items.Count; //Contador de codigos de remedios

                } else if (txtBuscarCodigo1.Text == "")
                {
                    // ListaRemedios.Items.Clear(); //Limpiar lista
                    HacerConexion();
                    string nombre = listadoRemedios.SelectedItem.ToString(); //Nombre seleccionado

                    object id_remedio = obj2.Obtener_IdRemedio(nombre); //Id remedio

                    //Obtener codigos de remedios en base a id remedio
                    DataTable CodigosdeRemedios = obj2.VisualizarCodigos_Remedios_IdRemedio(id_remedio.ToString());

                    for (int i = 0; i <= CodigosdeRemedios.Rows.Count - 1; i++)
                    {
                        ListaRemedios.Items.Add(new nuevoRemedio { nombrecodigo = CodigosdeRemedios.Rows[i][1].ToString(), codigo = CodigosdeRemedios.Rows[i][0].ToString(), potencia = CodigosdeRemedios.Rows[i][2].ToString(), metodo = CodigosdeRemedios.Rows[i][3].ToString(), codigocomplementario = CodigosdeRemedios.Rows[i][4].ToString(), nivel = CodigosdeRemedios.Rows[i][5].ToString() });

                    }

                    //Introduce detalles de los remedios
                    lblNombreRemedioResp.Content = listadoRemedios.SelectedItem.ToString();
                    lblFechaRemedioResp.Content = obj2.Buscar_Fecha_Remedio(listadoRemedios.SelectedItem.ToString()).ToString(); //Introduce fecha del remedio

                    lblContCodigosRemedios.Content = ListaRemedios.Items.Count; //Contador de codigos de remedios
                                                                                //comboOrdenarRemedios.SelectedIndex = -1;
                    CerrarConexion();

                }
            }

        }

        //Variable global para funciones
        bool function_activa = false;
        // En algún método dentro de tu archivo .cs
        

        public async void progressBarAnimation(int ms, string txt, string color, string _color)
        {
            LinearGradientBrush gradientBrush = new LinearGradientBrush();

            // Definir los puntos de inicio y fin del gradiente
            gradientBrush.StartPoint = new System.Windows.Point(0, 0);
            gradientBrush.EndPoint = new System.Windows.Point(1, 1);

            // Crear los dos colores del gradiente
            System.Windows.Media.Color color1 = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color);
            System.Windows.Media.Color color2 = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(_color); // Un tono más oscuro en hexadecimal sería "#FF3CB3DF"

            // Agregar los colores al gradiente
            gradientBrush.GradientStops.Add(new GradientStop(color1, 0.0));
            gradientBrush.GradientStops.Add(new GradientStop(color2, 1.0));

            // Asignar el LinearGradientBrush al Foreground del elemento deseado (por ejemplo, progressBar1)
            progressBar.Foreground = gradientBrush; // Reemplaza "tuLabel" con el nombre de tu Label
            /*
            // Convierte el valor hexadecimal a un objeto SolidColorBrush
            SolidColorBrush nuevoColor = (SolidColorBrush)new BrushConverter().ConvertFromString(color);

            // Asigna el nuevo color a la propiedad Foreground del Label
            progressBar.Foreground = nuevoColor; // Reemplaza "tuLabel" con el nombre de tu Label*/


            lblProgress.Content = txt;
            progressBar.Visibility = Visibility.Visible;
            lblProgress.Visibility = Visibility.Visible;
            lblPorcentProgress.Visibility = Visibility.Visible;

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
                string num = newValue.ToString("0") + "%";
                lblPorcentProgress.Content = num;
                // Pequeño retraso para actualizar la barra de progreso suavemente
                await Task.Delay(10);
            }

            // Ajustar el valor final a 100 en caso de que se haya excedido
            progressBar.Value = targetValue;
            progressClean();
            progressBar.Visibility = Visibility.Hidden;
            lblProgress.Visibility = Visibility.Hidden;
            lblPorcentProgress.Visibility = Visibility.Hidden;
        }


        public async void progressBarAnimationPGR(int ms, string txt)
        {

            LinearGradientBrush gradientBrush = new LinearGradientBrush();

            // Definir los puntos de inicio y fin del gradiente
            gradientBrush.StartPoint = new System.Windows.Point(0, 0);
            gradientBrush.EndPoint = new System.Windows.Point(1, 1);

            // Crear los dos colores del gradiente
            System.Windows.Media.Color color1 = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF3CB3DF");
            System.Windows.Media.Color color2 = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#16537e"); // Un tono más oscuro en hexadecimal sería "#FF3CB3DF"

            // Agregar los colores al gradiente
            gradientBrush.GradientStops.Add(new GradientStop(color1, 0.0));
            gradientBrush.GradientStops.Add(new GradientStop(color2, 1.0));

            // Asignar el LinearGradientBrush al Foreground del elemento deseado (por ejemplo, progressBar1)
            progressBar1.Foreground = gradientBrush; // Reemplaza "tuLabel" con el nombre de tu Label


            lblProgress1.Content = txt;
            progressBar1.Visibility = Visibility.Visible;
            lblProgress1.Visibility = Visibility.Visible;
            lblPorcentProgress1.Visibility = Visibility.Visible;

            // Valor final al que queremos llegar
            double targetValue = 100;

            // Valor inicial de la barra de progreso
            double startValue = progressBar1.Value;

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
                progressBar1.Value = newValue;
                string num = newValue.ToString("0") + "%";
                lblPorcentProgress1.Content = num;
                // Pequeño retraso para actualizar la barra de progreso suavemente
                await Task.Delay(10);
            }

            // Ajustar el valor final a 100 en caso de que se haya excedido
            progressBar1.Value = targetValue;
            progressBar1.Value = 0;
            progressBar1.Visibility = Visibility.Hidden;
            lblProgress1.Visibility = Visibility.Hidden;
            lblPorcentProgress1.Visibility = Visibility.Hidden;
        }

        public void progressClean()
        {
            progressBar.Value = 0;
           
        }

        //Funcion autosimile
        private void cmdAutosimile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (function_activa == false)
                {
                    //function_activa = true; //Activa la bandera

                    

                    //Si tiene o no tiene elementos
                    if (ListaRemedios.Items.Count != 0 || ListaRemedios.Items.Count == 0)
                    {
                        Random rdm = new Random();
                        Radionica auto = new Radionica();

                        //Codigo autosimile
                        string autosimile_codigo = auto.RandomDigits(rdm.Next(12, 18));

                        //Revisar si ya ahi un autosimile

                        IEnumerable itemsCodigos = this.ListaRemedios.Items;
                        List<nuevoRemedio> lista_objetos = new List<nuevoRemedio>();
                        List<string> lista_nombres = new List<string>();

                        //De objetos los pasamos a listas
                        foreach (nuevoRemedio codigo in itemsCodigos)
                        {
                            lista_objetos.Add(codigo);
                            lista_nombres.Add(codigo.nombrecodigo);
                        }

                        //Busqueda
                        IEnumerable<nuevoRemedio> results = lista_objetos.Where(codigo => codigo.nombrecodigo.Contains("Autosimile"));
                        int index = 0;

                        for (int i = 0; i <= lista_nombres.Count - 1; i++)
                        {
                            if (lista_nombres[i].Contains("Autosimile ") == true)
                            {
                                index = i;
                                break;
                            }
                        }

                        if (results.Count() > 0)
                        {
                            MessageBoxResult resp = MessageBox.Show(obtenerRecurso("messageWarning4"), obtenerRecurso("messageHeadWarning"), MessageBoxButton.YesNo, MessageBoxImage.Question);

                            if (resp == MessageBoxResult.Yes)
                            {
                                

                                progressBarAnimation(10000, "AUTOSIMIL", "#ffbd00", "#fcd66b");

                                new Thread((ThreadStart)delegate
                                {
                                    obj.Similie();
                                    Thread.Sleep(10000); //Tiempo

                                    Dispatcher.Invoke((ThreadStart)delegate
                                                {
                                                    ListaRemedios.Items.Remove(lista_objetos[index]); //Elimina objeto en base a index
                                                    ListaRemedios.Items.Add(new nuevoRemedio { codigo = autosimile_codigo, nombrecodigo = "Autosimile - " + DateTime.Now.ToString(), potencia = "1", metodo = "R", codigocomplementario = "-", nivel = "-" });
                                                    lblContCodigosRemedios.Content = ListaRemedios.Items.Count;
                                                    function_activa = false;
                                                    HS5.CustomMessageBox customMessageBox = new HS5.CustomMessageBox();
                                                    customMessageBox.Message = "Autosimil realizado correctamente";
                                                    customMessageBox.ShowDialog();
                                                });

                                }).Start();

                            }
                        }
                        else
                        {
                            progressBarAnimation(10000, obtenerRecurso("txtMessage1"), "#ffbd00", "#fcd66b");

                            new Thread((ThreadStart)delegate
                            {
                                obj.Similie();
                                Thread.Sleep(10000); //Tiempo

                                Dispatcher.Invoke((ThreadStart)delegate
                                            {
                                                ListaRemedios.Items.Add(new nuevoRemedio { codigo = autosimile_codigo, nombrecodigo = "Autosimile - " + DateTime.Now.ToString(), potencia = "1", metodo = "R", codigocomplementario = "-", nivel = "-" });
                                                lblContCodigosRemedios.Content = ListaRemedios.Items.Count;
                                                function_activa = false;
                                                HS5.CustomMessageBox customMessageBox = new HS5.CustomMessageBox();
                                                customMessageBox.Message = "Autosimil realizado correctamente";
                                                customMessageBox.ShowDialog();
                                            });

                            }).Start();
                            

                        }
                    }
                }
            }
            catch (Exception ex) { }
        }

        private void cmdNeutralizar_Click(object sender, RoutedEventArgs e)
        {
            if (function_activa == false)
            {
                function_activa = true;
                if (ListaRemedios.Items.Count != 0 || ListaRemedios.Items.Count == 0)
                {
                    progressBarAnimation(3000, obtenerRecurso("txtMessage2"), "#ff0000", "#ff6161");

                    new Thread((ThreadStart)delegate
                    {
                        obj.Neutralizando();
                        Thread.Sleep(3000); //Tiempo

                        Dispatcher.Invoke((ThreadStart)delegate
                                {
                                    loaderBack.Visibility = Visibility.Hidden;
                                    function_activa = false;
                                    HS5.CustomMessageBox customMessageBox = new HS5.CustomMessageBox();
                                    customMessageBox.Message = "Sustancia neutralizada correctamente";
                                    customMessageBox.ShowDialog();
                                });

                    }).Start();
                }
            }
        }

        private void cmdImprint_Click(object sender, RoutedEventArgs e)
        {
            if (function_activa == false)
            {
                function_activa = true;
                if (ListaRemedios.Items.Count != 0)
                {
                    progressBarAnimation(15000, obtenerRecurso("txtMessage3"), "#c90076", "#c15795");
                    

                    new Thread((ThreadStart)delegate
                    {
                        obj.Imprint();
                        Thread.Sleep(15000); //Tiempo

                        Dispatcher.Invoke((ThreadStart)delegate
                                {
                                    function_activa = false;
                                    HS5.CustomMessageBox customMessageBox = new HS5.CustomMessageBox();
                                    customMessageBox.Message = "Impresión realizada correctamente";
                                    customMessageBox.ShowDialog();

                                });

                    }).Start();
                }
            }
        }

        private void cmdCopiar_Click(object sender, RoutedEventArgs e)
        {
            if (function_activa == false)
            {
                if (ListaRemedios.Items.Count != 0)
                {
                    function_activa = true;
                    progressBarAnimation(15000, obtenerRecurso("txtMessage4"), "#ff0096", "#ff8dd0");
                    

                    new Thread((ThreadStart)delegate
                    {
                        obj.Copy();
                        Thread.Sleep(15000); //Tiempo

                        Dispatcher.Invoke((ThreadStart)delegate
                                {
                                    function_activa = false;
                                    HS5.CustomMessageBox customMessageBox = new HS5.CustomMessageBox();
                                    customMessageBox.Message = "Sustancia copiada correctamente";
                                    customMessageBox.ShowDialog();
                                });

                    }).Start();
                }
            }
        }

        private void cmdBorrarCodigo_Click(object sender, RoutedEventArgs e)
        {
            if (function_activa == false)
            {
                if (ListaRemedios.Items.Count != 0 || ListaRemedios.Items.Count == 0)
                {
                    function_activa = true;
                    progressBarAnimation(5000, obtenerRecurso("txtMessage5"), "#f44336", "#fa746a");

                    new Thread((ThreadStart)delegate
                    {
                        obj.Erase();
                        Thread.Sleep(5000); //Tiempo

                        Dispatcher.Invoke((ThreadStart)delegate
                                {
                                    loaderBack.Visibility = Visibility.Hidden;
                                    function_activa = false;
                                    HS5.CustomMessageBox customMessageBox = new HS5.CustomMessageBox();
                                    customMessageBox.Message = "Código de la tarjeta ECS borrado correctamente";
                                    customMessageBox.ShowDialog();
                                });

                    }).Start();
                }
            }
        }

        private void cmdGuardarCodigo_Click(object sender, RoutedEventArgs e)
        {
            if (function_activa == false)
            {

                if (ListaRemedios.Items.Count != 0)
                {
                    function_activa = true;
                    progressBarAnimation(5000, obtenerRecurso("txtMessage6"), "#ffc000", "#ffe599");
                   
                    new Thread((ThreadStart)delegate
                    {
                        obj.Save();
                        Thread.Sleep(5000); //Tiempo

                        Dispatcher.Invoke((ThreadStart)delegate
                                {
                                    loaderBack.Visibility = Visibility.Hidden;
                                    lblProgresRemedy.Visibility = Visibility.Hidden;
                                    function_activa = false;
                                    HS5.CustomMessageBox customMessageBox = new HS5.CustomMessageBox();
                                    customMessageBox.Message = "Código guardado en la tarjeta ECS correctamente";
                                    customMessageBox.ShowDialog();
                                });

                    }).Start();
                }
            }
        }

        public async void progressBarAnimationDT(int ms, string txt, string color)
        {
            SolidColorBrush nuevoColor = (SolidColorBrush)new BrushConverter().ConvertFromString(color);
            progressBar.Foreground = nuevoColor;

            lblProgress.Content = txt;
            progressBar.Visibility = Visibility.Visible;
            lblProgress.Visibility = Visibility.Visible;
            lblPorcentProgress.Visibility = Visibility.Visible;

            double targetValue = 100;
            double startValue = progressBar.Value;
            double incrementPerMillisecond = (targetValue - startValue) / ms;
            DateTime startTime = DateTime.Now;

            while ((DateTime.Now - startTime).TotalMilliseconds < ms && isAnimationActive)
            {
                double elapsedTimeMilliseconds = (DateTime.Now - startTime).TotalMilliseconds;
                double newValue = startValue + (incrementPerMillisecond * elapsedTimeMilliseconds);
                progressBar.Value = newValue;

                // Calcula el tiempo restante en minutos y segundos
                double remainingSeconds = (ms - elapsedTimeMilliseconds) / 1000;
                int remainingMinutes = (int)(remainingSeconds / 60);
                int remainingSecondsDisplay = (int)(remainingSeconds % 60);

                // Formatea el tiempo restante en una cadena
                string remainingTime = $"{remainingMinutes:00}:{remainingSecondsDisplay:00}";

                // Actualiza el contenido del TextBlock con el tiempo restante
                lblPorcentProgress.Content = remainingTime;

                await Task.Delay(10);
            }

            progressBar.Value = targetValue;
            progressClean();
            progressBar.Visibility = Visibility.Hidden;
            lblProgress.Visibility = Visibility.Hidden;
            lblPorcentProgress.Visibility = Visibility.Hidden;
        }

        System.Windows.Threading.DispatcherTimer Timer_segundos = new System.Windows.Threading.DispatcherTimer();
        System.Windows.Threading.DispatcherTimer Timer_minutos = new System.Windows.Threading.DispatcherTimer();

        double minutos_tratamiento;
        double segundos_tratamiento = 60;
        bool isAnimationActive = true;
        private void cmdTratamientoDirecto_Click(object sender, RoutedEventArgs e)
        {
            isAnimationActive = true;
            function_activa = true;
            if (function_activa == true)
            {

                if (ListaRemedios.Items.Count != 0)
                {
                    try
                    {

                        minutos_tratamiento = Double.Parse(Interaction.InputBox(obtenerRecurso("messageQuestion11"), obtenerRecurso("messageHeadQ7")," ", 300, 300));
                        //minutos_tratamiento -= 1; //Baja un minuto para coincidir valor
                        //Mostrar tiempo

                        if (minutos_tratamiento != 0 || minutos_tratamiento != -1)
                        {
                            //cmdTratamientoDirecto.IsEnabled = false;
                            opcionesHomoeonic.IsEnabled = false;

                            lblProgresRemedy.Text = "RUNNING DIRECT TREATMENT - " + minutos_tratamiento.ToString() + ":" + segundos_tratamiento.ToString();

                            //Timer 
                            Timer_segundos.Tick += new EventHandler(Timer2_Tick);
                            Timer_segundos.Interval = new TimeSpan(0, 0, 0, 0, 1000);
                            Timer_segundos.Start();

                            //El metodo timer tick maneja todo el control del tratamiento directo...
                            Timer_minutos.Tick += new EventHandler(Timer1_Tick);
                            Timer_minutos.Interval = new TimeSpan(0, 0, 0, 0, 60000);
                            Timer_minutos.Start();

                            //Barra de progreso
                            int tratamientoenms = Convert.ToInt32(minutos_tratamiento) * 60 * 1000;
                            progressBarAnimationDT(tratamientoenms, obtenerRecurso("progressBar3"), "#8ED6FF");
                            cmdTerminarDiag.Visibility = Visibility.Visible;
                            obj.Diagnostic();
                            //Lo inicia
                            
                        }
                        else
                        {
                            opcionesHomoeonic.IsEnabled = true;
                            // Detener y reiniciar Timer_segundos
                            Timer_segundos.Stop();
                            Timer_segundos.Interval = TimeSpan.Zero;

                            // Detener y reiniciar Timer_minutos
                            Timer_minutos.Stop();
                            Timer_minutos.Interval = TimeSpan.Zero;

                            function_activa = false;
                        }

                        function_activa = false;

                    }
                    catch (FormatException)
                    {
                        //MessageBox.Show("Please type a numeric value only!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            
        }

        public void cmdTerminarDiagnostico()
        {
            isAnimationActive = false;
            //Para ambos timers
            // Detener y reiniciar Timer_segundos
            Timer_segundos.Stop();
            Timer_segundos.Interval = TimeSpan.Zero;

            // Detener y reiniciar Timer_minutos
            Timer_minutos.Stop();
            Timer_minutos.Interval = TimeSpan.Zero;


            //Elimina todo
            new Thread((ThreadStart)delegate
            {
                Dispatcher.Invoke((ThreadStart)delegate
                {
                    cmdTerminarDiag.Visibility = Visibility.Hidden;
                    lblPorcentProgress.Visibility = Visibility.Hidden;
                    lblProgress.Visibility = Visibility.Hidden;
                    progressBar.Visibility = Visibility.Hidden;
                    cmdTerminarDiag.Visibility = Visibility.Hidden;
                    function_activa = false;
                    opcionesHomoeonic.IsEnabled = true;
                    HS5.CustomMessageBox customMessageBox = new HS5.CustomMessageBox();
                    customMessageBox.Message = "Tratamiento terminado";
                    customMessageBox.ShowDialog();
                });

            }).Start();
        }
        private void cmdTerminarDiag_Click(object sender, RoutedEventArgs e)
        {
            cmdTerminarDiagnostico();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            minutos_tratamiento -= 1;

            //Actualizara minutos
            lblProgresRemedy.Text = "RUNNING DIRECT TREATMENT - " + minutos_tratamiento.ToString() + ":" + segundos_tratamiento.ToString();

            if (minutos_tratamiento == 0)
            {
                Timer_minutos.Stop();
                Timer_segundos.Stop(); //Para los segundos

                //Elimina todo
                new Thread((ThreadStart)delegate
                {
                    Dispatcher.Invoke((ThreadStart)delegate
                    {
                        loaderBack.Visibility = Visibility.Hidden;
                        cmdTerminarDiag.Visibility = Visibility.Hidden;
                        opcionesHomoeonic.IsEnabled = true;
                        obj.Diagnostic();
                        function_activa = true;
                    });

                }).Start();

                cmdTerminarDiagnostico();
                

            }
        }

        private void Timer2_Tick(object sender, EventArgs e)
        {
            segundos_tratamiento -= 1; //Baja los segundos

            //Actualizara segundos
            lblProgresRemedy.Text = "RUNNING DIRECT TREATMENT - " + minutos_tratamiento.ToString() + ":" + segundos_tratamiento.ToString();

            if (segundos_tratamiento == 0)
            {
                segundos_tratamiento = 60;
            }
        }

        private void cmdNuevoRemedio_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string nombre_remedio_diagnostico;
                Random num = new Random();
                Radionica obj_rem = new Radionica();

                nombre_remedio_diagnostico = Interaction.InputBox(obtenerRecurso("messageQuestion3"), obtenerRecurso("messageHeadQ3"), "", 300, 300);

                if (nombre_remedio_diagnostico == "")
                {
                    MessageBox.Show(obtenerRecurso("messageError39"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    //Grabar registro en bd del inicio del remedio va a tabla rad_remedios
                    //try
                    //{
                    HacerConexion();

                    string id_remedio_generado = obj_rem.Generar_Id();
                    string codigo = obj2.Generarcodigoremedios();

                    //Crear registro de remedio proveniente de diagnostico
                    obj2.Registrar_Remedio_Diagnostico(id_remedio_generado, nombre_remedio_diagnostico, "", "", "", "", DateTime.Now, codigo);
                    /* }
                     catch(Exception err)
                     {
                         MessageBox.Show(err.ToString());
                     }*/
                    //Ir a remedios y homeopatia
                    CargarListadoRemedios(); //Actualiza el listado de remedios por aquello que se agregarara uno
                                             //opcionesHomoeonic.SelectedIndex = 2; 
                    CerrarConexion();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void cmdBorrarRemedio_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Obtener remedio seleccionado nombre
                string remedio = listadoRemedios.SelectedItem.ToString();

                HacerConexion();

                obj2.Eliminar_remedio_Nombre(remedio);

                CargarListadoRemedios();

                CerrarConexion();

            }
            catch (NullReferenceException)
            {
                MessageBox.Show(obtenerRecurso("messageError37"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        //Codigo complementario
        private void cmdCodigoComp_Click(object sender, RoutedEventArgs e)
        {
            if (ListaRemedios.Items.Count > 0)
            {
                //Selecciona un elemento o varios para poder efectuar el codigo complementario
                try
                {
                    Radionica obj_com = new Radionica();
                    int contador_codigos_seleccionados = ListaRemedios.SelectedItems.Count;


                    if (contador_codigos_seleccionados != 0 && contador_codigos_seleccionados <= 5)
                    {
                        //Obtiene los remedios que ahi en el cuadro
                        IEnumerable itemsCodigos = this.ListaRemedios.Items;
                        List<int> indexes = new List<int>();

                        //Listas
                        List<string> codigos_ord = new List<string>();
                        List<string> nombres_ord = new List<string>();
                        List<string> potencia_ord = new List<string>();
                        List<string> metodo_ord = new List<string>();
                        List<string> complementario_ord = new List<string>();
                        List<string> nivel_ord = new List<string>();

                        //Pasa todo a la lista
                        foreach (nuevoRemedio codigo in itemsCodigos)
                        {    //Guardamos complementarios para contar                               
                            complementario_ord.Add(codigo.codigocomplementario);
                        }

                        int contador_complementarios = 0;
                        //Contar cuantos ya complementarios se tienen
                        for (int p = 0; p <= complementario_ord.Count - 1; p++)
                        {
                            if (complementario_ord[p] != "")
                            {
                                //Sumame uno
                                contador_complementarios++;
                            }
                        }

                        if (contador_complementarios >= 5)
                        {
                            MessageBox.Show(obtenerRecurso("messageError38"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            complementario_ord.Clear(); //Limpiamos porque ya se uso

                            //De objetos los pasamos a listas
                            int acum = -1;

                            foreach (nuevoRemedio codigo in itemsCodigos)
                            {
                                acum = acum + 1;

                                for (int i = 0; i <= contador_codigos_seleccionados - 1; i++)
                                {
                                    //Leemos remedios de la lista y los comparamos con los seleccionados
                                    if (codigo == ListaRemedios.SelectedItems[i])
                                    {
                                        indexes.Add(acum); //Guardamos los indexes
                                    }
                                }

                                //Guardamos en listas todos los remedios de las listas
                                codigos_ord.Add(codigo.codigo);
                                nombres_ord.Add(codigo.nombrecodigo);
                                potencia_ord.Add(codigo.potencia);
                                metodo_ord.Add(codigo.metodo);
                                complementario_ord.Add(codigo.codigocomplementario);
                                nivel_ord.Add(codigo.nivel);
                            }

                            //Calculamos los codigos complementarios de los seleccionados
                            for (int w = 0; w <= indexes.Count - 1; w++)
                            {
                                if (codigos_ord[indexes[w]] == "50")
                                {
                                    complementario_ord[indexes[w]] = "90"; //Excepcion del cancer
                                }
                                else
                                {
                                    //obj_com.CodigoComplementario(codigos_ord[indexes[w]]);
                                    complementario_ord[indexes[w]] = obj_com.CodigoComplementario(codigos_ord[indexes[w]]);
                                }
                            }

                            //Borramos la lista de remedios sin codigos complementarios
                            ListaRemedios.Items.Clear();

                            //Cargamos con codigos complementarios
                            for (int i = 0; i <= codigos_ord.Count - 1; i++)
                            {
                                ListaRemedios.Items.Add(new nuevoRemedio { codigo = codigos_ord[i], nombrecodigo = nombres_ord[i], potencia = potencia_ord[i], metodo = metodo_ord[i], codigocomplementario = complementario_ord[i], nivel = nivel_ord[i] });
                            }

                            lblLeyenda.Visibility = Visibility.Visible; //Activa la leyenda
                        }
                    }
                }
                catch (NullReferenceException)
                {
                    MessageBox.Show(obtenerRecurso("messageError37"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                }
            }
        }

        private void checkOpcion3_Checked(object sender, RoutedEventArgs e)
        {
            DataTable RemediosLista = new DataTable();
            listadoRemedios.Items.Clear(); //Limpiar la lista

            HacerConexion();
            RemediosLista = obj2.VisualizarRemediosCreados_Usuario();

            //Agrega elementos al listbox
            for (int i = 0; i <= RemediosLista.Rows.Count - 1; i++)
            {
                listadoRemedios.Items.Add(RemediosLista.Rows[i][1].ToString());
            }
            CerrarConexion();
        }

        private void cmdAlmacenartarjetaCat_Click(object sender, RoutedEventArgs e)
        {
            if (ListaRemedios.Items.Count != 0 || ListaRemedios.Items.Count == 0)
            {
                string nombre_tarjeta;

                nombre_tarjeta = Interaction.InputBox(obtenerRecurso("messageQuestion2"), "Name", "", 300, 300);

                if (nombre_tarjeta == "")
                {
                    MessageBox.Show(obtenerRecurso("messageError36"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    HacerConexion();
                    Radionica obj_temp = new Radionica();
                    Random num = new Random();

                    //Mandar grabar en bd 
                    obj2.Registrar_Tarjeta_Categorias(obj_temp.Generar_Id(), nombre_tarjeta, obj_temp.RandomDigits(num.Next(12, 16)).ToString());

                    MessageBox.Show(obtenerRecurso("messageInfo2"), "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    CerrarConexion();
                }
            }
        }

        private void cmdPotenciacionAvanzada_Click(object sender, RoutedEventArgs e)
        {
            if (ListaRemedios.Items.Count != 0)
            {
                if (ListaRemedios.SelectedItems.Count > 0)
                {
                    try
                    {
                        ListaRemedios.Visibility = Visibility.Hidden;
                        cmdCodigo.Visibility = Visibility.Visible;
                        lblMultiples_Copy1.Visibility = Visibility.Visible;
                        cmdCodigo_Copy.Visibility = Visibility.Visible;
                        lblMultiples_Copy.Visibility = Visibility.Visible;
                        checkBox.Visibility = Visibility.Visible;
                        checkBox_Copy.Visibility = Visibility.Visible;
                        cmdCodigo_Copy1.Visibility = Visibility.Visible;
                        txtPotencia.Visibility = Visibility.Visible;
                        cmdCodigo_Copy2.Visibility = Visibility.Visible;
                        comboMetodo.Visibility = Visibility.Visible;
                        cmdCodigo_Copy3.Visibility = Visibility.Visible;
                        cmdNivel.Visibility = Visibility.Visible;
                        cmdAplicar.Visibility = Visibility.Visible;
                        cmdCancelar.Visibility = Visibility.Visible;

                        lblMultiples_Copy.Content = "There are [" + ListaRemedios.SelectedItems.Count.ToString() + "] rates selected";
                        checkBox.IsChecked = true;

                    }
                    catch (NullReferenceException)
                    {
                        MessageBox.Show(obtenerRecurso("messageError35"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    }
                }
            }
            else
            {
                MessageBox.Show(obtenerRecurso("messageError35"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmdAplicar_Click(object sender, RoutedEventArgs e)
        {
            if (txtPotencia.Text != "" && comboMetodo.SelectedIndex != -1 && cmdNivel.SelectedIndex != -1)
            {
                try
                {

                    //Casteo de la potencia
                    double casteo = Double.Parse(txtPotencia.Text);

                    Ocultar_Potenciacion();

                    //Obtener los elementos
                    IEnumerable itemsCodigos = this.ListaRemedios.Items;
                    List<int> indexes = new List<int>();

                    //Listas
                    List<string> codigos_ord = new List<string>();
                    List<string> nombres_ord = new List<string>();
                    List<string> potencia_ord = new List<string>();
                    List<string> metodo_ord = new List<string>();
                    List<string> complementario_ord = new List<string>();
                    List<string> nivel_ord = new List<string>();

                    if (checkBox.IsChecked == true)
                    {
                        //De objetos los pasamos a listas
                        int acum = -1;
                        //Buscamos los indexes de los seleccionados
                        foreach (nuevoRemedio codigo in itemsCodigos)
                        {
                            acum = acum + 1;

                            for (int i = 0; i <= ListaRemedios.SelectedItems.Count - 1; i++)
                            {
                                //Leemos remedios de la lista y los comparamos con los seleccionados
                                if (codigo == ListaRemedios.SelectedItems[i])
                                {
                                    indexes.Add(acum); //Guardamos los indexes
                                }
                            }

                            //Guardamos en listas todos los remedios de las listas
                            codigos_ord.Add(codigo.codigo);
                            nombres_ord.Add(codigo.nombrecodigo);
                            potencia_ord.Add(codigo.potencia);
                            metodo_ord.Add(codigo.metodo);
                            complementario_ord.Add(codigo.codigocomplementario);
                            nivel_ord.Add(codigo.nivel);
                        }

                        //Meter los datos de la potencializacion elegida
                        for (int w = 0; w <= indexes.Count - 1; w++)
                        {
                            //Potencia
                            potencia_ord[indexes[w]] = txtPotencia.Text;

                            //Metodo
                            metodo_ord[indexes[w]] = ((ComboBoxItem)comboMetodo.SelectedItem).Content.ToString();

                            //Nivel
                            nivel_ord[indexes[w]] = ((ComboBoxItem)cmdNivel.SelectedItem).Content.ToString();

                        }

                        //Borramos la lista de remedios sin codigos complementarios
                        ListaRemedios.Items.Clear();

                        //Cargamos con cambios integrados
                        for (int i = 0; i <= codigos_ord.Count - 1; i++)
                        {
                            ListaRemedios.Items.Add(new nuevoRemedio { codigo = codigos_ord[i], nombrecodigo = nombres_ord[i], potencia = potencia_ord[i], metodo = metodo_ord[i], codigocomplementario = complementario_ord[i], nivel = nivel_ord[i] });
                        }
                    }
                    else
                    {
                        //Buscamos los indexes de los seleccionados
                        foreach (nuevoRemedio codigo in itemsCodigos)
                        {
                            //Guardamos en listas todos los remedios de las listas
                            codigos_ord.Add(codigo.codigo);
                            nombres_ord.Add(codigo.nombrecodigo);
                            potencia_ord.Add(codigo.potencia);
                            metodo_ord.Add(codigo.metodo);
                            complementario_ord.Add(codigo.codigocomplementario);
                            nivel_ord.Add(codigo.nivel);
                        }

                        //Meter los datos de la potencializacion elegida
                        for (int w = 0; w <= codigos_ord.Count - 1; w++)
                        {
                            //Potencia
                            potencia_ord[w] = txtPotencia.Text;

                            //Metodo
                            metodo_ord[w] = ((ComboBoxItem)comboMetodo.SelectedItem).Content.ToString();

                            //Nivel
                            nivel_ord[w] = ((ComboBoxItem)cmdNivel.SelectedItem).Content.ToString();

                        }

                        //Borramos la lista de remedios sin codigos complementarios
                        ListaRemedios.Items.Clear();

                        //Cargamos con cambios integrados
                        for (int i = 0; i <= codigos_ord.Count - 1; i++)
                        {
                            ListaRemedios.Items.Add(new nuevoRemedio { codigo = codigos_ord[i], nombrecodigo = nombres_ord[i], potencia = potencia_ord[i], metodo = metodo_ord[i], codigocomplementario = complementario_ord[i], nivel = nivel_ord[i] });
                        }
                    }

                    txtPotencia.Text = "1";
                    comboNiveles.SelectedIndex = -1;
                    comboP.SelectedIndex= -1;
                    cmdNivel.SelectedIndex = -1;
                    checkBox.IsChecked = false;
                    checkBox.IsChecked = false;
                }
                catch (FormatException)
                {
                    MessageBox.Show(obtenerRecurso("messageError34"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show(obtenerRecurso("messageError33"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }

        }

        void Ocultar_Potenciacion()
        {
            ListaRemedios.Visibility = Visibility.Visible;
            cmdCodigo.Visibility = Visibility.Hidden;
            lblMultiples_Copy1.Visibility = Visibility.Hidden;
            cmdCodigo_Copy.Visibility = Visibility.Hidden;
            lblMultiples_Copy.Visibility = Visibility.Hidden;
            checkBox.Visibility = Visibility.Hidden;
            checkBox_Copy.Visibility = Visibility.Hidden;
            cmdCodigo_Copy1.Visibility = Visibility.Hidden;
            txtPotencia.Visibility = Visibility.Hidden;
            cmdCodigo_Copy2.Visibility = Visibility.Hidden;
            comboMetodo.Visibility = Visibility.Hidden;
            cmdCodigo_Copy3.Visibility = Visibility.Hidden;
            cmdNivel.Visibility = Visibility.Hidden;
            cmdAplicar.Visibility = Visibility.Hidden;
            cmdCancelar.Visibility = Visibility.Hidden;

        }

        private void cmdCancelar_Click(object sender, RoutedEventArgs e)
        {
            txtPotencia.Text = "1";
            comboNiveles.SelectedIndex = -1;
            comboP.SelectedIndex = -1;
            cmdNivel.SelectedIndex = -1;
            checkBox.IsChecked = false;
            checkBox.IsChecked = false;
            Ocultar_Potenciacion();
        }

        private void checkBox_Copy_Checked(object sender, RoutedEventArgs e)
        {
            lblMultiples_Copy.Content = "There are [" + ListaRemedios.Items.Count.ToString() + "] rates selected";

        }

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            lblMultiples_Copy.Content = "There are [" + ListaRemedios.SelectedItems.Count.ToString() + "] rates selected";

        }

        private void cmdDuplicarRemedio_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Grid_LostFocus(object sender, RoutedEventArgs e)
        {
            //Aparecer controles
            /* ControlRemedios.Visibility = Visibility.Hidden;
             lblNombreRemedioResp.Visibility = Visibility.Hidden;
             lblFechaRemedioResp.Visibility = Visibility.Hidden;
             lblFechaRemedio.Visibility = Visibility.Hidden;
             lblNombreRemedio.Visibility = Visibility.Hidden;
             cmdDuplicarRemedio.Visibility = Visibility.Hidden;
             listadoRemedios.SelectedIndex = -1;*/
        }

        private void cmdDuplicarRemedio_Click_1(object sender, RoutedEventArgs e)
        {
            //Duplicar el remedio
            string nombre_copia;

            nombre_copia = Interaction.InputBox(obtenerRecurso("inputMessageHQ1"), obtenerRecurso("inputMessage6"), "", 300, 300);

            if (nombre_copia != "")
            {
                string nombre_elemento_acopiar = lblNombreRemedioResp.Content.ToString();

                HacerConexion();
                //Trae - idr,nombre,idpaciente,nombrepaciente,idanalisis,nombreanalisis,fechac,codigo
                DataTable elemento_acopiar = obj2.Consultar_Remedio_Duplicar(nombre_elemento_acopiar);

                //Compruebo que no exista el nombre registrado
                object cantidad_mismo_nombre = obj2.Consultar_NombreDuplicado(nombre_copia);
                int cantidad = Int32.Parse(cantidad_mismo_nombre.ToString()); //Castea el valor del count a numerico

                //Si existen mas de 0 registros mostrar mensaje
                if (cantidad > 0)
                {
                    MessageBox.Show(obtenerRecurso("messageError32"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    Radionica obj_1 = new Radionica();
                    Random rdm = new Random();
                    string id_generado = obj_1.Generar_Id();

                    //Registrar_Remedio_Duplicado
                    //Envia - idr,nombre,idpaciente,nombrepaciente,idanalisis,nombreanalisis,fechac,codigo
                    obj2.Registrar_Remedio_Duplicado(id_generado, nombre_copia, elemento_acopiar.Rows[0][2].ToString(), elemento_acopiar.Rows[0][3].ToString(), elemento_acopiar.Rows[0][4].ToString(), elemento_acopiar.Rows[0][5].ToString(), Convert.ToDateTime(elemento_acopiar.Rows[0][6].ToString()), elemento_acopiar.Rows[0][7].ToString());

                    // codigo,nombrecodigo,idcodigo,potencia,metodo,codigocomplementario,nivel

                    //Codigos de remedios mandamos duplicar tambien en base al elegido
                    //Trae - codigo,nombrecodigo,idcodigo,potencia,metodo,codigocomplementario,nivel
                    DataTable codigosremedios_acopiar = obj2.VisualizarCodigos_Remedios_IdRemedio2(elemento_acopiar.Rows[0][0].ToString()); //Id_del original a ser copiado para obtener codigos de remedios

                    //Recorrer todos los codigos de remedios a copiar
                    for (int q = 0; q <= codigosremedios_acopiar.Rows.Count - 1; q++)
                    {
                        string id_gen = rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() +
                      rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() +
                       rdm.Next(0, 9).ToString() + "-" + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() +
                      rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() +
                       rdm.Next(0, 9).ToString();
                        //Trae - codigo,nombrecodigo,idcodigo,potencia,metodo,codigocomplementario,nivel
                        //Envia - codigo,codigocomplementario,nombrecodigo,idcodigo,potencia,metodo,nivel
                        obj2.Registrar_CodigosdeRemedios(id_gen, id_generado, codigosremedios_acopiar.Rows[q][0].ToString(), codigosremedios_acopiar.Rows[q][5].ToString(), codigosremedios_acopiar.Rows[q][1].ToString(), codigosremedios_acopiar.Rows[q][2].ToString(), codigosremedios_acopiar.Rows[q][3].ToString(), codigosremedios_acopiar.Rows[q][4].ToString(), codigosremedios_acopiar.Rows[q][6].ToString());
                    }

                    MessageBox.Show(obtenerRecurso("messageInfo1"), "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                CargarListadoRemedios(); //Actualiza el listado de remedios para que aparezca el duplicado
                ListaRemedios.Items.Clear();
                CerrarConexion();
            }
            else
            {
                MessageBox.Show(obtenerRecurso("messageError31"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TerapiaColor_GotFocus(object sender, RoutedEventArgs e)
        {

        }

        string id_categoria_padre;

        public void ClearData(System.Windows.Controls.ListView lv)
        {
            // Obtener la DataView subyacente del DataTable
            lv.ItemsSource = null;
        }
        
        public void ClearDataLb(System.Windows.Controls.ListBox lb)
        {
            lb.Items.Clear();
        }


        private void listadoCategorias_Copy_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ClearData(listadoCodigos_Copy);
            listadoSubcategorias_Copy.Items.Clear(); //Limpia antes de cada uso
            listadoCodigos_Copy.Items.Clear();
            Categorias_Codigos2.Clear();

            if (listadoCategorias_Copy.SelectedItem != null)
            {
                try
                {

                    HacerConexion();
                    DataTable CategoriasCodigos;

                    //Buscar id_categoria para encontrar las subcategorias
                    object id_categoria = obj2.BuscarCategoriasCodigos(listadoCategorias_Copy.SelectedItem.ToString());
                    // MessageBox.Show(id_categoria.ToString());
                    id_categoria_padre = id_categoria.ToString(); //Guarda la categoria padre para el nuevo codigo
                    DataTable SubCategorias = obj2.VisualizarSubCategoriasCodigos(id_categoria.ToString());

                    //Si no tienen subcategoria mostrarlos como codigos ya
                    if (SubCategorias.Rows.Count == 0)
                    {
                        CategoriasCodigos = obj2.VisualizarSubCategoriasCodigos2(id_categoria.ToString());
                        DataTable dtc = new DataTable();
                        dtc.Columns.Add("Id", typeof(string));
                        dtc.Columns.Add("Nombre", typeof(string));
                        dtc.Columns.Add("Categoria", typeof(string));
                        dtc.Columns.Add("SubCategoria", typeof(string));

                        // Llenar el DataTable con los datos de CategoriasCodigos
                        for (int y = 0; y < CategoriasCodigos.Rows.Count; y++)
                        {
                            if (!string.IsNullOrEmpty(CategoriasCodigos.Rows[y][1].ToString()))
                            {
                                string id = (CategoriasCodigos.Rows[y][1].ToString());
                                string nombre = CategoriasCodigos.Rows[y][2].ToString();
                                string categoria = listadoCategorias_Copy.SelectedItem.ToString();
                                string subcategoria = null;
                                dtc.Rows.Add(nombre, id, categoria, subcategoria);
                                Categorias_Codigos2.Add(CategoriasCodigos.Rows[y][2].ToString()); //Guarda el codigo
                            }
                        }

                        // Asignar el DataTable como origen de datos para la ListView
                        listadoCodigos_Copy.ItemsSource = dtc.AsDataView();

                        lblSubcategoriasCont.Content = listadoSubcategorias_Copy.Items.Count +" "+obtenerRecurso("labelSubCat");
                        lblCodigosCont.Content = listadoCodigos_Copy.Items.Count +" "+obtenerRecurso("labelRate");

                    }
                    else
                    {
                        //Agrega las categorias
                        for (int y = 0; y <= SubCategorias.Rows.Count - 1; y++)
                        {
                            if (SubCategorias.Rows[y][0].ToString() != "")
                            {
                                listadoSubcategorias_Copy.Items.Add(SubCategorias.Rows[y][0].ToString());
                            }
                        }

                        lblSubcategoriasCont.Content = listadoSubcategorias_Copy.Items.Count + " "+ obtenerRecurso("labelSubCat");

                        // IMPORTANTE: categorias con subcategorias y codigos en su categoria

                        //Tambien puede haber categorias con subcategorias y codigos en su categoria...
                        CategoriasCodigos = obj2.VisualizarSubCategoriasCodigos2(id_categoria.ToString());

                        for (int y = 0; y <= CategoriasCodigos.Rows.Count - 1; y++)
                        {
                            if (CategoriasCodigos.Rows[y][1].ToString() != "")
                            {
                                //listadoCodigos.Items.Add(new CheckBox { Content = SubCategorias.Rows[y][1].ToString() });
                                
                                //listadoCodigos_Copy.Items.Add(CategoriasCodigos.Rows[y][1].ToString() + " , " + CategoriasCodigos.Rows[y][2].ToString());
                                
                                Categorias_Codigos2.Add(CategoriasCodigos.Rows[y][2].ToString() + " , " + CategoriasCodigos.Rows[y][2].ToString()); //Guarda el codigo
                            }
                        }

                        lblCodigosCont.Content = listadoCodigos_Copy.Items.Count + " " + obtenerRecurso("labelRate");

                    }
                    CerrarConexion();
                }
                catch (NullReferenceException)
                {
                    // MessageBox.Show("Error seleccione una categoría antes de continuar!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        void Cargar_Subcategorias()
        {
            listadoSubcategorias_Copy.Items.Clear(); //Limpia antes de cada uso
            listadoCodigos_Copy.Items.Clear();
            Categorias_Codigos2.Clear();

            HacerConexion();
            DataTable CategoriasCodigos;

            //Buscar id_categoria para encontrar las subcategorias
            object id_categoria = obj2.BuscarCategoriasCodigos(listadoCategorias_Copy.SelectedItem.ToString());

            DataTable SubCategorias = obj2.VisualizarSubCategoriasCodigos(id_categoria.ToString());

            //Si no tienen subcategoria mostrarlos como codigos ya
            if (SubCategorias.Rows.Count == 0)
            {
                CategoriasCodigos = obj2.VisualizarSubCategoriasCodigos2(id_categoria.ToString());

                for (int y = 0; y <= CategoriasCodigos.Rows.Count - 1; y++)
                {
                    if (CategoriasCodigos.Rows[y][1].ToString() != "")
                    {
                        //listadoCodigos.Items.Add(new CheckBox { Content = SubCategorias.Rows[y][1].ToString() });
                        listadoCodigos_Copy.Items.Add(CategoriasCodigos.Rows[y][1].ToString() + " , " + CategoriasCodigos.Rows[y][2].ToString());

                        Categorias_Codigos2.Add(CategoriasCodigos.Rows[y][2].ToString()); //Guarda el codigo
                    }

                }
                lblSubcategoriasCont.Content = listadoSubcategorias_Copy.Items.Count + " "+ obtenerRecurso("labelSubCat");
                lblCodigosCont.Content = listadoCodigos_Copy.Items.Count + " "+ obtenerRecurso("labelRate");

            }
            else
            {
                //Agrega las categorias
                for (int y = 0; y <= SubCategorias.Rows.Count - 1; y++)
                {
                    if (SubCategorias.Rows[y][0].ToString() != "")
                    {
                        listadoSubcategorias_Copy.Items.Add(SubCategorias.Rows[y][0].ToString() + SubCategorias.Rows[y][1].ToString());
                    }
                }

                lblSubcategoriasCont.Content = listadoSubcategorias_Copy.Items.Count + " " +obtenerRecurso("labelSubCat");

                // IMPORTANTE: categorias con subcategorias y codigos en su categoria

                //Tambien puede haber categorias con subcategorias y codigos en su categoria...
                CategoriasCodigos = obj2.VisualizarSubCategoriasCodigos2(id_categoria.ToString());

                for (int y = 0; y <= CategoriasCodigos.Rows.Count - 1; y++)
                {
                    if (CategoriasCodigos.Rows[y][1].ToString() != "")
                    {
                        //listadoCodigos.Items.Add(new CheckBox { Content = SubCategorias.Rows[y][1].ToString() });
                        listadoCodigos_Copy.Items.Add(CategoriasCodigos.Rows[y][1].ToString() + ", " + CategoriasCodigos.Rows[y][2].ToString());

                        Categorias_Codigos2.Add(CategoriasCodigos.Rows[y][2].ToString()); //Guarda el codigo
                    }
                }

                lblCodigosCont.Content = listadoCodigos_Copy.Items.Count + " "+ obtenerRecurso("labelRate");

            }
            CerrarConexion();
        }

        private void listadoCategorias_Copy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Grid_GotFocus_1(object sender, RoutedEventArgs e)
        {
            //Cargar_Categorias();
            CargarListadoCompletoPacientes();
        }

        void Cargar_Categorias()
        {
            HacerConexion();
            DataTable Categorias = obj2.VisualizarCategoriasCodigos2();

            //listadoCategorias_Copy.Items.Clear(); //Limpiamos lista
            if (listadoCategorias_Copy.Items.Count == 0)
            {
                //Cargar categorias
                for (int i = 0; i <= Categorias.Rows.Count - 1; i++)
                {
                    if (listadoCategorias_Copy.Items.Contains(Categorias.Rows[i][1].ToString()) == false)
                    {
                        listadoCategorias_Copy.Items.Add(Categorias.Rows[i][1].ToString());
                    }
                }
                lblCategoriasCont.Content = listadoCategorias_Copy.Items.Count + " "+ obtenerRecurso("Categories");

            }
            CerrarConexion();
        }

        string id_categoria_cop;
        //DataTable codigos_cop;
        private void listadoSubcategorias_Copy_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ClearData(listadoCodigos_Copy);
            listadoCodigos_Copy.Items.Clear();
            Categorias_Codigos2.Clear(); //Limpia los codigos guardados

            if (listadoSubcategorias_Copy.SelectedItem != null)
            {
                try
                {
                    HacerConexion();
                    object id_categoria = obj2.BuscarCategoriasCodigos(listadoCategorias_Copy.SelectedItem.ToString());

                    object id_subcategoria = obj2.BuscarCategoriasCodigosSub(listadoSubcategorias_Copy.SelectedItem.ToString(), id_categoria.ToString());

                    DataTable Codigos = obj2.VisualizarSubCategoriasCodigosListado(id_subcategoria.ToString(), "T");
                    
                    if (Codigos.Rows.Count == 0) 
                    {
                        Codigos = obj2.VisualizarSubCategoriasCodigosListado(id_subcategoria.ToString(), "T");
                    }
                    
                    // Crear un nuevo DataTable
                    DataTable dtc = new DataTable();
                    dtc.Columns.Add("Id", typeof(string));
                    dtc.Columns.Add("Nombre", typeof(string));
                    dtc.Columns.Add("Categoria", typeof(string));
                    dtc.Columns.Add("SubCategoria", typeof(string));

                    // Llenar el DataTable con los datos de Codigos
                    for (int y = 0; y < Codigos.Rows.Count; y++)
                    {
                        if (!string.IsNullOrEmpty(Codigos.Rows[y][1].ToString()))
                        {
                            string id = Codigos.Rows[y][1].ToString();
                            string nombre = Codigos.Rows[y][2].ToString();
                            string catego = obj2.Categoria(id_categoria.ToString());
                            string subcat = listadoSubcategorias_Copy.SelectedItem.ToString();
                            // Agregar una nueva fila al DataTable
                            dtc.Rows.Add(nombre, id, catego, subcat);

                            // Guardar el código
                            Categorias_Codigos2.Add(id);
                        }
                    }

                    // Establecer el DataTable como origen de datos para la ListView
                    listadoCodigos_Copy.ItemsSource = dtc.DefaultView;

                    // Asignar el DataTable como origen de datos para la ListView
                    listadoCodigos_Copy.ItemsSource = dtc.AsDataView();

                    lblSubcategoriasCont.Content = listadoSubcategorias_Copy.Items.Count + " " + obtenerRecurso("labelSubCat");
                    lblCodigosCont.Content = listadoCodigos_Copy.Items.Count + " " + obtenerRecurso("labelRate");
                  

                    CerrarConexion();
                }
                catch (NullReferenceException)
                {
                    // MessageBox.Show("Por favor seleccione una categoría primero!", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void listadoSubcategorias_Copy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void txtBuscarBase_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        private void radioButton_Checked(object sender, RoutedEventArgs e)
        {
            //listVista.Items.Clear();
            if (radioMasculino.IsChecked == true)
            {
                ImageBrush brush = new ImageBrush();
                BitmapImage neww = ToBitmapImage(HS5.Properties.Resources.Cuerpo_HFront_500);
                brush.ImageSource = neww;
                canvas.Background = brush;
                listaSecciones.Items.Clear();
                listaSecciones.SelectedIndex = 0;
            }
        }

        void Cargar_TerapiaDefault()
        {
            //Mostrar imagen por defecto - hombre terapia de color
            ImageBrush brush = new ImageBrush();
            BitmapImage neww = ToBitmapImage(HS5.Properties.Resources.Cuerpo_HFront_500);
            brush.ImageSource = neww;
            canvas.Background = brush;

            radioMasculino.IsChecked = true;
            if (radioMasculino.IsChecked == true)
            {
                listVista.Items.Add(obtenerRecurso("valFront"));
                listVista.Items.Add(obtenerRecurso("valBack"));
            }
            listVista.SelectedIndex = 0;
            listaSecciones.SelectedIndex = -1;
        }

        private void radioFemenino_Checked(object sender, RoutedEventArgs e)
        {
            //listVista.Items.Clear();
            if (radioFemenino.IsChecked == true)
            {
                ImageBrush brush = new ImageBrush();
                BitmapImage neww = ToBitmapImage(HS5.Properties.Resources.Mujer_front_500);
                brush.ImageSource = neww;
                canvas.Background = brush;
                listaSecciones.Items.Clear();
                listaSecciones.SelectedIndex = 0;
            }
        }

        private void cmdNuevo_Click(object sender, RoutedEventArgs e)
        {
            //Duplicar el remedio
            string nombre_codigo;
            //string codigo; int codigo_num; string description;

            nombre_codigo = Interaction.InputBox(obtenerRecurso("inputMessage3"), obtenerRecurso("inputHeadMessage2"), "", 300, 300);

            if (nombre_codigo != "")
            {
                // codigo = Interaction.InputBox("Category", "New Category", "", 300, 300);
                // codigo_num = Int32.Parse(codigo);
                try
                {
                    // codigo_num = Int32.Parse(codigo);

                    // description = Interaction.InputBox("Description", "New Category", "", 300, 300);
                    HacerConexion();

                    //Objeto
                    Radionica obj_new = new Radionica();

                    //Genero
                    //object genero_para_codigo = obj2.Buscar_Genero(id_categoria_cop, id_categoria_padre);
                    string id_categoria_l = obj_new.Generar_Id();
                    //  obj2.Registrar_Categorias(id_categoria_l, nombre_codigo, id_categoria_l);
                    listadoCategorias_Remedios.Items.Clear();
                    obj2.Registrar_Categorias(id_categoria_l, nombre_codigo, "");
                    //Cargar_Codigos(id_categoria_padre, id_categoria_cop); //Carga los codigos actualizados con el agregado
                   // obj2.VisualizarCategoriasCodigos();

                    CerrarConexion();

                    listadoCategorias_Copy.Items.Clear();
                    Cargar_Categorias(); //Cargamos categorias

                }
                catch (FormatException)
                {
                    // MessageBox.Show("Only numbers are allowed", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                // MessageBox.Show("Please write the name of the category!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmdBorrar1_Click(object sender, RoutedEventArgs e)
        {
            //Seleccionar el elemento a eliminar
            try
            {
                string elemento_borrar = listadoCategorias_Copy.SelectedItem.ToString();

                //Eliminar objeto
                HacerConexion();
                obj2.Eliminar_Categoria(elemento_borrar);

                //Actualizar el listado de categorias
                listadoCategorias_Copy.Items.Clear();
                Cargar_Categorias();
                // lblCategoriasCont.Content = listadoCategorias_Copy.Items.Count + " Categorías";

                CerrarConexion();
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(obtenerRecurso("messageError30"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        private void cmdNueva_Click(object sender, RoutedEventArgs e)
        {
            //Agregar una subcategoria
            //Duplicar el remedio
            string nombre_subcategoria;
            //string codigo; int codigo_num; string description;

            nombre_subcategoria = Interaction.InputBox(obtenerRecurso("inputMessage4"), obtenerRecurso("inputHeadMessage3"), "", 300, 300);


            if (nombre_subcategoria != "")
            {
                // codigo = Interaction.InputBox("Category", "New Category", "", 300, 300);
                // codigo_num = Int32.Parse(codigo);
                try
                {
                    // codigo_num = Int32.Parse(codigo);

                    // description = Interaction.InputBox("Description", "New Category", "", 300, 300);
                    HacerConexion();

                    //Id de la categoria padre
                    object id_categoria_p = obj2.BuscarCategoriasCodigos(listadoCategorias_Copy.SelectedItem.ToString());

                    //Objeto
                    Radionica obj_new = new Radionica();


                    obj2.Registrar_Categorias(obj_new.Generar_Id(), nombre_subcategoria, id_categoria_p.ToString());

                    CerrarConexion();

                    Cargar_Codigos(id_categoria_padre, id_categoria_cop); //Carga los codigos actualizados con el agregado

                }
                catch (FormatException)
                {
                    //MessageBox.Show("Only numbers are allowed", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
              //  MessageBox.Show("Please write the name for the Sub-Category", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmdBorrarSub_Click(object sender, RoutedEventArgs e)
        {
            //Seleccionar el elemento a eliminar
            try
            {
                string elemento_borrar = listadoSubcategorias_Copy.SelectedItem.ToString();

                //Eliminar objeto
                HacerConexion();
                //Buscar el id de la categoria
                object id_categoria = obj2.BuscarCategoriasCodigos(listadoCategorias_Copy.SelectedItem.ToString());

                obj2.Eliminar_SubCategoria(elemento_borrar, id_categoria.ToString());

                CerrarConexion();
                Cargar_Subcategorias();
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(obtenerRecurso("messageError29"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        public static BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }


        public static Bitmap ChangeColor(Bitmap scrBitmap, System.Drawing.Color color)
        {
            //Color que cambiara...
            System.Drawing.Color newColor = color;
            //Color anterior ...
            System.Drawing.Color actualColor;
            //make an empty bitmap the same size as scrBitmap
            Bitmap newBitmap = new Bitmap(scrBitmap.Width, scrBitmap.Height);
            for (int i = 0; i < scrBitmap.Width; i++)
            {
                for (int j = 0; j < scrBitmap.Height; j++)
                {
                    //get the pixel from the scrBitmap image
                    actualColor = scrBitmap.GetPixel(i, j);
                    // > 150 because.. Images edges can be of low pixel colr. if we set all pixel color to new then there will be no smoothness left.
                    if (actualColor.A > 150)
                        newBitmap.SetPixel(i, j, newColor);
                    else
                        newBitmap.SetPixel(i, j, actualColor);
                }
            }
            return newBitmap;
        }


        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private void cmdGuardarTratamiento_Click(object sender, RoutedEventArgs e)
        {
            string nombre_tratamiento_color;
            Random num = new Random();
            Radionica obj_rem = new Radionica();

            if (partes_colores.Count != 0)
            {
                nombre_tratamiento_color = Interaction.InputBox(obtenerRecurso("inputMessageHQ"), obtenerRecurso("inputMessage5"), "", 300, 300);

                if (nombre_tratamiento_color == "")
                {
                    MessageBox.Show(obtenerRecurso("messageError28"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    Radionica obj1 = new Radionica();
                    HacerConexion();

                    string id_remedio_genrand = obj1.Generar_Id();

                    //Registra el tratamiento a color como remedio (Color -{nombre del tratamiento}
                    obj2.Registrar_Remedio_Color(id_remedio_genrand, "Color - " + nombre_tratamiento_color, DateAndTime.Now);

                    //Registrar codigo de remedio - campos: idcr,idr,codigo,codigocomplementario,nombrecodigo,idcodigo,potencia,metodo,nivel (METODO OSVALDO)
                    //Codigo del tratamiento general generado a nivel random
                    //obj2.Registrar_CodigosdeRemedios(obj1.Generar_Id(),id_remedio_genrand, obj1.RandomDigits(num.Next(12, 16)),"","TC-COLOR "+nombre_tratamiento_color,"","1","R","");

                    IEnumerable codigos_combinados = this.listadoCodigosComb.Items;

                    foreach (nuevoColorCombinado codigocomb in codigos_combinados)
                    {
                        //Metodo alternativo (Por colores y por partes del cuerpo)
                        //  for (int i = 0; i <= partes_colores.Count - 1; i++)
                        //    {
                        //Partes con los codigos y colores
                        // obj2.Registrar_CodigosdeRemedios(,,)
                        obj2.Registrar_CodigosdeRemedios(obj1.Generar_Id(), id_remedio_genrand, codigocomb.codigocomp.ToString(), "", codigocomb.nombrecodigocomp.ToString(), "", "1", "R", "");
                        // obj2.Registrar_CodigosdeRemedios(obj1.Generar_Id(), id_remedio_genrand, "TC-COLOR " + cod_partes_colores[i], "", partes_colores[i], "", "1", "R", "");
                        //   }
                    }

                    CerrarConexion();
                }
            }
            else
            {
                MessageBox.Show(obtenerRecurso("messageError27"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            //Limpiar campos e imagenes
            Reset_images();
            listadoCodigosComb.Items.Clear();

            //Limpiar listas para evitar mezclar con antiguas terapias
            partes_colores.Clear();
            cod_partes_color.Clear();
            cod_partes_colores.Clear();

            //Limpiar color elegido
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFF7F7F6");
            color_elegido.Content = "";
        }

        private void cmdQuitarColores_Click(object sender, RoutedEventArgs e)
        {
            Reset_images();
            listadoCodigosComb.Items.Clear();

            //Limpiar listas para evitar mezclar con antiguas terapias
            partes_colores.Clear();
            cod_partes_color.Clear();
            cod_partes_colores.Clear();
        }

        //Funcion que carga las terapias recientes
        void CargarTerapiasRecientes()
        {
            HacerConexion();

            DataTable terapiasrecientes = obj2.VisualizarTerapiasdeColor(); //nombre y fecha

            //Limpiamos el listview
            ListaPacientes_Recientes1_Copy.Items.Clear();

            //Las metemos al listview
            for (int i = 0; i <= terapiasrecientes.Rows.Count - 1; i++)
            {
                ListaPacientes_Recientes1_Copy.Items.Add(new nuevaTerapia { nombre = terapiasrecientes.Rows[i][0].ToString(), fecha = terapiasrecientes.Rows[i][1].ToString() });
            }

            CerrarConexion();
        }

        //private void part1_MouseMove(object sender, MouseEventArgs e)
        //{

        //}

        private void TerapiaColor_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //  MessageBox.Show(Mouse.GetPosition(Application.Current.MainWindow).X.ToString() + "," + Mouse.GetPosition(Application.Current.MainWindow).Y.ToString());

        }

        void Reset_images()
        {

            part1.Source = null;
            part2.Source = null;
            part3.Source = null;
            part4.Source = null;
            part5.Source = null;
            part6.Source = null;
            part7.Source = null;
            part8.Source = null;
            part9.Source = null;
            part10.Source = null;
            part11.Source = null;
            part12.Source = null;
            part13.Source = null;
            part14.Source = null;
            part15.Source = null;
            part16.Source = null;
            part17.Source = null;
            part18.Source = null;
            part19.Source = null;
            part20.Source = null;
            part21.Source = null;
            part22.Source = null;
            part23.Source = null;
            part24.Source = null;
            part25.Source = null;
            part26.Source = null;
            part27.Source = null;
            part28.Source = null;
            part29.Source = null;
            part30.Source = null;
            part31.Source = null;
            part32.Source = null;
            part33.Source = null;
            part34.Source = null;
            part35.Source = null;
            part36.Source = null;
            part37.Source = null;
            part38.Source = null;
            part39.Source = null;
            part40.Source = null;
            part41.Source = null;
            part42.Source = null;
            part43.Source = null;
            part44.Source = null;
            part45.Source = null;
            part46.Source = null;
            part47.Source = null;
            part48.Source = null;
            part49.Source = null;
            part50.Source = null;
            part51.Source = null;
            part52.Source = null;
            part53.Source = null;
            part54.Source = null;
            part55.Source = null;
            part56.Source = null;
            part57.Source = null;
            part58.Source = null;
            part59.Source = null;
            part60.Source = null;
            part61.Source = null;
            part62.Source = null;
        }

        private void listVista_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Hombre parte frontal
            if (listVista.SelectedItem.ToString() == obtenerRecurso("valFront") && radioMasculino.IsChecked == true)
            {
                ImageBrush brush = new ImageBrush();
                BitmapImage neww = ToBitmapImage(HS5.Properties.Resources.Cuerpo_HFront_500);
                brush.ImageSource = neww;
                canvas.Background = brush;
                listaSecciones.Items.Clear();

                Reset_images();

                //Agregar secciones
                string[] parts_body = new string[]
                                                   {
                                                     "Abdomen",
                                                     "Left-Forearm",
                                                     "Right-Forearm",
                                                     "Mouth",
                                                     "Right-Arm",
                                                     "Left-Arm",
                                                     "Hair",
                                                     "Right-Hip",
                                                     "Left-Hip",
                                                     "Face",
                                                     "Right-Clavicle",
                                                     "Left-Clavicle",
                                                     "Right-Ribs",
                                                     "Left-Ribs",
                                                     "Right-Neck",
                                                     "Left-Neck",
                                                     "Right-Hand-Fingers",
                                                     "Left-Hand-Fingers",
                                                     "Esophagus",
                                                     "Stomach",
                                                     "Left-Hand-Phalanges",
                                                     "Right-Hand-Phalanges",
                                                     "Forehead",
                                                     "Throat",
                                                     "Glans", // Consider replacing with a more appropriate term if needed
                                                     "Right-Shoulder",
                                                     "Left-Shoulder",
                                                     "Jaw",
                                                     "Right-Hand",
                                                     "Left-Hand",
                                                     "Right-Thigh",
                                                     "Left-Thigh",
                                                     "Nose",
                                                     "Right-Ear",
                                                     "Left-Ear",
                                                     "Right-Eye",
                                                     "Left-Eye",
                                                     "Umbilicus",
                                                     "Right-Calf",
                                                     "Left-Calf",
                                                     "Right-Eyelid",
                                                     "Left-Eyelid",
                                                     "Chest",
                                                     "Penis", // Consider replacing with a more appropriate term if needed
                                                     "Right-Nipple",
                                                     "Left-Nipple",
                                                     "Right-Foot",
                                                     "Left-Foot",
                                                     "Right-Thumb",
                                                     "Left-Thumb",
                                                     "Right-Big Toe",
                                                     "Left-Big Toe",
                                                     "Right-Knee",
                                                     "Left-Knee",
                                                     "Right-Testicle", // Consider replacing with a more appropriate term if needed
                                                     "Left-Testicle", // Consider replacing with a more appropriate term if needed
                                                     "Bladder"
                                                   };
                foreach (var item in parts_body)
                {
                    listaSecciones.Items.Add(obtenerRecurso(item));
                }


            }


            if (listVista.SelectedItem.ToString() == obtenerRecurso("valBack") && radioMasculino.IsChecked == true)
            {
                ImageBrush brush = new ImageBrush();
                BitmapImage neww = ToBitmapImage(HS5.Properties.Resources.Color_Hombre_Back_500);
                brush.ImageSource = neww;
                canvas.Background = brush;
                listaSecciones.Items.Clear();
                Reset_images();

                string[] parts_body = {
                            "Anus",
                            "Right-Forearm",
                            "Left-Forearm",
                            "Right-Arm",
                            "Left-Arm",
                            "Head",
                            "Right-Hip",
                            "Left-Hip",
                            "Right-Clavicle",
                            "Left-Clavicle",
                            "Right-Elbow",
                            "Left-Elbow",
                            "Column",
                            "Coxis",
                            "Neck",
                            "Right-Fingers",
                            "Left-Fingers",
                            "Back",
                            "Right-Phalanges",
                            "Left-Phalanges",
                            "Right-Shoulder",
                            "Left-Shoulder",
                            "Right-Hand",
                            "Left-Hand",
                            "Right-Wrist",
                            "Left-Wrist",
                            "Right-Thigh",
                            "Left-Thigh",
                            "Right-Buttock",
                            "Left-Buttock",
                            "Right-Calf",
                            "Left-Calf",
                            "Right-Foot",
                            "Left-Foot",
                            "Right-Leg",
                            "Left-Leg",
                            "Right-Thumb",
                            "Left-Thumb",
                            "Left-Heel",
                            "Right-Heel",
                            "Right-Achilles heel",
                            "Left-Achilles heel",
                            "Right-Ankle",
                            "Left-Ankle"
                          };

                foreach (var item in parts_body)
                {
                    listaSecciones.Items.Add(obtenerRecurso(item));
                }

            
            }

            //Sexo Femenino
            if (listVista.SelectedItem.ToString() == obtenerRecurso("valFront") && radioFemenino.IsChecked == true)
            {
                ImageBrush brush = new ImageBrush();
                BitmapImage neww = ToBitmapImage(HS5.Properties.Resources.Mujer_front_500);
                brush.ImageSource = neww;
                canvas.Background = brush;
                listaSecciones.Items.Clear();

                Reset_images();

                //Agregar secciones
                string[] parts_body = new string[]
                                                  {
                                                   "Abdomen",
                                                   "Left-Forearm",
                                                   "Right-Forearm",
                                                   "Right-Areola",
                                                   "Left-Areola",
                                                   "Mouth",
                                                   "Right-Arm",
                                                   "Left-Arm",
                                                   "Hair",
                                                   "Right-Hip",
                                                   "Left-Hip",
                                                   "Face",
                                                   "Right-Clavicle",
                                                   "Left-Clavicle",
                                                   "Right-Ribs",
                                                   "Left-Ribs",
                                                   "Right-Neck",
                                                   "Left-Neck",
                                                   "Right-Hand Fingers",
                                                   "Left-Hand Fingers",
                                                   "Esophagus",
                                                   "Stomach",
                                                   "Left-Hand Phalanges",
                                                   "Right-Hand Phalanges",
                                                   "Forehead",
                                                   "Throat",
                                                   "Right-Shoulder",
                                                   "Left-Shoulder",
                                                   "Jaw",
                                                   "Right-Hand",
                                                   "Left-Hand",
                                                   "Right-Thigh",
                                                   "Left-Thigh",
                                                   "Nose",
                                                   "Right-Ear",
                                                   "Left-Ear",
                                                   "Right-Eye",
                                                   "Left-Eye",
                                                   "Umbilicus",
                                                   "Right-Calf",
                                                   "Left-Calf",
                                                   "Right-Eyelid",
                                                   "Left-Eyelid",
                                                   "Right-Chest",
                                                   "Left-Chest",
                                                   "Chest",
                                                   "Right-Nipple",
                                                   "Left-Nipple",
                                                   "Right-Foot",
                                                   "Left-Foot",
                                                   "Right-Hand Thumb",
                                                   "Left-Hand Thumb",
                                                   "Right Big Toe",
                                                   "Left Big Toe",
                                                   "Right-Knee",
                                                   "Left-Knee",
                                                   "Right-Ankle",
                                                   "Left-Ankle",
                                                   "Uterus",
                                                   "Vagina",
                                                   "Bladder"
                                                  };

                foreach (var item in parts_body)
                {
                    listaSecciones.Items.Add(obtenerRecurso(item));
                }


            }


            if (listVista.SelectedItem.ToString() == obtenerRecurso("valBack") && radioFemenino.IsChecked == true)
            {
                ImageBrush brush = new ImageBrush();
                BitmapImage neww = ToBitmapImage(HS5.Properties.Resources.Fback_500);
                brush.ImageSource = neww;
                canvas.Background = brush;

                listaSecciones.Items.Clear();
                Reset_images();
                string[] parts_body = {
                            "Anus",
                            "Right-Forearm",
                            "Left-Forearm",
                            "Right-Arm",
                            "Left-Arm",
                            "Head",
                            "Right-Hip",
                            "Left-Hip",
                            "Right-Clavicle",
                            "Left-Clavicle",
                            "Right-Elbow",
                            "Left-Elbow",
                            "Column",
                            "Coxis",
                            "Neck",
                            "Right-Fingers",
                            "Left-Fingers",
                            "Back",
                            "Right-Phalanges",
                            "Left-Phalanges",
                            "Right-Shoulder",
                            "Left-Shoulder",
                            "Right-Hand",
                            "Left-Hand",
                            "Right-Wrist",
                            "Left-Wrist",
                            "Right-Thigh",
                            "Left-Thigh",
                            "Right-Buttock",
                            "Left-Buttock",
                            "Right-Calf",
                            "Left-Calf",
                            "Right-Foot",
                            "Left-Foot",
                            "Right-Leg",
                            "Left-Leg",
                            "Right-Thumb",
                            "Left-Thumb",
                            "Left-Heel",
                            "Right-Heel",
                            "Right-Achilles heel",
                            "Left-Achilles heel",
                            "Right-Ankle",
                            "Left-Ankle"
                          };

                foreach (var item in parts_body)
                {
                    listaSecciones.Items.Add(obtenerRecurso(item));
                }

               
            }

        }

        void Partes()
        {
            //Hombre parte frontal
            if (listVista.SelectedItem.ToString() == obtenerRecurso("valFront") && radioMasculino.IsChecked == true)
            {
                ImageBrush brush = new ImageBrush();
                BitmapImage neww = ToBitmapImage(HS5.Properties.Resources.Cuerpo_HFront_500);
                brush.ImageSource = neww;
                canvas.Background = brush;
                listaSecciones.Items.Clear();

                Reset_images();

                string[] parts_body = new string[]
                                              {
                                                     "Abdomen",
                                                     "Left-Forearm",
                                                     "Right-Forearm",
                                                     "Mouth",
                                                     "Right-Arm",
                                                     "Left-Arm",
                                                     "Hair",
                                                     "Right-Hip",
                                                     "Left-Hip",
                                                     "Face",
                                                     "Right-Clavicle",
                                                     "Left-Clavicle",
                                                     "Right-Ribs",
                                                     "Left-Ribs",
                                                     "Right-Neck",
                                                     "Left-Neck",
                                                     "Right-Hand-Fingers",
                                                     "Left-Hand-Fingers",
                                                     "Esophagus",
                                                     "Stomach",
                                                     "Left-Hand-Phalanges",
                                                     "Right-Hand-Phalanges",
                                                     "Forehead",
                                                     "Throat",
                                                     "Glans", // Consider replacing with a more appropriate term if needed
                                                     "Right-Shoulder",
                                                     "Left-Shoulder",
                                                     "Jaw",
                                                     "Right-Hand",
                                                     "Left-Hand",
                                                     "Right-Thigh",
                                                     "Left-Thigh",
                                                     "Nose",
                                                     "Right-Ear",
                                                     "Left-Ear",
                                                     "Right-Eye",
                                                     "Left-Eye",
                                                     "Umbilicus",
                                                     "Right-Calf",
                                                     "Left-Calf",
                                                     "Right-Eyelid",
                                                     "Left-Eyelid",
                                                     "Chest",
                                                     "Penis", // Consider replacing with a more appropriate term if needed
                                                     "Right-Nipple",
                                                     "Left-Nipple",
                                                     "Right-Foot",
                                                     "Left-Foot",
                                                     "Right-Thumb",
                                                     "Left-Thumb",
                                                     "Right-Big Toe",
                                                     "Left-Big Toe",
                                                     "Right-Knee",
                                                     "Left-Knee",
                                                     "Right-Testicle", // Consider replacing with a more appropriate term if needed
                                                     "Left-Testicle", // Consider replacing with a more appropriate term if needed
                                                     "Bladder"
                                              };
                foreach (var item in parts_body)
                {
                    listaSecciones.Items.Add(obtenerRecurso(item));
                }

            }


            if (listVista.SelectedItem.ToString() == obtenerRecurso("valBack") && radioMasculino.IsChecked == true)
            {
                ImageBrush brush = new ImageBrush();
                BitmapImage neww = ToBitmapImage(HS5.Properties.Resources.Color_Hombre_Back_500);
                brush.ImageSource = neww;
                canvas.Background = brush;
                listaSecciones.Items.Clear();
                Reset_images();
                string[] parts_body = {
                            "Anus",
                            "Right-Forearm",
                            "Left-Forearm",
                            "Right-Arm",
                            "Left-Arm",
                            "Head",
                            "Right-Hip",
                            "Left-Hip",
                            "Right-Clavicle",
                            "Left-Clavicle",
                            "Right-Elbow",
                            "Left-Elbow",
                            "Column",
                            "Coxis",
                            "Neck",
                            "Right-Fingers",
                            "Left-Fingers",
                            "Back",
                            "Right-Phalanges",
                            "Left-Phalanges",
                            "Right-Shoulder",
                            "Left-Shoulder",
                            "Right-Hand",
                            "Left-Hand",
                            "Right-Wrist",
                            "Left-Wrist",
                            "Right-Thigh",
                            "Left-Thigh",
                            "Right-Buttock",
                            "Left-Buttock",
                            "Right-Calf",
                            "Left-Calf",
                            "Right-Foot",
                            "Left-Foot",
                            "Right-Leg",
                            "Left-Leg",
                            "Right-Thumb",
                            "Left-Thumb",
                            "Left-Heel",
                            "Right-Heel",
                            "Right-Achilles heel",
                            "Left-Achilles heel",
                            "Right-Ankle",
                            "Left-Ankle"
                          };
                foreach (var item in parts_body)
                {
                    Console.WriteLine(item);
                    listaSecciones.Items.Add(obtenerRecurso(item));
                }
               
            }

            //Sexo Femenino
            if (listVista.SelectedItem.ToString() == obtenerRecurso("valFront") && radioFemenino.IsChecked == true)
            {
                ImageBrush brush = new ImageBrush();
                BitmapImage neww = ToBitmapImage(HS5.Properties.Resources.Mujer_front_500);
                brush.ImageSource = neww;
                canvas.Background = brush;
                listaSecciones.Items.Clear();

                Reset_images();

                string[] parts_body = new string[]
                                         {
                                                   "Abdomen",
                                                   "Left-Forearm",
                                                   "Right-Forearm",
                                                   "Right-Areola",
                                                   "Left-Areola",
                                                   "Mouth",
                                                   "Right-Arm",
                                                   "Left-Arm",
                                                   "Hair",
                                                   "Right-Hip",
                                                   "Left-Hip",
                                                   "Face",
                                                   "Right-Clavicle",
                                                   "Left-Clavicle",
                                                   "Right-Ribs",
                                                   "Left-Ribs",
                                                   "Right-Neck",
                                                   "Left-Neck",
                                                   "Right-Hand Fingers",
                                                   "Left-Hand Fingers",
                                                   "Esophagus",
                                                   "Stomach",
                                                   "Left-Hand Phalanges",
                                                   "Right-Hand Phalanges",
                                                   "Forehead",
                                                   "Throat",
                                                   "Right-Shoulder",
                                                   "Left-Shoulder",
                                                   "Jaw",
                                                   "Right-Hand",
                                                   "Left-Hand",
                                                   "Right-Thigh",
                                                   "Left-Thigh",
                                                   "Nose",
                                                   "Right-Ear",
                                                   "Left-Ear",
                                                   "Right-Eye",
                                                   "Left-Eye",
                                                   "Umbilicus",
                                                   "Right-Calf",
                                                   "Left-Calf",
                                                   "Right-Eyelid",
                                                   "Left-Eyelid",
                                                   "Right-Chest",
                                                   "Left-Chest",
                                                   "Chest",
                                                   "Right-Nipple",
                                                   "Left-Nipple",
                                                   "Right-Foot",
                                                   "Left-Foot",
                                                   "Right-Hand Thumb",
                                                   "Left-Hand Thumb",
                                                   "Right Big Toe",
                                                   "Left Big Toe",
                                                   "Right-Knee",
                                                   "Left-Knee",
                                                   "Right-Ankle",
                                                   "Left-Ankle",
                                                   "Uterus",
                                                   "Vagina",
                                                   "Bladder"
                                         };

                foreach (var item in parts_body)
                {
                    listaSecciones.Items.Add(obtenerRecurso(item));
                }
            }


            if (listVista.SelectedItem.ToString() == obtenerRecurso("valBack") && radioFemenino.IsChecked == true)
            {
                ImageBrush brush = new ImageBrush();
                BitmapImage neww = ToBitmapImage(HS5.Properties.Resources.Fback_500);
                brush.ImageSource = neww;
                canvas.Background = brush;
                listaSecciones.Items.Clear();
                Reset_images();
                string[] parts_body = {
                            "Anus",
                            "Right-Forearm",
                            "Left-Forearm",
                            "Right-Arm",
                            "Left-Arm",
                            "Head",
                            "Right-Hip",
                            "Left-Hip",
                            "Right-Clavicle",
                            "Left-Clavicle",
                            "Right-Elbow",
                            "Left-Elbow",
                            "Column",
                            "Coxis",
                            "Neck",
                            "Right-Fingers",
                            "Left-Fingers",
                            "Back",
                            "Right-Phalanges",
                            "Left-Phalanges",
                            "Right-Shoulder",
                            "Left-Shoulder",
                            "Right-Hand",
                            "Left-Hand",
                            "Right-Wrist",
                            "Left-Wrist",
                            "Right-Thigh",
                            "Left-Thigh",
                            "Right-Buttock",
                            "Left-Buttock",
                            "Right-Calf",
                            "Left-Calf",
                            "Right-Foot",
                            "Left-Foot",
                            "Right-Leg",
                            "Left-Leg",
                            "Right-Thumb",
                            "Left-Thumb",
                            "Left-Heel",
                            "Right-Heel",
                            "Right-Achilles heel",
                            "Left-Achilles heel",
                            "Right-Ankle",
                            "Left-Ankle"
                          };

                foreach (var item in parts_body)
                {
                    listaSecciones.Items.Add(obtenerRecurso(item));
                }
            }
        }

        private void ListBoxItem_Selected(object sender, RoutedEventArgs e)
        {

        }

        private void color1_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFFF0000");
            color_elegido.Content = "RED";
        }

        private void color3_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FF00FF00");
            color_elegido.Content = "GREEN";
        }

        private void color1_Copy1_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FF0000FF");
            color_elegido.Content = "BLUE";
        }

        private void color1_Copy2_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFFFBF00");
            color_elegido.Content = "AMBER 1";
        }

        private void color1_Copy3_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FF007FFF");
            color_elegido.Content = "AZURE 1";
        }

        private void color2_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFFFEF00");
            color_elegido.Content = "CANARY YELLOW 1";
        }

        private void color1_Copy4_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFC41E26");
            color_elegido.Content = "CARDINAL RED";
        }

        private void color2_Copy1_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FF960018");
            color_elegido.Content = "CARMINE 2";
        }

        private void color2_Copy2_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFDE3163");
            color_elegido.Content = "CHERRY RED 1";
        }

        private void color2_Copy3_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FF954535");
            color_elegido.Content = "CHESTNUT 1";
        }

        private void color2_Copy_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFD2691E");
            color_elegido.Content = "CINNAMON 1";
        }

        private void color1_Copy5_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FF0047AB");
            color_elegido.Content = "COBALT BLUE 1";
        }

        private void color2_Copy4_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFFF4040");
            color_elegido.Content = "CORAL RED 1";
        }

        private void color2_Copy5_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FF6495ED");
            color_elegido.Content = "CORNFLOWER BLUE 1";
        }

        private void color2_Copy6_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFFFFDD0");
            color_elegido.Content = "CREAM 1";
        }

        private void color2_Copy7_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFDB80C3");
            color_elegido.Content = "CYCLAMEN 1";
        }

        private void color1_Copy6_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FF654321");
            color_elegido.Content = "DARK BROWN 1";
        }

        private void color2_Copy8_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FF1C60F0");
            color_elegido.Content = "DELPHINIUM 1";
        }

        private void color2_Copy9_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFC2B280");
            color_elegido.Content = "ECRU 1";
        }

        private void color2_Copy10_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FF7DF9FF");
            color_elegido.Content = "ELECTRIC BLUE 1";
        }

        private void color2_Copy11_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FF50C878");
            color_elegido.Content = "EMERALD GREEN";
        }

        private void color1_Copy7_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FF486030");
            color_elegido.Content = "EVERGREEN 1";
        }

        private void color2_Copy12_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFFF230A");
            color_elegido.Content = "FLAMING RED 1";
        }

        private void color2_Copy13_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FF9D53B0");
            color_elegido.Content = "GERANIUM 1";
        }

        private void color2_Copy14_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FF498716");
            color_elegido.Content = "GOBLIN GREEN 1";
        }

        private void color2_Copy15_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FF996515");
            color_elegido.Content = "GOLDEN BROWN 1";
        }

        private void color1_Copy8_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFFDE336");
            color_elegido.Content = "GORSE YELLOW 1";
        }

        private void color2_Copy16_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FF68A900");
            color_elegido.Content = "GRASS GREEN 1";
        }

        private void color2_Copy17_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FF808080");
            color_elegido.Content = "GREY 1";
        }

        private void color2_Copy18_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FF4B0082");
            color_elegido.Content = "INDIGO";
        }

        private void color2_Copy19_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FF00A86B");
            color_elegido.Content = "JADE GREEN 1";
        }

        private void color1_Copy9_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFB1A3FC");
            color_elegido.Content = "KINGFISHER BLUE 1";
        }

        private void color2_Copy20_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FF678D5E");
            color_elegido.Content = "LAUREL GREEN 1";
        }

        private void color2_Copy21_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFC8A2C8");
            color_elegido.Content = "LILAC 1";
        }

        private void color2_Copy22_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFFAF0E6");
            color_elegido.Content = "LINEN 1";
        }

        private void color2_Copy23_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFBF321E");
            color_elegido.Content = "MADDER RED 1";
        }

        private void color1_Copy10_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFFFA41A");
            color_elegido.Content = "MANDARIN 1";
        }

        private void color2_Copy24_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFF3D005");
            color_elegido.Content = "MARIGOLD YELLOW 1";
        }

        private void color2_Copy25_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FF455CC4");
            color_elegido.Content = "MARINE BLUE 1";
        }

        private void color2_Copy26_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFDBAD1C");
            color_elegido.Content = "NASTURTIUM YELLOW 1";
        }

        private void color2_Copy27_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFBE8F5F");
            color_elegido.Content = "OAK 1";
        }

        private void color1_Copy11_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFC08081");
            color_elegido.Content = "OLD ROSE 1";
        }

        private void color2_Copy28_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FF808000");
            color_elegido.Content = "OLIVE GREEN";

        }

        private void color2_Copy29_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFFB9902");
            color_elegido.Content = "ORANGE";

        }

        private void color2_Copy30_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFFADADD");
            color_elegido.Content = "PALE PINK 1";
        }

        private void color2_Copy31_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFE84141");
            color_elegido.Content = "PALE RED 1";
        }

        private void color1_Copy12_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFEFB3F2");
            color_elegido.Content = "PARMA VIOLET 1";
        }

        private void color2_Copy32_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FF93DC69");
            color_elegido.Content = "PARROT GREEN 1";
        }

        private void color2_Copy33_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFFF2400");
            color_elegido.Content = "SCARLET";
        }

        private void color2_Copy34_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFBA4A09");
            color_elegido.Content = "TERRACOTTA 1";
        }

        private void color2_Copy35_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FF8F00FF");
            color_elegido.Content = "VIOLET";
        }

        private void color1_Copy13_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFFFFFFF");
            color_elegido.Content = "WHITE";
        }

        private void color2_Copy36_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFFFFF00");
            color_elegido.Content = "YELLOW";
        }

        private void color2_Copy37_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFFF0000");
            color_elegido.Content = "INFRARED";
        }

        private void color2_Copy38_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FF00FF00");
            color_elegido.Content = "ULTRASOUND ";
        }

        private void color2_Copy39_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFFFFF96");
            color_elegido.Content = "SUNLIGHT (PURE WHITE) ";
        }

        private void color1_Copy14_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFFFBF00");
            color_elegido.Content = "AMBER LIGHT ";
        }

        private void color2_Copy40_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FF8F00FF");
            color_elegido.Content = "ULTRA-VIOLET ";
        }

        private void color2_Copy41_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFFAFA96");
            color_elegido.Content = "FULL LIGHT SPECTRUM ";
        }

        private void color2_Copy42_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FF131313");
            color_elegido.Content = "BLACK (ABSENCE OF PHYSICAL LIGHT) ";
        }

        private void color1_Copy15_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFEFB3F2");
            color_elegido.Content = "SENSITIVITY TO LIGHT";
        }

        private void color2_Copy44_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var bc = new BrushConverter();

                color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFB1A3FC");
                color_elegido.Content = "DAYLIGHT BLUE ";
            }
            catch(Exception mx)
            {
                MessageBox.Show(mx.ToString());
            }
        }

        private void color2_Copy45_Click(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            color_elegido.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FFFFFF00");
            color_elegido.Content = "SUN";
        }

        //Clase para el nuevo color combinado...
        public class nuevoColorCombinado
        {
            public string codigocomp { get; set; }
            public string nombrecodigocomp { get; set; }
        }

        List<string> cod_partes_colores = new List<string>(); //Codigos de los colores
        List<string> cod_partes_color = new List<string>(); //Codigos de las secciones
        List<string> partes_colores = new List<string>(); //Terapia de color
        private void listaSecciones_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //POSIBLE AGREGAR VALIDACION DEL COLOR ELEGIDO..

            //Cambia el color al png elegido...
            //BitmapImage Abdomen = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Abdomen_500));
            // BitmapImage Antebrazo_Der = ToBitmapImage(ChangeColor(HS5.Properties.Resources.AnteBrazo_Der_500));

            // ImageBrush brush = new ImageBrush();
            //brush.ImageSource = prueba;
            // canvas.Background = brush;
            // part1.Source = Abdomen;
            //  part2.Source = Antebrazo_Der;


            //Si no ha elegido color no continuar..
            if (color_elegido.Content.ToString() != "")
            {
                Radionica obj = new Radionica(); //Objeto radionica
                Random obj_m = new Random();

                //Color elegido
                System.Windows.Media.Brush newColor = color_elegido.Background; //Brush a media brush
                SolidColorBrush newBrush = (SolidColorBrush)newColor; //Media brush a solidcolor brush
                                                                      //Solidcolor brush a drawing color
                System.Drawing.Color myColor = System.Drawing.Color.FromArgb(newBrush.Color.A,
                                                                  newBrush.Color.R,
                                                                  newBrush.Color.G,
                                                                  newBrush.Color.B);

                //Si es hombre y es parte frontal
                if (radioMasculino.IsChecked == true && listVista.SelectedItem.ToString() == obtenerRecurso("valFront"))
                {
                    //Partes que le corresponden
                    //Seccion elegida
                    string seccion = listaSecciones.SelectedItem.ToString();
                    //  MessageBox.Show(seccion);

                    //Agregue la parte a la lista y su codigo
                    HacerConexion();
                    object cod_color;


                    cod_color = obj2.Obtener_Color_ParteTerapia(color_elegido.Content.ToString());

                    object cod_seccion = obj2.Obtener_CodigoParte_Color_ParteTerapia(seccion);

                    //Sino existe la parte elegida en rad_codigos generar un random para ello
                    if (cod_seccion == null)
                    {
                        cod_seccion = obj.RandomDigits(obj_m.Next(8, 14));
                    }

                    CerrarConexion();

                    //Listas con las partes
                    cod_partes_colores.Add(cod_color.ToString()); //Guarda el codigo del color
                    cod_partes_color.Add(cod_seccion.ToString()); //Guarda el codigo de la parte elegida

                    // Color - Seccion (Parte) - Vista  - Genero
                    string descrip_color = color_elegido.Content.ToString() + " - " + seccion + " - " + listVista.SelectedItem.ToString() + " - " + radioMasculino.Content.ToString();

                    //Guarda descripcion
                    partes_colores.Add(descrip_color); //Guarda la parte

                    //Agregar a la lista para visualizar (Combina el codigo uniendo cod_seccion+cod_color)
                    listadoCodigosComb.Items.Add(new nuevoColorCombinado { codigocomp = cod_seccion.ToString() + cod_color.ToString(), nombrecodigocomp = descrip_color });

                    //////////
                    if (seccion == obtenerRecurso("Abdomen"))
                    {
                        BitmapImage Abdomen = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Abdomen_500, myColor));
                        part1.Source = Abdomen;
                    }
                    else if (seccion == obtenerRecurso("Left-Forearm"))
                    {
                        BitmapImage Ante1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.AnteBrazo_Izq_500, myColor));
                        part2.Source = Ante1;
                    }
                    else if (seccion == obtenerRecurso("Right-Forearm"))
                    {
                        BitmapImage Ante2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.AnteBrazo_Der_500, myColor));
                        part3.Source = Ante2;
                    }
                    else if (seccion == obtenerRecurso("Mouth"))
                    {
                        BitmapImage Boca = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Boca_B_500, myColor));
                        part4.Source = Boca;
                    }
                    else if (seccion == obtenerRecurso("Right-Arm"))
                    {
                        BitmapImage Brazo1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Brazo_Der_500, myColor));
                        part5.Source = Brazo1;
                    }
                    else if (seccion == obtenerRecurso("Left-Arm"))
                    {
                        BitmapImage Brazo2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Brazo_Izq_500, myColor));
                        part62.Source = Brazo2;
                    }
                    else if (seccion == obtenerRecurso("Hair"))
                    {
                        BitmapImage Cabello = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Cabello_500, myColor));
                        part6.Source = Cabello;
                    }
                    else if (seccion == obtenerRecurso("Right-Hip"))
                    {
                        BitmapImage Cadera1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Cadera_Der_500, myColor));
                        part7.Source = Cadera1;
                    }
                    else if (seccion == obtenerRecurso("Left-Hip"))
                    {
                        BitmapImage Cadera2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Cadera_Izq_500, myColor));
                        part8.Source = Cadera2;
                    }
                    else if (seccion == obtenerRecurso("Face"))
                    {
                        BitmapImage Cara = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Cara_500, myColor));
                        part9.Source = Cara;
                    }
                    else if (seccion == obtenerRecurso("Right-Clavicle"))
                    {
                        BitmapImage Clavícula1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Clavicula_Der_500, myColor));
                        part10.Source = Clavícula1;
                    }
                    else if (seccion == obtenerRecurso("Left-Clavicle"))
                    {
                        BitmapImage Clavícula2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Clavicula_Izq_500, myColor));
                        part11.Source = Clavícula2;
                    }
                    else if (seccion == obtenerRecurso("Right-Ribs"))
                    {
                        BitmapImage Costillas = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Costillas_Der_500, myColor));
                        part12.Source = Costillas;
                    }
                    else if (seccion == obtenerRecurso("Left-Ribs"))
                    {
                        BitmapImage Costillas2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Costillas_Izq_500, myColor));
                        part13.Source = Costillas2;
                    }
                    else if (seccion == obtenerRecurso("Right-Neck"))
                    {
                        BitmapImage Cuello1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Cuello_Der_500, myColor));
                        part14.Source = Cuello1;
                    }
                    else if (seccion == obtenerRecurso("Left-Neck"))
                    {
                        BitmapImage Cuello2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Cuello_Izq_500, myColor));
                        part15.Source = Cuello2;
                    }
                    else if (seccion == obtenerRecurso("Right-Hand-Fingers"))
                    {
                        BitmapImage Dedos = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Dedos_Mano_Der_500, myColor));
                        part16.Source = Dedos;
                    }
                    else if (seccion == obtenerRecurso("Left-Hand-Fingers"))
                    {
                        BitmapImage Dedos2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Dedos_Izq_500, myColor));
                        part17.Source = Dedos2;
                    }
                    else if (seccion == obtenerRecurso("Esophagus"))
                    {
                        BitmapImage Esofago = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Esofago_500, myColor));
                        part18.Source = Esofago;
                    }
                    else if (seccion == obtenerRecurso("Stomach"))
                    {
                        BitmapImage Estomago = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Estomago_500, myColor));
                        part19.Source = Estomago;
                    }
                    else if (seccion == obtenerRecurso("Left-Hand-Phalanges"))
                    {
                        BitmapImage Falanges1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Falange_Izq_500, myColor));
                        part20.Source = Falanges1;
                    }
                    else if (seccion == obtenerRecurso("Right-Hand-Phalanges"))
                    {
                        BitmapImage Falanges2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Falanges_Der_500, myColor));
                        part21.Source = Falanges2;
                    }
                    else if (seccion == obtenerRecurso("Forehead"))
                    {
                        BitmapImage Frente = ToBitmapImage(ChangeColor(HS5.Properties.Resources.frente_500, myColor));
                        part22.Source = Frente;
                    }
                    else if (seccion ==obtenerRecurso("Throat"))
                    {
                        BitmapImage Garganta = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Garganta_500, myColor));
                        part23.Source = Garganta;
                    }
                    else if (seccion == obtenerRecurso("Glans"))
                    {
                        BitmapImage Glande = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Glande_500, myColor));
                        part24.Source = Glande;
                    }
                    else if (seccion == obtenerRecurso("Right-Shoulder"))
                    {
                        BitmapImage Hombro1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Hombro_Der_500, myColor));
                        part25.Source = Hombro1;
                    }
                    else if (seccion == obtenerRecurso("Left-Shoulder"))
                    {
                        BitmapImage Hombro2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Hombro_Izq_500, myColor));
                        part26.Source = Hombro2;
                    }
                    else if (seccion == obtenerRecurso("Jaw"))
                    {
                        BitmapImage Man1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Mandibula_500, myColor));
                        part27.Source = Man1;
                    }
                    else if (seccion == obtenerRecurso("Right-Hand"))
                    {
                        BitmapImage Mano1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Mano_Der_500, myColor));
                        part28.Source = Mano1;
                    }
                    else if (seccion == obtenerRecurso("Left-Hand"))
                    {
                        BitmapImage Mano2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Mano_Izq_500, myColor));
                        part29.Source = Mano2;
                    }
                    else if (seccion == obtenerRecurso("Right-Thigh"))
                    {
                        BitmapImage Muslo = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Muslo_Der_500, myColor));
                        part30.Source = Muslo;
                    }
                    else if (seccion == obtenerRecurso("Left-Thigh"))
                    {
                        BitmapImage Muslo2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Muslo_Izq_500, myColor));
                        part31.Source = Muslo2;
                    }
                    else if (seccion == obtenerRecurso("Nose"))
                    {
                        BitmapImage Nariz = ToBitmapImage(ChangeColor(HS5.Properties.Resources.nariz_500, myColor));
                        part32.Source = Nariz;
                    }
                    else if (seccion == obtenerRecurso("Right-Ear"))
                    {
                        BitmapImage Oreja1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Oreja_Der_500, myColor));
                        part33.Source = Oreja1;
                    }
                    else if (seccion == obtenerRecurso("Left-Ear"))
                    {
                        BitmapImage Oreja2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Oreja_Izq_500, myColor));
                        part34.Source = Oreja2;
                    }
                    else if (seccion == obtenerRecurso("Right-Eye"))
                    {
                        BitmapImage Ojo1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Ojo_Der_500, myColor));
                        part35.Source = Ojo1;
                    }
                    else if (seccion == obtenerRecurso("Left-Eye"))
                    {
                        BitmapImage Ojo2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Ojo_Izq_500, myColor));
                        part36.Source = Ojo2;
                    }
                    else if (seccion == obtenerRecurso("Umbilicus"))
                    {
                        BitmapImage Ombligo = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Ombligo_500, myColor));
                        part37.Source = Ombligo;
                    }
                    else if (seccion == obtenerRecurso("Right-Calf"))
                    {
                        BitmapImage Pantorilla = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pantorrilla_Der_500, myColor));
                        part38.Source = Pantorilla;
                    }
                    else if (seccion == obtenerRecurso("Left-Calf"))
                    {
                        BitmapImage Pantorilla2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pantorrilla_Izq_500, myColor));
                        part39.Source = Pantorilla2;
                    }
                    else if (seccion == obtenerRecurso("Right-Eyelid"))
                    {
                        BitmapImage Parpado = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Parpado_DerB_500, myColor));
                        part40.Source = Parpado;
                    }
                    else if (seccion == obtenerRecurso("Left-Eyelid"))
                    {
                        BitmapImage Parpado2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Parpado_Izq_500, myColor));
                        part41.Source = Parpado2;
                    }
                    else if (seccion == obtenerRecurso("Chest"))
                    {
                        BitmapImage Pecho = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pecho_500, myColor));
                        part42.Source = Pecho;
                    }
                    else if (seccion == obtenerRecurso("Penis"))
                    {
                        BitmapImage Pene = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pene_500, myColor));
                        part43.Source = Pene;
                    }
                    else if (seccion == obtenerRecurso("Right-Nipple"))
                    {
                        BitmapImage Pezon1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pezon_Der_500, myColor));
                        part44.Source = Pezon1;
                    }
                    else if (seccion == obtenerRecurso("Left-Nipple"))
                    {
                        BitmapImage Pezon2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pezon_Izq_500, myColor));
                        part45.Source = Pezon2;
                    }
                    else if (seccion == obtenerRecurso("Right-Foot"))
                    {
                        BitmapImage Pie = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pie_Der_500, myColor));
                        part46.Source = Pie;
                    }
                    else if (seccion == obtenerRecurso("Left-Foot"))
                    {
                        BitmapImage Pie2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pie_Izq_500, myColor));
                        part47.Source = Pie2;
                    }
                    else if (seccion == obtenerRecurso("Right-Thumb"))
                    {
                        BitmapImage Pulgar = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pulgar_Mano_Der_500, myColor));
                        part48.Source = Pulgar;
                    }
                    else if (seccion == obtenerRecurso("Left-Thumb"))
                    {
                        BitmapImage Pulgar2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pulganr_Mano_Izq_500, myColor));
                        part49.Source = Pulgar2;
                    }
                    else if (seccion == obtenerRecurso("Right-Knee"))
                    {
                        BitmapImage Rodilla1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Rodilla_Der_500, myColor));
                        part50.Source = Rodilla1;
                    }
                    else if (seccion == obtenerRecurso("Left-Knee"))
                    {
                        BitmapImage Rodilla2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Rodilla_Izq_500, myColor));
                        part51.Source = Rodilla2;
                    }
                    else if (seccion == obtenerRecurso("Right-Testicle"))
                    {
                        BitmapImage Testi1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Testiculo_Der_500, myColor));
                        part52.Source = Testi1;
                    }
                    else if (seccion == obtenerRecurso("Left-Testicle"))
                    {
                        BitmapImage Testi2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Testiculo_Izq_500, myColor));
                        part53.Source = Testi2;
                    }
                    else if (seccion == obtenerRecurso("Bladder"))
                    {
                        BitmapImage Vegija = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Vegiga_500, myColor));
                        part54.Source = Vegija;
                    }
                    else if (seccion == obtenerRecurso("Right-Big Toe"))
                    {
                        BitmapImage Pulgar11 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pulgar_Pie_Der_500, myColor));
                        part55.Source = Pulgar11;
                    }
                    else if (seccion == obtenerRecurso("Left-Big Toe"))
                    {
                        BitmapImage Pulgar22 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pulgar_Pie_Izq_500, myColor));
                        part56.Source = Pulgar22;
                    }

                }

                    //Si es hombre y es parte trasera
                    if (radioMasculino.IsChecked == true && listVista.SelectedItem.ToString() == obtenerRecurso("valBack"))
                {
                    //Partes que le corresponden
                    //Seccion elegida
                    string seccion = listaSecciones.SelectedItem.ToString();
                    //  MessageBox.Show(seccion);

                    //Agregue la parte a la lista y su codigo
                    HacerConexion();

                    //Obtiene el codigo del color..
                    object cod_color = obj2.Obtener_Color_ParteTerapia(color_elegido.Content.ToString());
                    object cod_seccion = obj2.Obtener_CodigoParte_Color_ParteTerapia(seccion);

                    //Sino existe la parte elegida en rad_codigos generar un random para ello
                    if (cod_seccion == null)
                    {
                        cod_seccion = obj.RandomDigits(obj_m.Next(8, 14));
                    }

                    CerrarConexion();

                    //Listas con las partes
                    cod_partes_colores.Add(cod_color.ToString()); //Guarda el codigo del color
                    cod_partes_color.Add(cod_seccion.ToString()); //Guarda el codigo de la parte elegida

                    // Color - Seccion (Parte) - Vista  - Genero
                    string descrip_color = color_elegido.Content.ToString() + " - " + seccion + " - " + listVista.SelectedItem.ToString() + " - " + radioMasculino.Content.ToString();

                    //Guarda descripcion
                    partes_colores.Add(descrip_color); //Guarda la parte

                    //Agregar a la lista para visualizar (Combina el codigo uniendo cod_seccion+cod_color)
                    listadoCodigosComb.Items.Add(new nuevoColorCombinado { codigocomp = cod_seccion.ToString() + cod_color.ToString(), nombrecodigocomp = descrip_color });

                    //condition to show or add color to the image of body 
                    if (seccion == obtenerRecurso("Anus"))
                    {
                        BitmapImage ano = ToBitmapImage(ChangeColor(HS5.Properties.Resources.MBAnus_500, myColor));
                        part1.Source = ano;
                    }
                    else if (seccion == obtenerRecurso("Right-Forearm"))
                    {
                        BitmapImage ante1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.MBAnteBrazo_Der_500, myColor));
                        part2.Source = ante1;
                    }
                    else if (seccion == obtenerRecurso("Left-Forearm"))
                    {
                        BitmapImage ante2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Antebrazo_Izq_5001, myColor));
                        part3.Source = ante2;
                    }
                    else if (seccion == obtenerRecurso("Right-Arm"))
                    {
                        BitmapImage brazo1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.MBBrazo_Der_500, myColor));
                        part4.Source = brazo1;
                    }
                    else if (seccion == obtenerRecurso("Left-Arm"))
                    {
                        BitmapImage brazo2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.MBBrazo_Izq_500, myColor));
                        part5.Source = brazo2;
                    }
                    else if (seccion == obtenerRecurso("Head"))
                    {
                        BitmapImage cabeza = ToBitmapImage(ChangeColor(HS5.Properties.Resources.MBCabeza_500, myColor));
                        part6.Source = cabeza;
                    }
                    else if (seccion == obtenerRecurso("Right-Hip"))
                    {
                        BitmapImage cadera1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Cadera_Der_5001, myColor));
                        part7.Source = cadera1;
                    }
                    else if (seccion == obtenerRecurso("Left-Hip"))
                    {
                        BitmapImage cadera2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Cadera_Izq_5001, myColor));
                        part8.Source = cadera2;
                    }
                    else if (seccion == obtenerRecurso("Right-Trapezius"))
                    {
                        BitmapImage trapecio1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Trapecio_Der_500, myColor));
                        part9.Source = trapecio1;
                    }
                    else if (seccion == obtenerRecurso("Left-Trapezius"))
                    {
                        BitmapImage trapecio2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Trapecio_Izp_500, myColor));
                        part10.Source = trapecio2;
                    }
                    else if (seccion == obtenerRecurso("Right-Elbow"))
                    {
                        BitmapImage codo1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.MBCodo_Der_500, myColor));
                        part11.Source = codo1;
                    }
                    else if (seccion == obtenerRecurso("Left-Elbow"))
                    {
                        BitmapImage codo2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Codo_Izq_500, myColor));
                        part12.Source = codo2;
                    }
                    else if (seccion == obtenerRecurso("Column"))
                    {
                        BitmapImage columna = ToBitmapImage(ChangeColor(HS5.Properties.Resources.MBColumna_500, myColor));
                        part13.Source = columna;
                    }
                    else if (seccion == obtenerRecurso("Neck"))
                    {
                        BitmapImage cuello = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Cuello_500, myColor));
                        part14.Source = cuello;
                    }
                    else if (seccion == obtenerRecurso("Right-Fingers"))
                    {
                        BitmapImage dedos1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Dedos_Der_500, myColor));
                        part15.Source = dedos1;
                    }
                    else if (seccion == obtenerRecurso("Left-Fingers"))
                    {
                        BitmapImage dedos2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Dedos_izq_5001, myColor));
                        part16.Source = dedos2;
                    }
                    else if (seccion == obtenerRecurso("Back"))
                    {
                        BitmapImage espalda = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Espalda_500, myColor));
                        part17.Source = espalda;
                    }
                    else if (seccion == obtenerRecurso("Right-Phalanges"))
                    {
                        BitmapImage falanges = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Falanges_Der_5001, myColor));
                        part18.Source = falanges;
                    }
                    else if (seccion == obtenerRecurso("Left-Phalanges"))
                    {
                        BitmapImage falanges2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.HBFalages_Izq_500, myColor));
                        part19.Source = falanges2;
                    }
                    else if (seccion == obtenerRecurso("Right-Shoulder"))
                    {
                        BitmapImage hombro1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.HBHombro_Der_500, myColor));
                        part20.Source = hombro1;
                    }
                    else if (seccion == obtenerRecurso("Left-Shoulder"))
                    {
                        BitmapImage hombro2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.MBHombro_Izq_500, myColor));
                        part21.Source = hombro2;
                    }
                    else if (seccion == obtenerRecurso("Right-Hand"))
                    {
                        BitmapImage mano1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Mano_Der_5001, myColor));
                        part22.Source = mano1;
                    }
                    else if (seccion == obtenerRecurso("Left-Hand"))
                    {
                        BitmapImage mano2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Mano_Izq_5001, myColor));
                        part23.Source = mano2;
                    }
                    else if (seccion == obtenerRecurso("Right-Wrist"))
                    {
                        BitmapImage mun1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.MBMuñeca_der_500, myColor));
                        part24.Source = mun1;
                    }
                    else if (seccion == obtenerRecurso("Left-Wrist"))
                    {
                        BitmapImage mun2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Muñeca_Izq_500, myColor));
                        part25.Source = mun2;
                    }
                    else if (seccion == obtenerRecurso("Right-Thigh"))
                    {
                        BitmapImage muslo1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Muslo_Der_5001, myColor));
                        part26.Source = muslo1;
                    }
                    else if (seccion == obtenerRecurso("Left-Thigh"))
                    {
                        BitmapImage muslo2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Muslo_Izq_5001, myColor));
                        part27.Source = muslo2;
                    }
                    else if (seccion == obtenerRecurso("Right-Buttock"))
                    {
                        BitmapImage nalga1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.MBNalga_Der_500, myColor));
                        part28.Source = nalga1;
                    }
                    else if (seccion == obtenerRecurso("Left-Buttock"))
                    {
                        BitmapImage nalga2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.MBNalga_Izq_500, myColor));
                        part29.Source = nalga2;
                    }
                    else if (seccion == obtenerRecurso("Right-Ear"))
                    {
                        BitmapImage oido1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.MBOido_der_500, myColor));
                        part30.Source = oido1;
                    }
                    else if (seccion == obtenerRecurso("Left-Ear"))
                    {
                        BitmapImage oido2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.MBOido_Izq_500, myColor));
                        part31.Source = oido2;
                    }
                    else if (seccion == obtenerRecurso("Right-Calf"))
                    {
                        BitmapImage panto1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pantorrilla_Der_5001, myColor));
                        part32.Source = panto1;
                    }
                    else if (seccion == obtenerRecurso("Left-Calf"))
                    {
                        BitmapImage panto2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pantorrilla_Izq_5001, myColor));
                        part33.Source = panto2;
                    }
                    else if (seccion == obtenerRecurso("Right-Foot"))
                    {
                        BitmapImage pie1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pie_Der_5001, myColor));
                        part34.Source = pie1;
                    }
                    else if (seccion == obtenerRecurso("Left-Foot"))
                    {
                        BitmapImage pie2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pie_Izq_5001, myColor));
                        part35.Source = pie2;
                    }
                    else if (seccion == obtenerRecurso("Pelvis"))
                    {
                        BitmapImage pelvis = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pelvis_500, myColor));
                        part36.Source = pelvis;
                    }
                    else if (seccion == obtenerRecurso("Right-Thumb"))
                    {
                        BitmapImage pulga1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.MBPulgar_Der_500, myColor));
                        part37.Source = pulga1;
                    }
                    else if (seccion == obtenerRecurso("Left-Thumb"))
                    {
                        BitmapImage pulga2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pulgar_Izq_5001, myColor));
                        part38.Source = pulga2;
                    }
                    else if (seccion == obtenerRecurso("Left-Heel"))
                    {
                        BitmapImage talon1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Talon_Izq_500, myColor));
                        part39.Source = talon1;
                    }
                    else if (seccion == obtenerRecurso("Right-Heel"))
                    {
                        BitmapImage talon2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Talon_Der_500, myColor));
                        part40.Source = talon2;
                    }
                    else if (seccion == obtenerRecurso("Right-Achilles heel"))
                    {
                        BitmapImage talonn1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Aquiles_Der_500, myColor));
                        part41.Source = talonn1;
                    }
                    else if (seccion == obtenerRecurso("Left-Achilles heel"))
                    {
                        BitmapImage talonn2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Aquiles_Izq_500, myColor));
                        part42.Source = talonn2;
                    }
                    else if (seccion == obtenerRecurso("Right-Ankle"))
                    {
                        BitmapImage tobillo1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Tobillo_Der_500, myColor));
                        part43.Source = tobillo1;
                    }
                    else if (seccion == obtenerRecurso("Left-Ankle"))
                    {
                        BitmapImage tobillo2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Tobillo_Izq_500, myColor));
                        part44.Source = tobillo2;
                    }
                } 
   

                //Si es mujer y es la parte frontal
                if (radioFemenino.IsChecked == true && listVista.SelectedItem.ToString() == obtenerRecurso("valFront"))
                {
                    //////////////

                    //Partes que le corresponden
                    //Seccion elegida
                    string seccion = listaSecciones.SelectedItem.ToString();
                    //  MessageBox.Show(seccion);

                    //Agregue la parte a la lista y su codigo
                    HacerConexion();

                    //Obtiene el codigo del color..
                    object cod_color = obj2.Obtener_Color_ParteTerapia(color_elegido.Content.ToString());
                    object cod_seccion = obj2.Obtener_CodigoParte_Color_ParteTerapia(seccion);

                    //Sino existe la parte elegida en rad_codigos generar un random para ello
                    if (cod_seccion == null)
                    {
                        cod_seccion = obj.RandomDigits(obj_m.Next(8, 14));
                    }

                    CerrarConexion();

                    //Listas con las partes
                    cod_partes_colores.Add(cod_color.ToString()); //Guarda el codigo del color
                    cod_partes_color.Add(cod_seccion.ToString()); //Guarda el codigo de la parte elegida

                    // Color - Seccion (Parte) - Vista  - Genero
                    string descrip_color = color_elegido.Content.ToString() + " - " + seccion + " - " + listVista.SelectedItem.ToString() + " - " + radioMasculino.Content.ToString();

                    //Guarda descripcion
                    partes_colores.Add(descrip_color); //Guarda la parte

                    //Agregar a la lista para visualizar (Combina el codigo uniendo cod_seccion+cod_color)
                    listadoCodigosComb.Items.Add(new nuevoColorCombinado { codigocomp = cod_seccion.ToString() + cod_color.ToString(), nombrecodigocomp = descrip_color });

                    //////////

                    if (seccion == obtenerRecurso("Abdomen"))
                    {
                        BitmapImage abdo = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Abdomen_5001, myColor));
                        part1.Source = abdo;
                    }
                    else if (seccion == obtenerRecurso("Left-Forearm"))
                    {
                        BitmapImage ant1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.AnteBraxo_Izq_500, myColor));
                        part2.Source = ant1;
                    }
                    else if (seccion == obtenerRecurso("Right-Forearm"))
                    {
                        BitmapImage ant2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.AnteBrazo_Der_5001, myColor));
                        part3.Source = ant2;
                    }
                    else if (seccion == obtenerRecurso("Right-Areola"))
                    {
                        BitmapImage aure = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Aureola_Der_500, myColor));
                        part4.Source = aure;
                    }
                    else if (seccion == obtenerRecurso("Left-Areola"))
                    {
                        BitmapImage aure2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Aureola_Izq_500, myColor));
                        part5.Source = aure2;
                    }
                    else if (seccion == obtenerRecurso("Mouth"))
                    {
                        BitmapImage boca = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Boca_500, myColor));
                        part6.Source = boca;
                    }
                    else if (seccion == obtenerRecurso("Right-Arm"))
                    {
                        BitmapImage brazo = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Brazo_Der_5001, myColor));
                        part7.Source = brazo;
                    }
                    else if (seccion == obtenerRecurso("Left-Arm"))
                    {
                        BitmapImage brazo2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Brazo_Izq_5001, myColor));
                        part8.Source = brazo2;
                    }
                    else if (seccion == obtenerRecurso("Hair"))
                    {
                        BitmapImage cabello = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Cabello_5001, myColor));
                        part9.Source = cabello;
                    }
                    else if (seccion == obtenerRecurso("Right-Hip"))
                    {
                        BitmapImage cadera = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Cadera_Der_5002, myColor));
                        part10.Source = cadera;
                    }
                    else if (seccion == obtenerRecurso("Left-Hip"))
                    {
                        BitmapImage cadera2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Cadera_Izq_5002, myColor));
                        part11.Source = cadera2;
                    }
                    else if (seccion == obtenerRecurso("Face"))
                    {
                        BitmapImage cara = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Cara_5001, myColor));
                        part12.Source = cara;
                    }
                    else if (seccion == obtenerRecurso("Right-Clavicle"))
                    {
                        BitmapImage clavi = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Clavicula_Der_5001, myColor));
                        part13.Source = clavi;
                    }
                    else if (seccion == obtenerRecurso("Left-Clavicle"))
                    {
                        BitmapImage clavi2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Clavicula_Izq_5001, myColor));
                        part14.Source = clavi2;
                    }
                    else if (seccion == obtenerRecurso("Right-Ribs"))
                    {
                        BitmapImage cost1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Costillas_Der_5001, myColor));
                        part15.Source = cost1;
                    }
                    else if (seccion == obtenerRecurso("Left-Ribs"))
                    {
                        BitmapImage cost2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Costillas_Izq_5001, myColor));
                        part16.Source = cost2;
                    }
                    else if (seccion == obtenerRecurso("Right-Neck"))
                    {
                        BitmapImage cuell1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Cuello_Der_5001, myColor));
                        part17.Source = cuell1;
                    }
                    else if (seccion == obtenerRecurso("Left-Neck"))
                    {
                        BitmapImage cuell2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Cuello_Izq_5001, myColor));
                        part18.Source = cuell2;
                    }
                    else if (seccion == obtenerRecurso("Right-Hand Fingers"))
                    {
                        BitmapImage dedos1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Dedos_Mano_Der_5001, myColor));
                        part19.Source = dedos1;
                    }
                    else if (seccion == obtenerRecurso("Left-Hand Fingers"))
                    {
                        BitmapImage dedos2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Dedos_Mano_Izq_500, myColor));
                        part20.Source = dedos2;
                    }
                    else if (seccion == obtenerRecurso("Esophagus"))
                    {
                        BitmapImage eso1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Esofago_5001, myColor));
                        part21.Source = eso1;
                    }
                    else if (seccion == obtenerRecurso("Stomach"))
                    {
                        BitmapImage esto1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Estomago_5001, myColor));
                        part22.Source = esto1;
                    }
                    else if (seccion == obtenerRecurso("Left-Hand Phalanges"))
                    {
                        BitmapImage falanges = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Falanges_Der_5002, myColor));
                        part23.Source = falanges;
                    }
                    else if (seccion == obtenerRecurso("Right-Hand Phalanges"))
                    {
                        BitmapImage falanges2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Falanges_Mano_Der_500, myColor));
                        part24.Source = falanges2;
                    }
                    else if (seccion == obtenerRecurso("Forehead"))
                    {
                        BitmapImage frente = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Frente_5001, myColor));
                        part25.Source = frente;
                    }
                    else if (seccion == obtenerRecurso("Throat"))
                    {
                        BitmapImage garg = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Garganta_5001, myColor));
                        part26.Source = garg;
                    }
                    else if (seccion == obtenerRecurso("Right-Shoulder"))
                    {
                        BitmapImage hombro1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Hombro_Der_5001, myColor));
                        part27.Source = hombro1;
                    }
                    else if (seccion == obtenerRecurso("Left-Shoulder"))
                    {
                        BitmapImage hombro2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Hombro_Izq_5001, myColor));
                        part28.Source = hombro2;
                    }
                    else if (seccion == obtenerRecurso("Jaw"))
                    {
                        BitmapImage man1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Mandibula_5001, myColor));
                        part29.Source = man1;
                    }
                    else if (seccion == obtenerRecurso("Right-Hand"))
                    {
                        BitmapImage mano1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Mano_Der_5002, myColor));
                        part30.Source = mano1;
                    }
                    else if (seccion == obtenerRecurso("Left-Hand"))
                    {
                        BitmapImage mano2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Mano_Izq_5002, myColor));
                        part31.Source = mano2;
                    }
                    else if (seccion == obtenerRecurso("Right-Thigh"))
                    {
                        BitmapImage muslo1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Muslo_Der_5002, myColor));
                        part32.Source = muslo1;
                    }
                    else if (seccion == obtenerRecurso("Left-Thigh"))
                    {
                        BitmapImage muslo2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Muslo_Izq_5002, myColor));
                        part33.Source = muslo2;
                    }
                    else if (seccion == obtenerRecurso("Nose"))
                    {
                        BitmapImage nariz = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Nariz_5001, myColor));
                        part34.Source = nariz;
                    }
                    else if (seccion == obtenerRecurso("Right-Ear"))
                    {
                        BitmapImage oido1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Oido_Der_500, myColor));
                        part35.Source = oido1;
                    }
                    else if (seccion == obtenerRecurso("Left-Ear"))
                    {
                        BitmapImage oido2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Oido_Izq_500, myColor));
                        part36.Source = oido2;
                    }
                    else if (seccion == obtenerRecurso("Right-Eye"))
                    {
                        BitmapImage ojo1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Ojo_Der_5001, myColor));
                        part37.Source = ojo1;
                    }
                    else if (seccion == obtenerRecurso("Left-Eye"))
                    {
                        BitmapImage ojo2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Ojo_Izq_5001, myColor));
                        part38.Source = ojo2;
                    }
                    else if (seccion == obtenerRecurso("Umbilicus"))
                    {
                        BitmapImage ombligo1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Ombligo_5001, myColor));
                        part39.Source = ombligo1;
                    }
                    else if (seccion == obtenerRecurso("Right-Calf"))
                    {
                        BitmapImage panto1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pantorrilla_Der_5002, myColor));
                        part40.Source = panto1;
                    }
                    else if (seccion == obtenerRecurso("Left-Calf"))
                    {
                        BitmapImage panto2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pantorrilla_Izq_5002, myColor));
                        part41.Source = panto2;
                    }
                    else if (seccion == obtenerRecurso("Right-Eyelid"))
                    {
                        BitmapImage parp = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Parpado_Der_500, myColor));
                        part42.Source = parp;
                    }
                    else if (seccion == obtenerRecurso("Left-Eyelid"))
                    {
                        BitmapImage parp2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Parpado_Izq_5001, myColor));
                        part43.Source = parp2;
                    }
                    else if (seccion == obtenerRecurso("Right-Chest"))
                    {
                        BitmapImage pecho1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pecho_Der_500, myColor));
                        part44.Source = pecho1;
                    }
                    else if (seccion == obtenerRecurso("Left-Chest"))
                    {
                        BitmapImage pecho2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pecho_Izq_500, myColor));
                        part45.Source = pecho2;
                    }
                    else if (seccion == obtenerRecurso("Chest"))
                    {
                        BitmapImage pecho = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pecho_5001, myColor));
                        part46.Source = pecho;
                    }
                    else if (seccion == obtenerRecurso("Right-Nipple"))
                    {
                        BitmapImage pezo1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pezon_Der_5001, myColor));
                        part47.Source = pezo1;
                    }
                    else if (seccion == obtenerRecurso("Left-Nipple"))
                    {
                        BitmapImage pezo2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pezon_Izq_5001, myColor));
                        part48.Source = pezo2;
                    }
                    else if (seccion == obtenerRecurso("Right-Foot"))
                    {
                        BitmapImage pie1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pie_Der_5002, myColor));
                        part49.Source = pie1;
                    }
                    else if (seccion == obtenerRecurso("Left-Foot"))
                    {
                        BitmapImage pie2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pie_Izq_5002, myColor));
                        part50.Source = pie2;
                    }
                    else if (seccion == obtenerRecurso("Right-Hand Thumb"))
                    {
                        BitmapImage pulgar1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pulgar_Mano_Der_5001, myColor));
                        part51.Source = pulgar1;
                    }
                    else if (seccion == obtenerRecurso("Left-Hand Thumb"))
                    {
                        BitmapImage pulgar2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pulganr_Mano_Izq_500, myColor));
                        part52.Source = pulgar2;
                    }
                    else if (seccion == obtenerRecurso("Right Big Toe"))
                    {
                        BitmapImage pulgar3 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pulgar_Pie_Der_5001, myColor));
                        part53.Source = pulgar3;
                    }
                    else if (seccion == obtenerRecurso("Left Big Toe"))
                    {
                        BitmapImage pulgar4 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pulgar_Pie_Izq_5001, myColor));
                        part54.Source = pulgar4;
                    }
                    else if (seccion == obtenerRecurso("Right-Knee"))
                    {
                        BitmapImage rod = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Rodilla_Der_5001, myColor));
                        part55.Source = rod;
                    }
                    else if (seccion == obtenerRecurso("Left-Knee"))
                    {
                        BitmapImage rod2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Rodilla_Izq_5001, myColor));
                        part56.Source = rod2;
                    }
                    else if (seccion == obtenerRecurso("Right-Ankle"))
                    {
                        BitmapImage tob = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Tobillo_Der_5001, myColor));
                        part57.Source = tob;
                    }
                    else if (seccion == obtenerRecurso("Left-Ankle"))
                    {
                        BitmapImage tob2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Tobillo_Izq_5001, myColor));
                        part58.Source = tob2;
                    }
                    else if (seccion == obtenerRecurso("Uterus"))
                    {
                        BitmapImage ut = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Utero_500, myColor));
                        part59.Source = ut;
                    }
                    else if (seccion == obtenerRecurso("Vagina"))
                    {
                        BitmapImage vag = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Vagina_500, myColor));
                        part60.Source = vag;
                    }
                    else if (seccion == obtenerRecurso("Bladder"))
                    {
                        BitmapImage vej = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Vejiga_500, myColor));
                        part61.Source = vej;
                    }


                }

                if (radioFemenino.IsChecked == true && listVista.SelectedItem.ToString() == obtenerRecurso("valBack"))
                {
                    //////////////

                    //Partes que le corresponden
                    //Seccion elegida
                    string seccion = listaSecciones.SelectedItem.ToString();
                    //  MessageBox.Show(seccion);

                    //Agregue la parte a la lista y su codigo
                    HacerConexion();

                    //Obtiene el codigo del color..
                    object cod_color = obj2.Obtener_Color_ParteTerapia(color_elegido.Content.ToString());
                    object cod_seccion = obj2.Obtener_CodigoParte_Color_ParteTerapia(seccion);

                    //Sino existe la parte elegida en rad_codigos generar un random para ello
                    if (cod_seccion == null)
                    {
                        cod_seccion = obj.RandomDigits(obj_m.Next(8, 14));
                    }

                    CerrarConexion();

                    //Listas con las partes
                    cod_partes_colores.Add(cod_color.ToString()); //Guarda el codigo del color
                    cod_partes_color.Add(cod_seccion.ToString()); //Guarda el codigo de la parte elegida

                    // Color - Seccion (Parte) - Vista  - Genero
                    string descrip_color = color_elegido.Content.ToString() + " - " + seccion + " - " + listVista.SelectedItem.ToString() + " - " + radioMasculino.Content.ToString();

                    //Guarda descripcion
                    partes_colores.Add(descrip_color); //Guarda la parte

                    //Agregar a la lista para visualizar (Combina el codigo uniendo cod_seccion+cod_color)
                    listadoCodigosComb.Items.Add(new nuevoColorCombinado { codigocomp = cod_seccion.ToString() + cod_color.ToString(), nombrecodigocomp = descrip_color });

                    //////////

                    //switch (seccion)
                    //{
                    //    case "Anus":
                    //        BitmapImage ano = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Ano_500, myColor));
                    //        part1.Source = ano;
                    //        break;

                    //    case "Right-Forearm":
                    //        BitmapImage ant1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Antebrazo_Der_5002, myColor));
                    //        part2.Source = ant1;
                    //        break;

                    //    case "Left-Forearm":
                    //        BitmapImage ant2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Antebrazo_Izq_5002, myColor));
                    //        part3.Source = ant2;
                    //        break;

                    //    case "Right-Arm":
                    //        BitmapImage braz = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Brazo_Der_5002, myColor));
                    //        part4.Source = braz;
                    //        break;

                    //    case "Left-Arm":
                    //        BitmapImage braz2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Brazo_Izq_5002, myColor));
                    //        part5.Source = braz2;
                    //        break;

                    //    case "Head":
                    //        BitmapImage cab = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Cabeza_500, myColor));
                    //        part6.Source = cab;
                    //        break;

                    //    case "Right-Hip":
                    //        BitmapImage cad1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Cadera_Der_5003, myColor));
                    //        part7.Source = cad1;
                    //        break;

                    //    case "Left-Hip":
                    //        BitmapImage cad2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Cadera_Izq_5003, myColor));
                    //        part8.Source = cad2;
                    //        break;

                    //    case "Right-Clavicle":
                    //        BitmapImage clav = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Clavicula_Der_5002, myColor));
                    //        part9.Source = clav;
                    //        break;

                    //    case "Left-Clavicle":
                    //        BitmapImage clav2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Clavicula_Izq_5002, myColor));
                    //        part10.Source = clav2;
                    //        break;

                    //    case "Right-Elbow":
                    //        BitmapImage codo1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Codo_Der_500, myColor));
                    //        part11.Source = codo1;
                    //        break;

                    //    case "Left-Elbow":
                    //        BitmapImage codo2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Codo_Izq_5001, myColor));
                    //        part12.Source = codo2;
                    //        break;

                    //    case "Column":
                    //        BitmapImage colum = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Columna_500, myColor));
                    //        part13.Source = colum;
                    //        break;

                    //    case "Coxis":
                    //        BitmapImage coxis = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Coxis_500, myColor));
                    //        part14.Source = coxis;
                    //        break;

                    //    case "Neck":
                    //        BitmapImage cuell = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Cuello_5001, myColor));
                    //        part15.Source = cuell;
                    //        break;

                    //    case "Right-Fingers":
                    //        BitmapImage dedoss = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Dedos_Der_5001, myColor));
                    //        part16.Source = dedoss;
                    //        break;

                    //    case "Left-Fingers":
                    //        BitmapImage dedoss2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Dedos_Izq_5002, myColor));
                    //        part17.Source = dedoss2;
                    //        break;

                    //    case "Back":
                    //        BitmapImage esp1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Espalda_5001, myColor));
                    //        part18.Source = esp1;
                    //        break;

                    //    case "Right-Phalanges":
                    //        BitmapImage faag = ToBitmapImage(ChangeColor(HS5.Properties.Resources.falanges_Der_5003, myColor));
                    //        part19.Source = faag;
                    //        break;

                    //    case "Left-Phalanges":
                    //        BitmapImage falag2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Falanges_Izq_500, myColor));
                    //        part20.Source = falag2;
                    //        break;

                    //    case "Right-Shoulder":
                    //        BitmapImage hom = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Hombro_Der_5002, myColor));
                    //        part21.Source = hom;
                    //        break;

                    //    case "Left-Shoulder":
                    //        BitmapImage hom2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Hombro_Izq_5002, myColor));
                    //        part22.Source = hom2;
                    //        break;

                    //    case "Right-Hand":
                    //        BitmapImage man = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Mano_Der_5003, myColor));
                    //        part23.Source = man;
                    //        break;

                    //    case "Left-Hand":
                    //        BitmapImage man2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Mano_Izq_5003, myColor));
                    //        part24.Source = man2;
                    //        break;

                    //    case "Right-Wrist":
                    //        BitmapImage mun = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Muñeca_Der_500, myColor));
                    //        part25.Source = mun;
                    //        break;

                    //    case "Left-Wrist":
                    //        BitmapImage mun2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Muñeca_izq_5001, myColor));
                    //        part26.Source = mun2;
                    //        break;

                    //    case "Right-Buttock":
                    //        BitmapImage nal = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Nalga_Der_500, myColor));
                    //        part27.Source = nal;
                    //        break;

                    //    case "Left-Buttock":
                    //        BitmapImage nal2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Nalga_Izq_500, myColor));
                    //        part28.Source = nal2;
                    //        break;

                    //    case "Right-Calf":
                    //        BitmapImage pant = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pantorrilla_Der_5003, myColor));
                    //        part29.Source = pant;
                    //        break;

                    //    case "Left-Calf":
                    //        BitmapImage pant2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pantorrilla_Izq_5003, myColor));
                    //        part30.Source = pant2;
                    //        break;

                    //    case "Right-Foot":
                    //        BitmapImage pie2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pie_Der_5003, myColor));
                    //        part31.Source = pie2;
                    //        break;

                    //    case "Left-Foot":
                    //        BitmapImage pie = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pie_Izq_5003, myColor));
                    //        part32.Source = pie;
                    //        break;

                    //    case "Right-Leg":
                    //        BitmapImage pierna = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pierna_Der_500, myColor));
                    //        part33.Source = pierna;
                    //        break;

                    //    case "Left-Leg":
                    //        BitmapImage pierna2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pierna_Izq_500, myColor));
                    //        part34.Source = pierna2;
                    //        break;

                    //    case "Right-Thumb":
                    //        BitmapImage pulgar = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pulgar_Der_500, myColor));
                    //        part35.Source = pulgar;
                    //        break;

                    //    case "Left-Thumb":
                    //        BitmapImage pulgar2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pulgar_Izq_5002, myColor));
                    //        part36.Source = pulgar2;
                    //        break;

                    //    case "Left-Heel":
                    //        BitmapImage talon = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Talon_Der_5001, myColor));
                    //        part37.Source = talon;
                    //        break;

                    //    case "Right-Heel":
                    //        BitmapImage talon2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Talon_Izq_5001, myColor));
                    //        part38.Source = talon2;
                    //        break;

                    //    case "Right-Achilles heel":
                    //        BitmapImage aq = ToBitmapImage(ChangeColor(HS5.Properties.Resources.TAquiles_Der_500, myColor));
                    //        part39.Source = aq;
                    //        break;

                    //    case "Left-Achilles heel":
                    //        BitmapImage aq2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.TAquiles_Izq_500, myColor));
                    //        part40.Source = aq2;
                    //        break;

                    //    case "Right-Ankle":
                    //        BitmapImage tob = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Tobillo_Derecho_500, myColor));
                    //        part41.Source = tob;
                    //        break;

                    //    case "Left-Ankle":
                    //        BitmapImage tob2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Tobillo_Izq_5002, myColor));
                    //        part42.Source = tob2;
                    //        break;

                    //}

                    if (seccion == obtenerRecurso("Anus"))
                    {
                        BitmapImage ano = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Ano_500, myColor));
                        part1.Source = ano;
                    }
                    else if (seccion == obtenerRecurso("Right-Forearm"))
                    {
                        BitmapImage ant1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Antebrazo_Der_5002, myColor));
                        part2.Source = ant1;
                    }
                    else if (seccion == obtenerRecurso("Left-Forearm"))
                    {
                        BitmapImage ant2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Antebrazo_Izq_5002, myColor));
                        part3.Source = ant2;
                    }
                    else if (seccion == obtenerRecurso("Right-Arm"))
                    {
                        BitmapImage braz = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Brazo_Der_5002, myColor));
                        part4.Source = braz;
                    }
                    else if (seccion == obtenerRecurso("Left-Arm"))
                    {
                        BitmapImage braz2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Brazo_Izq_5002, myColor));
                        part5.Source = braz2;
                    }
                    else if (seccion == obtenerRecurso("Head"))
                    {
                        BitmapImage cab = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Cabeza_500, myColor));
                        part6.Source = cab;
                    }
                    else if (seccion == obtenerRecurso("Right-Hip"))
                    {
                        BitmapImage cad1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Cadera_Der_5003, myColor));
                        part7.Source = cad1;
                    }
                    else if (seccion == obtenerRecurso("Left-Hip"))
                    {
                        BitmapImage cad2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Cadera_Izq_5003, myColor));
                        part8.Source = cad2;
                    }
                    else if (seccion == obtenerRecurso("Right-Clavicle"))
                    {
                        BitmapImage clav = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Clavicula_Der_5002, myColor));
                        part9.Source = clav;
                    }
                    else if (seccion == obtenerRecurso("Left-Clavicle"))
                    {
                        BitmapImage clav2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Clavicula_Izq_5002, myColor));
                        part10.Source = clav2;
                    }
                    else if (seccion == obtenerRecurso("Right-Elbow"))
                    {
                        BitmapImage codo1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Codo_Der_500, myColor));
                        part11.Source = codo1;
                    }
                    else if (seccion == obtenerRecurso("Left-Elbow"))
                    {
                        BitmapImage codo2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Codo_Izq_5001, myColor));
                        part12.Source = codo2;
                    }
                    else if (seccion == obtenerRecurso("Column"))
                    {
                        BitmapImage colum = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Columna_500, myColor));
                        part13.Source = colum;
                    }
                    else if (seccion == obtenerRecurso("Coxis"))
                    {
                        BitmapImage coxis = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Coxis_500, myColor));
                        part14.Source = coxis;
                    }
                    else if (seccion == obtenerRecurso("Neck"))
                    {
                        BitmapImage cuell = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Cuello_5001, myColor));
                        part15.Source = cuell;
                    }
                    else if (seccion == obtenerRecurso("Right-Fingers"))
                    {
                        BitmapImage dedoss = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Dedos_Der_5001, myColor));
                        part16.Source = dedoss;
                    }
                    else if (seccion == obtenerRecurso("Left-Fingers"))
                    {
                        BitmapImage dedoss2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Dedos_Izq_5002, myColor));
                        part17.Source = dedoss2;
                    }
                    else if (seccion == obtenerRecurso("Back"))
                    {
                        BitmapImage esp1 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Espalda_5001, myColor));
                        part18.Source = esp1;
                    }
                    else if (seccion == obtenerRecurso("Right-Phalanges"))
                    {
                        BitmapImage faag = ToBitmapImage(ChangeColor(HS5.Properties.Resources.falanges_Der_5003, myColor));
                        part19.Source = faag;
                    }
                    else if (seccion == obtenerRecurso("Left-Phalanges"))
                    {
                        BitmapImage falag2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Falanges_Izq_500, myColor));
                        part20.Source = falag2;
                    }
                    else if (seccion == obtenerRecurso("Right-Shoulder"))
                    {
                        BitmapImage hom = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Hombro_Der_5002, myColor));
                        part21.Source = hom;
                    }
                    else if (seccion == obtenerRecurso("Left-Shoulder"))
                    {
                        BitmapImage hom2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Hombro_Izq_5002, myColor));
                        part22.Source = hom2;
                    }
                    else if (seccion == obtenerRecurso("Right-Hand"))
                    {
                        BitmapImage man = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Mano_Der_5003, myColor));
                        part23.Source = man;
                    }
                    else if (seccion == obtenerRecurso("Left-Hand"))
                    {
                        BitmapImage man2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Mano_Izq_5003, myColor));
                        part24.Source = man2;
                    }
                    else if (seccion == obtenerRecurso("Right-Wrist"))
                    {
                        BitmapImage mun = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Muñeca_Der_500, myColor));
                        part25.Source = mun;
                    }
                    else if (seccion == obtenerRecurso("Left-Wrist"))
                    {
                        BitmapImage mun2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Muñeca_izq_5001, myColor));
                        part26.Source = mun2;
                    }
                    else if (seccion == obtenerRecurso("Right-Buttock"))
                    {
                        BitmapImage nal = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Nalga_Der_500, myColor));
                        part27.Source = nal;
                    }
                    else if (seccion == obtenerRecurso("Left-Buttock"))
                    {
                        BitmapImage nal2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Nalga_Izq_500, myColor));
                        part28.Source = nal2;
                    }
                    else if (seccion == obtenerRecurso("Right-Calf"))
                    {
                        BitmapImage pant = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pantorrilla_Der_5003, myColor));
                        part29.Source = pant;
                    }
                    else if (seccion == obtenerRecurso("Left-Calf"))
                    {
                        BitmapImage pant2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pantorrilla_Izq_5003, myColor));
                        part30.Source = pant2;
                    }
                    else if (seccion == obtenerRecurso("Right-Foot"))
                    {
                        BitmapImage pie2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pie_Der_5003, myColor));
                        part31.Source = pie2;
                    }
                    else if (seccion == obtenerRecurso("Left-Foot"))
                    {
                        BitmapImage pie = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pie_Izq_5003, myColor));
                        part32.Source = pie;
                    }
                    else if (seccion == obtenerRecurso("Right-Leg"))
                    {
                        BitmapImage pierna = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pierna_Der_500, myColor));
                        part33.Source = pierna;
                    }
                    else if (seccion == obtenerRecurso("Left-Leg"))
                    {
                        BitmapImage pierna2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pierna_Izq_500, myColor));
                        part34.Source = pierna2;
                    }
                    else if (seccion == obtenerRecurso("Right-Thumb"))
                    {
                        BitmapImage pulgar = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pulgar_Der_500, myColor));
                        part35.Source = pulgar;
                    }
                    else if (seccion == obtenerRecurso("Left-Thumb"))
                    {
                        BitmapImage pulgar2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Pulgar_Izq_5002, myColor));
                        part36.Source = pulgar2;
                    }
                    else if (seccion == obtenerRecurso("Left-Heel"))
                    {
                        BitmapImage talon = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Talon_Der_5001, myColor));
                        part37.Source = talon;
                    }
                    else if (seccion == obtenerRecurso("Right-Heel"))
                    {
                        BitmapImage talon2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Talon_Izq_5001, myColor));
                        part38.Source = talon2;
                    }
                    else if (seccion == obtenerRecurso("Right-Achilles heel"))
                    {
                        BitmapImage aq = ToBitmapImage(ChangeColor(HS5.Properties.Resources.TAquiles_Der_500, myColor));
                        part39.Source = aq;
                    }
                    else if (seccion == obtenerRecurso("Left-Achilles heel"))
                    {
                        BitmapImage aq2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.TAquiles_Izq_500, myColor));
                        part40.Source = aq2;
                    }
                    else if (seccion == obtenerRecurso("Right-Ankle"))
                    {
                        BitmapImage tob = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Tobillo_Derecho_500, myColor));
                        part41.Source = tob;
                    }
                    else if (seccion == obtenerRecurso("Left-Ankle"))
                    {
                        BitmapImage tob2 = ToBitmapImage(ChangeColor(HS5.Properties.Resources.Tobillo_Izq_5002, myColor));
                        part42.Source = tob2;
                    }



                }
            }
            else
            {
                MessageBox.Show(obtenerRecurso("messageError26"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }

        }

        private void listaSecciones_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void listaSecciones_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }

        void Cargar_Codigos(string id_categoria_padre, string id_subcategoria_c)
        {
            ClearData(listadoCodigos_Copy);
           
            listadoCodigos_Copy.Items.Clear();
            Categorias_Codigos2.Clear(); //Limpia los codigos guardados
            try
            {
                HacerConexion();
                object id_categoria = id_categoria_padre;

                object id_subcategoria = id_subcategoria_c;

                /// AGREGADO
                /// 
                //Buscar el sexo del paciente
                // DataTable paciente_sexo_tabla = obj2.VisualizarAnalisisPorGenero();
                /*string sexo = "";

                for (int a = 0; a <= paciente_sexo_tabla.Rows.Count - 1; a++)
                {
                    //Si es igual el nombre obtener el sexo
                    if (paciente_sexo_tabla.Rows[a][1].ToString() == lblPacienteAnalisis_P1.Content.ToString())
                    {
                        //MessageBox.Show(paciente_sexo_tabla.Rows[a][0].ToString());
                        if (paciente_sexo_tabla.Rows[a][0].ToString() == "Masculino")
                        {
                            sexo = "M";
                        }

                        if (paciente_sexo_tabla.Rows[a][0].ToString() == "Femenino")
                        {
                            sexo = "F";
                        }

                        if (paciente_sexo_tabla.Rows[a][0].ToString() == "Animal")
                        {
                            sexo = "A";
                        }

                        if (paciente_sexo_tabla.Rows[a][0].ToString() == "Plantas y suelo")
                        {
                            sexo = "P";
                        }

                    }
                }*/

                DataTable Codigos = obj2.VisualizarSubCategoriasCodigosListado(id_subcategoria.ToString(), "T");

                //MessageBox.Show(Codigos.Rows.Count.ToString());
                // MessageBox.Show(id_subcategoria.ToString());
                id_categoria_cop = id_categoria.ToString(); //Guarda la categoria para generar nuevo codigo;

                // MessageBox.Show(Codigos.Rows.Count.ToString());

                if (Codigos.Rows.Count == 0) //Sino hay por genero pues utiliza el de todos...
                {
                    Codigos = obj2.VisualizarSubCategoriasCodigosListado(id_subcategoria.ToString(), "T");
                }
                //  else // De lo contrario si existen codigos por genero entonces.. que seria genero mas genero=T
                //  {
                //       Codigos = obj2.VisualizarSubCategoriasCodigosListadoGenero_Todos(id_subcategoria.ToString(), sexo);
                //   }

                //MessageBox.Show(Codigos.Rows[0][1].ToString());
                //AGREGADO HASTA AQUI

                for (int y = 0; y <= Codigos.Rows.Count - 1; y++)
                {
                    if (Codigos.Rows[y][1].ToString() != "")
                    {
                        //listadoCodigos.Items.Add(new CheckBox { Content = Codigos.Rows[y][1].ToString() });
                        listadoCodigos_Copy.Items.Add(Codigos.Rows[y][1].ToString() + "  ,  " + Codigos.Rows[y][2].ToString());
                        Categorias_Codigos2.Add(Codigos.Rows[y][2].ToString()); //Guarda el codigo
                    }
                }

                lblCodigosCont.Content = listadoCodigos_Copy.Items.Count + " " + obtenerRecurso("labelRate");

                CerrarConexion();
            }
            catch (NullReferenceException)
            {
               // MessageBox.Show("Please select a category first!", "Warning", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void cmdNuevoCod_Click(object sender, RoutedEventArgs e)
        {
            //Duplicar el remedio
            string nombre_codigo= Interaction.InputBox(obtenerRecurso("inputMessage2"), obtenerRecurso("inputHeadMessage1"), "", 300, 300);

            if (nombre_codigo != ""  )
            {
                try
                {

                    // description = Interaction.InputBox("Description - (OPTIONAL)", "New Rate", "", 300, 300);
                    HacerConexion();

                    //Objeto
                    Radionica obj_new = new Radionica();

                    //Genero
                    //object genero_para_codigo = obj2.Buscar_Genero(id_categoria_cop, id_categoria_padre);

                    if (listadoSubcategorias_Copy.SelectedItem != null)
                    {
                        
                        Console.WriteLine(listadoSubcategorias_Copy.SelectedItem.ToString());
                        //Categoria padre
                        string id_cat_pad = obj2.Obtener_IDCategoria(listadoCategorias_Copy.SelectedItem.ToString()).ToString();

                        //Subcategoria
                        string id_subcat = obj2.Obtener_IDCategoria(listadoSubcategorias_Copy.SelectedItem.ToString()).ToString();
                        Console.WriteLine(id_subcat);

                        object genero_para_codigo = "T";

                        obj2.Registrar_Codigo_Categorias(obj_new.Generar_Id(), nombre_codigo, obj2.Generarcodigo(), "-", id_subcat, id_cat_pad, genero_para_codigo.ToString());

                        //Cargar_Codigos(id_categoria_padre, id_categoria_cop); //Carga los codigos actualizados con el agregado
                        listadoSubcategorias_Copy.Items.Clear();
                        ClearData(listadoCodigos_Copy);
                        listadoCodigos_Copy.Items.Clear();
                        Categorias_Codigos2.Clear();
                    }
                    else
                    {
                        MessageBox.Show(obtenerRecurso("messageError25"), obtenerRecurso("messageHeadWarning"), MessageBoxButton.OK, MessageBoxImage.Exclamation);

                    }
                    lblCodigosCont.Content = listadoCodigos_Copy.Items.Count + " " + obtenerRecurso("labelRate");
                    CerrarConexion();
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

        private void cmdBorrarCod_Click(object sender, RoutedEventArgs e)
        {
            //Seleccionar el elemento a eliminar
            try
            {
                DataRowView rowView = (DataRowView)listadoCodigos_Copy.SelectedItem;

                DataTable dataTable = new DataTable();

                // Agregar columnas al DataTable
                foreach (DataColumn column in rowView.Row.Table.Columns)
                {
                    dataTable.Columns.Add(column.ColumnName, column.DataType);
                }

                // Agregar la fila de DataRowView al DataTable
                dataTable.ImportRow(rowView.Row);

                string elemento_borrar = dataTable.Rows[0][0].ToString();
                Console.WriteLine(elemento_borrar);
        

                //Eliminar objeto
                HacerConexion();
               
                obj2.Eliminar_Codigo(elemento_borrar);
                //obj2.Eliminar_CodigosCategorias()

                CerrarConexion();
                Cargar_Codigos(id_categoria_padre, id_categoria_cop);
            }
            catch (NullReferenceException)
            {
                //MessageBox.Show("Please select a!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        private void cmdEliminarCodComp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                object seleccionado = listadoCodigosComb.SelectedItem;

                IEnumerable seleccionado_color = this.listadoCodigosComb.SelectedItems;

                foreach (nuevoColorCombinado color in seleccionado_color)
                {

                    //Ciclo para borrar elementos de la lista
                    for (int i = 0; i <= partes_colores.Count - 1; i++)
                    {
                        //MessageBox.Show("HOLA");
                        //Remueve el elemento de las 3 listas
                        if (color.codigocomp == cod_partes_color[i] + cod_partes_colores[i])
                        {
                            cod_partes_colores.RemoveAt(i);
                            cod_partes_color.RemoveAt(i);
                            partes_colores.RemoveAt(i);
                            break;
                        }
                    }

                }
                listadoCodigosComb.Items.Remove(seleccionado);

                for (int i = 0; i <= partes_colores.Count - 1; i++)
                {
                    MessageBox.Show(partes_colores[i].ToString());
                }

            }
            catch (NullReferenceException)
            {
                MessageBox.Show(obtenerRecurso("messageError23"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        //Busqueda de pacientes recientes en la area de diagnostico
        private void cmdAnalisisPaciente_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (cmdAnalisisPaciente.Text != "")
            {
                DataTable pacientes = obj2.Mostrar_Pacientes_Listado_Sencillo_2(cmdAnalisisPaciente.Text);

                listadoPacientes.Items.Clear();

                //Pasarlo al listado de pacientes
                for (int j = 0; j <= pacientes.Rows.Count - 1; j++)
                {
                    listadoPacientes.Items.Add(pacientes.Rows[j][0].ToString());
                }
            }else
            {
                Cargar_Pacientes_Diagnostico();
            }
        }



        //this function is used when the user chose the patient to create a new analysis
        private void listadoPacientes_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            comboOtrosAnal.Items.Clear();

            try
            {
                DataTable AnalisisPaciente_Seleccionado = obj2.Obtener_Analisis_Pacientes_Recientes_PorNombrePaciente(listadoPacientes.SelectedItem.ToString());

                lblPacienteAnalisis_Copy.Content = obtenerRecurso("labelRAN") +":";
                lblPacienteAnalisis_Copy.Content = lblPacienteAnalisis_Copy.Content + "  " + listadoPacientes.SelectedItem.ToString();

                //Llenar el combobox con analisis relacionados
                for (int i = 0; i <= AnalisisPaciente_Seleccionado.Rows.Count - 1; i++)
                {
                    //Agregar solo nombre del analisis
                    comboOtrosAnal.Items.Add(AnalisisPaciente_Seleccionado.Rows[i][0].ToString());
                }
                CargarRegistrosPacientesRecientes();
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(obtenerRecurso("messageError22"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmdAnalizarr_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                HacerConexion();

                //Revisar si el nombre del analisis ya esta en uso ...
                object id_analisis = obj2.Obtener_Id_Analisis(cmdAnalisisPaciente_Copy.Text);

                if (id_analisis == null)
                {
                    try
                    {
                        if (cmdAnalisisPaciente_Copy.Text != "")
                        {
                            string[] pac = new string[2] { listadoPacientes.SelectedItem.ToString(), cmdAnalisisPaciente_Copy.Text };


                            //Registrar analisis con el nombre elegido en bd
                            DataTable pacientes = obj2.Buscar_IdPaciente_Nombre(listadoPacientes.SelectedItem.ToString());

                            obj2.RegistrarAnalisisPaciente_Diag(pacientes.Rows[0][0].ToString(), cmdAnalisisPaciente_Copy.Text, DateTime.Now, 4, 0, 0, DateTime.Now);

                            cmdAnalisisPaciente_Copy.Clear(); //Limpia la casilla
                            A_Diagnosticar(pac);
                            cmdNuevoAnalisis1.IsEnabled = true; //Activa el boton de nuevo analisis o reanalisis
                            cmdReanalizarr.IsEnabled = true;

                        }
                        else
                        {
                            MessageBox.Show(obtenerRecurso("messageError21"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            cmdAnalisisPaciente_Copy.Focus();
                        }
                    }
                    catch (NullReferenceException)
                    {
                        MessageBox.Show(obtenerRecurso("messageError20"));
                    }
                }
                else
                {
                    MessageBox.Show(string.Join(cmdAnalisisPaciente_Copy.Text, obtenerRecurso("messageError19").Split('-')), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    cmdAnalisisPaciente_Copy.Focus();
                }
                CerrarConexion();
            }
            catch(Exception mx)
            {
                MessageBox.Show(mx.ToString());
            }
        }

        void Ocultar_Diag()
        {

            //Controles al elegir un analisis los oculta
            //Principales
            BusquedaAnalisisGroup1.Visibility = Visibility.Hidden;
            Lista_Analisis_Group1.Visibility = Visibility.Hidden;
            lblBusqueda_Copy1.Visibility = Visibility.Hidden;
            txtBuscarPaciente2.Visibility = Visibility.Hidden;
            cmdNuevoAnalisis1.Visibility = Visibility.Hidden;
            cmdEliminar_Copy1.Visibility = Visibility.Hidden;
            ListaPacientes_Recientes1.Visibility = Visibility.Hidden;

            //Parte del reanalisis
            listadoPacientes.Visibility = Visibility.Hidden;
            BusquedaReanalisis.Visibility = Visibility.Hidden;
            lblPacienteAnalisis2.Visibility = Visibility.Hidden;
            cmdAnalisisPaciente.Visibility = Visibility.Hidden;
            cmdAnalisisPaciente_Copy.Visibility = Visibility.Hidden;
            cmdAnalizarr.Visibility = Visibility.Hidden;
            cmdReanalizarr.Visibility = Visibility.Hidden;
            lblPacienteAnalisis_Copy.Visibility = Visibility.Hidden;
            comboOtrosAnal.Visibility = Visibility.Hidden;
            lblPacienteAnalisis.Visibility = Visibility.Hidden;
            cmdAnalisisReanalisisCerrar.Visibility = Visibility.Hidden;
            ventana1.Visibility = Visibility.Hidden;
        }

        //Lista con id's de referencia
        //List<string> idscodigos = new List<string>();

        void Checar_SiYaFueAnalizado(string validacion_analisis)
        {
            if (validacion_analisis.ToString() == "1")
            {
                listadoCategorias.Visibility = Visibility.Hidden;
                listadoSubcategorias.Visibility = Visibility.Hidden;
                listadoCodigos.Visibility = Visibility.Hidden;
                lblCategorias.Visibility = Visibility.Hidden;
                lblSubCategorias.Visibility = Visibility.Hidden;
                lblCodigoBuscar.Visibility = Visibility.Hidden;
                txtCodigoBuscar.Visibility = Visibility.Hidden;
                cmdTodos.Visibility = Visibility.Hidden;
                cmdNinguno.Visibility = Visibility.Hidden;

                //Parte superior de las tabs
                lblPacienteAnalisis1.Visibility = Visibility.Visible;
                lblPacienteAnalisis_P1.Visibility = Visibility.Visible;
                lblNombreAnalisis1.Visibility = Visibility.Visible;
                lblNombre_Anal1.Visibility = Visibility.Visible;
                lblFechaAnalisis2.Visibility = Visibility.Visible;
                lblFechaAnalisis3.Visibility = Visibility.Visible;
                //cmdGuardarDiagnostico.Visibility = Visibility.Visible;
                // cmdNuevoDiagnos.Visibility = Visibility.Visible;
                cmdListaDiagnos.Visibility = Visibility.Visible;
                Diagnosticos_Tabs.Visibility = Visibility.Visible;

                //Contenido de las tabs
                TipoAnalisis_Group.Visibility = Visibility.Visible;
                //optionProbabilidad.Visibility = Visibility.Visible;
                optionPorcentaje.Visibility = Visibility.Visible;
                option100.Visibility = Visibility.Visible;
                //optionPolaridad.Visibility = Visibility.Visible;
                //optionPronunciamiento.Visibility = Visibility.Visible;
                optionSugerirNiv.Visibility = Visibility.Visible;
                optionSugerirPot.Visibility = Visibility.Visible;
                optionradionico.Visibility = Visibility.Visible;
                nivellabel.Visibility = Visibility.Visible;
                comboNiveles.Visibility = Visibility.Visible;
                nivelP.Visibility = Visibility.Visible;
                comboP.Visibility=Visibility.Visible;
                cmdIniciarDiag.Visibility = Visibility.Visible;
                cmdAgregarCodigos.Visibility = Visibility.Visible;
                cmdHacerRemedios.Visibility = Visibility.Visible;
                cmdGuardarTarjeta.Visibility = Visibility.Visible;
                cmdEnviarFrecuencia.Visibility = Visibility.Visible;
                cmdDocumento.Visibility = Visibility.Visible;

                Panel_opciones();
 
                //Siempre visible
                borderInfobasica_Copy2.Visibility = Visibility.Visible;
                cmdProcesarAnalisis.Visibility = Visibility.Visible;
                cmdBorrar.Visibility = Visibility.Visible;
                cmdRango.Visibility = Visibility.Visible;
                cmdProcesarAnalisis.IsEnabled = false;
                ListaCodigos.Visibility = Visibility.Visible;

                HacerConexion();

                //PENDIENTE CARGAR CODIGOS DE UN ANALISIS PREVIAMENTE REGISTRADO... JALAR DE LISTACODIGOS..
                object id_analisis = obj2.Obtener_Id_Analisis(lblPacienteAnalisis_P1.Content.ToString());
              //  MessageBox.Show(lblPacienteAnalisis_P1.Content.ToString() + " " + id_analisis.ToString());
                DataTable tabla_codigosanalisis = obj2.Obtener_CodigosAnalisis(id_analisis.ToString());

                //Ciclo con los codigos de analisis
                for (int p = 0; p <= tabla_codigosanalisis.Rows.Count - 1; p++)
                {  // 3
                   // idscodigos.Add(tabla_codigosanalisis.Rows[p][2].ToString()); //Guarda idcodigo del analisis
                   //codigo,nombrecodigo,nivel,nivelsugerido,valor,vinicial,vfinal,decimales,tipo
                    ListaCodigos.Items.Add(new nuevoCodigo { rates = tabla_codigosanalisis.Rows[p][3].ToString(), nombre = tabla_codigosanalisis.Rows[p][4].ToString(), niveles = tabla_codigosanalisis.Rows[p][5].ToString(), nsugerido = tabla_codigosanalisis.Rows[p][6].ToString(), ftester = Convert.ToInt32(tabla_codigosanalisis.Rows[p][7].ToString()), potencia = tabla_codigosanalisis.Rows[p][12].ToString(), potenciaSugeridad = tabla_codigosanalisis.Rows[p][13].ToString() });

                }

                obj2.Eliminar_Codigos(id_analisis.ToString());

                //Cargar informacion biologica
                DataTable info_biologica = obj2.Obtener_InfoBiologica(lblPacienteAnalisis_P1.Content.ToString());

                if (info_biologica.Rows[0][0].ToString() == "")
                {
                    txtEstatura.Text = "0.0";
                }
                else
                {
                    txtEstatura.Text = info_biologica.Rows[0][0].ToString();
                }

                if (info_biologica.Rows[0][1].ToString() == "")
                {
                    txtPresionSist.Text = "0.0";
                }
                else
                {
                    txtPresionSist.Text = info_biologica.Rows[0][1].ToString();
                }

                if (info_biologica.Rows[0][2].ToString() == "")
                {
                    txtIMC.Text = "0.0";
                }
                else
                {
                    txtIMC.Text = info_biologica.Rows[0][2].ToString();

                }

                if (info_biologica.Rows[0][3].ToString() == "")
                {
                    txtFR.Text = "0.0";
                }
                else
                {
                    txtFR.Text = info_biologica.Rows[0][3].ToString();
                }

                if (info_biologica.Rows[0][4].ToString() == "")
                {
                    txtTA.Text = "0.0";
                }
                else
                {
                    txtTA.Text = info_biologica.Rows[0][4].ToString();

                }

                if (info_biologica.Rows[0][5].ToString() == "")
                {
                    txtPeso.Text = "0.0";
                }
                else
                {
                    txtPeso.Text = info_biologica.Rows[0][5].ToString();
                }

                if (info_biologica.Rows[0][6].ToString() == "")
                {
                    txtPresionDistolica.Text = "0.0";
                }
                else
                {
                    txtPresionDistolica.Text = info_biologica.Rows[0][6].ToString();

                }

                if (info_biologica.Rows[0][7].ToString() == "")
                {
                    txtFC.Text = "0.0";
                }
                else
                {
                    txtFC.Text = info_biologica.Rows[0][7].ToString();
                }

                if (info_biologica.Rows[0][8].ToString() == "")
                {
                    txtTemp.Text = "0.0";
                }
                else
                {
                    txtTemp.Text = info_biologica.Rows[0][8].ToString();
                }


                //Cargar padecimiento
                DataTable info_padecimiento = obj2.Obtener_InfoPadecimiento(lblPacienteAnalisis_P1.Content.ToString());

                if (info_padecimiento.Rows[0][0].ToString() == "")
                {
                    txtPadecimiento.Text = "";
                }
                else
                {
                    txtPadecimiento.Text = info_padecimiento.Rows[0][0].ToString();
                }

                if (info_padecimiento.Rows[0][1].ToString() == "")
                {
                    txtInterrogatorio.Text = "";
                }
                else
                {
                    txtInterrogatorio.Text = info_padecimiento.Rows[0][1].ToString();
                }

                CerrarConexion();
                lblContCodigos.Content = ListaCodigos.Items.Count;
            }

            if (validacion_analisis.ToString() == "" || validacion_analisis.ToString() == "0")
            {
                lblPacienteAnalisis1.Visibility = Visibility.Visible;
                lblPacienteAnalisis_P1.Visibility = Visibility.Visible;
                lblNombreAnalisis1.Visibility = Visibility.Visible;
                lblNombre_Anal1.Visibility = Visibility.Visible;
                lblFechaAnalisis2.Visibility = Visibility.Visible;
                lblFechaAnalisis3.Visibility = Visibility.Visible;
                //cmdGuardarDiagnostico.Visibility = Visibility.Visible;
                //   cmdNuevoDiagnos.Visibility = Visibility.Visible;
                cmdListaDiagnos.Visibility = Visibility.Visible;

                //Agregar a los listados elementos de las consultas ..

                HacerConexion();
                DataTable Categorias = obj2.VisualizarCategoriasCodigos();
                for (int i = 0; i <= Categorias.Rows.Count - 1; i++)
                {
                    listadoCategorias.Items.Add(Categorias.Rows[i][1].ToString());
                }
                CerrarConexion();

                //De agregar codigos y categorias
                Diagnosticos_Tabs.Visibility = Visibility.Visible;
                //borderInfobasica_Copy5.Visibility = Visibility.Visible;
                listadoCategorias.Visibility = Visibility.Visible;
                listadoSubcategorias.Visibility = Visibility.Visible;
                listadoCodigos.Visibility = Visibility.Visible;
                lblCategorias.Visibility = Visibility.Visible;
                lblSubCategorias.Visibility = Visibility.Visible;
                //lblCodigos.Visibility = Visibility.Visible;
                lblCodigoBuscar.Visibility = Visibility.Visible;
                txtCodigoBuscar.Visibility = Visibility.Visible;
                //cmdCodigoBuscar.Visibility = Visibility.Visible;
                cmdTodos.Visibility = Visibility.Visible;
                cmdNinguno.Visibility = Visibility.Visible;

                //Siempre visible
                borderInfobasica_Copy2.Visibility = Visibility.Visible;
                cmdProcesarAnalisis.Visibility = Visibility.Visible;
                cmdBorrar.Visibility = Visibility.Visible;
                cmdRango.Visibility = Visibility.Visible;
                ListaCodigos.Visibility = Visibility.Visible;
            }
        }


        void A_Diagnosticar(string[] datos_paciente=null)
        {
            try
            {

                //Si es un analisis nuevo
                if (datos_paciente != null)
                {
                    lblPacienteAnalisis_P1.Content = datos_paciente[1];
                    lblNombre_Anal1.Content = datos_paciente[0];
                    string date;
                    date = DateTime.Now.ToString();

                    //Fecha actual para el analisis
                    lblFechaAnalisis3.Content = date;

                    Ocultar_Diag();
                    Checar_SiYaFueAnalizado("");

                }
                else
                {
                    //Si se selecciona uno en vez de crear el analisis...

                    //Agarra
                    DataRowView paciente_seleccionado_reciente = (DataRowView)ListaPacientes_Recientes1.SelectedItem;
                    lblPacienteAnalisis_P1.Content = paciente_seleccionado_reciente[0].ToString();
                    lblNombre_Anal1.Content = paciente_seleccionado_reciente[2].ToString();
                    lblFechaAnalisis3.Content = paciente_seleccionado_reciente[1].ToString();

                    Console.WriteLine(paciente_seleccionado_reciente[0].ToString());

                    //Revisar si ya fue analizado
                    HacerConexion();
                    object validacion_analisis = obj2.Validar_Analisis_Analizado(paciente_seleccionado_reciente[0].ToString());
                    CerrarConexion();

                    // MessageBox.Show(validacion_analisis.ToString());
                    Ocultar_Diag();

                    Checar_SiYaFueAnalizado(validacion_analisis.ToString());

                }

            }
            catch(Exception ) {
                MessageBox.Show(obtenerRecurso("messageError70"),"Error",MessageBoxButton.OK,MessageBoxImage.Error);
            }

        }

        //This function is used to  display  recent patient analyses 
        private void ListaPacientes_Recientes1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
                A_Diagnosticar(); 
        }

        private void cmdEliminar_Copy1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRowView paciente_seleccionado_reciente = (DataRowView)ListaPacientes_Recientes1.SelectedItem;

                //Abrir conexion
                HacerConexion();
                //Eliminar analisis elegido
                obj2.EliminarAnalisisPorNombre(paciente_seleccionado_reciente[0].ToString());
                CerrarConexion();
                CargarRegistrosPacientesRecientes();
                CargarListadoCompletoPacientes();
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(obtenerRecurso("messageError18"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void comboOtrosAnal_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboOtrosAnal.Items.Count != 0)
            {
                //Revisar si ya fue reanalizado
                //Nuevos registros sin el ultimo espacio en blanco
                DataTable registro_validar = obj2.Validar_Reanalisis_Hecho(comboOtrosAnal.SelectedItem.ToString());

                //Si no esta como el nuevo registro usar el espacio en blanco..
                if (registro_validar.Rows.Count == 0)
                {
                    registro_validar = obj2.Validar_Reanalisis_Hecho(comboOtrosAnal.SelectedItem.ToString() + " ");
                }

                //Revisar si fue reanalizado para desactivar el boton
                //  for (int q = 0; q <= registro_validar.Rows.Count - 1; q++)
                //   {
                if (registro_validar.Rows[0][0].ToString() == "" || registro_validar.Rows[0][0].ToString() == "0")
                {
                    cmdReanalizarr.IsEnabled = true;
                }
                else
                {
                    cmdReanalizarr.IsEnabled = false;
                }
            }

        }

        private void cmdReanalizarr_Click(object sender, RoutedEventArgs e)
        {
            HacerConexion();

            //Revisar si el nombre del analisis ya esta en uso ...
            object id_reanalisis2 = obj2.Obtener_Id_Analisis(comboOtrosAnal.SelectedItem.ToString()+ " - Reanalysis");
            try
            {
                if (id_reanalisis2 == null)
                {
                    object id_padre = obj2.Obtener_Id_Analisis(comboOtrosAnal.SelectedItem.ToString());

                    // MessageBox.Show(id_padre.ToString());

                    // if (comboOtrosAnal.SelectedItem.ToString()!=null)
                    // {
                    string[] pac = new string[2] { listadoPacientes.SelectedItem.ToString(), comboOtrosAnal.SelectedItem.ToString() + " - Reanalysis" };

                    //Registrar analisis con el nombre elegido en bd
                    // HacerConexion();
                    DataTable pacientes = obj2.Buscar_IdPaciente_Nombre(listadoPacientes.SelectedItem.ToString());

                    obj2.RegistrarReAnalisisPaciente_Diag(pacientes.Rows[0][0].ToString(), comboOtrosAnal.SelectedItem.ToString() + " - Reanalysis", DateTime.Now, 4, 0, 1, DateTime.Now, id_padre.ToString());
                    // CerrarConexion();


                    DataTable tabla_codigosanalisis = obj2.Obtener_CodigosAnalisis(id_padre.ToString());

                    //Ciclo con los codigos de analisis
                    for (int p = 0; p <= tabla_codigosanalisis.Rows.Count - 1; p++)
                    {  // 3
                       // idscodigos.Add(tabla_codigosanalisis.Rows[p][2].ToString()); //Guarda idcodigo del analisis
                       //codigo,nombrecodigo,nivel,nivelsugerido,valor,vinicial,vfinal,decimales,tipo
                        ListaCodigos.Items.Add(new nuevoCodigo { rates = tabla_codigosanalisis.Rows[p][3].ToString(), nombre = tabla_codigosanalisis.Rows[p][4].ToString(), niveles = "-", nsugerido = "-", ftester = Convert.ToInt32(0) });

                    }

                    //  obj2.Eliminar_Codigos(id_padre.ToString());

                    //Cargar informacion biologica
                    DataTable info_biologica = obj2.Obtener_InfoBiologica(comboOtrosAnal.SelectedItem.ToString());

                    if (info_biologica.Rows[0][0].ToString() == "")
                    {
                        txtEstatura.Text = "0.0";
                    }
                    else
                    {
                        txtEstatura.Text = info_biologica.Rows[0][0].ToString();
                    }

                    if (info_biologica.Rows[0][1].ToString() == "")
                    {
                        txtPresionSist.Text = "0.0";
                    }
                    else
                    {
                        txtPresionSist.Text = info_biologica.Rows[0][1].ToString();
                    }

                    if (info_biologica.Rows[0][2].ToString() == "")
                    {
                        txtIMC.Text = "0.0";
                    }
                    else
                    {
                        txtIMC.Text = info_biologica.Rows[0][2].ToString();

                    }

                    if (info_biologica.Rows[0][3].ToString() == "")
                    {
                        txtFR.Text = "0.0";
                    }
                    else
                    {
                        txtFR.Text = info_biologica.Rows[0][3].ToString();
                    }

                    if (info_biologica.Rows[0][4].ToString() == "")
                    {
                        txtTA.Text = "0.0";
                    }
                    else
                    {
                        txtTA.Text = info_biologica.Rows[0][4].ToString();

                    }

                    if (info_biologica.Rows[0][5].ToString() == "")
                    {
                        txtPeso.Text = "0.0";
                    }
                    else
                    {
                        txtPeso.Text = info_biologica.Rows[0][5].ToString();
                    }

                    if (info_biologica.Rows[0][6].ToString() == "")
                    {
                        txtPresionDistolica.Text = "0.0";
                    }
                    else
                    {
                        txtPresionDistolica.Text = info_biologica.Rows[0][6].ToString();

                    }

                    if (info_biologica.Rows[0][7].ToString() == "")
                    {
                        txtFC.Text = "0.0";
                    }
                    else
                    {
                        txtFC.Text = info_biologica.Rows[0][7].ToString();
                    }

                    if (info_biologica.Rows[0][8].ToString() == "")
                    {
                        txtTemp.Text = "0.0";
                    }
                    else
                    {
                        txtTemp.Text = info_biologica.Rows[0][8].ToString();
                    }


                    //Cargar padecimiento
                    DataTable info_padecimiento = obj2.Obtener_InfoPadecimiento(comboOtrosAnal.SelectedItem.ToString());

                    if (info_padecimiento.Rows[0][0].ToString() == "")
                    {
                        txtPadecimiento.Text = "";
                    }
                    else
                    {
                        txtPadecimiento.Text = info_padecimiento.Rows[0][0].ToString();
                    }

                    if (info_padecimiento.Rows[0][1].ToString() == "")
                    {
                        txtInterrogatorio.Text = "";
                    }
                    else
                    {
                        txtInterrogatorio.Text = info_padecimiento.Rows[0][1].ToString();
                    }

                    lblContCodigos.Content = ListaCodigos.Items.Count;



                    cmdAnalisisPaciente_Copy.Clear(); //Limpia la casilla
                    A_Diagnosticar(pac);
                    cmdNuevoAnalisis1.IsEnabled = true; //Activa el boton de nuevo analisis o reanalisis
                    cmdReanalizarr.IsEnabled = true;
                }
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(obtenerRecurso("messageError17"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            /*  }
              else
              {
                  MessageBox.Show("Introduzca otro nombre para el reanálisis - " + comboOtrosAnal.SelectedItem.ToString() + " ya se encuentra en uso", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                  comboOtrosAnal.Focus();
              }*/
            CerrarConexion();

        }

        private void cmdAnalisisReanalisisCerrar_Click(object sender, RoutedEventArgs e)
        {
            //Parte del reanalisis
            listadoPacientes.Visibility = Visibility.Hidden;
            BusquedaReanalisis.Visibility = Visibility.Hidden;
            lblPacienteAnalisis2.Visibility = Visibility.Hidden;
            cmdAnalisisPaciente.Visibility = Visibility.Hidden;
            cmdAnalisisPaciente_Copy.Visibility = Visibility.Hidden;
            cmdAnalizarr.Visibility = Visibility.Hidden;
            cmdReanalizarr.Visibility = Visibility.Hidden;
            lblPacienteAnalisis_Copy.Visibility = Visibility.Hidden;
            comboOtrosAnal.Visibility = Visibility.Hidden;
            lblPacienteAnalisis.Visibility = Visibility.Hidden;
            cmdNuevoAnalisis1.IsEnabled = true;
            cmdAnalisisReanalisisCerrar.Visibility = Visibility.Hidden;
            lblPacienteAnalisis_Copy.Content = "Otros análisis";
        }

        private void Pacientes_GotFocus(object sender, RoutedEventArgs e)
        {
            //Validar_RegistroVersion();

        }

        private void cmdRegresar_Click(object sender, RoutedEventArgs e)
        {
            Limpiar_Campos();
            image1.Source = null;
            PacienteGroup.Visibility = Visibility.Visible;
            PacienteGroup_Copy.Visibility = Visibility.Hidden;

        }

        void Cerrar_Broadcast(object sender, RoutedEventArgs e)
        {
            Ocultar_TratamientoDirecto();
            Mostrar_TratamientoDiag();
        }

        private void cmdNuevoTratamiento_Click(object sender, RoutedEventArgs e)
        {
            Ocultar_TratamientoDiag();
            Mostrar_TratamientoDirecto();
        }

        void Ocultar_TratamientoDiag()
        {
            lblDiagActivo.Visibility = Visibility.Hidden;
            cmdBorrarDiagactivo.Visibility = Visibility.Hidden;
            //cmdBorrarDiagActSec.Visibility = Visibility.Hidden;
            ListadoDiagActivos.Visibility = Visibility.Hidden;
            lblDiagEnEspera.Visibility = Visibility.Hidden;
            //cmdBorrarDiagactivo_Copy.Visibility = Visibility.Hidden;
            // cmdBorrarDiagActSec_Copy.Visibility = Visibility.Hidden;
            ListadoDiagNoActiv.Visibility = Visibility.Hidden;
          //  tratamientoencola.Visibility = Visibility.Hidden;
            lblNoEnEspera.Visibility = Visibility.Hidden;
        }

        void Mostrar_TratamientoDiag()
        {
            lblDiagActivo.Visibility = Visibility.Visible;
            cmdBorrarDiagactivo.Visibility = Visibility.Visible;
            //  cmdBorrarDiagActSec.Visibility = Visibility.Visible;
            ListadoDiagActivos.Visibility = Visibility.Visible;
            lblDiagEnEspera.Visibility = Visibility.Visible;
            // cmdBorrarDiagactivo_Copy.Visibility = Visibility.Visible;
            //   cmdBorrarDiagActSec_Copy.Visibility = Visibility.Visible;
             ListadoDiagNoActiv.Visibility = Visibility.Visible;
            lblDiagEnEspera.Visibility = Visibility.Visible;
            //tratamientoencola.Visibility = Visibility.Visible;
        }

        void Ocultar_ListGenerico()
        {
            listgenerico.Visibility = Visibility.Hidden;
        }

        void Mostrar_ListGenerico()
        {
            listgenerico.Visibility = Visibility.Visible;
        }

        void Mostrar_ListGenericoParaCodigos()
        {
            listCategoriasTrat.Visibility = Visibility.Visible;
            listSubCategoriasTrat.Visibility = Visibility.Visible;
            listCodigosTrat.Visibility = Visibility.Visible;
        }

        void Cerrar_ListGenericoParaCodigos()
        {
            listCategoriasTrat.Visibility = Visibility.Hidden;
            listSubCategoriasTrat.Visibility = Visibility.Hidden;
            listCodigosTrat.Visibility = Visibility.Hidden;
        }

        void MostrarRemedy()
        {
            Remedy1.Visibility = Visibility.Visible;
            Remedy2.Visibility = Visibility.Visible;
        }

        void CerrarRemedy()
        {
            Remedy1.Visibility = Visibility.Hidden;
            Remedy2.Visibility = Visibility.Hidden;
        }

        List<string> Rates_Codigos = new List<string>();
        private void comboTipoTratamiento_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            listgenerico.Items.Clear();

            if (comboPacientesTratamiento.SelectedIndex == -1)
            {
                MessageBox.Show(obtenerRecurso("messageError16"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                comboTipoTratamiento.SelectedIndex = -1;
            }
            else
            {
                //comboTipoTratamiento.Items.Clear();
                try
                {
                    ComboBoxItem selectedItem = (ComboBoxItem)comboTipoTratamiento.SelectedItem;
                    //Si cambia la seleccion... hacer
                    if (selectedItem != null)
                    {
                        if (selectedItem.Content.ToString() == obtenerRecurso("MenuAnalysis"))
                        {
                            CerrarRemedy();
                            Cerrar_ListGenericoParaCodigos();
                            Mostrar_ListGenerico();
                            //MessageBox.Show(comboPacientesTratamiento.SelectedItem.ToString());

                            //Analisis en base al paciente elegido
                            DataTable AnalisisPaciente_Seleccionado = obj2.Obtener_Analisis_Pacientes_Recientes_PorNombrePaciente(comboPacientesTratamiento.SelectedItem.ToString());

                            //Llenar el combobox con analisis relacionados
                            for (int i = 0; i <= AnalisisPaciente_Seleccionado.Rows.Count - 1; i++)
                            {
                                //Agregar solo nombre del analisis
                                listgenerico.Items.Add(AnalisisPaciente_Seleccionado.Rows[i][0].ToString());
                            }

                            Trata.Header = obtenerRecurso("MenuAnalysis");

                        }

                        //Si cambia la seleccion... hacer
                        if (selectedItem.Content.ToString() == obtenerRecurso("labelRemedy"))
                        {
                            listgenerico.Visibility = Visibility.Hidden;
                            Cerrar_ListGenericoParaCodigos();
                            MostrarRemedy();
                            //MessageBox.Show(comboPacientesTratamiento.SelectedItem.ToString());

                            //Analisis en base al paciente elegido
                            // DataTable AnalisisPaciente_Seleccionado = obj2.Obtener_Analisis_Pacientes_Recientes_PorNombrePaciente(comboPacientesTratamiento.SelectedItem.ToString());
                            DataTable ListaRemedios = obj2.VisualizarRemedios();

                            //Llenar el combobox con analisis relacionados
                            for (int i = 0; i <= ListaRemedios.Rows.Count - 1; i++)
                            {
                                //Agregar solo nombre del analisis
                                Remedy1.Items.Add(ListaRemedios.Rows[i][1].ToString());
                            }

                            Trata.Header = obtenerRecurso("labelRemedy");

                        }

                        //Por Códigos Individuales

                        //Si cambia la seleccion... hacer
                        if (selectedItem.Content.ToString() == obtenerRecurso("labelRate"))
                        {
                            CerrarRemedy();
                            Ocultar_ListGenerico();
                            Mostrar_ListGenericoParaCodigos();
                            HacerConexion();

                            //CARGA CATEGORIAS
                            DataTable Categorias = obj2.VisualizarCategoriasCodigos2();

                            if (listCategoriasTrat.Items.Count == 0)
                            {
                                //Cargar categorias
                                for (int i = 0; i <= Categorias.Rows.Count - 1; i++)
                                {
                                    if (listCategoriasTrat.Items.Contains(Categorias.Rows[i][1].ToString()) == false)
                                    {
                                        listCategoriasTrat.Items.Add(Categorias.Rows[i][1].ToString());
                                    }
                                }
                            }

                            CerrarConexion();

                            Trata.Header = obtenerRecurso("labelRate");

                        }

                    }
                }
                catch (System.NullReferenceException)
                {

                }
            }
        }

        List<string> Categorias_Codigos3 = new List<string>();
        private void listCategoriasTrat_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            listSubCategoriasTrat.Items.Clear(); //Limpia antes de cada uso
            listCodigosTrat.Items.Clear();
            Categorias_Codigos3.Clear();
            try
            {

                HacerConexion();
                DataTable CategoriasCodigos;

                //Buscar id_categoria para encontrar las subcategorias
                object id_categoria = obj2.BuscarCategoriasCodigos(listCategoriasTrat.SelectedItem.ToString());
                // MessageBox.Show(id_categoria.ToString());
                id_categoria_padre = id_categoria.ToString(); //Guarda la categoria padre para el nuevo codigo
                DataTable SubCategorias = obj2.VisualizarSubCategoriasCodigos(id_categoria.ToString());

                //Si no tienen subcategoria mostrarlos como codigos ya
                if (SubCategorias.Rows.Count == 0)
                {
                    CategoriasCodigos = obj2.VisualizarSubCategoriasCodigos2(id_categoria.ToString());

                    for (int y = 0; y <= CategoriasCodigos.Rows.Count - 1; y++)
                    {
                        if (CategoriasCodigos.Rows[y][1].ToString() != "")
                        {
                            //listadoCodigos.Items.Add(new CheckBox { Content = SubCategorias.Rows[y][1].ToString() });
                            listCodigosTrat.Items.Add(CategoriasCodigos.Rows[y][1].ToString()); //+ " , " + CategoriasCodigos.Rows[y][2].ToString());

                            Categorias_Codigos3.Add(CategoriasCodigos.Rows[y][2].ToString()); //Guarda el codigo
                        }

                    }
                    //lblSubcategoriasCont.Content = listadoSubcategorias_Copy.Items.Count + " Sub-Categorías";
                    //lblCodigosCont.Content = listadoCodigos_Copy.Items.Count + " Códigos";

                }
                else
                {
                    //Agrega las categorias
                    for (int y = 0; y <= SubCategorias.Rows.Count - 1; y++)
                    {
                        if (SubCategorias.Rows[y][0].ToString() != "")
                        {
                            listSubCategoriasTrat.Items.Add(SubCategorias.Rows[y][0].ToString());
                        }
                    }

                    // lblSubcategoriasCont.Content = listadoSubcategorias_Copy.Items.Count + " Sub-Categorías";

                    // IMPORTANTE: categorias con subcategorias y codigos en su categoria

                    //Tambien puede haber categorias con subcategorias y codigos en su categoria...
                    CategoriasCodigos = obj2.VisualizarSubCategoriasCodigos2(id_categoria.ToString());

                    for (int y = 0; y <= CategoriasCodigos.Rows.Count - 1; y++)
                    {
                        if (CategoriasCodigos.Rows[y][1].ToString() != "")
                        {
                            //listadoCodigos.Items.Add(new CheckBox { Content = SubCategorias.Rows[y][1].ToString() });
                            //listCodigosTrat.Items.Add(CategoriasCodigos.Rows[y][1].ToString());
                            listCodigosTrat.Items.Add(CategoriasCodigos.Rows[y][1].ToString()); //+ " , " + CategoriasCodigos.Rows[y][2].ToString());
                            Categorias_Codigos3.Add(CategoriasCodigos.Rows[y][2].ToString()); //Guarda el codigo
                        }
                    }

                    //lblCodigosCont.Content = listadoCodigos_Copy.Items.Count + " Códigos";

                }
                CerrarConexion();
            }
            catch (NullReferenceException)
            {
                // MessageBox.Show("Error seleccione una categoría antes de continuar!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public class nuevoTratamiento
        {
            public string paciente { get; set; }
            public string tratamiento { get; set; }
            public string inicio { get; set; }
            public string duracion { get; set; }
            public string tfaltante { get; set; }

        }


        int[] TiempoBase = new int[3] { 60, 3600, 86400 }; //Tiempo base (Minutos=60 segundos, Hora=3600 segundos, Dia=86400 segundos)

        DateTime fecha_selec;

        void Iniciar_Tratamiento(object sender, RoutedEventArgs e)
        {
            Remedy1.Visibility = Visibility.Hidden;
            Remedy2.Visibility = Visibility.Hidden;
            //(((ComboBoxItem)comboTipoTratamiento.SelectedItem).Content.ToString() == "Usando un Análisis")
            try
            {
                HacerConexion();

                //Comprueba si se introdujo un nombre para el tratamiento directo
                if (txtNombreTratamiento.Text == string.Empty)
                {
                    MessageBox.Show(obtenerRecurso("messageError15"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    //Sino se ha seleccionado ningun elemento mostrar msj
                    if (comboTipoTratamiento.SelectedIndex == -1)
                    {
                        MessageBox.Show(obtenerRecurso("messageError14"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        try
                        {
                            //Datos Generales...
                            object id_paciente = obj2.Obtener_IdPaciente_NombreCompleto(comboPacientesTratamiento.SelectedItem.ToString());
                            string nombre_paciente = comboPacientesTratamiento.SelectedItem.ToString();
                            string nombre_tratamiento = txtNombreTratamiento.Text;

                            //Comprobar nombre del tratamiento
                            object existe_nombre = obj2.Comprobar_NombreTratamiento(nombre_tratamiento);

                            //Condiciona el nombre a que no exista para continuar...
                            if (existe_nombre.ToString() == "0")
                            {
                                ComboBoxItem selectedItem = (ComboBoxItem)comboTipoProg.SelectedItem;
                                //Parte de la programacion del tratamiento
                                if (selectedItem.Content.ToString() == "Simple")
                                {
                                    if (txtHoras.Text == "" && txtMinutos.Text == "")
                                    {
                                        MessageBox.Show(obtenerRecurso("messageError13"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                    else
                                    {
                                        //Datos - idt, idpadre, idpaciente, nombrepaciente, nombre, duracion, tiempoemitido, fechainicio, fechac, estado
                                        int Horas = Int32.Parse(txtHoras.Text);
                                        int Minutos = Int32.Parse(txtMinutos.Text);
                                        int Horas_segundos = Horas * 3600;
                                        int Minutos_segundos = Minutos * 60;

                                        //Duracion en segundos
                                        int duracion = Horas_segundos + Minutos_segundos;

                                        //string fecha_inicio = dateProg.Text;
                                        fecha_selec = Convert.ToDateTime(dateProg.Text); //Convertimos

                                        DateTime hora_actual = DateTime.Now;
                                        //hora_actual.Hour;

                                        DateTime sfecha_hora;
                                        DateTime sfecha_inicio;
                                        TimeSpan ts = new TimeSpan(hora_actual.Hour + Horas, hora_actual.Minute + Minutos, hora_actual.Second);
                                        TimeSpan ts2 = new TimeSpan(hora_actual.Hour, hora_actual.Minute, hora_actual.Second);
                                        sfecha_inicio = fecha_selec + ts2;
                                        sfecha_hora = fecha_selec + ts;

                                        //Registrar nuevo tratamiento sin padre y con estado 1
                                        obj2.Registrar_TratamientoNuevoSencillo("", id_paciente.ToString(), nombre_paciente, nombre_tratamiento, duracion, 0, sfecha_hora, sfecha_inicio, 1);

                                        //Obtener Id del tratamiento a distancia para guardar su contenido acorde al ID
                                        object id_tratamiento = obj2.Obtener_IDTratamiento(id_paciente.ToString(), nombre_paciente, nombre_tratamiento);
                                        //MessageBox.Show(id_tratamiento.ToString());

                                        //Para remedios
                                        for (int q = 0; q <= lista_remedios.Count - 1; q++)
                                        {
                                            //Registrar contenido del tratamiento sencillo
                                            obj2.Registrar_ContenidoTratamiento(id_tratamiento.ToString(), lista_remedios[q], "R");
                                        }

                                        //Para analisis
                                        for (int q = 0; q <= lista_analisis.Count - 1; q++)
                                        {
                                            //Registrar contenido del tratamiento sencillo
                                            obj2.Registrar_ContenidoTratamiento(id_tratamiento.ToString(), lista_analisis[q], "A");
                                        }

                                        //Para codigos individuales
                                        for (int q = 0; q <= lista_codigos.Count - 1; q++)
                                        {
                                            //Registrar contenido del tratamiento sencillo
                                            obj2.Registrar_ContenidoTratamiento(id_tratamiento.ToString(), lista_codigos[q], "C");
                                        }

                                        //Formato reloj de la duracion..
                                        string duracion_formatoreloj = CalcularTiempo_FormatoReloj(Int32.Parse(duracion.ToString()));

                                        //Agregar al listado activo
                                        ListadoDiagActivos.Items.Add(new nuevoTratamiento { paciente = nombre_paciente, tratamiento = nombre_tratamiento, inicio = fecha_selec.ToString(), duracion = duracion_formatoreloj, tfaltante = "0" });

                                        Cargar_Tratamientos_Pendientes_Y_Activos();
                                        //Parte crucial de ocultar
                                        Mostrar_TratamientoDiag();
                                        Ocultar_TratamientoDirecto();
                                        //Empezar_Temporizador();

                                    }
                                }
                                else if (selectedItem.Content.ToString() == "Periodic")
                                {
                                    List<DateTime> fechas = new List<DateTime>();
                                    List<DateTime> fecha_origen = new List<DateTime>();
                                    //Horas y Minutos
                                    int Horas = Int32.Parse(txtHoras.Text);
                                    int Minutos = Int32.Parse(txtMinutos.Text);
                                    int Horas_segundos = Horas * 3600;
                                    int Minutos_segundos = Minutos * 60;

                                    //Agarra hora seleccionada
                                    fecha_selec = Convert.ToDateTime(dateProg.Text); //Convertimos

                                    DateTime hora_actual = DateTime.Now;
                                    //hora_actual.Hour;

                                    DateTime sfecha_hora;
                                    DateTime sfecha_inicio;
                                    TimeSpan ts = new TimeSpan(hora_actual.Hour + Horas, hora_actual.Minute + Minutos, hora_actual.Second);
                                    TimeSpan ts2 = new TimeSpan(hora_actual.Hour, hora_actual.Minute, hora_actual.Second);
                                    sfecha_inicio = fecha_selec + ts2;
                                    sfecha_hora = fecha_selec + ts;

                                    //Duracion en segundos - Tiempo
                                    int duracion = Horas_segundos + Minutos_segundos;
                                    int duracion_cada = 0;

                                    //Agrega a un arreglo para la pausa
                                    Fechas_Diag_Activos.Add(fecha_selec);
                                    Banderas_Fechas_Activos.Add(true);

                                    //Duracion en segundos - Cada
                                    if (((ComboBoxItem)tiempo1.SelectedItem).Content.ToString() == "Minute(s)")
                                    {
                                        duracion_cada = Int32.Parse(txtcantidad1.Text) * 60;
                                    }

                                    if (((ComboBoxItem)tiempo1.SelectedItem).Content.ToString() == "Hour(s)")
                                    {
                                        duracion_cada = Int32.Parse(txtcantidad1.Text) * 3600;
                                    }

                                    if (((ComboBoxItem)tiempo1.SelectedItem).Content.ToString() == "Day(s)")
                                    {
                                        duracion_cada = Int32.Parse(txtcantidad1.Text) * 86400;
                                    }

                                    //Duracion en segundos - Durante
                                    int duracion_durante;

                                    duracion_durante = Int32.Parse(txtcantidad2.Text) * 86400;

                                    if (txtcantidad1.Text != "0" && txtcantidad2.Text != "0")
                                    {
                                        //if (duracion_durante > duracion_cada)
                                        // {

                                        //Cada para adelantar los segundos y el tiempo..
                                        int cada = duracion_cada;

                                        //Calcula el numero de veces del periodo
                                        int no_veces = duracion_durante / duracion_cada;

                                        //Fecha original
                                        fechas.Add(sfecha_hora);
                                        fecha_origen.Add(sfecha_inicio);

                                        //Creamos la fecha padre
                                        obj2.Registrar_TratamientoNuevoSencillo("", id_paciente.ToString(), nombre_paciente, nombre_tratamiento, duracion, 0, sfecha_hora, sfecha_inicio, 1);

                                        string duracion_formatoreloj = CalcularTiempo_FormatoReloj(Int32.Parse(duracion.ToString()));

                                        //Agregar al listado activo el padre y condiciona los hijos a espera
                                        ListadoDiagActivos.Items.Add(new nuevoTratamiento { paciente = nombre_paciente, tratamiento = nombre_tratamiento, inicio = sfecha_hora.ToString(), duracion = duracion_formatoreloj, tfaltante = "0" });

                                        int acum = cada;  //Acumulador para el cada

                                        //Obtener id_padre del id_tratamiento
                                        object id_padre = obj2.Obtener_IdTratamiento(id_paciente.ToString(), nombre_paciente, nombre_tratamiento);

                                        //Calcula las fechas del periodo en base al numero de veces
                                        for (int s = 1; s <= no_veces - 1; s++)
                                        {

                                            ts = new TimeSpan(hora_actual.Hour, hora_actual.Minute, hora_actual.Second + acum);
                                            ts2 = new TimeSpan(hora_actual.Hour, hora_actual.Minute, hora_actual.Second);
                                            sfecha_inicio = fecha_selec + ts2;
                                            sfecha_hora = fecha_selec + ts;

                                            fechas.Add(sfecha_hora);
                                            fecha_origen.Add(sfecha_inicio);
                                            acum = acum + cada;
                                        }

                                        string padre = id_padre.ToString();

                                        //Crea la secuencia de registros en base a las fechas...
                                        for (int s = 1; s <= fechas.Count - 1; s++)
                                        {
                                            //MessageBox.Show(padre);
                                            obj2.Registrar_TratamientoNuevoSencillo(padre, id_paciente.ToString(), nombre_paciente, nombre_tratamiento, duracion, 0, fechas[s], fecha_origen[s], 0);
                                            //MessageBox.Show(fechas[s].ToString());

                                            string duracion_formatoreloj_in = CalcularTiempo_FormatoReloj(Int32.Parse(duracion.ToString()));

                                            //Agregar al listado activo
                                             ListadoDiagNoActiv.Items.Add(new nuevoTratamiento { paciente = nombre_paciente, tratamiento = nombre_tratamiento, inicio = fechas[s].ToString(), duracion = duracion_formatoreloj_in, tfaltante = "0" });

                                        }

                                        //Registro de los codigos del tratamiento periodico.....

                                        //Para remedios
                                        for (int q = 0; q <= lista_remedios.Count - 1; q++)
                                        {
                                            //Registrar contenido del tratamiento sencillo
                                            obj2.Registrar_ContenidoTratamiento(padre, lista_remedios[q], "R");
                                        }

                                        //Para analisis
                                        for (int q = 0; q <= lista_analisis.Count - 1; q++)
                                        {
                                            //Registrar contenido del tratamiento sencillo
                                            obj2.Registrar_ContenidoTratamiento(padre, lista_analisis[q], "A");
                                        }

                                        //Para codigos individuales
                                        for (int q = 0; q <= lista_codigos.Count - 1; q++)
                                        {
                                            //Registrar contenido del tratamiento sencillo
                                            obj2.Registrar_ContenidoTratamiento(padre, lista_codigos[q], "C");
                                        }

                                        Cargar_Tratamientos_Pendientes_Y_Activos();

                                        //Parte crucial de ocultar
                                        Mostrar_TratamientoDiag();
                                        Ocultar_TratamientoDirecto();

                                        // }
                                        // else
                                        //  {
                                        //     MessageBox.Show("El valor de 'Durante' debe ser mayor que el 'Tiempo' en la programación del tratamiento", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                        //  }

                                    }
                                    else
                                    {
                                        MessageBox.Show(obtenerRecurso("messageError12"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                        tiempo1.Focus();
                                    }
                                }
                                else
                                {
                                    MessageBox.Show(obtenerRecurso("messageError11"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                            else
                            {
                                MessageBox.Show("The treatment's name '" + nombre_tratamiento + "' is already in used, use another", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        catch (NullReferenceException ex)
                        {
                            MessageBox.Show(obtenerRecurso("messageError11"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }

                }
            }
            catch (FormatException)
            {
                MessageBox.Show(obtenerRecurso("messageError10"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                CerrarConexion();
            }

        }

        private void comboTipoProg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (((ComboBoxItem)comboTipoProg.SelectedItem).Content.ToString() == "Periodic")
            {
                lblNombreTratamiento_Copy7.Visibility = Visibility.Visible;
                txtcantidad1.Visibility = Visibility.Visible;
                tiempo1.Visibility = Visibility.Visible;

                lblNombreTratamiento_Copy8.Visibility = Visibility.Visible;
                txtcantidad2.Visibility = Visibility.Visible;
                tiempo2.Visibility = Visibility.Visible;

            }
            else
            {
                lblNombreTratamiento_Copy7.Visibility = Visibility.Hidden;
                txtcantidad1.Visibility = Visibility.Hidden;
                tiempo1.Visibility = Visibility.Hidden;

                lblNombreTratamiento_Copy8.Visibility = Visibility.Hidden;
                txtcantidad2.Visibility = Visibility.Hidden;
                tiempo2.Visibility = Visibility.Hidden;
            }
        }

        //Variables para el tratamiento directo en elementos agregados
        List<string> lista_remedios = new List<string>();
        List<string> lista_analisis = new List<string>();
        List<string> lista_codigos = new List<string>();

        private void listgenerico_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (listelemagregados.Items.Count != 0 || listelemagregados.Items.Count == 0)
                {
                    if (listgenerico.SelectedItem != null)
                    {
                        listelemagregados.Items.Add(listgenerico.SelectedItem.ToString());
                        //Si son remedios 
                        if (((ComboBoxItem)comboTipoTratamiento.SelectedItem).Content.ToString() == "Remedy")
                        {
                            //Almacenar en lista
                            lista_remedios.Add(listgenerico.SelectedItem.ToString());
                        }

                        //Si son analisis
                        if (((ComboBoxItem)comboTipoTratamiento.SelectedItem).Content.ToString() == "Analysis")
                        {
                            // Almacenar en lista
                            lista_analisis.Add(listgenerico.SelectedItem.ToString());
                        }
                    }

                   
                }
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(obtenerRecurso("messageError8"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void txtNombreTratamiento_Copy_SelectionChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                if (((ComboBoxItem)comboTipoTratamiento.SelectedItem).Content.ToString() == "Analysis")
                {
                    if (txtNombreTratamiento_Copy.Text != "")
                    {
                        listgenerico.Items.Clear();
                        HacerConexion();
                        //Llama y obtiene posibles matches
                        DataTable PacientesAnalisisBuscado = obj2.Obtener_Analisis_Pacientes_Recientes_PorNombrePaciente2(comboPacientesTratamiento.SelectedItem.ToString(), txtNombreTratamiento_Copy.Text);
                        for (int j = 0; j <= PacientesAnalisisBuscado.Rows.Count - 1; j++)
                        {
                            listgenerico.Items.Add(PacientesAnalisisBuscado.Rows[j][0].ToString());
                        }
                        CerrarConexion();
                    }
                    else
                    {
                        if (txtNombreTratamiento_Copy.Text == "")
                        {
                            listgenerico.Items.Clear();
                            HacerConexion();
                            DataTable AnalisisPaciente_Seleccionado = obj2.Obtener_Analisis_Pacientes_Recientes_PorNombrePaciente(comboPacientesTratamiento.SelectedItem.ToString());

                            //Llenar el combobox con analisis relacionados
                            for (int i = 0; i <= AnalisisPaciente_Seleccionado.Rows.Count - 1; i++)
                            {
                                //Agregar solo nombre del analisis
                                listgenerico.Items.Add(AnalisisPaciente_Seleccionado.Rows[i][0].ToString());
                            }
                            CerrarConexion();
                        }
                        else
                        {
                            MessageBox.Show(obtenerRecurso("messageError9"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            txtNombreTratamiento_Copy.Focus();
                        }
                    }
                }

                if (((ComboBoxItem)comboTipoTratamiento.SelectedItem).Content.ToString() == "Remedy")
                {
                    ClearDataLb(Remedy2);
                    Remedy2.SelectedIndex = -1;

                    if (txtNombreTratamiento_Copy.Text != "")
                    {
                        // Busqueda activada
                        busqueda = true;

                        Remedy2.Items.Clear();

                        HacerConexion();

                        DataTable Codigos = obj2.BuscarCodigoRem(txtNombreTratamiento_Copy.Text);

                        for (int y = 0; y < Codigos.Rows.Count; y++)
                        {
                            if (!string.IsNullOrEmpty(Codigos.Rows[y][0].ToString()))
                            {
                                string id = Codigos.Rows[y][0].ToString();
                                string nombre = Codigos.Rows[y][1].ToString();

                                // Crear un nuevo objeto ListBoxItem con la concatenación de id y nombre
                                ListBoxItem item = new ListBoxItem();
                                item.Content = id + ", " + nombre;

                                // Agregar el ListBoxItem a la ListBox
                                Remedy2.Items.Add(item);
                            }
                        }

                        lblCodigosCont.Content = Remedy2.Items.Count + " " + obtenerRecurso("labelRate");

                        CerrarConexion();
                    }
                }

                if (((ComboBoxItem)comboTipoTratamiento.SelectedItem).Content.ToString() == "Rate")
                {

                    listCategoriasTrat.SelectedIndex = -1;
                    listSubCategoriasTrat.SelectedIndex = -1;
                    listCodigosTrat.SelectedIndex = -1;

                    if (txtNombreTratamiento_Copy.Text != "")
                    {
                        //Busqueda on
                        //busqueda = true;

                        listCodigosTrat.Items.Clear();

                        HacerConexion();

                        DataTable Codigos = obj2.BuscarCodigo(txtNombreTratamiento_Copy.Text);

                        for (int y = 0; y <= Codigos.Rows.Count - 1; y++)
                        {
                            if (Codigos.Rows[y][0].ToString() != "")
                            {
                                //listadoCodigos.Items.Add(new CheckBox { Content = Codigos.Rows[y][1].ToString() });
                                listCodigosTrat.Items.Add(Codigos.Rows[y][0].ToString() + " , " + Codigos.Rows[y][1].ToString());
                            }
                        }
                        //lblCodigosCont.Content = listadoCodigos_Copy.Items.Count + " Códigos";

                        CerrarConexion();
                    }
                    else
                    {
                        listSubCategoriasTrat.Items.Clear();
                        listCodigosTrat.Items.Clear(); //limpiar la lista de codigos
                                                       //lblCodigosCont.Content = listadoCodigos.Items.Count + " Códigos";
                    }
                }
            }
            catch (Npgsql.PostgresException)
            {
               // MessageBox.Show("Entrada inválida!, utilice otras teclas", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void Remedy2_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (listelemagregados.Items.Count != 0 || listelemagregados.Items.Count == 0)
                {
                    if (Remedy2.SelectedItem != null)
                    {
                        string selectedItemText = ((ListBoxItem)Remedy2.SelectedItem).Content.ToString();
                        listelemagregados.Items.Add(selectedItemText);

                        //Si son remedios 
                        if (((ComboBoxItem)comboTipoTratamiento.SelectedItem).Content.ToString() == "Remedy")
                        {
                            //Almacenar en lista
                            lista_remedios.Add(selectedItemText);
                        }
                    }
                }
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(obtenerRecurso("messageError8"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void listSubCategoriasTrat_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            listCodigosTrat.Items.Clear();
            Categorias_Codigos3.Clear(); //Limpia los codigos guardados

            try
            {
                HacerConexion();
                object id_categoria = obj2.BuscarCategoriasCodigos(listCategoriasTrat.SelectedItem.ToString());

                object id_subcategoria = obj2.BuscarCategoriasCodigosSub(listSubCategoriasTrat.SelectedItem.ToString(), id_categoria.ToString());

                /// AGREGADO
                /// 
                //Buscar el sexo del paciente
                //Paciente
                string paciente_nombre = comboPacientesTratamiento.SelectedItem.ToString();
                DataTable paciente_sexo_tabla = obj2.VisualizarAnalisisPorGenero2(paciente_nombre);
                string sexo = "";

                for (int a = 0; a <= paciente_sexo_tabla.Rows.Count - 1; a++)
                {
                    //Si es igual el nombre obtener el sexo
                    // if (paciente_sexo_tabla.Rows[a][1].ToString() == lblPacienteAnalisis_P1.Content.ToString())
                    // {
                    //MessageBox.Show(paciente_sexo_tabla.Rows[a][0].ToString());
                    if (paciente_sexo_tabla.Rows[a][0].ToString() == "Masculino")
                    {
                        sexo = "M";
                    }

                    if (paciente_sexo_tabla.Rows[a][0].ToString() == "Femenino")
                    {
                        sexo = "F";
                    }

                    if (paciente_sexo_tabla.Rows[a][0].ToString() == "Animal")
                    {
                        sexo = "A";
                    }

                    if (paciente_sexo_tabla.Rows[a][0].ToString() == "Plantas y suelo")
                    {
                        sexo = "P";
                    }

                    //  }
                }

                // MessageBox.Show(id_subcategoria.ToString() +" - "+sexo);
                DataTable Codigos = obj2.VisualizarSubCategoriasCodigosListado(id_subcategoria.ToString(), sexo);

                //MessageBox.Show(Codigos.Rows.Count.ToString());
                //MessageBox.Show(id_subcategoria.ToString());

                if (Codigos.Rows.Count == 0) //Sino hay por genero pues utiliza el de todos...
                {
                    Codigos = obj2.VisualizarSubCategoriasCodigosListadoGenero_Todos(id_subcategoria.ToString(), sexo);
                }

                // else // De lo contrario si existen codigos por genero entonces.. que seria genero mas genero=T
                //{
                // }

                //AGREGADO HASTA AQUI

                for (int y = 0; y <= Codigos.Rows.Count - 1; y++)
                {
                    if (Codigos.Rows[y][1].ToString() != "")
                    {
                        //listadoCodigos.Items.Add(new CheckBox { Content = Codigos.Rows[y][1].ToString() });
                        listCodigosTrat.Items.Add(Codigos.Rows[y][1].ToString());
                        Categorias_Codigos3.Add(Codigos.Rows[y][2].ToString()); //Guarda el codigo
                    }
                }

                CerrarConexion();
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(obtenerRecurso("messageInf1"), obtenerRecurso("messageHeadInf"), MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void listCodigosTrat_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (listelemagregados.Items.Count == 0)
                {
                    listelemagregados.Items.Clear();
                    listelemagregados.Items.Add(listCodigosTrat.SelectedItem.ToString());
                    lista_codigos.Add(listCodigosTrat.SelectedItem.ToString());
                }
                else
                {
                    int index = listelemagregados.Items.IndexOf(listCodigosTrat.SelectedItem.ToString());

                    if (index == -1)
                    {
                        listelemagregados.Items.Add(listCodigosTrat.SelectedItem.ToString());

                        lista_codigos.Add(listCodigosTrat.SelectedItem.ToString());
                    }
                }
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(obtenerRecurso("messageWarning3"), obtenerRecurso("messageHeadWarning"), MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void cmdBorrarElemento_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Eliminar de las listas y buscar

                //Analisis
                for (int i = 0; i <= lista_analisis.Count - 1; i++)
                {
                    if (listelemagregados.SelectedItem.ToString() == lista_analisis[i])
                    {
                        lista_analisis.RemoveAt(i);
                    }

                }

                //Remedios
                for (int q = 0; q <= lista_remedios.Count - 1; q++)
                {
                    if (listelemagregados.SelectedItem.ToString() == lista_remedios[q])
                    {
                        lista_remedios.RemoveAt(q);
                    }

                }

                //Codigos individuales
                for (int k = 0; k <= lista_codigos.Count - 1; k++)
                {
                    if (listelemagregados.SelectedItem.ToString() == lista_codigos[k])
                    {
                        lista_codigos.RemoveAt(k);
                    }
                }

                //Eliminar de listbox
                listelemagregados.Items.Remove(listelemagregados.SelectedItem);


            }
            catch (NullReferenceException)
            {
                MessageBox.Show(obtenerRecurso("messageError8"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void tratamientos_GotFocus(object sender, RoutedEventArgs e)
        {
            //Cargar_Tratamientos_Pendientes_Y_Activos();
        }


        private void cmdBorrarDiagactivo_Click(object sender, RoutedEventArgs e)
        {
            //Borra un tratamiento activo (Solo padres)
            try
            {
                if (ListadoDiagActivos.Items.Count != 0)
                {
                    //Arreglo de nombres
                    List<string> nombres_tratamientos_antes = new List<string>();
                    List<string> nombres_tratamientos_desp = new List<string>();

                    //Extraemos elementos de la lista antes de borrar los seleccionados
                    IEnumerable diagnosticos_items = this.ListadoDiagActivos.Items;

                    foreach (nuevoTratamiento tratamiento in diagnosticos_items)
                    {
                        nombres_tratamientos_antes.Add(tratamiento.tratamiento);
                    }

                    //Los elimina del listview de listadosactivos
                    for (int i = 0; i <= ListadoDiagActivos.SelectedItems.Count - 1; i++)
                    {
                        ListadoDiagActivos.Items.Remove(ListadoDiagActivos.SelectedItems[i]);
                    }

                    //Extraemos elementos de la lista despues de borrar
                    diagnosticos_items = this.ListadoDiagActivos.Items;

                    foreach (nuevoTratamiento tratamiento in diagnosticos_items)
                    {
                        nombres_tratamientos_desp.Add(tratamiento.tratamiento);
                    }

                    HacerConexion();
                    //Compara y elimina elementos
                    for (int q = 0; q <= nombres_tratamientos_antes.Count - 1; q++)
                    {
                        if (!nombres_tratamientos_desp.Contains(nombres_tratamientos_antes[q]))
                        {
                            object NoVeces = obj2.Obtener_NoVeces_Tratamiento(nombres_tratamientos_antes[q]);
                            //Buscar si ese tratamiento tiene una secuencia definida
                            if (NoVeces.ToString() != "1")
                            {
                                //Preguntar
                                // var result = MessageBox.Show("El tratamiento '" + nombres_tratamientos_antes[q] + "' tiene una secuencia definida, aun asi desea borrarlo?", "Pregunta", MessageBoxButton.YesNo, MessageBoxImage.Question);

                                //Validacion
                                // if (result == MessageBoxResult.Yes)
                                // {
                                var result2 = MessageBox.Show(obtenerRecurso("messageQuestion1"), obtenerRecurso("messageHeadQ"), MessageBoxButton.YesNo, MessageBoxImage.Question);

                                if (result2 == MessageBoxResult.Yes)
                                {
                                    //Borrar padre e hijos

                                    //Hijos
                                    obj2.Eliminar_TratamientoPadre_Hijos(nombres_tratamientos_antes[q]);

                                    //Padre
                                    obj2.Eliminar_TratamientoPadre(nombres_tratamientos_antes[q]);
                                }
                                else
                                {
                                    //Borrar solo padre
                                    obj2.Eliminar_TratamientoPadre(nombres_tratamientos_antes[q]);
                                }

                                // }
                            }
                            else
                            {
                                //Elimina padre
                                obj2.Eliminar_TratamientoPadre(nombres_tratamientos_antes[q]);
                            }
                        }
                    }
                    CerrarConexion();

                }
                else
                {
                    obj.BroadcastOFF();
                }
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(obtenerRecurso("messageError7"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmdBorrarDiagactivo_Copy_Click(object sender, RoutedEventArgs e)
        {
            try
            {

            }
            catch (NullReferenceException)
            {
                MessageBox.Show(obtenerRecurso("messageError6"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ListadoDiagActivos_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //MessageBox.Show(codigos_tratamiento.Rows.Count.ToString());

            //Limpiamos las listas primero
            detalleAnalisis.Items.Clear();
            detalleCodigosIndiv.Items.Clear();
            detalleRemedios.Items.Clear();
            detalleAsociados.Items.Clear();

            if (ListadoDiagActivos.SelectedItems.Count != 0)
            {
                //MessageBox.Show("HOLA");
                string nombrepaciente = "";
                string nombre = "";
                string fecha = "";

                HacerConexion();

                //Extraemos elementos de la lista antes de borrar los seleccionados
                IEnumerable diagnosticos_items = ListadoDiagActivos.SelectedItems;

                //if(diagnosticos_items)
                foreach (nuevoTratamiento tratamiento in diagnosticos_items)
                {
                    nombrepaciente = tratamiento.paciente;
                    nombre = tratamiento.tratamiento;
                    fecha = tratamiento.inicio;
                }

                //Obtiene el numero de veces para ver si es periodico..
                object NoVeces = obj2.Obtener_NoVeces_Tratamiento(nombre);
                object id_paciente = obj2.Obtener_IdPaciente_NombreCompleto(nombrepaciente);

                //Cargar los remedios, analisis y codigos individuales

                //obtener id tratamiento
                object id = obj2.Obtener_IDTratamiento(id_paciente.ToString(), nombrepaciente, nombre);

                // MessageBox.Show(id.ToString());

                //Obtiene los codigos del tratamiento almacenados..
                DataTable codigos_tratamiento = obj2.CodigosTratamiento(id.ToString());
                

                //Casteo de los valores de acuerdo a su naturaleza
                for (int j = 0; j <= codigos_tratamiento.Rows.Count - 1; j++)
                {
                    //Remedios
                    if (codigos_tratamiento.Rows[j][3].ToString() == "R")
                    {
                        detalleRemedios.Items.Add(codigos_tratamiento.Rows[j][2].ToString());
                    }

                    //Analisis
                    if (codigos_tratamiento.Rows[j][3].ToString() == "A")
                    {
                        detalleAnalisis.Items.Add(codigos_tratamiento.Rows[j][2].ToString());
                    }

                    //Cod Individuales
                    if (codigos_tratamiento.Rows[j][3].ToString() == "C")
                    {
                        detalleCodigosIndiv.Items.Add(codigos_tratamiento.Rows[j][2].ToString());

                    }
                }

                //Si es periodico hacer..
                if (NoVeces.ToString() != "1")
                {
                    DataTable Tratamientos_asociados = obj2.Obtener_Tratamientos_Asociados(id.ToString());

                    //Cargar al listbox
                    for (int d = 0; d <= Tratamientos_asociados.Rows.Count - 1; d++)
                    {
                        // detalleAsociados.Items.Add(Tratamientos_asociados.Rows[d][4].ToString() + "     " + Tratamientos_asociados.Rows[d][7].ToString());
                        detalleAsociados.Items.Add(Tratamientos_asociados.Rows[d][7].ToString());
                    }
                }
                else
                {
                    detalleAsociados.Items.Add(obtenerRecurso("contentMessage"));
                }

                CerrarConexion();

                Mostrar_Detalle_Tratamiento();
                // Ocultar_Detalle_Tratamiento();
            }
        }

        void Cerrar_Detalle(object sender, RoutedEventArgs e)
        {
            Ocultar_Detalle_Tratamiento();
        }
        void Mostrar_Detalle_Tratamiento()
        {
            groupBox3_Copy3.Visibility = Visibility.Visible;
            lblDiagActivo_Copy1.Visibility = Visibility.Visible;
            detalleRemedios.Visibility = Visibility.Visible;
            lblDiagActivo_Copy2.Visibility = Visibility.Visible;
            detalleAnalisis.Visibility = Visibility.Visible;
            lblDiagActivo_Copy3.Visibility = Visibility.Visible;
            detalleCodigosIndiv.Visibility = Visibility.Visible;
            cmdGenerarReporte.Visibility = Visibility.Visible;
            cerrarDetalle.Visibility = Visibility.Visible;
            groupBox3_Copy4.Visibility = Visibility.Visible;
            detalleAsociados.Visibility = Visibility.Visible;
            lblDiagActivo_Copy.Visibility = Visibility.Visible;
        }

        void Ocultar_Detalle_Tratamiento()
        {
            groupBox3_Copy3.Visibility = Visibility.Hidden;
            lblDiagActivo_Copy1.Visibility = Visibility.Hidden;
            detalleRemedios.Visibility = Visibility.Hidden;
            lblDiagActivo_Copy2.Visibility = Visibility.Hidden;
            detalleAnalisis.Visibility = Visibility.Hidden;
            lblDiagActivo_Copy3.Visibility = Visibility.Hidden;
            detalleCodigosIndiv.Visibility = Visibility.Hidden;
            cmdGenerarReporte.Visibility = Visibility.Hidden;
            cerrarDetalle.Visibility = Visibility.Hidden;
            groupBox3_Copy4.Visibility = Visibility.Hidden;
            detalleAsociados.Visibility = Visibility.Hidden;
            lblDiagActivo_Copy.Visibility = Visibility.Hidden;
        }

        //Funcion para generar reporte de un analisis en particular
        private void cmdGenerarReporte_Click(object sender, RoutedEventArgs e)
        {
            //Obtenemos datos del listview para generar el reporte..
            string nombre_tratamiento = "";
            string nombre_paciente = "";
            string fecha_inicio = "";
            string duracion = "";

            IEnumerable diagnosticos_items = ListadoDiagActivos.SelectedItems;

            foreach (nuevoTratamiento tratamiento in diagnosticos_items)
            {
                nombre_tratamiento = tratamiento.tratamiento;
                nombre_paciente = tratamiento.paciente;
                fecha_inicio = tratamiento.inicio;
                duracion = tratamiento.duracion;
            }

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "PDF Document|*.pdf";
            saveFileDialog1.Title = "Guardar - Reporte del Tratamiento";
            saveFileDialog1.ShowDialog();

            //Si se eligio ruta haz...
            if (saveFileDialog1.FileName != "")
            {
                Document reporte = new Document(iTextSharp.text.PageSize.LETTER, 40, 40, 40, 40);
                PdfWriter buffer = PdfWriter.GetInstance(reporte, new FileStream(saveFileDialog1.FileName.ToString(), FileMode.Create));

                reporte.Open();
                reporte.AddTitle("Reporte del Tratamiento - HS5");
                reporte.AddCreator("Homoeonic Software 5");
                reporte.AddAuthor("HS5");

                iTextSharp.text.Font titulos = iTextSharp.text.FontFactory.GetFont("HELVETICA", 14, iTextSharp.text.Font.BOLD);
                iTextSharp.text.Font subtitulos = iTextSharp.text.FontFactory.GetFont("HELVETICA", 12, iTextSharp.text.Font.BOLD);
                iTextSharp.text.Font texto = iTextSharp.text.FontFactory.GetFont("HELVETICA", 10, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Font texto2 = iTextSharp.text.FontFactory.GetFont("HELVETICA", 10, iTextSharp.text.Font.BOLD);

                iTextSharp.text.Font LineBreak = iTextSharp.text.FontFactory.GetFont("HELVETICA", 10, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Paragraph linebreak = new iTextSharp.text.Paragraph("\n", LineBreak);

                //Documento titulo
                iTextSharp.text.Paragraph parrafo = new iTextSharp.text.Paragraph("Reporte del Tratamiento - HS5", titulos);
                reporte.Add(parrafo);

                reporte.Add(linebreak);

                iTextSharp.text.Paragraph parrafo2 = new iTextSharp.text.Paragraph("Datos generales", subtitulos);
                reporte.Add(parrafo2);
                reporte.Add(linebreak);

                /*nombre_tratamiento = tratamiento.tratamiento;
                nombre_paciente = tratamiento.paciente;
                fecha_inicio = tratamiento.inicio;
                duracion = tratamiento.duracion;*/

                Chunk parrafo3 = new Chunk("Nombre: ", texto2);
                Chunk parrafo3_1 = new Chunk(nombre_tratamiento, texto);
                reporte.Add(parrafo3);
                reporte.Add(parrafo3_1);
                reporte.Add(Chunk.NEWLINE);

                Chunk parrafo4 = new Chunk("Paciente: ", texto2);
                Chunk parrafo4_1 = new Chunk(nombre_paciente, texto);
                reporte.Add(parrafo4);
                reporte.Add(parrafo4_1);
                reporte.Add(Chunk.NEWLINE);

                Chunk parrafo5 = new Chunk("Fecha: ", texto2);
                Chunk parrafo5_1 = new Chunk(fecha_inicio, texto);
                reporte.Add(parrafo5);
                reporte.Add(parrafo5_1);
                reporte.Add(Chunk.NEWLINE);

                //string duracion_reloj = CalcularTiempo_FormatoReloj(Int32.Parse(duracion));
                Chunk parrafo6 = new Chunk("Duración: ", texto2);
                Chunk parrafo6_1 = new Chunk(duracion, texto);
                reporte.Add(parrafo6);
                reporte.Add(parrafo6_1);
                reporte.Add(Chunk.NEWLINE);

                if (detalleAsociados.Items.Count != 0)
                {
                    Chunk parrafo7 = new Chunk("Tipo: ", texto2);
                    Chunk parrafo7_1 = new Chunk("Periódico", texto);
                    reporte.Add(parrafo7);
                    reporte.Add(parrafo7_1);
                    reporte.Add(linebreak);

                    iTextSharp.text.Paragraph parrafod = new iTextSharp.text.Paragraph("Período", texto2);
                    reporte.Add(parrafod);

                    //Si es periodico carga sus asociados..
                    for (int i = 0; i <= detalleAsociados.Items.Count - 1; i++)
                    {
                        iTextSharp.text.Paragraph detalleaso = new iTextSharp.text.Paragraph(detalleAsociados.Items[i].ToString(), texto);
                        reporte.Add(detalleaso);
                    }
                }
                else
                {
                    Chunk parrafo7 = new Chunk("Tipo: ", texto2);
                    Chunk parrafo7_1 = new Chunk("Sencillo", texto);
                    reporte.Add(parrafo7);
                    reporte.Add(parrafo7_1);
                    reporte.Add(linebreak);
                }
                reporte.Add(linebreak);

                iTextSharp.text.Paragraph detalles = new iTextSharp.text.Paragraph("Detalle del tratamiento", subtitulos);
                reporte.Add(detalles);
                reporte.Add(linebreak);

                //  iTextSharp.text.Paragraph detallesremedios = new iTextSharp.text.Paragraph("Remedios", texto2);
                //  reporte.Add(detallesremedios);
                //  reporte.Add(linebreak);

                //Agregar el contenido de los remedios

                //Si hay remedios agregar
                if (detalleRemedios.Items.Count != 0)
                {
                    //Cantidad de columnas
                    PdfPTable table_remedios = new PdfPTable(1);
                    table_remedios.TotalWidth = 144;
                    //table_codigos.AddCell(new Phrase("Código",texto2));
                    table_remedios.AddCell(new Phrase("Remedios", texto2));

                    table_remedios.HeaderRows = 1;

                    for (int i = 0; i <= detalleRemedios.Items.Count - 1; i++)
                    {
                        //ListaCodigos.Items.RemoveAt(ListaCodigos.Items.IndexOf(codigo.nombre.ToString()));
                        //table_codigos.AddCell(new Phrase(codigo.rates.ToString()));
                        table_remedios.AddCell(new Phrase(detalleRemedios.Items[i].ToString(), texto));
                    }

                    reporte.Add(table_remedios);
                    reporte.Add(linebreak);
                    reporte.Add(linebreak);


                }

                //Agregar el contenido de los analisis

                if (detalleAnalisis.Items.Count != 0)
                {
                    //Cantidad de columnas
                    PdfPTable table_analisis = new PdfPTable(1);
                    table_analisis.TotalWidth = 144;
                    //table_codigos.AddCell(new Phrase("Código",texto2));
                    table_analisis.AddCell(new Phrase("Análisis", texto2));

                    table_analisis.HeaderRows = 1;

                    for (int i = 0; i <= detalleAnalisis.Items.Count - 1; i++)
                    {
                        //ListaCodigos.Items.RemoveAt(ListaCodigos.Items.IndexOf(codigo.nombre.ToString()));
                        //table_codigos.AddCell(new Phrase(codigo.rates.ToString()));
                        table_analisis.AddCell(new Phrase(detalleAnalisis.Items[i].ToString(), texto));
                    }

                    reporte.Add(table_analisis);
                    reporte.Add(linebreak);
                    reporte.Add(linebreak);

                }

                //Agregar el contenido de los codigos individuales

                if (detalleCodigosIndiv.Items.Count != 0)
                {
                    //Cantidad de columnas
                    PdfPTable table_codigos = new PdfPTable(1);
                    table_codigos.TotalWidth = 144;
                    //table_codigos.AddCell(new Phrase("Código",texto2));
                    table_codigos.AddCell(new Phrase("Códigos Individuales", texto2));

                    table_codigos.HeaderRows = 1;

                    for (int i = 0; i <= detalleCodigosIndiv.Items.Count - 1; i++)
                    {
                        //ListaCodigos.Items.RemoveAt(ListaCodigos.Items.IndexOf(codigo.nombre.ToString()));
                        //table_codigos.AddCell(new Phrase(codigo.rates.ToString()));
                        table_codigos.AddCell(new Phrase(detalleCodigosIndiv.Items[i].ToString(), texto));
                    }

                    reporte.Add(table_codigos);

                }

                reporte.Close();
            }
            else
            {
                MessageBox.Show(obtenerRecurso("messageError5"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmdReanudar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ListadoDiagActivos.Items.Count != 0)
                {
                    obj.BroadcastON();
                    _timer.Start();

                }

            }
            catch (NullReferenceException)
            { 

            }
        }

        private void tratamientoencola_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //Cargar los elementos en la lista en espera
            // Mostrar_ListadoEnEspera();
        }

        private void cerrarListadoEspera_Click(object sender, RoutedEventArgs e)
        {
            Ocultar_ListadoEnEspera();
        }

        void Ocultar_ListadoEnEspera()
        {
            tratamencola.Visibility = Visibility.Hidden;
            listadoEnEspera.Visibility = Visibility.Hidden;
            cmdBorrarSec.Visibility = Visibility.Hidden;
            cmdBorrarSecCompleta.Visibility = Visibility.Hidden;
            cerrarListadoEspera.Visibility = Visibility.Hidden;
        }

        void Mostrar_ListadoEnEspera()
        {
            tratamencola.Visibility = Visibility.Visible;
            listadoEnEspera.Visibility = Visibility.Visible;
            cmdBorrarSec.Visibility = Visibility.Visible;
            cmdBorrarSecCompleta.Visibility = Visibility.Visible;
            cerrarListadoEspera.Visibility = Visibility.Visible;
        }

        private void tratamientoencola_Click(object sender, RoutedEventArgs e)
        {
            //Cargar en la lista los elementos en cola
            HacerConexion();

            DataTable Tratamientos_Espera_Resumido_Lista = obj2.Tratamientos_Espera_Total_Lista();

            listadoEnEspera.Items.Clear();

            for (int y = 0; y <= Tratamientos_Espera_Resumido_Lista.Rows.Count - 1; y++)
            {
                // listadoEnEspera.Items.Add(Tratamientos_Espera_Resumido_Lista.Rows[y][0].ToString() + " - " + Tratamientos_Espera_Resumido_Lista.Rows[y][1].ToString());
                listadoEnEspera.Items.Add(new nuevoTratamiento { tratamiento = Tratamientos_Espera_Resumido_Lista.Rows[y][0].ToString(), inicio = Tratamientos_Espera_Resumido_Lista.Rows[y][1].ToString() });

            }

            CerrarConexion();
            Mostrar_ListadoEnEspera();
        }

        private void cmdBorrarSec_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string nombre_trat = "";
                string fecha_trat_selecc = "";

                //Extraemos elementos de la lista antes de borrar los seleccionados
                IEnumerable tratamientos_items = listadoEnEspera.SelectedItems;

                foreach (nuevoTratamiento tratamiento in tratamientos_items)
                {
                    nombre_trat = tratamiento.tratamiento;
                    fecha_trat_selecc = tratamiento.inicio;
                }

                HacerConexion();
                //Borrar elemento
                obj2.Eliminar_Tratamiento(nombre_trat.ToString(), fecha_trat_selecc);

                //Borrar del listview
                listadoEnEspera.Items.Remove(listadoEnEspera.SelectedItems);
                CerrarConexion();

            }
            catch (NullReferenceException)
            {
                MessageBox.Show(obtenerRecurso("messageError4"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //PENDIENTE BOTON DE BORRAR SECUENCIA EN LA LISTA DE TRATAMIENTOS EN ESPERA
        private void cmdBorrarSecCompleta_Click(object sender, RoutedEventArgs e)
        {
            try
            {

            }
            catch (NullReferenceException)
            {

            }
        }

        private void cmdPausar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ListadoDiagActivos.Items.Count != 0)
                {

                    _timer.Stop(); // Desactivamos el timer principal.. para que deje de correr el tiempo
                    obj.BroadcastOFF();

                    //Actualiza el texto a PAUSADO
                    IEnumerable itemsCodigos = this.ListadoDiagActivos.Items;
                    List<string> nombre_pacientes = new List<string>();
                    List<string> nombre_tratamientos = new List<string>();
                    List<string> fechas = new List<string>();
                    List<string> duraciones = new List<string>();
                    List<string> tfaltante = new List<string>();

                    //Copiado
                    foreach (nuevoTratamiento codigo in itemsCodigos)
                    {
                        nombre_pacientes.Add(codigo.paciente.ToString());
                        nombre_tratamientos.Add(codigo.tratamiento.ToString());
                        fechas.Add(codigo.inicio.ToString());
                        duraciones.Add(codigo.duracion.ToString());
                        tfaltante.Add(obtenerRecurso("PAUSED"));
                    }

                    ListadoDiagActivos.Items.Clear();

                    //Actualizado
                    for (int i = 0; i <= nombre_pacientes.Count - 1; i++)
                    {
                        ListadoDiagActivos.Items.Add(new nuevoTratamiento { paciente = nombre_pacientes[i], tratamiento = nombre_tratamientos[i], inicio = fechas[i], duracion = duraciones[i], tfaltante = tfaltante[i] });

                    }
                }
            }
            catch (NullReferenceException)
            { }
        }

        //Cargar imagen al paciente..
        private void AsignarImagenPaciente_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AsignarImagenPaciente_Click_1(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Seleccione una imagen";
            op.Filter = "Formatos soportados|*.jpg;*.jpeg;*.png|" +
              "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
              "Portable Network Graphic (*.png)|*.png";
            if (op.ShowDialog() == true)
            {
                //Se almacena en memoria y evita hacer uso directo de ella.
                BitmapImage imageb = new BitmapImage();
                imageb.BeginInit();
                Uri imageSource = new Uri(op.FileName);
                imageb.UriSource = imageSource;
                imageb.EndInit();
                image.Source = imageb;
            }
        }

        //Funcion para generar reporte del paciente (Expediente)
        public void cmdGenerarExpPDF_Click(object sender, RoutedEventArgs e)
        {
            //Comprobamos si la version esta registrada
            HacerConexion();

            DataTable Version = obj2.Consultar_Version();
            //Campos para los datos de registro de la version
            string nombre = "";
            string descripcion = "";

            //Recorremos valores
            for (int i = 0; i <= Version.Rows.Count - 1; i++)
            {
                nombre = Version.Rows[i][1].ToString();
                descripcion = Version.Rows[i][2].ToString();
            }

            //En caso de que no este registrada
            if (Version.Rows.Count == 0)
            {
                nombre = "<REGISTRE VERSION PARA PERSONALIZAR>";
                descripcion = "<REGISTRE VERSION PARA PERSONALIZAR>";
            }

            CerrarConexion();
            CargarListadoCompletoPacientes();

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Archivos de Pdf|*.pdf";
            saveFileDialog.Title = "Save - Patient's Report";
            saveFileDialog.FileName = "";
            saveFileDialog.ShowDialog();

            Document reporte = new Document(iTextSharp.text.PageSize.LETTER, 40, 40, 80, 80);
            var path = string.Empty;
            path = saveFileDialog.FileName;
            PdfWriter buffer = PdfWriter.GetInstance(reporte, new FileStream(path, FileMode.Create));
            
            /*
            //Comprobamos si la version esta registrada
            HacerConexion();

            DataTable Version = obj2.Consultar_Version();
            //Campos para los datos de registro de la version
            string nombre = "";
            string descripcion = "";

            //Recorremos valores
            for (int i = 0; i <= Version.Rows.Count - 1; i++)
            {
                nombre = Version.Rows[i][1].ToString();
                descripcion = Version.Rows[i][2].ToString();
            }

            //En caso de que no este registrada
            if (Version.Rows.Count == 0)
            {
                nombre = "<REGISTRE VERSION PARA PERSONALIZAR>";
                descripcion = "<REGISTRE VERSION PARA PERSONALIZAR>";
            }

            CerrarConexion();
            CargarListadoCompletoPacientes();
            
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "PDF Document|*.pdf";
            saveFileDialog1.Title = "Save - Patient's Report";
            saveFileDialog1.ShowDialog();

            //Si se eligio ruta haz...
            if (saveFileDialog1.FileName != "")
            {
                Document reporte = new Document();
                PdfWriter.GetInstance(reporte, new FileStream("prueba.pdf",FileMode.Create));
            
                //Mandamos datos del registro de version
                //buffer.PageEvent = new HS5.reporte_ext("Patient's Report", nombre, descripcion); //Agrega el encabezado y pie de pagina
                */
                reporte.Open();
                // reporte.AddTitle("Expediente del Paciente - HS5");
                //reporte.AddCreator("Homoeonic Software 5");
                // reporte.AddAuthor("HS5");

                iTextSharp.text.Font titulos = iTextSharp.text.FontFactory.GetFont("HELVETICA", 14, iTextSharp.text.Font.BOLD);
                iTextSharp.text.Font subtitulos = iTextSharp.text.FontFactory.GetFont("HELVETICA", 12, iTextSharp.text.Font.BOLD);
                iTextSharp.text.Font texto = iTextSharp.text.FontFactory.GetFont("HELVETICA", 10, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Font texto2 = iTextSharp.text.FontFactory.GetFont("HELVETICA", 10, iTextSharp.text.Font.BOLD);

                iTextSharp.text.Font LineBreak = iTextSharp.text.FontFactory.GetFont("HELVETICA", 10, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Paragraph linebreak = new iTextSharp.text.Paragraph("\n", LineBreak);

                //Documento titulo
                reporte.Add(linebreak);
                reporte.Add(linebreak);

                //FONDO DEL DOCUMENTO
                string path_ins = RutaInstalacion() + "//fotos//portada_hoja.png";

                //Fondo del documento
                iTextSharp.text.Image fondodoc = iTextSharp.text.Image.GetInstance(path_ins);
                fondodoc.ScaleToFit(reporte.PageSize);
                fondodoc.Alignment = iTextSharp.text.Image.UNDERLYING;
                fondodoc.SetAbsolutePosition(0, 0);
                reporte.Add(fondodoc);

                //Imagen del paciente
                string ruta = id_ppaciente.Content.ToString();
                if (ruta != "NA")
                {
                    string imageURL = ruta;
                    iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(imageURL);
                    jpg.ScaleToFit(250f, 250f);
                    // jpg.ScaleToFit(500f,30f);
                    jpg.Alignment = iTextSharp.text.Image.TEXTWRAP | iTextSharp.text.Image.ALIGN_RIGHT;
                    jpg.IndentationLeft = 9f;
                    jpg.SpacingAfter = 9f;
                    // jpg.BorderWidthTop = 36f;
                    reporte.Add(jpg);
                }

                iTextSharp.text.Paragraph parrafo2 = new iTextSharp.text.Paragraph("Personal Details", subtitulos);
                reporte.Add(parrafo2);
                reporte.Add(linebreak);

                Chunk parrafo3 = new Chunk("Patient's Name: ", texto2);
                Chunk parrafo3_1 = new Chunk(txtNombre1.Text + " " + txtApellidoPat1.Text + " " + txtApellidoMat1.Text, texto);
                reporte.Add(parrafo3);
                reporte.Add(parrafo3_1);
                reporte.Add(Chunk.NEWLINE);

                Chunk parrafo4 = new Chunk("Birthday: ", texto2);
                Chunk parrafo4_1 = new Chunk(txtFecha1.Text, texto);
                reporte.Add(parrafo4);
                reporte.Add(parrafo4_1);
                reporte.Add(Chunk.NEWLINE);

                Chunk parrafo5 = new Chunk("Gender: ", texto2);
                Chunk parrafo5_1 = new Chunk(txtSexo1.Text, texto);
                reporte.Add(parrafo5);
                reporte.Add(parrafo5_1);
                reporte.Add(Chunk.NEWLINE);

                Chunk parrafo6 = new Chunk("E-mail: ", texto2);
                Chunk parrafo6_1 = new Chunk(txtEmail1.Text, texto);
                reporte.Add(parrafo6);
                reporte.Add(parrafo6_1);
                reporte.Add(Chunk.NEWLINE);

                //DOMICILIOS
                if (listadodomicilios1_Copy.Items.Count != 0)
                {
                    reporte.Add(linebreak);
                    iTextSharp.text.Paragraph parrafod = new iTextSharp.text.Paragraph("Addresses", texto2);
                    reporte.Add(parrafod);

                    //Se cargan los domicilios asociados
                    for (int i = 0; i <= listadodomicilios1_Copy.Items.Count - 1; i++)
                    {
                        iTextSharp.text.Paragraph detalleaso = new iTextSharp.text.Paragraph(listadodomicilios1_Copy.Items[i].ToString(), texto);
                        reporte.Add(detalleaso);
                    }
                }

                //TELEFONOS
                if (listaTelefonos1.Items.Count != 0)
                {
                    reporte.Add(linebreak);
                    iTextSharp.text.Paragraph parrafod2 = new iTextSharp.text.Paragraph("Phones", texto2);
                    reporte.Add(parrafod2);

                    //Se cargan los teléfonos asociados
                    for (int i = 0; i <= listaTelefonos1.Items.Count - 1; i++)
                    {
                        iTextSharp.text.Paragraph detalleaso = new iTextSharp.text.Paragraph(listaTelefonos1.Items[i].ToString(), texto);
                        reporte.Add(detalleaso);
                    }
                }

                reporte.Add(linebreak);
                reporte.Add(linebreak);

                iTextSharp.text.Paragraph parrafoante = new iTextSharp.text.Paragraph("Background", subtitulos);
                reporte.Add(parrafoante);
                reporte.Add(linebreak);

                //ANTECEDENTES

                //Heredo
                if (ListadoHeredo1_Copy.Items.Count != 0)
                {
                    iTextSharp.text.Paragraph parrafoh = new iTextSharp.text.Paragraph("Hereditary", texto2);
                    reporte.Add(parrafoh);

                    //Se cargan los teléfonos asociados
                    for (int i = 0; i <= ListadoHeredo1_Copy.Items.Count - 1; i++)
                    {
                        iTextSharp.text.Paragraph detalleaso = new iTextSharp.text.Paragraph(ListadoHeredo1_Copy.Items[i].ToString(), texto);
                        reporte.Add(detalleaso);
                    }
                }
                reporte.Add(linebreak);

                //Patologicos
                if (listadoPatologicos1_Copy.Items.Count != 0)
                {
                    iTextSharp.text.Paragraph parrafop = new iTextSharp.text.Paragraph("Pathological", texto2);
                    reporte.Add(parrafop);

                    //Se cargan los teléfonos asociados
                    for (int i = 0; i <= listadoPatologicos1_Copy.Items.Count - 1; i++)
                    {
                        iTextSharp.text.Paragraph detalleaso = new iTextSharp.text.Paragraph(listadoPatologicos1_Copy.Items[i].ToString(), texto);
                        reporte.Add(detalleaso);
                    }
                }
                reporte.Add(linebreak);

                //No Patologicos
                if (listadoNoPatologicos1_Copy.Items.Count != 0)
                {
                    iTextSharp.text.Paragraph parrafonp = new iTextSharp.text.Paragraph("Non Pathological", texto2);
                    reporte.Add(parrafonp);

                    //Se cargan los teléfonos asociados
                    for (int i = 0; i <= listadoNoPatologicos1_Copy.Items.Count - 1; i++)
                    {
                        iTextSharp.text.Paragraph detalleaso = new iTextSharp.text.Paragraph(listadoNoPatologicos1_Copy.Items[i].ToString(), texto);
                        reporte.Add(detalleaso);
                    }
                }
                reporte.Add(linebreak);

                //Comentarios
                if (listadoComentarios1_Copy.Items.Count != 0)
                {
                    iTextSharp.text.Paragraph parrafonc = new iTextSharp.text.Paragraph("Comments", texto2);
                    reporte.Add(parrafonc);

                    //Se cargan los teléfonos asociados
                    for (int i = 0; i <= listadoComentarios1_Copy.Items.Count - 1; i++)
                    {
                        iTextSharp.text.Paragraph detalleaso = new iTextSharp.text.Paragraph(listadoComentarios1_Copy.Items[i].ToString(), texto);
                        reporte.Add(detalleaso);
                    }
                }
                reporte.Add(linebreak);

                reporte.Close();
            
        }

        private void cmdRegistroVersion_Click(object sender, RoutedEventArgs e)
        {
            if (txtNombreRegistro.Text != "" && txtDescripcionRegistro.Text != "")
            {
                //Grabamos en BD
                HacerConexion();
                obj2.RegistrarVersion(txtNombreRegistro.Text, txtDescripcionRegistro.Text);
                CerrarConexion();

                //PANEL DE REGISTRO DE VERSION
                registrarGroup.Visibility = Visibility.Hidden;
                lblNombreRegistro.Visibility = Visibility.Hidden;
                txtNombreRegistro.Visibility = Visibility.Hidden;
                lblDescripcionRegistro.Visibility = Visibility.Hidden;
                txtDescripcionRegistro.Visibility = Visibility.Hidden;
                lblDescripcionEjemplo.Visibility = Visibility.Hidden;
                lblNombreEjemplo.Visibility = Visibility.Hidden;
                cmdRegistroVersion.Visibility = Visibility.Hidden;
                txtNombreRegistro.Text = "";
                txtDescripcionRegistro.Text = "";

                //BUSQUEDA DEL PACIENTE Y REGISTRO
                groupBox.Visibility = Visibility.Visible;
                txtBuscarPaciente.Visibility = Visibility.Visible;
                lblBusqueda.Visibility = Visibility.Visible;
                cmdEliminar.Visibility = Visibility.Visible;
                ListaPacientes.Visibility = Visibility.Visible;
                lblPacientes.Visibility = Visibility.Visible;
                PacienteGroup.Visibility = Visibility.Visible;
                AnalisisTab.IsEnabled = true;
                RemediosTab.IsEnabled = true;
                ColorTab.IsEnabled = true;
                CategoriasTab.IsEnabled = true;
                tratamientos.IsEnabled = true;
                RegistroVersionBoton.Visibility = Visibility.Hidden; //Ocultar Boton
            }
            else
            {
                MessageBox.Show(obtenerRecurso("messageError3"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                txtNombreRegistro.Focus();
            }
        }

        //Funcion para elaborar reporte con la lista de todos los pacientes del doctor.
        private void cmdImprimirListado_Click(object sender, RoutedEventArgs e)
        {
            //Obtiene la info de los pacientes
            HacerConexion();

            //MessageBox.Show(Listado_PacientesT.Rows.Count.ToString());

            DataTable Version = obj2.Consultar_Version();
            //Campos para los datos de registro de la version
            string nombre = "";
            string descripcion = "";

            //Recorremos valores
            for (int i = 0; i <= Version.Rows.Count - 1; i++)
            {
                nombre = Version.Rows[i][1].ToString();
                descripcion = Version.Rows[i][2].ToString();
            }

            //En caso de que no este registrada
            if (Version.Rows.Count == 0)
            {
                nombre = "<REGISTRE VERSION PARA PERSONALIZAR>";
                descripcion = "<REGISTRE VERSION PARA PERSONALIZAR>";
            }

            DataTable Listado_PacientesT = obj2.Obtener_Pacientes();

            List<string> val0 = new List<string>();
            List<string> val1 = new List<string>();
            List<string> val2 = new List<string>();
            List<string> val3 = new List<string>();
            List<string> val4 = new List<string>();
            List<string> val5 = new List<string>();
            List<string> val6 = new List<string>();

            for (int i = 0; i <= Listado_PacientesT.Rows.Count - 1; i++)
            {
                val0.Add(Listado_PacientesT.Rows[i][0].ToString());
                val1.Add(Listado_PacientesT.Rows[i][1].ToString());
                val2.Add(Listado_PacientesT.Rows[i][2].ToString());
                val3.Add(Listado_PacientesT.Rows[i][3].ToString());
                val4.Add(Listado_PacientesT.Rows[i][4].ToString());
                val5.Add(Listado_PacientesT.Rows[i][5].ToString());
                val6.Add(Listado_PacientesT.Rows[i][6].ToString());

            }

            CerrarConexion();
            CargarListadoCompletoPacientes();

            //Salvar PDF
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "PDF Document|*.pdf";
            saveFileDialog1.Title = "Save - Patient's List";
            saveFileDialog1.ShowDialog();

            //Si se eligio ruta haz...
            if (saveFileDialog1.FileName != "")
            {

                Document reporte = new Document(iTextSharp.text.PageSize.LETTER, 40, 40, 80, 80);
                PdfWriter buffer;
                try
                {
                    buffer = PdfWriter.GetInstance(reporte, new FileStream(saveFileDialog1.FileName.ToString(), FileMode.Create));
                    buffer.PageEvent = new HS5.reporte_ext("Patient's List", nombre, descripcion); //Agrega el encabezado y pie de pagina

                }
                catch (Exception ex)
                {
                }
                //Mandamos datos del registro de version
                reporte.Open();
               
                iTextSharp.text.Font titulos = iTextSharp.text.FontFactory.GetFont("HELVETICA", 14, iTextSharp.text.Font.BOLD);
                iTextSharp.text.Font subtitulos = iTextSharp.text.FontFactory.GetFont("HELVETICA", 12, iTextSharp.text.Font.BOLD);
                iTextSharp.text.Font texto = iTextSharp.text.FontFactory.GetFont("HELVETICA", 10, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Font texto2 = iTextSharp.text.FontFactory.GetFont("HELVETICA", 10, iTextSharp.text.Font.BOLD);

                iTextSharp.text.Font LineBreak = iTextSharp.text.FontFactory.GetFont("HELVETICA", 10, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Paragraph linebreak = new iTextSharp.text.Paragraph("\n", LineBreak);

                //Documento titulo
                reporte.Add(linebreak);
                reporte.Add(linebreak);

                //FONDO DEL DOCUMENTO
                string path = RutaInstalacion() + "//fotos//portada_hoja.png";

                //Fondo del documento
                iTextSharp.text.Image fondodoc = iTextSharp.text.Image.GetInstance(path);
                fondodoc.ScaleToFit(reporte.PageSize);
                fondodoc.Alignment = iTextSharp.text.Image.UNDERLYING;
                fondodoc.SetAbsolutePosition(0, 0);
                reporte.Add(fondodoc);

                //DATOS GENERALES DEL PACIENTE PARA EL LISTADO

                //Si existen pacientes registrados haz...
                if (val0.Count != 0)
                {
                    for (int i = 0; i <= val0.Count - 1; i++)
                    {
                        Chunk parrafo3 = new Chunk("Patient Name: ", texto2);
                        Chunk parrafo3_1 = new Chunk(val0[i] + " " + val1[i] + " " + val2[i], texto);
                        reporte.Add(parrafo3);
                        reporte.Add(parrafo3_1);
                        reporte.Add(Chunk.NEWLINE);

                        if (val3[i] != "")
                        {
                            Chunk parrafo4 = new Chunk("E-mail: ", texto2);
                            Chunk parrafo4_1 = new Chunk(val3[i], texto);
                            reporte.Add(parrafo4);
                            reporte.Add(parrafo4_1);
                            reporte.Add(Chunk.NEWLINE);
                        }

                        Chunk parrafo5 = new Chunk("Gender: ", texto2);
                        Chunk parrafo5_1 = new Chunk(val4[i], texto);
                        reporte.Add(parrafo5);
                        reporte.Add(parrafo5_1);
                        reporte.Add(Chunk.NEWLINE);

                        Chunk parrafo6 = new Chunk("Birthday: ", texto2);
                        Chunk parrafo6_1 = new Chunk(val5[i], texto);
                        reporte.Add(parrafo6);
                        reporte.Add(parrafo6_1);
                        reporte.Add(Chunk.NEWLINE);

                        if (val6[i] != "")
                        {
                            Chunk parrafo7 = new Chunk("PGR: ", texto2);
                            Chunk parrafo7_1 = new Chunk(val6[i], texto);
                            reporte.Add(parrafo7);
                            reporte.Add(parrafo7_1);
                            reporte.Add(Chunk.NEWLINE);
                        }
                        reporte.Add(linebreak);

                    }

                }
                reporte.Close();
            }
        }

        //Funcion para generar el reporte de los analisis del paciente (Lista de analisis)
        private void cmdGenerarAnalisisPDF_Click(object sender, RoutedEventArgs e)
        {
            //Comprobamos si la version esta registrada
            HacerConexion();

            DataTable Version = obj2.Consultar_Version();
            //Campos para los datos de registro de la version
            string nombre = "";
            string descripcion = "";

            //Recorremos valores
            for (int i = 0; i <= Version.Rows.Count - 1; i++)
            {
                nombre = Version.Rows[i][1].ToString();
                descripcion = Version.Rows[i][2].ToString();
            }

            //En caso de que no este registrada
            if (Version.Rows.Count == 0)
            {
                nombre = "<REGISTRE VERSION PARA PERSONALIZAR>";
                descripcion = "<REGISTRE VERSION PARA PERSONALIZAR>";
            }

            CerrarConexion();
            CargarListadoCompletoPacientes();

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "PDF Document|*.pdf";
            saveFileDialog1.Title = "Save - Analysis' List";
            saveFileDialog1.ShowDialog();
            try
            {
                if (saveFileDialog1.FileName != "")
                {
                    Document reporte = new Document(iTextSharp.text.PageSize.LETTER, 40, 40, 80, 80);
                    PdfWriter buffer = PdfWriter.GetInstance(reporte, new FileStream(saveFileDialog1.FileName.ToString(), FileMode.Create));
                    //Mandamos datos del registro de version
                    buffer.PageEvent = new HS5.reporte_ext("Analysis' List", nombre, descripcion); //Agrega el encabezado y pie de pagina

                    reporte.Open();
                    // reporte.AddTitle("Expediente del Paciente - HS5");
                    //reporte.AddCreator("Homoeonic Software 5");
                    // reporte.AddAuthor("HS5");

                    iTextSharp.text.Font titulos = iTextSharp.text.FontFactory.GetFont("HELVETICA", 14, iTextSharp.text.Font.BOLD);
                    iTextSharp.text.Font subtitulos = iTextSharp.text.FontFactory.GetFont("HELVETICA", 12, iTextSharp.text.Font.BOLD);
                    iTextSharp.text.Font texto = iTextSharp.text.FontFactory.GetFont("HELVETICA", 10, iTextSharp.text.Font.NORMAL);
                    iTextSharp.text.Font texto2 = iTextSharp.text.FontFactory.GetFont("HELVETICA", 10, iTextSharp.text.Font.BOLD);

                    iTextSharp.text.Font LineBreak = iTextSharp.text.FontFactory.GetFont("HELVETICA", 10, iTextSharp.text.Font.NORMAL);
                    iTextSharp.text.Paragraph linebreak = new iTextSharp.text.Paragraph("\n", LineBreak);

                    //Documento titulo
                    reporte.Add(linebreak);
                    reporte.Add(linebreak);

                    //FONDO DEL DOCUMENTO
                    string path = RutaInstalacion() + "//fotos//portada_hoja.png";

                    //Fondo del documento
                    iTextSharp.text.Image fondodoc = iTextSharp.text.Image.GetInstance(path);
                    fondodoc.ScaleToFit(reporte.PageSize);
                    fondodoc.Alignment = iTextSharp.text.Image.UNDERLYING;
                    fondodoc.SetAbsolutePosition(0, 0);
                    reporte.Add(fondodoc);

                    //Imagen del paciente
                    string ruta = id_ppaciente.Content.ToString();
                    if (ruta != "NA")
                    {
                        string imageURL = ruta;
                        iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(imageURL);
                        jpg.ScaleToFit(250f, 250f);
                        jpg.Alignment = iTextSharp.text.Image.TEXTWRAP | iTextSharp.text.Image.ALIGN_RIGHT;
                        jpg.IndentationLeft = 9f;
                        jpg.SpacingAfter = 9f;
                        reporte.Add(jpg);
                    }

                    //iTextSharp.text.Paragraph parrafo2 = new iTextSharp.text.Paragraph("Datos generales", subtitulos);
                    // reporte.Add(parrafo2);
                    reporte.Add(linebreak);

                    Chunk parrafo3 = new Chunk("Patient's Name: ", texto2);
                    Chunk parrafo3_1 = new Chunk(txtNombre1.Text + " " + txtApellidoPat1.Text + " " + txtApellidoMat1.Text, texto);
                    reporte.Add(parrafo3);
                    reporte.Add(parrafo3_1);
                    reporte.Add(Chunk.NEWLINE);

                    Chunk parrafo4 = new Chunk("Birthday: ", texto2);
                    Chunk parrafo4_1 = new Chunk(txtFecha1.Text, texto);
                    reporte.Add(parrafo4);
                    reporte.Add(parrafo4_1);
                    reporte.Add(Chunk.NEWLINE);

                    Chunk parrafo5 = new Chunk("Gender: ", texto2);
                    Chunk parrafo5_1 = new Chunk(txtSexo1.Text, texto);
                    reporte.Add(parrafo5);
                    reporte.Add(parrafo5_1);
                    reporte.Add(Chunk.NEWLINE);

                    Chunk parrafo6 = new Chunk("E-mail: ", texto2);
                    Chunk parrafo6_1 = new Chunk(txtEmail1.Text, texto);
                    reporte.Add(parrafo6);
                    reporte.Add(parrafo6_1);
                    reporte.Add(linebreak);

                    //ANALISIS (LISTADO) 
                    if (listadoAnalisis1_Copy.Items.Count != 0)
                    {
                        iTextSharp.text.Paragraph parrafoan = new iTextSharp.text.Paragraph("Analysis", texto2);
                        reporte.Add(parrafoan);

                        //Se cargan los teléfonos asociados
                        for (int i = 0; i <= listadoAnalisis1_Copy.Items.Count - 1; i++)
                        {
                            iTextSharp.text.Paragraph detalleaso = new iTextSharp.text.Paragraph(listadoAnalisis1_Copy.Items[i].ToString(), texto);
                            reporte.Add(detalleaso);
                        }

                        reporte.Close();
                    }
                    else
                    {
                        reporte.Close();
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("EXCEPCIONNNN");
            }
            //Si se eligio ruta haz...
            
        }

        //Funcion para mostrar la terapia de color
        void Mostrar_TerapiaColor()
        {
            groupBox3.Visibility = Visibility.Visible;
            radioMasculino.Visibility = Visibility.Visible;
            radioFemenino.Visibility = Visibility.Visible;
            lblVista.Visibility = Visibility.Visible;
            listVista.Visibility = Visibility.Visible;
            groupBox3_Copy.Visibility = Visibility.Visible;
            canvas.Visibility = Visibility.Visible;
            color1.Visibility = Visibility.Visible;
            color3.Visibility = Visibility.Visible;
            color1_Copy1.Visibility = Visibility.Visible;
            color1_Copy2.Visibility = Visibility.Visible;
            color1_Copy3.Visibility = Visibility.Visible;
            color2.Visibility = Visibility.Visible;
            color1_Copy4.Visibility = Visibility.Visible;
            color2_Copy1.Visibility = Visibility.Visible;
            color2_Copy2.Visibility = Visibility.Visible;
            color2_Copy3.Visibility = Visibility.Visible;
            color2_Copy.Visibility = Visibility.Visible;
            color1_Copy5.Visibility = Visibility.Visible;
            color2_Copy4.Visibility = Visibility.Visible;
            color2_Copy5.Visibility = Visibility.Visible;
            color2_Copy6.Visibility = Visibility.Visible;
            color2_Copy7.Visibility = Visibility.Visible;
            color1_Copy6.Visibility = Visibility.Visible;
            color2_Copy8.Visibility = Visibility.Visible;
            color2_Copy9.Visibility = Visibility.Visible;
            color2_Copy10.Visibility = Visibility.Visible;
            color2_Copy11.Visibility = Visibility.Visible;
            color1_Copy7.Visibility = Visibility.Visible;
            color2_Copy12.Visibility = Visibility.Visible;
            color2_Copy13.Visibility = Visibility.Visible;
            color2_Copy14.Visibility = Visibility.Visible;
            color2_Copy15.Visibility = Visibility.Visible;
            color1_Copy8.Visibility = Visibility.Visible;
            color2_Copy16.Visibility = Visibility.Visible;
            color2_Copy17.Visibility = Visibility.Visible;
            color2_Copy18.Visibility = Visibility.Visible;
            color2_Copy19.Visibility = Visibility.Visible;
            color1_Copy9.Visibility = Visibility.Visible;
            color2_Copy20.Visibility = Visibility.Visible;
            color2_Copy21.Visibility = Visibility.Visible;
            color2_Copy22.Visibility = Visibility.Visible;
            color2_Copy23.Visibility = Visibility.Visible;
            color1_Copy10.Visibility = Visibility.Visible;
            color2_Copy24.Visibility = Visibility.Visible;
            color2_Copy25.Visibility = Visibility.Visible;
            color2_Copy26.Visibility = Visibility.Visible;
            color2_Copy27.Visibility = Visibility.Visible;
            color1_Copy11.Visibility = Visibility.Visible;
            color2_Copy28.Visibility = Visibility.Visible;
            color2_Copy29.Visibility = Visibility.Visible;
            color2_Copy30.Visibility = Visibility.Visible;
            color2_Copy31.Visibility = Visibility.Visible;
            color1_Copy12.Visibility = Visibility.Visible;
            color2_Copy32.Visibility = Visibility.Visible;
            color2_Copy33.Visibility = Visibility.Visible;
            color2_Copy34.Visibility = Visibility.Visible;
            color2_Copy35.Visibility = Visibility.Visible;
            color1_Copy13.Visibility = Visibility.Visible;
            color2_Copy36.Visibility = Visibility.Visible;
            color2_Copy37.Visibility = Visibility.Visible;
            color2_Copy38.Visibility = Visibility.Visible;
            color2_Copy39.Visibility = Visibility.Visible;
            color1_Copy14.Visibility = Visibility.Visible;
            color2_Copy40.Visibility = Visibility.Visible;
            color2_Copy41.Visibility = Visibility.Visible;
            color2_Copy42.Visibility = Visibility.Visible;
            color2_Copy43.Visibility = Visibility.Visible;
            color1_Copy15.Visibility = Visibility.Visible;
            color2_Copy44.Visibility = Visibility.Visible;
            color2_Copy45.Visibility = Visibility.Visible;
            lblVista2_Copy1.Visibility = Visibility.Visible;
            color_elegido.Visibility = Visibility.Visible;
            cmdQuitarColores.Visibility = Visibility.Visible;
            cmdGuardarTratamiento.Visibility = Visibility.Visible;
            lblVista2_Copy.Visibility = Visibility.Visible;
            listaSecciones.Visibility = Visibility.Visible;
            lblVista2_Copy2.Visibility = Visibility.Visible;
            listadoCodigosComb.Visibility = Visibility.Visible;
           // cmdRegresarTerapia.Visibility = Visibility.Visible;

            //Oculta panel anterior
            BusquedaTerapia.Visibility = Visibility.Hidden;
            lblBusqueda_Copy.Visibility = Visibility.Hidden;
            txtBuscarPaciente2_Copy.Visibility = Visibility.Hidden;
            cmdNuevoAnalisis1_Copy.Visibility = Visibility.Hidden;
            cmdEliminar_Copy.Visibility = Visibility.Hidden;
            Lista_Analisis_Group1_Copy.Visibility = Visibility.Hidden;
            ListaPacientes_Recientes1_Copy.Visibility = Visibility.Hidden;
            cmdReporteTerapia.Visibility = Visibility.Hidden;

        }

        //Funcion para mostrar la terapia de color
        void Ocultar_TerapiaColor()
        {
            groupBox3.Visibility = Visibility.Hidden;
            radioMasculino.Visibility = Visibility.Hidden;
            radioFemenino.Visibility = Visibility.Hidden;
            lblVista.Visibility = Visibility.Hidden;
            listVista.Visibility = Visibility.Hidden;
            groupBox3_Copy.Visibility = Visibility.Hidden;
            canvas.Visibility = Visibility.Hidden;
            color1.Visibility = Visibility.Hidden;
            color3.Visibility = Visibility.Hidden;
            color1_Copy1.Visibility = Visibility.Hidden;
            color1_Copy2.Visibility = Visibility.Hidden;
            color1_Copy3.Visibility = Visibility.Hidden;
            color2.Visibility = Visibility.Hidden;
            color1_Copy4.Visibility = Visibility.Hidden;
            color2_Copy1.Visibility = Visibility.Hidden;
            color2_Copy2.Visibility = Visibility.Hidden;
            color2_Copy3.Visibility = Visibility.Hidden;
            color2_Copy.Visibility = Visibility.Hidden;
            color1_Copy5.Visibility = Visibility.Hidden;
            color2_Copy4.Visibility = Visibility.Hidden;
            color2_Copy5.Visibility = Visibility.Hidden;
            color2_Copy6.Visibility = Visibility.Hidden;
            color2_Copy7.Visibility = Visibility.Hidden;
            color1_Copy6.Visibility = Visibility.Hidden;
            color2_Copy8.Visibility = Visibility.Hidden;
            color2_Copy9.Visibility = Visibility.Hidden;
            color2_Copy10.Visibility = Visibility.Hidden;
            color2_Copy11.Visibility = Visibility.Hidden;
            color1_Copy7.Visibility = Visibility.Hidden;
            color2_Copy12.Visibility = Visibility.Hidden;
            color2_Copy13.Visibility = Visibility.Hidden;
            color2_Copy14.Visibility = Visibility.Hidden;
            color2_Copy15.Visibility = Visibility.Hidden;
            color1_Copy8.Visibility = Visibility.Hidden;
            color2_Copy16.Visibility = Visibility.Hidden;
            color2_Copy17.Visibility = Visibility.Hidden;
            color2_Copy18.Visibility = Visibility.Hidden;
            color2_Copy19.Visibility = Visibility.Hidden;
            color1_Copy9.Visibility = Visibility.Hidden;
            color2_Copy20.Visibility = Visibility.Hidden;
            color2_Copy21.Visibility = Visibility.Hidden;
            color2_Copy22.Visibility = Visibility.Hidden;
            color2_Copy23.Visibility = Visibility.Hidden;
            color1_Copy10.Visibility = Visibility.Hidden;
            color2_Copy24.Visibility = Visibility.Hidden;
            color2_Copy25.Visibility = Visibility.Hidden;
            color2_Copy26.Visibility = Visibility.Hidden;
            color2_Copy27.Visibility = Visibility.Hidden;
            color1_Copy11.Visibility = Visibility.Hidden;
            color2_Copy28.Visibility = Visibility.Hidden;
            color2_Copy29.Visibility = Visibility.Hidden;
            color2_Copy30.Visibility = Visibility.Hidden;
            color2_Copy31.Visibility = Visibility.Hidden;
            color1_Copy12.Visibility = Visibility.Hidden;
            color2_Copy32.Visibility = Visibility.Hidden;
            color2_Copy33.Visibility = Visibility.Hidden;
            color2_Copy34.Visibility = Visibility.Hidden;
            color2_Copy35.Visibility = Visibility.Hidden;
            color1_Copy13.Visibility = Visibility.Hidden;
            color2_Copy36.Visibility = Visibility.Hidden;
            color2_Copy37.Visibility = Visibility.Hidden;
            color2_Copy38.Visibility = Visibility.Hidden;
            color2_Copy39.Visibility = Visibility.Hidden;
            color1_Copy14.Visibility = Visibility.Hidden;
            color2_Copy40.Visibility = Visibility.Hidden;
            color2_Copy41.Visibility = Visibility.Hidden;
            color2_Copy42.Visibility = Visibility.Hidden;
            color2_Copy43.Visibility = Visibility.Hidden;
            color1_Copy15.Visibility = Visibility.Hidden;
            color2_Copy44.Visibility = Visibility.Hidden;
            color2_Copy45.Visibility = Visibility.Hidden;
            lblVista2_Copy1.Visibility = Visibility.Hidden;
            color_elegido.Visibility = Visibility.Hidden;
            cmdQuitarColores.Visibility = Visibility.Hidden;
            cmdGuardarTratamiento.Visibility = Visibility.Hidden;
            lblVista2_Copy.Visibility = Visibility.Hidden;
            listaSecciones.Visibility = Visibility.Hidden;
            lblVista2_Copy2.Visibility = Visibility.Hidden;
            listadoCodigosComb.Visibility = Visibility.Hidden;
            cmdRegresarTerapia.Visibility = Visibility.Hidden;

            //Muestra panel anterior
            BusquedaTerapia.Visibility = Visibility.Visible;
            lblBusqueda_Copy.Visibility = Visibility.Visible;
            txtBuscarPaciente2_Copy.Visibility = Visibility.Visible;
            cmdNuevoAnalisis1_Copy.Visibility = Visibility.Visible;
            cmdEliminar_Copy.Visibility = Visibility.Visible;
            Lista_Analisis_Group1_Copy.Visibility = Visibility.Visible;
            ListaPacientes_Recientes1_Copy.Visibility = Visibility.Visible;
            cmdReporteTerapia.Visibility = Visibility.Visible;
        }

        private void cmdNuevoAnalisis1_Copy_Click(object sender, RoutedEventArgs e)
        {
            Mostrar_TerapiaColor();
        }

        //PENDIENTE TERAPIA DE COLOR BUSQUEDA Y REPORTE DE TERAPIA DE COLOR
        private void txtBuscarPaciente2_Copy_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (txtBuscarPaciente2_Copy.Text != "")
            {
                HacerConexion();

                //Limpia los elementos antes de continuar
                ListaPacientes_Recientes1_Copy.Items.Clear();

                //Llama y obtiene posibles matches
                DataTable TerapiaBuscada = new DataTable();
                TerapiaBuscada = obj2.Buscar_Terapia(txtBuscarPaciente2_Copy.Text);
                //ListaPacientes_Recientes1_Copy.ItemsSource = TerapiaBuscada.DefaultView;

                //Las metemos al listview
                for (int i = 0; i <= TerapiaBuscada.Rows.Count - 1; i++)
                {
                    ListaPacientes_Recientes1_Copy.Items.Add(new nuevaTerapia { nombre = TerapiaBuscada.Rows[i][0].ToString(), fecha = TerapiaBuscada.Rows[i][1].ToString() });
                }

                CerrarConexion();
            }
            else
            {
                if (txtBuscarPaciente2_Copy.Text == "")
                {
                    CargarTerapiasRecientes(); //
                }
                else
                {
                    // MessageBox.Show("Introduzca el nombre del paciente a buscar", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    txtBuscarPaciente2_Copy.Focus();
                }
            }
        }

        //Funcion que elimina la terapia de color elegida
        private void cmdEliminar_Copy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRowView terapia_seleccionada = (DataRowView)ListaPacientes_Recientes1_Copy.SelectedItem;

                //Abrir conexion
                HacerConexion();

                //Obtener Id Remedio es para las terapias tambien
                object id_terapia=obj2.Obtener_IdRemedio_ConFecha(terapia_seleccionada[0].ToString(), terapia_seleccionada[1].ToString());

                //Eliminar codigos de colores (Codigosderemedios)
                obj2.Eliminar_remedio_Id(id_terapia.ToString());

                CerrarConexion();

                //Carga terapias recientes
                CargarTerapiasRecientes();
                CargarListadoCompletoPacientes();
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(obtenerRecurso("messageError2"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmdRegresarTerapia_Click(object sender, RoutedEventArgs e)
        {
            Ocultar_TerapiaColor();

            Reset_images();
            listadoCodigosComb.Items.Clear();

            //Limpiar listas para evitar mezclar con antiguas terapias
            partes_colores.Clear();
            cod_partes_color.Clear();
            cod_partes_colores.Clear();
        }

        //Funcion que genera el reporte de la terapia
        private void cmdReporteTerapia_Click(object sender, RoutedEventArgs e)
        {
            //Terapia valores
            string nombret = "";
            string fechat = "";

            //Comprobamos si la version esta registrada
            HacerConexion();

            DataTable Version = obj2.Consultar_Version();
            //Campos para los datos de registro de la version
            string nombre = "";
            string descripcion = "";

            //Recorremos valores
            for (int i = 0; i <= Version.Rows.Count - 1; i++)
            {
                nombre = Version.Rows[i][1].ToString();
                descripcion = Version.Rows[i][2].ToString();
            }

            //En caso de que no este registrada
            if (Version.Rows.Count == 0)
            {
                nombre = "<REGISTRE VERSION PARA PERSONALIZAR>";
                descripcion = "<REGISTRE VERSION PARA PERSONALIZAR>";
            }

            //Terapia seleccionada
            IEnumerable items = this.ListaPacientes_Recientes1_Copy.SelectedItems;

            foreach (nuevaTerapia terapia in items)
            {
                nombret = terapia.nombre;
                fechat = terapia.fecha;
            }

            //Obtener Id Remedio es para las terapias tambien
            object id_terapia = obj2.Obtener_IdRemedio_ConFecha(nombret, fechat);

            //MessageBox.Show(id_terapia.ToString());

            List<string> codcomb = new List<string>();

            DataTable TablaCodigoCombinados = obj2.VisualizarCodigosCombinados(id_terapia.ToString());

            //Pasamos a listas
            for(int k = 0; k <= TablaCodigoCombinados.Rows.Count - 1; k++)
            {
                codcomb.Add(TablaCodigoCombinados.Rows[k][0].ToString() + " - " + TablaCodigoCombinados.Rows[k][1].ToString());
            }

            CerrarConexion();
            CargarListadoCompletoPacientes();


            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "PDF Document|*.pdf";
            saveFileDialog1.Title = "Guardar - Reporte de la Terapia";
            saveFileDialog1.ShowDialog();

            //Si se eligio ruta haz...
            if (saveFileDialog1.FileName != "")
            {
                Document reporte = new Document(iTextSharp.text.PageSize.LETTER, 40, 40, 80, 80);
                PdfWriter buffer = PdfWriter.GetInstance(reporte, new FileStream(saveFileDialog1.FileName.ToString(), FileMode.Create));
                //Mandamos datos del registro de version
                buffer.PageEvent = new HS5.reporte_ext("Reporte de la Terapia", nombre, descripcion); //Agrega el encabezado y pie de pagina

                reporte.Open();
                // reporte.AddTitle("Expediente del Paciente - HS5");
                //reporte.AddCreator("Homoeonic Software 5");
                // reporte.AddAuthor("HS5");

                iTextSharp.text.Font titulos = iTextSharp.text.FontFactory.GetFont("HELVETICA", 14, iTextSharp.text.Font.BOLD);
                iTextSharp.text.Font subtitulos = iTextSharp.text.FontFactory.GetFont("HELVETICA", 12, iTextSharp.text.Font.BOLD);
                iTextSharp.text.Font texto = iTextSharp.text.FontFactory.GetFont("HELVETICA", 10, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Font texto2 = iTextSharp.text.FontFactory.GetFont("HELVETICA", 10, iTextSharp.text.Font.BOLD);

                iTextSharp.text.Font LineBreak = iTextSharp.text.FontFactory.GetFont("HELVETICA", 10, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Paragraph linebreak = new iTextSharp.text.Paragraph("\n", LineBreak);

                //Documento titulo
                // iTextSharp.text.Paragraph parrafo = new iTextSharp.text.Paragraph("Expediente del Paciente", titulos);
                /// reporte.Add(parrafo);
                /// 
                reporte.Add(linebreak);
                reporte.Add(linebreak);

                //FONDO DEL DOCUMENTO
                string path = RutaInstalacion() + "//fotos//portada_hoja.png";

                //Fondo del documento
                iTextSharp.text.Image fondodoc = iTextSharp.text.Image.GetInstance(path);
                fondodoc.ScaleToFit(reporte.PageSize);
                fondodoc.Alignment = iTextSharp.text.Image.UNDERLYING;
                fondodoc.SetAbsolutePosition(0, 0);
                reporte.Add(fondodoc);

                iTextSharp.text.Paragraph parrafo2 = new iTextSharp.text.Paragraph("Datos de la Terapia", subtitulos);
                reporte.Add(parrafo2);
                reporte.Add(linebreak);

                //CODIGOS COMBINADOS (LISTADO) 
                if (codcomb.Count != 0)
                {
                    PdfPTable table_codigoscomb = new PdfPTable(1);
                    table_codigoscomb.TotalWidth = 144;
                    //table_codigos.AddCell(new Phrase("Código",texto2));
                    table_codigoscomb.AddCell(new Phrase("Códigos Combinados", texto2));

                    table_codigoscomb.HeaderRows = 1;

                    for (int i = 0; i <= codcomb.Count - 1; i++)
                    {
                        table_codigoscomb.AddCell(new Phrase(codcomb[i].ToString(), texto));

                    }
                    reporte.Add(table_codigoscomb);
                }
                else
                {
                    iTextSharp.text.Paragraph parrafoan = new iTextSharp.text.Paragraph("NO HAY CODIGOS COMBINADOS DE LA TERAPIA", texto2);
                    reporte.Add(parrafoan);
                }

                reporte.Close();

            }
        }

        private void cmdCerrarRemedio_Click(object sender, RoutedEventArgs e)
        {
            cmdCerrarRemedio.Visibility = Visibility.Hidden;
            ControlRemedios.Visibility = Visibility.Hidden;
            lblNombreRemedioResp.Visibility = Visibility.Hidden;
            lblFechaRemedioResp.Visibility = Visibility.Hidden;
            lblFechaRemedio.Visibility = Visibility.Hidden;
            lblNombreRemedio.Visibility = Visibility.Hidden;
            cmdDuplicarRemedio.Visibility = Visibility.Hidden;
        }

        private void cmdEscanearGlob_Click(object sender, RoutedEventArgs e)
        {
            HacerConexion();

            DataTable tabl1 = obj2.BDCodigosconFilas();

            Random rnd = new Random();
            int nofilas = rnd.Next(1, 250);

            string[] lista_detecciones = new string[nofilas];
            Random rnd2 = new Random();

            for (int i = 0; i <= lista_detecciones.Length - 1; i++)
            {
              //  Random rnd2 = new Random();
                int nofilas2 = rnd2.Next(1, 8500);
                lista_detecciones[i] = tabl1.Rows[nofilas2][0].ToString();
            }

            loaderBack.Visibility = Visibility.Visible;
            lblProgresRemedy.Text = "ESCANEANDO...";
            lblProgresRemedy.Visibility = Visibility.Visible;

            new Thread((ThreadStart)delegate
            {
                obj.Imprint();
                Thread.Sleep(15000); //Tiempo

                Dispatcher.Invoke((ThreadStart)delegate
                {
                   
                    loaderBack.Visibility = Visibility.Hidden;
                    lblProgresRemedy.Visibility = Visibility.Hidden;

                    for (int i = 0; i <= lista_detecciones.Length - 1; i++)
                    {
                        listaDetecciones.Items.Add(lista_detecciones[i]);
                    }

                });

            }).Start();

            CerrarConexion();
        }

        private void listadoRemedios_Initialized(object sender, EventArgs e)
        {
            if (Settings.Default.Lenguaje.ToString() == "es-MX")
            {
                Database.db = "rad_es";
            }
            else if (Settings.Default.Lenguaje.ToString() == "en-US")
            {
                Database.db = "rad_en";
            }
            CargarListadoRemedios();
        }

        private void ListaPacientes_Recientes1_Initialized(object sender, EventArgs e)
        {
            CargarRegistrosPacientesRecientes();
        }

        private void ListadoDiagActivos_Initialized(object sender, EventArgs e)
        {
                    }

        private void ListaPacientes_Recientes1_Copy_Initialized(object sender, EventArgs e)
        {
            CargarTerapiasRecientes();
        }

        private void opcionesHomoeonic_Initialized(object sender, EventArgs e)
        {
            
        }

        private void ListaPacientes_Initialized(object sender, EventArgs e)
        {
          //  CargarListadoCompletoPacientes(); //Carga el listado del paciente
        }

        private void ListadoDiagActivos_GotFocus(object sender, RoutedEventArgs e)
        {

        }

        private void ListaPacientes_Loaded(object sender, RoutedEventArgs e)
        {
           // CargarListadoCompletoPacientes(); //Carga el listado del paciente

        }

        private void cmdDuplicarRemedio_Copy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Antes de cambiar salvar el contenido - SALVAMOS
                HacerConexion();

                //Borramos los resultados de un analisis en caso de venir de ahi
                obj2.Eliminar_CodigosAnalisis(verificacion_remedio_de_analisis.ToString());

                string nombre_s = listadoRemedios.SelectedItem.ToString(); //Nombre seleccionado

                object id_remedio_s = obj2.Obtener_IdRemedio(nombre_s); //Id remedio
                //MessageBox.Show(id_remedio_s.ToString());

                //Obtener los elementos
                IEnumerable itemsCodigos = this.ListaRemedios.Items;

                //Listas
                List<string> codigos_ord = new List<string>();
                List<string> nombres_ord = new List<string>();
                List<string> potencia_ord = new List<string>();
                List<string> metodo_ord = new List<string>();
                List<string> complementario_ord = new List<string>();
                List<string> nivel_ord = new List<string>();

                //De objetos los pasamos a listas
                foreach (nuevoRemedio codigo in itemsCodigos)
                {
                    //Guardamos en listas todos los remedios de las listas
                    codigos_ord.Add(codigo.codigo);
                    nombres_ord.Add(codigo.nombrecodigo);
                    potencia_ord.Add(codigo.potencia);
                    metodo_ord.Add(codigo.metodo);
                    complementario_ord.Add(codigo.codigocomplementario);
                    nivel_ord.Add(codigo.nivel);
                }

                //Antes borramos todo lo de un remedio anterior para evitar duplicados
                obj2.Eliminar_codigos_remedio(id_remedio_s.ToString());

                Random rdm = new Random();
                //Copiamos los nuevos en la bd
                for (int i = 0; i <= codigos_ord.Count - 1; i++)
                {
                    //Pasarlos a la bd los codigos del remedio
                    Radionica obj_1 = new Radionica();
                    string id_generado = rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() +
                         rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() +
                          rdm.Next(0, 9).ToString() + "-" + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() +
                         rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() +
                          rdm.Next(0, 9).ToString();

                    //Busca id de un rate en especifico dentro de un remedio
                    object id_codigo_buscado = obj2.Buscar_IdCodigo_Codigo(codigos_ord[i]);

                    // PENDIENTE APLICAR LO DE TERAPIA DE COLOR

                    if (id_codigo_buscado == null)
                    {
                        obj2.Registrar_CodigosdeRemedios(id_generado + "-CR", id_remedio_s.ToString(), codigos_ord[i], complementario_ord[i], nombres_ord[i], "123", potencia_ord[i], metodo_ord[i], nivel_ord[i]);

                    }
                    else
                    {
                        obj2.Registrar_CodigosdeRemedios(id_generado + "-CR", id_remedio_s.ToString(), codigos_ord[i], complementario_ord[i], nombres_ord[i], id_codigo_buscado.ToString(), potencia_ord[i], metodo_ord[i], nivel_ord[i]);
                    }

                    id_generado = obj_1.Generar_Id();
                }
                CerrarConexion();
            }
            catch(Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
        }

        private void cmdNuevoCod_Copy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string nombre_codigo;
                string codigo_num; // string description;
                string inpMB1 = obtenerRecurso("inputMessBox1");
                string inpMH1 = obtenerRecurso("inputMessHead1");


                try
                {
                    nombre_codigo = Interaction.InputBox(inpMB1, inpMH1, "", 300, 300);
                }
                catch (NullReferenceException ex)
                {
                    MessageBox.Show("Error al mostrar el cuadro de diálogo de entrada. Detalles del error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return; // Salir del método o manejar el flujo de ejecución según sea necesario
                }

                if (nombre_codigo != "")
                {
                    Radionica obj_new = new Radionica();
                    Random numnum = new Random();

                    codigo_num = obj_new.RandomDigits(numnum.Next(16, 22)); //Obtiene valor numerico

                    try
                    {
                        HacerConexion();
                        //Cambios en el genero_para_codigo a fin de que meta el codigo en la categoria y subcategoria deseada..
                        obj.Diagnostic();

                        if (listadoCategorias_Copy.SelectedItem != null && listadoSubcategorias_Copy.SelectedItem != null)
                        {
                            //Categoria padre
                            string id_cat_pad = obj2.Obtener_IDCategoria(listadoCategorias_Copy.SelectedItem.ToString()).ToString();

                            //Subcategoria
                            string id_subcat = obj2.Obtener_IDCategoria(listadoSubcategorias_Copy.SelectedItem.ToString()).ToString();

                            //object genero_para_codigo = obj2.Buscar_Genero(id_subcat, id_cat_pad);
                            string genero_para_codigo = "T";

                            obj2.Registrar_Codigo_Categorias(obj_new.Generar_Id(), nombre_codigo, obj2.Generarcodigo(), "Obtenida", id_subcat, id_cat_pad, genero_para_codigo);

                            Cargar_Codigos(id_categoria_padre, id_categoria_cop); //Carga los codigos actualizados con el agregado
                        }
                        else
                        {
                            MessageBox.Show(obtenerRecurso("messageWarning2"), obtenerRecurso("mesageHeadWarning"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        }

                        lblCodigosCont.Content = listadoCodigos_Copy.Items.Count + " " + obtenerRecurso("labelRate");
                        CerrarConexion();
                        obj.Diagnostic();
                    }
                    catch (FormatException)
                    {
                        MessageBox.Show(obtenerRecurso("messageError"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show(obtenerRecurso("messageError1"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void radioMasculino_Click(object sender, RoutedEventArgs e)
        {
            listVista.Focus();
            Partes();
        }

        private void radioFemenino_Click(object sender, RoutedEventArgs e)
        {
            listVista.Focus();
            Partes();

        }

        private void opcionesHomoeonic_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void cmdBuscar_Click_1(object sender, RoutedEventArgs e)
        {
            ClearData(listadoCodigos_Copy);
            listadoCategorias_Copy.SelectedIndex = -1;
            listadoSubcategorias_Copy.SelectedIndex = -1;
            listadoCodigos_Copy.SelectedIndex = -1;

            if (txtBuscarBase.Text != "")
            {
                //Busqueda on
                busqueda = true;

                listadoCodigos_Copy.Items.Clear();

                HacerConexion();

                DataTable CodigosCat = obj2.BuscarCategoriaCodigo(txtBuscarBase.Text);
                // Crear un nuevo DataTable
                DataTable dtc = new DataTable();
                dtc.Columns.Add("Id", typeof(string));
                dtc.Columns.Add("Nombre", typeof(string));
                dtc.Columns.Add("Categoria", typeof(string));
                dtc.Columns.Add("SubCategoria", typeof(string));

                for (int y = 0; y < CodigosCat.Rows.Count; y++)
                {
                    if (!string.IsNullOrEmpty(CodigosCat.Rows[y][0].ToString()))
                    {
                        string columna1 = CodigosCat.Rows[y][0].ToString();
                        string columna2 = CodigosCat.Rows[y][1].ToString();
                        string columna3 = CodigosCat.Rows[y][2].ToString();
                        string columna4 = CodigosCat.Rows[y][3].ToString();
                        dtc.Rows.Add(columna1, columna2, columna3, columna4);
                    }
                }

                // Establecer el DataTable como origen de datos para la ListView
                listadoCodigos_Copy.ItemsSource = dtc.DefaultView;
                lblCodigosCont.Content = listadoCodigos_Copy.Items.Count + " " + obtenerRecurso("labelRate");

                CerrarConexion();
            }
        }

        private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
           // obj.BroadcastOFF();
        }

        private void window_Closed(object sender, EventArgs e)
        {
           // obj.BroadcastOFF();

        }

        private void ListadoDiagNoActiv_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        //this function is used to change the language in the desktop applicacion 
        private void ChangeLanguage(object sender, SelectionChangedEventArgs e)
        {
            if(languageOption.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedLanguage = selectedItem.Content.ToString();
               
                if (selectedLanguage == "Español" || selectedLanguage == "Spanish")
                {
                    ChoseLanguage("es-MX");
                    Database.db = "rad_es";
                }
                else if (selectedLanguage == "Ingles" || selectedLanguage == "English")
                {
                    ChoseLanguage("en-US");
                    Database.db = "rad_en";
                }

            }
        }

        //this function is used to save the language in the desktop
        private void ChoseLanguage (string value)
        {
            Console.WriteLine(Settings.Default.Lenguaje.ToString());
            Settings.Default.Lenguaje = value;
            Settings.Default.Save();

            //var confirmacion = MessageBox.Show(obtenerRecurso("Message13"), obtenerRecurso("Message14"), MessageBoxButton.YesNo, MessageBoxImage.Question);
            var confirmacion = MessageBox.Show(obtenerRecurso("messageWarning"), obtenerRecurso("messageHeadWarning"), MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirmacion == MessageBoxResult.Yes)
            {
                // Iniciar una nueva instancia de la aplicación
                System.Diagnostics.Process.Start(System.AppDomain.CurrentDomain.FriendlyName);
                // Cerrar la instancia actual de la aplicación
                System.Windows.Application.Current.Shutdown();
            }
        }

        private void TabItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TabItem tabItem)
            {
                tabItem.Background = (SolidColorBrush)tabItem.FindResource("PressedBackgroundBrush");
            }
        }

        private void Remedy1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ClearDataLb(Remedy2);
            Remedy2.SelectedIndex = -1;
            HacerConexion();
            string nombreR = Remedy1.SelectedItem.ToString();
            string idr = obj2.ObtenerIdr(nombreR);


            if (Remedy1.SelectedItem != null)
            {
                // Busqueda activada
                Remedy2.Items.Clear();

                

                DataTable Codigos = obj2.BuscarCodigoRem1(idr);

                for (int y = 0; y < Codigos.Rows.Count; y++)
                {
                    if (!string.IsNullOrEmpty(Codigos.Rows[y][0].ToString()))
                    {
                        string id = Codigos.Rows[y][0].ToString();
                        string nombre = Codigos.Rows[y][1].ToString();

                        // Crear un nuevo objeto ListBoxItem con la concatenación de id y nombre
                        ListBoxItem item = new ListBoxItem();
                        item.Content = id + ", " + nombre;

                        // Agregar el ListBoxItem a la ListBox
                        Remedy2.Items.Add(item);
                    }
                }

                lblCodigosCont.Content = Remedy2.Items.Count + " " + obtenerRecurso("labelRate");
                CerrarConexion();
            }
        
        }

        private void ListaPacientes_Recientes1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }


    }
}
 