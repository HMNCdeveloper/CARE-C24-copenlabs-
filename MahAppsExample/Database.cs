using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using Npgsql; //BD 
using System.Drawing; //Imagenes
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Controls;
using System.Windows;
using HS5;
using System.Diagnostics;
using System.Windows.Input;
using HS5.Resources.Idiomas;
using System.Windows.Media;
using System.Runtime.Serialization;

namespace MahAppsExample
{
    public class Database
    {
        //Globals
        NpgsqlConnection conn; //Conexion
        NpgsqlCommand command;

        //Elementos para la consulta de base
        private DataSet ds = new DataSet();
        private DataTable dt = new DataTable();

        private DataSet ds2 = new DataSet();
        private DataTable dt2 = new DataTable();

        private DataSet ds3 = new DataSet();
        private DataTable dt3 = new DataTable();

        private string constring; //Cadena de la conexion
        string sql = ""; //Cadena para cada consulta

        //Funcion de comprobar cuantos domicilios se introdujeron
        public bool ComprobarDomicilios(string Calle, string Colonia, string Numero, string CP, string Municipio, string Estado, string Countries)
        {
            bool domflag;

            if (Calle == "" && Colonia == "" && Numero == "" && CP == "" && Municipio == "" && Estado == "" && Countries == "")
            {
                domflag = true;
            }
            else
            {
                domflag = false;
            }

            return domflag;
        }




        //Funcion para registrar el paciente y devuelve su Id_paciente
        public object RegistrarPacienteD(string nombre, string apellido1, string apellido2, string email, string sexo, string profesion, string titulo, string fechanac, string fpg)
        {
            //Insercion datos del paciente
            sql = "INSERT INTO rad_pacientes(nombre,apellido1,apellido2,email,sexo,profesion,titulo,fechanacimiento,fpg) VALUES($$" + nombre + "$$,$$" + apellido1 + "$$,$$" + apellido2 + "$$,$$" + email + "$$,$$" + sexo + "$$,$$" + profesion + "$$,$$" + titulo + "$$,$$" + fechanac + "$$,$$" + fpg + "$$)";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery(); //Sin retorno de resp de la consulta

            //Devuelve Id_paciente
            return Obtener_IdPaciente(nombre, apellido1, apellido2); //Id_paciente
        }

        //Funcion para obtener id del paciente
        public object Obtener_IdPaciente(string nombre, string apellidopat, string apellidomat)
        {
            sql = "SELECT idp from rad_pacientes where nombre=$$" + nombre + "$$ and apellido1=$$" + apellidopat + "$$ and apellido2=$$" + apellidomat + "$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar(); //Valor del id_paciente lo regresa como objeto
        }

        //Funcion para obtener id del telefono del paciente
        public object Obtener_IdTelefono_Paciente(string telefono, string extension)
        {
            sql = "SELECT idtel from rad_telefonos where numero=$$" + telefono + "$$ and extension=$$" + extension + "$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar(); //Valor del id_paciente lo regresa como objeto
        }

        //Funcion para obtener id del antecedente del paciente
        public object Obtener_IdAntecedente_Paciente(string titulo, string descripcion, string tipo)
        {
            sql = "SELECT ida from rad_antecedentes where titulo=$$" + titulo + "$$ and texto=$$" + descripcion + "$$ and tipo=$$" + tipo + "$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar(); //Valor del id_paciente lo regresa como objeto
        }



        //Funcion para obtener los analisis recientemente de acuerdo a nombre del paciente
        public DataTable Obtener_Analisis_Pacientes_Recientes_PorNombrePaciente(string paciente_nombre)
        {
            sql = "SELECT * FROM ObtenerAnalisisPorPaciente($$" + paciente_nombre + "$$)";
            Console.WriteLine(sql);
            NpgsqlDataAdapter da2 = new NpgsqlDataAdapter(sql, conn);
            ds2.Reset();
            da2.Fill(ds2);
            dt2 = ds2.Tables[0];
            return dt2; //regresa tabla con datos del paciente
        }

        //Funcion para obtener los analisis recientemente de acuerdo a nombre del paciente
        public DataTable Obtener_Analisis_Pacientes_Recientes_PorNombrePaciente2(string paciente_nombre, string nombre_analisis)
        {
            sql = "SELECT * FROM(SELECT rad_analisis.nombre, rad_analisis.fecha, CONCAT(rad_pacientes.nombre, ' ', rad_pacientes.apellido1, ' ', rad_pacientes.apellido2) as nombrepaciente from rad_analisis INNER JOIN rad_pacientes ON(rad_analisis.paciente = rad_pacientes.idp)) as tabla where UPPER(nombre) like '%" + nombre_analisis + "%' and nombrepaciente = $$" + paciente_nombre + "$$";
            NpgsqlDataAdapter da2 = new NpgsqlDataAdapter(sql, conn);
            ds2.Reset();
            da2.Fill(ds2);
            dt2 = ds2.Tables[0];
            return dt2; //regresa tabla con datos del paciente
        }






        //Funcion para obtener id del domicilio del paciente
        public object Obtener_IdDomicilio_Paciente(string calle, string numero, string colonia, string CP, string municipio, string estado, string pais, string id_paciente)
        {
            //txtCalle.Text, txtNum.Text, txtColonia.Text, 
            //txtCP.Text, txtMunicipio.Text, txtEstado.Text, comboCountries.Text, id_paciente.ToString()
            sql = "SELECT iddom from rad_domicilios where calle=$$" + calle + "$$ and numero=$$" + numero + "$$ and colonia=$$" + colonia + "$$ and cp=$$" + CP + "$$ and municipio=$$" + municipio + "$$ and estado=$$" + estado + "$$ and pais=$$" + pais + "$$ and idpaciente=$$" + id_paciente + "$$";
            Console.WriteLine(sql);
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar(); //Valor del id_paciente lo regresa como objeto
        }


        //Funcion para validar paciente
        public object Validar_IdPaciente(string nombre, string apellidopat, string apellidomat)
        {
            sql = "SELECT count(*) from rad_pacientes where nombre=$$" + nombre + "$$ and apellido1=$$" + apellidopat + "$$ and apellido2=$$" + apellidomat + "$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar(); //Valor del id_paciente lo regresa como objeto
        }



        //Funcion para validar analisis
        public object Validar_Analisis(string paciente, string nombre_analisis)
        {
            sql = "SELECT count(*) from rad_analisis WHERE idpaciente=$$" + paciente + "$$ and nombre=$$" + nombre_analisis + "$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar(); //Valor del id_paciente lo regresa como objeto
        }

        //Funcion para contar los codigos de analisis en la tabla de rad_codigosdeanalisis
        public object Obtener_Codigos_Cantidad_Analisis(string id_analisis)
        {
            sql = "SELECT count(*) from rad_codigosdeanalisis WHERE analisis=$$" + id_analisis + "$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar(); //Valor del id_paciente lo regresa como objeto
        }


        //Funcion para obtener ida del analisis para obtener sus codigos de analisis en base a id analisis
        public object Obtener_Id_Analisis(string nombre)
        {
            sql = "select id from rad_analisis where nombre=$$" + nombre + "$$";
            Console.WriteLine(sql);
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar();
        }

        //Funcion para obtener codigos del analisis
        public DataTable Obtener_CodigosAnalisis(int id_analisis)
        {
            sql = "select * from ObtenerCodigosdeAnalsis($$" + id_analisis + "$$,$$" + Database.table + "$$)";
            //command = new NpgsqlCommand(sql, conn);
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt; //regresa tabla con datos del paciente
        }

        //Funcion que busca si existe el paciente
        public DataTable Buscar_Paciente(string paciente)
        {
            //sql = "SELECT idp, CONCAT(nombre,' ', apellido1, ' ', apellido2) as nombrepaciente from rad_pacientes where UPPER((nombre || ' ' || apellido1 || ' ' || apellido2)) like $$%" + paciente + "%$$ order by nombre";
            sql = "SELECT idp, CONCAT(nombre, ' ', apellido1, ' ', apellido2) AS nombrepaciente FROM rad_pacientes WHERE UPPER(CONCAT(nombre, ' ', apellido1, ' ', apellido2)) LIKE '%" + paciente + "%'ORDER BY nombre";
            //command = new NpgsqlCommand(sql, conn);
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt; //regresa tabla con datos del paciente
        }

        //Funcion para modificar un paciente
        public DataTable Modificar_Paciente(string paciente)
        {
            sql = "SELECT * from rad_pacientes WHERE idp=$$" + paciente + "$$";
            //command = new NpgsqlCommand(sql, conn);
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt; //regresa tabla con datos del paciente
        }

        //Funcion que despliega los nombres de los pacientes
        public DataTable Mostrar_Pacientes_Listado_Sencillo()
        {
            sql = "select nombre,apellido1,apellido2 from rad_pacientes order by nombre";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt; //regresa tabla con datos del paciente
        }

        //Funcion que despliega los nombres de los pacientes
        public DataTable Mostrar_Pacientes_Listado_Sencillo_2(string nombre_paciente)
        {
            sql = "select concat(nombre,' ',apellido1,' ',apellido2) as nombre from rad_pacientes where UPPER(nombre) like $$%" + nombre_paciente + "%$$";
            //command = new NpgsqlCommand(sql, conn);
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt; //regresa tabla con datos del paciente
        }

        //Funcion para modificar paciente telefonos
        public DataTable Modificar_PacienteTelefonos(string paciente)
        {
            sql = "SELECT numero,extension from rad_telefonos WHERE idobj=$$" + paciente + "$$";
            //command = new NpgsqlCommand(sql, conn);
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt; //regresa tabla con datos del paciente
        }

        //Funcion para modificar paciente domicilios
        public DataTable ListadoPacienteDomicilios(string paciente)
        {
            sql = "SELECT calle,numero,colonia,cp,municipio,estado,pais,idpaciente FROM rad_domicilios WHERE idpaciente=$$" + paciente + "$$ ORDER BY calle";
            //command = new NpgsqlCommand(sql, conn);
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt; //regresa tabla con datos del paciente
        }

        //Funcion para buscar un analisis del paciente
        public DataTable BuscarAnalisisPaciente(string paciente, string nombre_analisis)
        {
            sql = "SELECT idpaciente,ida,nombre,fecha from rad_analisis WHERE idpaciente=$$" + paciente + "$$ and nombre=$$" + nombre_analisis + "$$";
            //command = new NpgsqlCommand(sql, conn);
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt; //regresa tabla con datos del paciente
        }

        //Funcion para desplegar detalles de un analisis del paciente buscado
        public DataTable HistorialAnalisisPacienteBuscado(string id_analisis)
        {
            sql = "SELECT nombrecodigo,nivel,valor,nivelsugerido,ida from rad_codigosdeanalisis WHERE ida=$$" + id_analisis + "$$";
            //command = new NpgsqlCommand(sql, conn);
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt; //regresa tabla con datos del paciente
        }

        //Funcion para traer historial de analisis del paciente
        public DataTable HistorialAnalisisPacienteCompleto(string paciente)
        {
            sql = "SELECT paciente,id,nombre,fecha from rad_analisis WHERE paciente=$$" + paciente + "$$";
            //command = new NpgsqlCommand(sql, conn);
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt; //regresa tabla con datos del paciente
        }

        //Funcion para modificar paciente antecedentes
        public DataTable Modificar_PacienteAntecedentes(string paciente)
        {
            sql = "SELECT titulo,texto,tipo from rad_antecedentes WHERE idpaciente=$$" + paciente + "$$";
            //command = new NpgsqlCommand(sql, conn);
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt; //regresa tabla con datos del paciente
        }

        //Funcion para mostrar todo el listado de pacientes
        public DataTable Listado_Pacientes()
        {
            sql = "SELECT idp,fpg, CONCAT(nombre,' ', apellido1, ' ', apellido2) as nombrepaciente from rad_pacientes order by nombre";
            //command = new NpgsqlCommand(sql, conn);
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds3.Reset();
            da.Fill(ds3);
            dt3 = ds3.Tables[0];
            return dt3; //Valor del id_paciente lo regresa como objeto
        }

        //Funcion para registrar historial de analisis del paciente
        public void RegistrarAnalisisPaciente_Historial(string id_paciente, string nombre, string fecha, string tipoanalisis)
        {
            sql = "INSERT INTO rad_analisis(idpaciente,nombre,fecha,tipoanalisis,fechac) VALUES($$" + id_paciente + "$$,$$" + nombre + "$$,$$" + fecha + "$$,$$" + tipoanalisis + "$$,$$" + fecha + "$$)";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        //Funcion para actualizar el tiempo emitido mientras corre el tiempo restante
        public void Registar_TiempoEmitido(int tiempoemitido, int idTratamiento)
        {
            sql = "UPDATE rad_tratamientosadistancia SET tiempoemitido=@tiempoemitido WHERE idt=@idTratamiento";
            command = new NpgsqlCommand(sql, conn);

            command.Parameters.AddWithValue("@tiempoemitido", tiempoemitido);
            command.Parameters.AddWithValue("@idTratamiento", idTratamiento);

            command.ExecuteNonQuery();
        }

        //Funcion para buscar paciente de acuerdo a su nombre completo
        public DataTable Buscar_IdPaciente_Nombre(string nombre)
        {
            sql = "Select idp from(SELECT idp, fpg, CONCAT(nombre, ' ', apellido1, ' ', apellido2) as nombrepaciente from rad_pacientes) as Tabla where nombrepaciente like $$%" + nombre + "%$$";
            //command = new NpgsqlCommand(sql, conn);
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt; //Valor del id_paciente lo regresa como objeto
        }

        //Funcion para cargar el listado de tratamientos activos
        public DataTable Tratamientos_Activos()
        {
            sql = "select * from rad_tratamientosadistancia where estado=1 and duracion != tiempoemitido  ORDER BY fechainicio DESC";
            //command = new NpgsqlCommand(sql, conn);
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt; //Valor del id_paciente lo regresa como objeto
        }

        public void EliminarTratamientosVencidos()
        {
            sql = "DELETE FROM rad_codigosdetratamientos USING rad_tratamientosadistancia WHERE rad_codigosdetratamientos.idcr = CAST(rad_tratamientosadistancia.idt AS text)  AND rad_tratamientosadistancia.duracion = rad_tratamientosadistancia.tiempoemitido   AND rad_tratamientosadistancia.estado = 1 AND (SELECT COUNT(*) FROM rad_tratamientosadistancia t WHERE t.idt=rad_tratamientosadistancia.idt)=1;";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();

            command.CommandText = "DELETE FROM rad_tratamientosadistancia WHERE tiempoemitido=duracion and estado=1";
            command.ExecuteNonQuery();
        }

        public List<string> listaTratimentos_Nombres()
        {
            string sql = "SELECT DISTINCT(nombre) FROM rad_tratamientosadistancia WHERE estado IN (0,1)";

            // Usamos una lista para almacenar los resultados directamente
            List<string> nombres = new List<string>();            
           using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
           using (NpgsqlDataReader reader = cmd.ExecuteReader())
           {
             

               while (reader.Read())
               {
                  nombres.Add(reader.GetString(0)); // Obtiene el valor de la primera columna
                  
               }
           }
            

            return nombres;

        }

      
        //Funcion para cargar el listado de tratamientos en espera - sin censura de un solo elemento
        public object Tratamientos_Espera_Total()
        {
            sql = "select count(*) from rad_tratamientosadistancia where estado=0";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar(); //Valor del id_paciente lo regresa como objeto
        }

        //Funcion para cargar el listado de tratamientos en espera - sin censura de un solo elemento
        public DataTable Tratamientos_Espera_Total_Lista()
        {
            sql = "select nombre,fechainicio from rad_tratamientosadistancia where estado=0";
            //command = new NpgsqlCommand(sql, conn);
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt; //Valor del id_paciente lo regresa como objeto
        }

        //Funcion para cargar los tratamientos asociados
        public DataTable Obtener_Tratamientos_Asociados(string id_tratamiento)
        {
            sql = "select * from rad_tratamientosadistancia where idpadre=$$" + id_tratamiento + "$$";
            //command = new NpgsqlCommand(sql, conn);
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt; //Valor del id_paciente lo regresa como objeto
        }

        //Funcion para eliminar tratamiento padre  
        public void Eliminar_Tratamiento(string nombreTratamiento, string Fecha)
        {
            sql = "delete from rad_tratamientosadistancia where estado=0 and (nombre=$$" + nombreTratamiento + "$$ and inicio=$$" + Fecha + "$$)";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        //Funcion para eliminar tratamiento padre
        public void Eliminar_TratamientoPadre(string nombreTratamientoP)
        {
            sql = "delete from rad_tratamientosadistancia where estado=1 and nombre=$$" + nombreTratamientoP + "$$ and idpadre=''";
            command = new NpgsqlCommand(sql, conn);
            int i = command.ExecuteNonQuery();
            //Console.WriteLine(i);
        }

        //Funcion para eliminar tratamiento padre e hijos
        public void Eliminar_TratamientoPadre_Hijos(string nombreTratamientoP)
        {
            sql = "delete from rad_tratamientosadistancia where estado=0 and nombre=$$" + nombreTratamientoP + "$$ and idpadre<>''";
            command = new NpgsqlCommand(sql, conn);
            int i = command.ExecuteNonQuery();
        }

        //Funcion para eliminar tratamiento padre e hijos
        public void Eliminar_TratamientoPasado(string idTratamiento)
        {
            sql = "delete from rad_tratamientosadistancia where idt =$$" + idTratamiento + "$$  ";
            command = new NpgsqlCommand(sql, conn);
            int i = command.ExecuteNonQuery();
        }
        //Funcion para buscar si el nombre del tratamiento esta mas de una vez y determinar si es una secuencia...
        public object Obtener_NoVeces_Tratamiento(string nombreTratamiento)
        {
            sql = "SELECT count(*) FROM rad_tratamientosadistancia where nombre=$$" + nombreTratamiento + "$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar(); //Valor del id_paciente lo regresa como objeto
        }

        //Funcion para registrar analisis del paciente  
        public void RegistrarAnalisisPaciente_Diag(int id_paciente, string nombre, DateTime fecha, bool analizado, bool reanalizar)
        {
            sql = "INSERT INTO rad_analisis(paciente, nombre, fecha, analisado, reanalizado) VALUES(@paciente, @nombre, @fecha,  @analizado, @reanalizar)";
            command = new NpgsqlCommand(sql, conn);

            command.Parameters.AddWithValue("@paciente", id_paciente);
            command.Parameters.AddWithValue("@nombre", nombre);
            command.Parameters.AddWithValue("@fecha", fecha);
            command.Parameters.AddWithValue("@analizado", analizado);
            command.Parameters.AddWithValue("@reanalizar", reanalizar);

            command.ExecuteNonQuery();
        }


        public void Actualizar_Fecha_Analisis(DateTime fecha,string id_analisis)
        {
            sql = "UPDATE rad_analisis SET fecha=$$"+fecha+"$$  WHERE id=$$"+id_analisis+"$$";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        public object Obtener_Fecha_Analisis(string id_analisis)
        {
            sql = "SELECT fecha FROM  rad_analisis WHERE id=$$"+id_analisis+"$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar();
        }

        //public object Obtener_Fecha_Analisis(string id_analisis)


        //Funcion para registrar version del software
        public void RegistrarVersion(string nombre, string descripcion)
        {
            sql = "INSERT INTO rad_rversion(nombre,descripcion) VALUES($$" + nombre + "$$,$$" + descripcion + "$$)";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        //Funcion para registrar domicilios del paciente
        public void RegistrarDomicilios(string calle, string numero, string colonia, string cp, string municipio, string estado, string pais, string id_paciente)
        {
            sql = "INSERT INTO rad_domicilios(calle,numero,colonia,cp,municipio,estado,pais,idpaciente) VALUES($$" + calle + "$$,$$" + numero + "$$,$$" + colonia + "$$,$$" + cp + "$$,$$" + municipio + "$$,$$" + estado + "$$,$$" + pais + "$$,$$" + id_paciente + "$$)";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        //Funcion para registrar antecedentes del paciente
        public void RegistrarAntecedentes(string titulo, string texto, string id_paciente, string tipo)
        {
            sql = "INSERT INTO rad_antecedentes(titulo,texto,idpaciente,tipo) VALUES($$" + titulo + "$$,$$" + texto + "$$,$$" + id_paciente + "$$,$$" + tipo + "$$)";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        //Funcion para registrar telefonos del paciente
        public void RegistrarTelefonos(string telefono, string extension, string id_paciente)
        {
            sql = "INSERT INTO rad_telefonos(numero,extension,idobj) VALUES($$" + telefono + "$$,$$" + extension + "$$,$$" + id_paciente + "$$)";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        //Funcion para consultar elementos del remedio a duplicar
        public DataTable Consultar_Remedio_Duplicar(string nombre_remedio)
        {
            sql = "SELECT * FROM rad_remedios where nombre=$$" + nombre_remedio + "$$";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }

        //Funcion para consultar valores del registro
        public DataTable Consultar_Version()
        {
            sql = "SELECT * FROM rad_rversion";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }

        //Funcion para obtener listado de pacientes para generar reporte PDF
        public DataTable Obtener_Pacientes()
        {
            sql = "select nombre,apellido1,apellido2,email,sexo,fechanacimiento,fpg from rad_pacientes";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }

        //Funcion para consultar si el nombre del duplicado no ha sido usado
        public object Consultar_NombreDuplicado(string nombre_del_duplicado)
        {
            sql = "SELECT count(*) FROM rad_remedios where nombre=$$" + nombre_del_duplicado + "$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar(); //Valor del id_paciente lo regresa como objeto
        }

        //Funcion para registrar remedio duplicado
        public void Registrar_Remedio_Duplicado(string nombre, DateTime fecha, string idioma)
        {
            sql = "INSERT INTO rad_remedios(nombre,fecha,idioma) values(@nombre,@fecha,@idioma)";
            command = new NpgsqlCommand(sql, conn);

            command.Parameters.AddWithValue("@nombre", nombre);
            command.Parameters.AddWithValue("@fecha", fecha);
            command.Parameters.AddWithValue("@idioma", idioma);
            command.ExecuteNonQuery();

        }



        //Datos - idt, idpadre, idpaciente, nombrepaciente, nombre, duracion, tiempoemitido, fechainicio, fechac, estado
        public void Registrar_TratamientoNuevoSencillo(string id_padre, string idpaciente, string nombrepaciente, string nombre, int duracion, int tiempoemitido, DateTime fechainic, DateTime fechac, int estado)
        {
            sql = "INSERT INTO rad_tratamientosadistancia(idpadre, idpaciente, nombrepaciente, nombre, duracion, tiempoemitido, fechainicio, fechac, estado) VALUES(@id_padre, @idpaciente, @nombrepaciente, @nombre, @duracion, @tiempoemitido, @fechainic, @fechac, @estado)";

            command = new NpgsqlCommand(sql, conn);

            command.Parameters.AddWithValue("@id_padre", id_padre);
            command.Parameters.AddWithValue("@idpaciente", idpaciente);
            command.Parameters.AddWithValue("@nombrepaciente", nombrepaciente);
            command.Parameters.AddWithValue("@nombre", nombre);
            command.Parameters.AddWithValue("@duracion", duracion);
            command.Parameters.AddWithValue("@tiempoemitido", tiempoemitido);
            command.Parameters.AddWithValue("@fechainic", fechainic);
            command.Parameters.AddWithValue("@fechac", fechac);
            command.Parameters.AddWithValue("@estado", estado);

            command.ExecuteNonQuery();
        }


        //Funcion para registrar contenido del tratamiento a distancia
        public void Registrar_ContenidoTratamiento(string idt, string descripcion, string tipo)
        {
            sql = "INSERT INTO rad_codigosdetratamientos(idcr,descripcion,tipo) VALUES($$" + idt + "$$,$$" + descripcion + "$$,$$" + tipo + "$$)";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        //Funcion para obtener Id del tratamiento
        public object Obtener_IDTratamiento(string id_paciente, string nombre_paciente, string nombre_tratamiento)
        {
            sql = "SELECT idt FROM rad_tratamientosadistancia where idpaciente=$$" + id_paciente + "$$ and nombrepaciente=$$" + nombre_paciente + "$$ and nombre=$$" + nombre_tratamiento + "$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar(); //Valor del id_paciente lo regresa como objeto
        }
        public object Obtener_IDPadre(string id_paciente, string nombre_paciente, string nombre_tratamiento)
        {
            sql = "SELECT idpadre FROM rad_tratamientosadistancia where idpaciente=$$" + id_paciente + "$$ and nombrepaciente=$$" + nombre_paciente + "$$ and nombre=$$" + nombre_tratamiento + "$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar(); //Valor del id_paciente lo regresa como objeto
        }

        //Funcion para obtener los codigos de un tratamiento 
        public DataTable CodigosTratamiento(string idt)
        {
            sql = "select * from rad_codigosdetratamientos where idcr=$$" + idt + "$$";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }

        //Funcion para comprobar si el nombre del tratamiento ya esta en uso!...
        public object Comprobar_NombreTratamiento(string nombre_tratamiento)
        {
            sql = "SELECT count(*) FROM rad_tratamientosadistancia where nombre like $$%" + nombre_tratamiento + "%$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar(); //Valor del id_paciente lo regresa como objeto
        }


        //Funcion para eliminar un código
        public object ObtenerIDporNomyCat(string frecuencia, Guid subcategoria)
        {
            sql = "SELECT ID FROM rad_codigo WHERE  frecuencia=$$" + frecuencia + "$$ AND SubcategoriaID=$$" + subcategoria + "$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar();
        }

        public void Eliminar_Codigo(Guid id)
        {
            sql = "DELETE FROM rad_codigo WHERE  id=$$" + id + "$$";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        public void Eliminar_Codigo_Tratamientos(string nameCode, string idpadre)
        {
            sql = "DELETE FROM rad_codigosdetratamientos WHERE descripcion=$$" + nameCode+"$$ AND idcr=$$"+idpadre+"$$";
            command= new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }


        public void Eliminar_Codigo_Personalizado(Guid id)
        {
            sql = "DELETE FROM rad_codigoCustom WHERE  codigoid=$$" + id + "$$";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        //Funcion para guardar tarjeta en categorias
        public void Registrar_Tarjeta_Categorias(string id_generado, string nombre_tarjeta, string codigo_generado)
        {
            sql = "INSERT INTO rad_codigos(idcodigo,nombre,codigo,idcat,genero) VALUES($$" + id_generado + "$$,$$" + nombre_tarjeta + "$$,$$" + codigo_generado + "$$,$$" + "-tarjetas-" + "$$,$$" + "0" + "$$)";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        //Funcion para eliminar pacientes
        public void EliminarPaciente(string id_paciente)
        {
            sql = "DELETE FROM rad_telefonos WHERE idobj=$$" + id_paciente + "$$";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();

            command.CommandText = "DELETE FROM rad_antecedentes WHERE idpaciente= $$" + id_paciente + "$$";
            command.ExecuteNonQuery();

            command.CommandText = "DELETE FROM rad_domicilios WHERE idpaciente=$$" + id_paciente + "$$";
            command.ExecuteNonQuery();

            command.CommandText = "DELETE FROM rad_analisis WHERE paciente=$$" + id_paciente + "$$";
            command.ExecuteNonQuery();

            //Finalmente el paciente
            command.CommandText = "DELETE FROM rad_pacientes WHERE idp=$$" + id_paciente + "$$";
            command.ExecuteNonQuery();
        }

        //Funcion que modifica la fecha del tratamiento a distancia
        public void ModificarFechaTratamiento(int IDs, DateTime fecha)
        {
            sql = "UPDATE rad_tratamientosadistancia SET fechainicio=@fecha WHERE idt=@IDs";
            command = new NpgsqlCommand(sql, conn);

            command.Parameters.AddWithValue("@fecha", fecha);
            command.Parameters.AddWithValue("@IDs", IDs);

            command.ExecuteNonQuery();
        }

        //Funcion que modifica el estado del tratamiento a distancia
        public void ModificarEstadoTratamientoActivo(string IDs)
        {
            sql = "UPDATE rad_tratamientosadistancia SET estado=1 WHERE idt=$$" + IDs + "$$";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }
        public void ModificarEstadoTratamientoVencido(string IDs)
        {
            sql = "UPDATE rad_tratamientosadistancia SET estado=2 WHERE idt=$$" + IDs + "$$";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        //Funcion que modifica el registro del paciente de uno actual
        public void ModificarRegistroPaciente(string id_paciente, string nombre, string apellido1, string apellido2, string email, string sexo, string profesion, string titulo, string fechanac, string fpg)
        {
            sql = "UPDATE rad_pacientes SET nombre=$$" + nombre + "$$,apellido1=$$" + apellido1 + "$$,apellido2=$$" + apellido2 + "$$,email=$$" + email + "$$,sexo=$$" + sexo + "$$,profesion=$$" + profesion + "$$,titulo=$$" + titulo + "$$,fechanacimiento=$$" + fechanac + "$$,fpg=$$" + fpg + "$$ WHERE idp=$$" + id_paciente + "$$";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }


        //Funcion que modifica el estado del analisis
        public void Modificar_Estado_Analisis_Analizado(string id_analisis)
        {
            sql = "UPDATE rad_analisis SET analisado = '1' WHERE id = $$" + id_analisis + "$$";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        public bool checkExistCode(string id_analisis,string rate)
        {
            sql = "SELECT COUNT(*) FROM rad_codigosdeanalisis WHERE  analisis=$$" + id_analisis + "$$ AND rate=$$"+rate+"$$";
            command = new NpgsqlCommand(sql, conn);
            return Convert.ToInt32(command.ExecuteScalar().ToString()) > 0  ;

        }

        //Funcion para eliminar el registro de un domicilio
        public void EliminarDomicilioPaciente(string id_dom)
        {
            sql = "DELETE FROM rad_domicilios WHERE iddom=$$" + id_dom + "$$";
            // sql = "UPDATE rad_pacientes SET nombre='" + nombre + "',apellido1='" + apellido1 + "',apellido2='" + apellido2 + "',email='" + email + "',sexo='" + sexo + "',profesion='" + profesion + "',titulo='" + titulo + "',fechanacimiento='" + fechanac + "',fpg='" + fpg + "' WHERE idp='" + id_paciente + "'";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

       
        //Funcion que elimina el registro del telefono
        public void EliminarTelefonosPacientePorID(string id_telefono)
        {
            //INSERT INTO rad_telefonos(numero,extension,idobj) VALUES('
            sql = "DELETE FROM rad_telefonos WHERE idtel=$$" + id_telefono + "$$";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        //Funcion que elimina el registro de un analisis de acuerdo a su nombre
        public void EliminarAnalisisPorNombre(string nombre_analisis)
        {
            object id_analisis = Obtener_Id_Analisis(nombre_analisis);
            command.CommandText = "DELETE FROM rad_codigosdeanalisis where analisis=$$" + id_analisis.ToString() + "$$";
            command.ExecuteNonQuery();

            //INSERT INTO rad_telefonos(numero,extension,idobj) VALUES('
            sql = "DELETE FROM rad_analisis WHERE nombre=$$" + nombre_analisis + "$$";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        //Funcion que elimina el registro del antecedente por IP
        public void EliminarAntecedentesPorIDPaciente(string id_antecedente)
        {
            //INSERT INTO rad_telefonos(numero,extension,idobj) VALUES('
            sql = "DELETE FROM rad_antecedentes WHERE ida=$$" + id_antecedente + "$$";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        //Funcion que elimina el registro del antecedente
        public void EliminarAntecedentesPaciente(string titulo, string descripcion, string tipo)
        {
            //INSERT INTO rad_telefonos(numero,extension,idobj) VALUES('
            sql = "DELETE FROM rad_antecedentes WHERE titulo=$$" + titulo + "$$ and texto=$$" + descripcion + "$$ and tipo=$$" + tipo + "$$";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        //Funcion que elimina un remedio elegido por el usuario por nombre
        public void Eliminar_remedio_Nombre(string nombre)
        {
            //INSERT INTO rad_telefonos(numero,extension,idobj) VALUES('
            sql = "DELETE FROM rad_remedios WHERE nombre=$$" + nombre + "$$";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();

        }

        public void Eliminar_remedio_codigo(int id)
        {
            //ELIMINA  AUTOSIMIL SI TIENE
            sql = "DELETE FROM rad_codigosdeterapia WHERE  id  IN ( select terapiacolor from rad_codigosderemedios WHERE remedio=$$" + id + "$$)";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();

            //ELIMINAR COLOR DE LA TERAPIA SI LO TIENE
            sql = "DELETE   FROM  rad_autosimil WHERE  id IN (SELECT autosimil FROM rad_codigosderemedios WHERE remedio=$$" + id + "$$)";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();

            sql = "DELETE FROM rad_codigosderemedios WHERE remedio=$$" + id + "$$";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }


        //Funcion para visualizar categorias de los codigos
        public DataTable VisualizarCategoriasCodigos()
        {
            sql = "SELECT * FROM rad_categoria" + Database.table + "  order by Nombre ASC";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }

        //Funcion para visualizar categorias de los codigos
        public DataTable VisualizarCategoriasCodigosPersonalizada()
        {
            sql = "SELECT * FROM rad_categoriacustom order by Nombre ASC";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }


        //Funcion para buscar categorias de los codigos
        public object BuscarCategoriasCodigos(string nombre_categoria)
        {
            sql = "SELECT ID FROM rad_categoria" + Database.table + " where Nombre=$$" + nombre_categoria + "$$ ";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar();
        }

        public object BuscarCategoriasCodigosPersonalizadas(string nombre_categoria)
        {
            sql = "SELECT ID FROM rad_categoriacustom  where Nombre=$$" + nombre_categoria + "$$ ";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar();
        }



        public object GetIdSubcategorieByName(string nameSub, string categorieID)
        {
            sql = "SELECT subcategoriaid  FROM rad_Subcategoria" + Database.table + " where Nombre= $$" + nameSub + "$$ AND  subcategoriaid IN (SELECT id FROM rad_subcategoria WHERE CategoriaID=$$" + categorieID + "$$)";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar();
        }


        public object GetIdSubcategorieByNameCustom(string nameSub)
        {
            sql = "SELECT  subcategoriaid FROM rad_SubcategoriaCustom where Nombre= $$" + nameSub + "$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar();
        }


        //Funcion para visualizar sub-categorias de los codigos
        public DataTable VisualizarSubCategoriasCodigos(string categoria)
        {
            sql = "SELECT Nombre FROM rad_Subcategoria" + Database.table + " Where SubcategoriaID IN  (Select ID FROM rad_SubCategoria Where CategoriaID='" + categoria + "') ORDER BY Nombre";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }

        public DataTable VisualizarSubCategoriasCodigosPerosnalizadas(string categoria)
        {
            sql = "SELECT Nombre FROM rad_Subcategoriacustom Where SubcategoriaID IN  (Select ID FROM rad_SubCategoria Where CategoriaID='" + categoria + "') ORDER BY Nombre";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }




        public DataTable getRatesBySubcategorie(string idSubcategorie)
        {
            sql = "SELECT * FROM  ObtenerCodigo" + Database.table + "($$" + idSubcategorie + "$$)";   
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }

        public DataTable getRatesByCustomSubcategorie(string idSubcategorie)
        {
            sql = "SELECT * FROM  ObtenerCodigoPerzonalizada($$" + idSubcategorie + "$$)";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }

        //Funcion para visualizar sub-categorias de los codigos v2 cuando seleccione el listado de subcategorias
        public DataTable VisualizarSubCategoriasCodigosListado(string categoria, string genero_paciente)
        {
            // sql = "SELECT * FROM rad_codigos where idcat='" + categoria + "' order by nombre";
            sql = "SELECT * FROM rad_codigos where idcat=$$" + categoria + "$$ and (genero=$$" + genero_paciente + "$$ or genero='P') order by nombre";

            Console.WriteLine(sql);
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }

        //Funcion para buscar codigo en base al nombre de la categoria
        public object BuscarCodigoPorNombreCategoria(string nombre_categoria)
        {
            sql = "SELECT rate FROM rad_codigo where UPPER(nombre)=$$" + nombre_categoria + "$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar();
        }

        public DataTable BuscarCategoriaCodigo(string nombre_codigo)
        {
            sql = "SELECT * FROM BuscarIndice($$" + nombre_codigo + "$$,$$" + Database.table + "$$)";
            Console.WriteLine(sql);
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }

        public DataTable BuscarCodigoRem(string nombre_codigo)
        {
            string leng = "EN";

            if (Database.table == "espanol")
            {
                leng = "ES";
            }else if (Database.table== "bulgaro")
            {
                leng = "BG";
            }

            Console.WriteLine(leng);
            sql = "SELECT nombre,id FROM rad_remedios WHERE UPPER(nombre) LIKE $$%" + nombre_codigo + "%$$ AND  lenguaje in ('CUS','"+leng+"')";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }
 

        //Funcion para buscar codigos directamente en caso del boton de BUSCAR
        public DataTable BuscarCodigo2(string nombre_codigo)
        {
            sql = "SELECT nombre FROM rad_codigos WHERE nombre LIKE $$" + nombre_codigo + "$$";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }


        //Funcion para validar si el analisis ha sido procesado
        public object Validar_Analisis_Analizado(string nombre_analisis)
        {
            sql = "SELECT analisado from rad_analisis where nombre =$$" + nombre_analisis + "$$";
            Console.WriteLine(sql);
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar(); //Valor del id_paciente lo regresa como objeto
        }

        //Funcion para validar si ya fue reanalizado un analisis en base a su nombre y el campo de reanalizado
        public DataTable Validar_Reanalisis_Hecho(string nombre_analisis)
        {
            sql = "SELECT reanalizado from rad_analisis where nombre =$$" + nombre_analisis + "$$";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }

        //Buscar Id_codigo en base al codigo
        public object Buscar_IdCodigo_Codigo(string codigoid)
        {
            sql = "SELECT codigoid  from rad_codigo" + Database.table + " where id=$$" + codigoid + "$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar(); //Valor del id_paciente lo regresa como objeto
        }

        //Buscar Id_analisis en base al nombre del analisis
        public object Buscar_IdAnalisis_Nombre(string nombre_analisis)
        {
            sql = "SELECT id from rad_analisis where nombre=$$" + nombre_analisis + "$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar(); //Valor del id_paciente lo regresa como objeto
        }

        //Funcion para registrar nuevas categorias y subcategorias en el database control
        public void Registrar_Categorias(string nombre, Guid CategoriaID)
        {
            sql = "INSERT INTO rad_categoriaCustom(ID,Nombre) VALUES($$" + CategoriaID + "$$,$$" + nombre + "$$)";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }
        public void Registrar_Codigo(Guid id, string frecuencia, Guid subCategoria)
        {
            sql = "INSERT INTO rad_codigo(id,frecuencia,SubcategoriaID) VALUES($$" + id + "$$,$$" + frecuencia + "$$,$$" + subCategoria + "$$)";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }
        public void Registrar_CodigoPersonalizado( string nombre, Guid codigo)
        {
            sql = "INSERT INTO rad_codigoCustom(nombre,CodigoID) VALUES($$" + nombre + "$$,$$" + codigo + "$$)";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();

        }

        public bool validarExisteFrecuencia(string frecuencia )
        {
            sql = "SELECT frecuencia FROM rad_codigo where frecuencia=$$"+frecuencia+"$$";
            command=new NpgsqlCommand(sql, conn);    
            return command.ExecuteScalar() != null;
        }

        public void Registrar_SubcategoriaPersonalizada(string nombre, Guid SubcategoriaID)
        {
            sql = "INSERT INTO rad_Subcategoriacustom(nombre,SubcategoriaID) VALUES($$" + nombre + "$$,$$" + SubcategoriaID + "$$)";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        public void Registrar_Subcategoria(Guid id, Guid CategoriaID)
        {
            sql = "INSERT INTO rad_Subcategoria(id,CategoriaID) VALUES($$" + id + "$$,$$" + CategoriaID + "$$)";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }



        //Funcion para actualizar el estado de un tratamiento hijo
        public void Actualizar_Estado(string idTratamiento)
        {
            //UPDATE rad_tratamientosadistancia SET tiempoemitido='25' WHERE nombrepaciente='Raul Lopez ' and nombre='prueba completa' and duracion='7200'
            sql = "UPDATE rad_tratamientosadistancia SET estado=1 WHERE idt=$$" + idTratamiento + "$$";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        public void limpiart_Tratamientos_Pasados()
        {
            //UPDATE rad_tratamientosadistancia SET tiempoemitido='25' WHERE nombrepaciente='Raul Lopez ' and nombre='prueba completa' and duracion='7200'
            sql = "DELETE FROM rad_tratamientosadistancia where CAST(EXTRACT(DAY FROM fechainicio)as INT) < CAST(EXTRACT(DAY FROM NOW()) as INT)";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        //Funcion para cargar el listado de tratamientos activos
        public DataTable Tratamientos_Inactivos()
        {
            sql = "SELECT * FROM rad_tratamientosadistancia WHERE estado = 0 AND fechainicio >= NOW() ORDER BY fechainicio ASC  LIMIT 30";
            //command = new NpgsqlCommand(sql, conn);
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt; //Valor del id_paciente lo regresa como objeto
        }


        //Obtiene codigo de la parte elegida en la terapia de color
        public object Obtener_IDCategoria(string nombre)
        {
            sql = "select categoriaID from rad_categoria" + Database.table + " where Nombre=$$" + nombre + "$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar();
        }

        public DataTable Obtener_IDSubcategoria(Guid CategoriaID)
        {
            sql = "select ID from rad_subcategoria where CategoriaID=$$" + CategoriaID + "$$";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }

        //Buscar fecha del remedio en base al nombre del remedio
        public object Buscar_Fecha_Remedio(string nombre)
        {
            sql = "SELECT fecha from rad_remedios where nombre=$$" + nombre + "$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar(); //Valor del id_paciente lo regresa como objeto
        }

     

        //Guardar el registro en rad_codigosdeanalisis
        public void Registrar_Codigo_de_Analisis(int id_analisis, string codigo, string valor, string nivel, string nivelsugerido, string potencia, string potenciaSugeridad)
        {
            sql = "INSERT INTO rad_codigosdeanalisis(analisis,rate,valor,nivel,nivelsugerido,potencia,potenciasugerido) VALUES($$" + id_analisis + "$$,$$" + codigo + "$$,$$" + valor + "$$,$$" + nivel + "$$,$$" + nivelsugerido + "$$,$$" + potencia + "$$,$$" + potenciaSugeridad + "$$)";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        public void Actualizar_codigo_de_analisis(int id_analisis, string codigo, string valor, string nivel, string nivelsugerido, string potencia, string potenciaSugeridad)
        {
            sql = "UPDATE rad_codigosdeanalisis SET valor=$$"+valor+"$$, nivel=$$"+nivel+"$$, nivelsugerido=$$"+nivelsugerido+"$$, potencia=$$"+potencia+ "$$, potenciasugerido=$$"+potenciaSugeridad+"$$ WHERE analisis=$$"+id_analisis+"$$ AND rate=$$"+codigo+"$$";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        //Funcion que actualiza rad_analisis agregando lo de la informacion biologica
        public void Registrar_Informacion_Biologica(double[] infobio, string nombre_analisis)
        {
            /*
             * estatura
                peso
                presionsisto
                presiondias
                imc
                fc
                fr
                ta
                valorini

             infobiologica[0] = Double.Parse(txtEstatura.Text);
                    infobiologica[1] = Double.Parse(txtPresionSist.Text);
                    infobiologica[2] = Double.Parse(txtIMC.Text);
                    infobiologica[3] = Double.Parse(txtFR.Text);
                    infobiologica[4] = Double.Parse(txtTA.Text);
                    infobiologica[5] = Double.Parse(txtPeso.Text);
                    infobiologica[6] = Double.Parse(txtPresionDistolica.Text);
                    infobiologica[7] = Double.Parse(txtFC.Text);
                    infobiologica[8] = Double.Parse(txtTemp.Text);
             */

            sql = "UPDATE rad_analisis SET estatura=$$" + infobio[0].ToString() + "$$, presionsisto=$$" + infobio[1].ToString() + "$$, imc=$$" + infobio[2].ToString() + "$$, fr=$$" + infobio[3].ToString() + "$$, ta=$$" + infobio[4].ToString() + "$$, peso=$$" + infobio[5].ToString() + "$$, presiondias=$$" + infobio[6].ToString() + "$$, fc=$$" + infobio[7].ToString() + "$$, valorini=$$" + infobio[8].ToString() + "$$ WHERE nombre=$$" + nombre_analisis + "$$";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        public void Registrar_Padecimiento_Analisis(string padecimiento, string interrogatorio_por_aparatos, string nombre_analisis)
        {
            sql = "UPDATE rad_analisis SET padecimientoactual=$$" + padecimiento + "$$, intporaparatos=$$" + interrogatorio_por_aparatos + "$$ WHERE nombre=$$" + nombre_analisis + "$$";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

      

        //Funcion para obtener tabla con num de filas
        public DataTable BDCodigosconFilas()
        {
            sql = "select nombre,codigo,ROW_NUMBER () OVER (ORDER BY nombre) from rad_codigos";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }

        //Funcion para visualizar las terapias de color recientes
        public DataTable VisualizarTerapiasdeColor()
        {
            sql = "select nombre,fechac from rad_remedios where nombre like $$%Color%$$ order by fechac";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }


        //select * from (SELECT sexo, CONCAT(nombre,' ', apellido1, ' ', apellido2) as nombrepaciente from rad_pacientes) as tabla where nombrepaciente like '%Raul Lopez%'

        //Funcion para visualizar remedios directamente dependiendo de la categoria y la letra
        public DataTable VisualizarRemedios(string language)
        {
            sql = "select * from  rad_remedios where lenguaje IN ($$" + language + "$$,'CUS') Order by nombre";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }

        //Funcion para visualizar remedios directamente dependiendo de la categoria y la letra
        public DataTable CodigoRemedioBuscado(string nombre, string language)
        {
            sql = "select * from rad_remedios where upper(nombre) like $$%" + nombre + "%$$ AND lenguaje in ($$" + language + "$$,'CUS')";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }

      

        //Funcion para visualizar remedios directamente dependiendo por el giro
        public DataTable VisualizarRemediosPorDiagnostico()
        {
            sql = "select * from rad_remedios where codigo='' or codigo is null";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }


        //Funcion para visualizar remedios directamente creados por el usuario
        public DataTable VisualizarRemediosCreados_Usuario()
        {
            sql = "select * from rad_remedios where lenguaje='CUS' order by nombre ASC";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }

        //Funcion para obtener el id del remedio en base al nombre
        public object Obtener_IdRemedio(string nombre)
        {
            sql = "SELECT id from rad_remedios where nombre=$$" + nombre + "$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar(); //Valor del id_paciente lo regresa como objeto
        }

        public bool isCustomRemedy(string nombre)
        {
            if (!string.IsNullOrEmpty(nombre))
            {
                sql = "SELECT lenguaje from rad_remedios where nombre=$$" + nombre + "$$";
                command = new NpgsqlCommand(sql, conn);
                return command.ExecuteScalar().ToString() == "CUS"; //Valor del id_paciente lo regresa como objeto
            }

            return false;
           
        }

        public object Obtener_IDAnalisis(string nombre)
        {
            sql = "SELECT id from rad_analisis where nombre=$$" + nombre + "$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar();
        }



        //Funcion para obtener el id del remedio en base al nombre y a la fecha
        public object Obtener_IdRemedio_ConFecha(string nombre, string fecha)
        {
            sql = "SELECT idr from rad_remedios where nombre=$$" + nombre + "$$ and fechac=$$" + fecha + "$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar(); //Valor del id_paciente lo regresa como objeto
        }

        //Funcion para obtener el id del paciente en base al nombre completo
        public object Obtener_IdPaciente_NombreCompleto(string nombre)
        {
            sql = "select idp from (select idp,concat(nombre,' ',apellido1,' ',apellido2) from rad_pacientes) as tabla where concat like $$" + nombre + "%$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar();
        }

        //Funcion para obtener el id del tratamiento para utilizarlo como Padre
        public object Obtener_IdTratamiento(string id_paciente, string nombre_paciente, string nombre_tratamiento)
        {
            sql = "select idt from rad_tratamientosadistancia where idpaciente=$$" + id_paciente + "$$ and nombrepaciente=$$" + nombre_paciente + "$$ and nombre=$$" + nombre_tratamiento + "$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar();
        }

        //Funcion para visualizar los codigos del remedio directamente dependiendo del id_remedio
        public DataTable VisualizarCodigos_Remedios_IdRemedio(int id_remedio)
        {
            sql = "SELECT * FROM ObtenerCodigodeRemedios($$" + id_remedio + "$$,$$" + Database.table + "$$)";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }


        public DataTable VisualizarCodigos_TerapiaColor(int id_remedio)
        {
            sql = "SELECT * FROM ObtenerCodigosdeTerapiaColor($$" + id_remedio + "$$)";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }

            //Funcion para visualizar los codigos combinados de una terapia de color
            public DataTable VisualizarCodigosCombinados(string id_remedio)
        {
            sql = "select codigo,nombrecodigo from rad_codigosderemedios where idr=$$" + id_remedio + "$$";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }

        //Funcion para visualizar los codigos del remedio directamente dependiendo del id_remedio
        public DataTable VisualizarCodigos_Remedios_IdRemedio2(string id_remedio)
        {
            sql = "select * from rad_codigoderemedios where remedio=$$" + id_remedio + "$$";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }


        //Funcion para registrar codigos de remedio de un remedio duplicado
        public void Registrar_CodigosdeRemedios(string codigo, string potencia, string metodo, string nivel, string complemento, int remedio)
        {
            sql = "INSERT INTO rad_codigosderemedios(rate,potencia,metodo,nivel,complemento,remedio) VALUES($$" + codigo + "$$,$$" + potencia + "$$,$$" + metodo + "$$,$$" + nivel + "$$,$$" + complemento + "$$,$$" + remedio + "$$)";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }


        public void Registrar_CodigosdeRemediosColorTerapia(string codigo, string potencia, string metodo, string nivel, string complemento, int remedio)
        {
            sql = "INSERT INTO rad_codigosderemedios(terapiacolor,potencia,metodo,nivel,complemento,remedio) VALUES($$" + codigo + "$$,$$" + potencia + "$$,$$" + metodo + "$$,$$" + nivel + "$$,$$" + complemento + "$$,$$" + remedio + "$$)";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        public void Registrar_AutosimilCodigoRemedio(string codigoAutosimil, string autosimilNombre, string  potencia, string metodo, string nivel, string complemento, int remedio)
        {
            sql = "INSERT INTO rad_codigosderemedios(autosimil,potencia,metodo,nivel,complemento,remedio) VALUES(@codigo,@potencia,@metodo,@nivel,@complemento,@remedio)";
            command = new NpgsqlCommand(sql, conn);
            command.Parameters.AddWithValue("@codigo", codigoAutosimil);
            command.Parameters.AddWithValue("@potencia", potencia);
            command.Parameters.AddWithValue("@metodo", metodo);
            command.Parameters.AddWithValue("@nivel", nivel);
            command.Parameters.AddWithValue("@complemento", complemento);
            command.Parameters.AddWithValue("@remedio", remedio);
            command.ExecuteNonQuery();


            sql = "INSERT INTO rad_autosimil(id,nombre) VALUES($$"+codigoAutosimil+"$$,$$"+autosimilNombre+"$$)";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        //Guardar el registro en rad_remedios
        public void Registrar_Remedio_Diagnostico(string nombre, DateTime fecha, string idioma)
        {
            sql = "INSERT INTO rad_remedios(nombre, fecha , lenguaje) VALUES(@nombre, @fecha, @idioma)";

            command = new NpgsqlCommand(sql, conn);
            command.Parameters.AddWithValue("@nombre", nombre);
            command.Parameters.AddWithValue("@fecha", fecha);
            command.Parameters.AddWithValue("@idioma", idioma);

            command.ExecuteNonQuery();
        }


        public void Registrar_Codigo_Terapia(string id, string codigo, string nombre)
        {
            sql = "INSERT INTO rad_codigosdeterapia(id,codigo,nombre) VALUES($$" + id + "$$,$$" + codigo + "$$,$$" + nombre + "$$)";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }


   

        public DataTable BuscarGenero(string nombre)
        {
            
            sql = "SELECT * FROM rad_pacientes WHERE (nombre || ' ' || apellido1 || ' ' || apellido2) = '" + nombre +"'";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }

     

        //funcion eliminar categorias, subcategorias y codigo personalizada
        public void Eliminar_SubcategoriaPorID(string subcategoriaID )
        {
            sql = "DELETE FROM rad_Subcategoriacustom Where SubcategoriaID='"+subcategoriaID+"'";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();

            sql = "DELETE FROM rad_SubCategoria Where ID='" +subcategoriaID + "'";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        public void EliminarTodocodigosPorSubcategoriaID(string subcategoriaID)
        {
            //delete custom rates
            sql = "DELETE FROM rad_codigocustom Where codigoid IN  (select ID from rad_codigo where subcategoriaID='"+subcategoriaID+"')";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();

            //delete rates
            sql = "delete from rad_codigo where subcategoriaID='" + subcategoriaID + "'";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }



        public void Eliminar_Subcategoria(string categoriaID)
        {
            sql = "DELETE FROM rad_Subcategoriacustom Where SubcategoriaID IN  (Select ID FROM rad_SubCategoria Where CategoriaID='" +categoriaID + "')";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();

            sql = "DELETE FROM rad_SubCategoria Where CategoriaID='" + categoriaID + "'";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        public void EliminarTodocodigos(string categoriaID)
        {
            //delete custom rates
            sql = "DELETE FROM rad_codigocustom Where codigoid IN  (select ID from rad_codigo where subcategoriaID IN(Select ID FROM rad_SubCategoria Where CategoriaID='" + categoriaID + "'))";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();

            //delete rates
            sql = "delete from rad_codigo where subcategoriaID IN(Select ID FROM rad_SubCategoria Where CategoriaID='" + categoriaID + "')";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        public void Eliminar_Categoria(string categoriaID)
        {
            sql = "DELETE FROM rad_categoriacustom Where ID='"+ categoriaID+ "'";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

    

        //Funcion para eliminar los codigos de un remedio sin borrar la raiz
        public void Eliminar_codigos_remedio(string id_remedy, string nameRate,string nameRemedy)
        {
            if (nameRemedy.StartsWith("ChromoTherapy"))
            {
                sql = "DELETE FROM rad_codigosdeterapia WHERE  nombre=$$" + nameRate + "$$ and id  = (select terapiacolor from rad_codigosderemedios where id=$$" + id_remedy  + "$$)";
                command = new NpgsqlCommand(sql, conn);
                command.ExecuteNonQuery();
            }
            else if (nameRate.StartsWith("Autosimile"))
            {
                sql = "DELETE   FROM  rad_autosimil WHERE nombre=$$" + nameRate + "$$ and id  in (select autosimil from rad_codigosderemedios where id=$$" + id_remedy + "$$)";
                command = new NpgsqlCommand(sql, conn);
                command.ExecuteNonQuery();
            }

            sql = "DELETE FROM rad_codigosderemedios WHERE id=$$" + id_remedy + "$$";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

       

        public void Eliminar_codigos_analisis(int id_analisis, string id_codigo)
        {
            //Elimina los colores de codigosderemedios
            sql = "DELETE FROM rad_codigosdeanalisis WHERE analisis=$$" + id_analisis + "$$ AND rate=CAST($$" + id_codigo + "$$ AS uuid)";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        public void Eliminar_autosimilcodigos_remedio(int id_remedio, string id_autosimil)
        {
            //Elimina los colores de codigosderemedios
            sql = "DELETE FROM rad_codigosderemedios WHERE remedio=$$" + id_remedio + "$$ AND autosimil=$$" + id_autosimil + "$$ ";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();

            sql = "DELETE FROM rad_autosimil WHERE id=$$" + id_autosimil + "$$ ";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }


 

        //Obtiene codigo del color en base a tabla y color elegido
        public object Obtener_Color_ParteTerapia(string color)
        {
            sql = "select codigo from rad_colordeterapia where upper(texto)=$$" + color + "$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar();
        }

      
        //Funcion para registrar codigo nuevo paciente
        public void Registrar_Codigo_Categorias(string id_codigo, string nombre, string codigo, string description, string id_subcategoria, string id_categoria, string genero)
        {
            sql = "INSERT INTO rad_codigos(idcodigo,nombre,codigo,descripcion,idcat,idcatp,genero) values($$" + id_codigo + "$$,$$" + nombre + "$$,$$" + codigo + "$$,$$" + description + "$$,$$" + id_subcategoria + "$$,$$" + id_categoria + "$$,$$" + genero + "$$)";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        public void CancelarTratamientoADistancia(string nombre)
        {

            sql = "DELETE FROM rad_codigosdetratamientos USING rad_tratamientosadistancia WHERE rad_codigosdetratamientos.idcr = CAST(rad_tratamientosadistancia.idt AS text)  AND rad_tratamientosadistancia.nombre=$$" + nombre + "$$";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();

            sql = "DELETE FROM  rad_tratamientosadistancia WHERE nombre=$$" + nombre + "$$";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        public DataTable ExisteRemedio(string columnName, string value)
        {
            // Lista de nombres de columnas permitidos
            var validColumns = new HashSet<string> { "id", "nombre" }; // Agrega los nombres de columna válidos

            if (!validColumns.Contains(columnName))
                throw new ArgumentException("Nombre de columna no válido");

            // Construir la consulta SQL de forma segura
            string sql = $"SELECT * FROM rad_remedios WHERE UPPER({columnName}) LIKE @value";

            using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
            {
                // Agregar el valor del parámetro con comodines para el LIKE
                cmd.Parameters.Add(new NpgsqlParameter("@value", "%" + value.ToUpper() + "%"));

                using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd))
                {
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    return ds.Tables[0];
                }
            }
        }
        
        public DataTable ExisteCodigoRemedio(string codigo)
        {
            sql = "SELECT idcr FROM rad_codigosderemedios WHERE idcr = '" + codigo + "'";

            // data adapter making request from our connection
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            // i always reset DataSet before i do
            // something with it.... i don't know why :-)
            ds.Reset();
            // filling DataSet with result from NpgsqlDataAdapter
            da.Fill(ds);
            // since it C# DataSet can handle multiple tables, we will select first
            dt = ds.Tables[0];
            // connect grid to DataTable
            return dt;
        }
   
      
    
        public DataTable ExisteIdCodigoRemedio(string codigo)
        {
            sql = "SELECT idcr FROM rad_codigosderemedios WHERE idcodigo = '" + codigo + "'";

            // data adapter making request from our connection
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            // i always reset DataSet before i do
            // something with it.... i don't know why :-)
            ds.Reset();
            // filling DataSet with result from NpgsqlDataAdapter
            da.Fill(ds);
            // since it C# DataSet can handle multiple tables, we will select first
            dt = ds.Tables[0];
            // connect grid to DataTable
            return dt;
        }



        public static string table = "ingles";

        //Conectar a la bd
        public bool ConexionBD(string user, string password)
        {
            //Conexion
            constring = String.Format("Server=localhost;Port=5432;" +
            "User Id={0};Password={1};Database={2}", user, password, "copenDB" );
            conn = new NpgsqlConnection(constring);
            conn.Open();
            return true;
        }


        public bool ExistCodeCategorie(Guid  id )
        {
           sql = "SELECT count(*) FROM  rad_subcategoria WHERE  CategoriaID=$$"+id+"$$";
           NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
           int count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }


        public bool ExistNameCategorie(string name)
        {
            sql = "SELECT count(*) FROM  rad_categoriaCustom WHERE  Nombre=$$" + name+"$$";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            int countCatCustom = Convert.ToInt32(cmd.ExecuteScalar());


            sql = "SELECT count(*) FROM  rad_categoriaIngles WHERE  Nombre=$$" + name+"$$";
            NpgsqlCommand cmd2 = new NpgsqlCommand(sql, conn);
            int countCatIngles = Convert.ToInt32(cmd2.ExecuteScalar());

            sql = "SELECT count(*) FROM  rad_categoriaEspanol WHERE  Nombre=$$" + name+"$$";
            NpgsqlCommand cmd3 = new NpgsqlCommand(sql, conn);
            int countCatEspanol = Convert.ToInt32(cmd3.ExecuteScalar());

  
            return countCatCustom>0 || countCatIngles>0 || countCatEspanol>0;
        }
      
        //Cerrar conexion a la bd
        public void CerrarBD()
        {
            conn.Close();
        }

        public object getIDCodebyFrec(string frecuencia)
        {
            sql = "SELECT  id FROM rad_codigo where Frecuencia= $$" + frecuencia + "$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar();
        }

        public object getNameByFrec( Guid codigo)
        {
            sql = "SELECT  nombre FROM rad_codigo" + Database.table + " where codigoid= $$" + codigo + "$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar();
        }




        public bool isRegisterRemedyCode(int IDRemedy, string IDRate)
        {

            if (IDRemedy != 0 && !string.IsNullOrEmpty(IDRate))
            {
                sql = "SELECT count(*) FROM  rad_codigosderemedios WHERE remedio=@remedio and  rate=CAST(@rate AS uuid)";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@remedio", IDRemedy);
                cmd.Parameters.AddWithValue("@rate", IDRate);
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }

            return false;

        }

        public bool isRegisterRemedyCodeColor(int IDRemedy, string IDRate)
        {

            if (IDRemedy != 0 && !string.IsNullOrEmpty(IDRate))
            {
                sql = "SELECT count(*) FROM  rad_codigosderemedios WHERE remedio=@remedio and   terapiacolor=CAST(@color AS uuid)";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@remedio", IDRemedy);
                cmd.Parameters.AddWithValue("@color", IDRate);
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }

            return false;

        }

        public object GetCodeIDCustom(string nombre )
        {
            
            sql = "SELECT codigoid from rad_codigocustom  where nombre=$$" + nombre + "$$";
            Console.WriteLine(sql);
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar();
        }

        public object GetCodeID(string nombre)
        {
            sql = "SELECT codigoid from rad_codigo" + Database.table + "  where nombre=$$" + nombre + "$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar();
        }

      
        public void updateCodeofRemedies(string idCoderemedy, string metodo, string potencia, string nivel,string complemento)
        {
            sql = "update rad_codigosderemedios set metodo=@metodo, potencia=@potencia,complemento=@complemento, nivel=@nivel WHERE id=CAST(@id as integer)";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            
            cmd.Parameters.AddWithValue("@id", idCoderemedy);
            cmd.Parameters.AddWithValue("@metodo", metodo);
            cmd.Parameters.AddWithValue("@potencia", potencia);
            cmd.Parameters.AddWithValue("@nivel", nivel);
            cmd.Parameters.AddWithValue("@complemento", complemento);
            cmd.ExecuteNonQuery();
        }

 


        public bool isAlreadyRegisterAutosimile(string codigo, int remedio)
        {
            string query = "SELECT COUNT(*) FROM rad_codigosderemedios WHERE autosimil= @codigo and remedio=@remedioid";
            NpgsqlCommand cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@codigo", codigo);
            cmd.Parameters.AddWithValue("@remedioid", remedio);
            int count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }

     
        public static bool ExisteCodigo(string codigo, NpgsqlConnection conn)
        {
            string query = "SELECT COUNT(*) FROM rad_codigo WHERE frecuencia = @codigo";
            NpgsqlCommand cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@codigo", codigo);
            int count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }

        public string Generarcodigo()
        {
            Random rdm = new Random();
            string id = rdm.Next(1000000, 9999999).ToString();
    
            if (!ExisteCodigo(id, conn))
            {
                // Asegúrate de cerrar la conexión si el código no existe
                return id;
            }
            else
            {
                // Asegúrate de cerrar la conexión si el código existe
                return Generarcodigo(); // Llama recursivamente al método hasta encontrar un código único
            }
        }

        public static bool ExisteCodigoRemedio(string codigo, NpgsqlConnection conn)
        {
            string query = "SELECT COUNT(*) FROM rad_codigosderemedios WHERE codigo = @codigo";
            NpgsqlCommand cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@codigo", codigo);
            int count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }

        public string Generarcodigoremedios()
        {
            Random rdm = new Random();
            string id = rdm.Next(1000000, 9999999).ToString();

            if (!ExisteCodigoRemedio(id, conn))
            {
                // Asegúrate de cerrar la conexión si el código no existe
                return id;
            }
            else
            {
                // Asegúrate de cerrar la conexión si el código existe
                return Generarcodigo(); // Llama recursivamente al método hasta encontrar un código único
            }
        }

        //this method is used to upload all query sql in the databa already is installed in postgesql
        

        public void Backup(string user, string password, string backupfile, Func<string, string> getMessage)
        {
            string host = "localhost";
            string port = "5432";
            string pgDumpPath = @"C:\Program Files\PostgreSQL\9.4\bin\pg_dump.exe";
            ProcessStartInfo processStartInfo = new ProcessStartInfo();

            processStartInfo.FileName = pgDumpPath;
            processStartInfo.Arguments = $"-h {host} -p {port} -U {user}   -F c -b -v -f \"{backupfile}\" radionica";
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.UseShellExecute = false;
            processStartInfo.CreateNoWindow = true;
            processStartInfo.EnvironmentVariables["PGPASSWORD"] = password;


            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo = processStartInfo;
                    process.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
                    process.ErrorDataReceived += (sender, args) => Console.WriteLine(args.Data);

                    process.Start();
                    process.BeginErrorReadLine();
                    process.BeginOutputReadLine();
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        MessageBox.Show(getMessage("messBackDatabase"), getMessage("headBackDatabase"), MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(getMessage("messErrBacDatabase"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }


        public void clearToDB(Func<string, string> getMessage)
        {

            try
            {

                string deleteDataCommand = @"
                        DO $$ DECLARE
                        r RECORD;
                        BEGIN
                            -- Eliminar triggers
                            DROP FUNCTION IF EXISTS ajustar_longitud_codigo();

                            -- Eliminar tablas
                            FOR r IN (SELECT tablename 
                                      FROM pg_tables 
                                      WHERE schemaname = 'public') LOOP
                                EXECUTE 'DROP TABLE IF EXISTS ' || quote_ident(r.tablename) || ' CASCADE';
                            END LOOP;
                        END $$;";

                using (NpgsqlCommand command = new NpgsqlCommand(deleteDataCommand, conn))
                {
                    command.ExecuteNonQuery();
                }

                MessageBox.Show(getMessage("messageDel"), getMessage("headBackDatabase"), MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }


        public void Restore(string user, string password, string backupfile, Func<string, string> getMessage)
        {
            string host = "localhost";
            string port = "5432";
            string pgRestorePath = @"C:\Program Files\PostgreSQL\9.4\bin\pg_restore.exe";
            ProcessStartInfo processStartInfo = new ProcessStartInfo();

            // Comando para ejecutar pg_restore
            Console.WriteLine(backupfile);
            processStartInfo.FileName = pgRestorePath;
            processStartInfo.Arguments = $"-h {host} -p {port} -U {user} -d radionica   -v \"{backupfile}\"";
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.UseShellExecute = false;
            processStartInfo.CreateNoWindow = true;
            processStartInfo.EnvironmentVariables["PGPASSWORD"] = password;



            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo = processStartInfo;
                    //process.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
                    //process.ErrorDataReceived += (sender, args) => Console.WriteLine(args.Data);

                    process.Start();
                    process.BeginErrorReadLine();
                    process.BeginOutputReadLine();
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        MessageBox.Show(getMessage("messRestDatabase"), getMessage("headBackDatabase"), MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(getMessage("messErrRestDatabase"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }


        }


        public void adddColumn(string nameTable, string columnName, string typeValue)
        {
            try
            {

                string deleteDataCommand = $@"
                        DO $$ DECLARE
                        r RECORD;
                        BEGIN
                             ALTER TABLE {nameTable} ADD {columnName} {typeValue};
                        END $$;";

                using (NpgsqlCommand command = new NpgsqlCommand(deleteDataCommand, conn))
                {
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception)
            { }

        }

    }
}
