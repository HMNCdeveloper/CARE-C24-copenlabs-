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

namespace MahAppsExample
{
    class Database
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


        //Visualizar toda una tabla (Seleccionada)
        public DataTable VisualizarBD(string tabla)
        {
            // quite complex sql statement
            sql = "SELECT * FROM " + tabla + ";";
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

        //Funcion para registrar el paciente
        /* public void RegistrarPaciente(string nombre, string apellido1, string apellido2, string email, string tel1, string ext1, string tel2, string ext2, string sexo, string profesion, string titulo, string fechanac, string fpg, string image)
         {
             //Insercion datos del paciente
             sql = "INSERT INTO rad_pacientes(nombre,apellido1,apellido2,email,tel1,ext1,tel2,ext2,sexo,profesion,titulo,fechanacimiento,fpg,img) VALUES('"+nombre+"','"+apellido1+"','"+apellido2+"','"+email+"','"+tel1+"','"+ext1+"','"+tel2+"','"+ext2+"','"+sexo+"','"+profesion+"','"+titulo+"','"+fechanac+"','"+fpg+"','"+image+"')";
             //sql = "INSERT INTO rad_pacientes(nombre,apellido1,apellido2,email,tel1,ext1,tel2,ext2,sexo,profesion,titulo,fechanacimiento,fpg,img) VALUES(:nombre, :apellido1, :apellido2, :email, :tel1, :ext1, :tel2, :ext2, :sexo, :profesion, :titulo, :fechanacimiento, :fpg, :img)";
             command = new NpgsqlCommand(sql, conn);
             command.Parameters.AddWithValue("nombre", nombre);
             command.Parameters.AddWithValue("apellido1", apellido1);
             command.Parameters.AddWithValue("apellido2", apellido2);
             command.Parameters.AddWithValue("email", email);
             command.Parameters.AddWithValue("tel1", tel1);
             command.Parameters.AddWithValue("ext1", ext1);
             command.Parameters.AddWithValue("tel2", tel2);
             command.Parameters.AddWithValue("ext2", ext2);
             command.Parameters.AddWithValue("sexo", sexo);
             command.Parameters.AddWithValue("profesion", profesion);
             command.Parameters.AddWithValue("titulo", titulo);
             command.Parameters.AddWithValue("fechanacimiento", fechanac);
             command.Parameters.AddWithValue("fpg", fpg);
             command.Parameters.AddWithValue("img", image);
             command.ExecuteNonQuery(); //Sin retorno de resp de la consulta
         }*/

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

        //Funcion para obtener los pacientes recientemente registrados Solo 15
        public DataTable Obtener_Analisis_Pacientes_Recientes()
        {
            sql = "SELECT rad_analisis.nombre,rad_analisis.fecha,CONCAT(rad_pacientes.nombre,' ',rad_pacientes.apellido1,' ',rad_pacientes.apellido2) as nombrepaciente from rad_analisis INNER JOIN rad_pacientes ON (rad_analisis.idpaciente=cast(rad_pacientes.idp as text)) ORDER BY fecha";
            NpgsqlDataAdapter da2 = new NpgsqlDataAdapter(sql, conn);
            ds2.Reset();
            da2.Fill(ds2);
            dt2 = ds2.Tables[0];
            return dt2; //regresa tabla con datos del paciente
        }

        //Funcion para obtener los analisis recientemente de acuerdo a nombre del paciente
        public DataTable Obtener_Analisis_Pacientes_Recientes_PorNombrePaciente(string paciente_nombre)
        {
            sql = "SELECT * FROM (SELECT rad_analisis.nombre,rad_analisis.fecha,CONCAT(rad_pacientes.nombre,' ',rad_pacientes.apellido1,' ',rad_pacientes.apellido2) as nombrepaciente from rad_analisis INNER JOIN rad_pacientes ON (rad_analisis.idpaciente=cast(rad_pacientes.idp as text))) as tabla where nombrepaciente=$$"+paciente_nombre+"$$";
            NpgsqlDataAdapter da2 = new NpgsqlDataAdapter(sql, conn);
            ds2.Reset();
            da2.Fill(ds2);
            dt2 = ds2.Tables[0];
            return dt2; //regresa tabla con datos del paciente
        }

        //Funcion para obtener los analisis recientemente de acuerdo a nombre del paciente
        public DataTable Obtener_Analisis_Pacientes_Recientes_PorNombrePaciente2(string paciente_nombre,string nombre_analisis)
        {
            sql = "SELECT * FROM (SELECT rad_analisis.nombre,rad_analisis.fecha,CONCAT(rad_pacientes.nombre,' ',rad_pacientes.apellido1,' ',rad_pacientes.apellido2) as nombrepaciente from rad_analisis INNER JOIN rad_pacientes ON (rad_analisis.idpaciente=cast(rad_pacientes.idp as text))) as tabla where UPPER(nombre) like '$$"+nombre_analisis+"$$' and nombrepaciente=$$" + paciente_nombre + "$$";
            NpgsqlDataAdapter da2 = new NpgsqlDataAdapter(sql, conn);
            ds2.Reset();
            da2.Fill(ds2);
            dt2 = ds2.Tables[0];
            return dt2; //regresa tabla con datos del paciente
        }

        //Funcion para buscar analisis en los pacientes recientemente registrados 
        /*public DataTable Buscar_Analisis(string nombre)
        {
            sql = "SELECT * FROM (SELECT rad_analisis.nombre,rad_analisis.fecha,CONCAT(rad_pacientes.nombre,' ',rad_pacientes.apellido1,' ',rad_pacientes.apellido2) as nombrepaciente from rad_analisis INNER JOIN rad_pacientes ON (rad_analisis.idpaciente=cast(rad_pacientes.idp as text))) as Tabla WHERE nombre LIKE '$$"+nombre+"$$'";
            NpgsqlDataAdapter da2 = new NpgsqlDataAdapter(sql, conn);
            ds2.Reset();
            da2.Fill(ds2);
            dt2 = ds2.Tables[0];
            return dt2; //regresa tabla con datos del paciente
        }*/

        public DataTable Buscar_Analisis(string nombre)
        {
            sql = "SELECT * FROM (SELECT rad_analisis.nombre, rad_analisis.fecha, CONCAT(rad_pacientes.nombre,' ',rad_pacientes.apellido1,' ',rad_pacientes.apellido2) as nombrepaciente FROM rad_analisis INNER JOIN rad_pacientes ON (rad_analisis.idpaciente=cast(rad_pacientes.idp as text))) as Tabla WHERE UPPER(nombre) LIKE '%' || @nombre || '%'";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@nombre", nombre);

            NpgsqlDataAdapter da2 = new NpgsqlDataAdapter(cmd);
            ds2.Reset();
            da2.Fill(ds2);
            dt2 = ds2.Tables[0];

            return dt2; // regresa tabla con datos del paciente
        }

        //Funcion para buscar terapia de color en las registradas
        public DataTable Buscar_Terapia(string nombre)
        {
            sql = "select nombre,fechac from rad_remedios where nombre like '%Color%' and nombre like '%"+nombre+"%'";
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

        //Funcion para contar registro de version
        public object Validar_RegistroVersion()
        {
            sql = "SELECT count(*) from rad_rversion";
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
            sql = "SELECT count(*) from rad_codigosdeanalisis WHERE ida=$$" + id_analisis + "$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar(); //Valor del id_paciente lo regresa como objeto
        }


        //Funcion para obtener ida del analisis para obtener sus codigos de analisis en base a id analisis
        public object Obtener_Id_Analisis(string nombre)
        {
            sql = "select ida from rad_analisis where nombre=$$"+ nombre + "$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar(); 
        }

        //Funcion para obtener codigos del analisis
        public DataTable Obtener_CodigosAnalisis(string id_analisis)
        {
            sql = "select * from rad_codigosdeanalisis where ida=$$"+id_analisis+"$$";
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
            sql = "SELECT idp, CONCAT(nombre, ' ', apellido1, ' ', apellido2) AS nombrepaciente FROM rad_pacientes WHERE UPPER(CONCAT(nombre, ' ', apellido1, ' ', apellido2)) LIKE '%" + paciente +  "%'ORDER BY nombre";
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
            sql = "select nombre,apellido1,apellido2,sexo from rad_pacientes order by nombre";
            //command = new NpgsqlCommand(sql, conn);
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt; //regresa tabla con datos del paciente
        }

        //Funcion que despliega los nombres de los pacientes
        public DataTable Mostrar_Pacientes_Listado_Sencillo_2(string nombre_paciente)
        {
            sql = "select concat(nombre,' ',apellido1,' ',apellido2) as nombre from rad_pacientes where UPPER(nombre) like $$%"+nombre_paciente+"%$$";
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
            sql = "SELECT idpaciente,ida,nombre,fecha from rad_analisis WHERE idpaciente=$$" + paciente + "$$";
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
            sql = "Select idp from(SELECT idp, fpg, CONCAT(nombre, ' ', apellido1, ' ', apellido2) as nombrepaciente from rad_pacientes) as Tabla where nombrepaciente like $$%" + nombre+"%$$";
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
            sql = "select * from rad_tratamientosadistancia where estado=1 and duracion != tiempoemitido";
            //command = new NpgsqlCommand(sql, conn);
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt; //Valor del id_paciente lo regresa como objeto
        }

        //Funcion para cargar el listado de tratamientos en espera
        public DataTable Tratamientos_Espera()
        {
            sql = "select * from rad_tratamientosadistancia where estado=0";
            //command = new NpgsqlCommand(sql, conn);
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt; //Valor del id_paciente lo regresa como objeto
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
            sql = "select * from rad_tratamientosadistancia where idpadre=$$"+id_tratamiento+"$$";
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
            sql = "delete from rad_tratamientosadistancia where estado=0 and (nombre=$$" + nombreTratamiento + "$$ and inicio=$$"+Fecha+"$$)";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        //Funcion para eliminar tratamiento padre
        public void Eliminar_TratamientoPadre(string nombreTratamientoP)
        {
            sql = "delete from rad_tratamientosadistancia where estado=1 and nombre=$$"+nombreTratamientoP+ "$$ and idpadre=''";
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
        public void RegistrarAnalisisPaciente_Diag(string id_paciente, string nombre, DateTime fecha, int tipoanalisis, int analizado, int reanalizar, DateTime fechac)
        {
            sql = "INSERT INTO rad_analisis(idpaciente, nombre, fecha, tipoanalisis, analizado, reanalizar, fechac) VALUES(@id_paciente, @nombre, @fecha, @tipoanalisis, @analizado, @reanalizar, @fechac)";
            command = new NpgsqlCommand(sql, conn);

            command.Parameters.AddWithValue("@id_paciente", id_paciente);
            command.Parameters.AddWithValue("@nombre", nombre);
            command.Parameters.AddWithValue("@fecha", fecha);
            command.Parameters.AddWithValue("@tipoanalisis", tipoanalisis);
            command.Parameters.AddWithValue("@analizado", analizado);
            command.Parameters.AddWithValue("@reanalizar", reanalizar);
            command.Parameters.AddWithValue("@fechac", fechac);

            command.ExecuteNonQuery();
        }


        //Funcion para registrar version del software
        public void RegistrarVersion(string nombre, string descripcion)
        {
            sql = "INSERT INTO rad_rversion(nombre,descripcion) VALUES($$" + nombre + "$$,$$" + descripcion + "$$)";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        //Funcion para registrar reanalisis del paciente  
        public void RegistrarReAnalisisPaciente_Diag(string id_paciente, string nombre, DateTime fecha, int tipoanalisis, int analizado, int reanalizar, DateTime fechac, string padre)
        {
            sql = "INSERT INTO rad_analisis(idpaciente, nombre, fecha, tipoanalisis, analizado, reanalizar, fechac, padre) VALUES(@id_paciente, @nombre, @fecha, @tipoanalisis, @analizado, @reanalizar, @fechac, @padre)";

            command = new NpgsqlCommand(sql, conn);

            command.Parameters.AddWithValue("@id_paciente", id_paciente);
            command.Parameters.AddWithValue("@nombre", nombre);
            command.Parameters.AddWithValue("@fecha", fecha);
            command.Parameters.AddWithValue("@tipoanalisis", tipoanalisis);
            command.Parameters.AddWithValue("@analizado", analizado);
            command.Parameters.AddWithValue("@reanalizar", reanalizar);
            command.Parameters.AddWithValue("@fechac", fechac);
            command.Parameters.AddWithValue("@padre", padre);

            command.ExecuteNonQuery();
        }


        //Funcion para registrar domicilios del paciente
        public void  RegistrarDomicilios(string calle, string numero, string colonia, string cp, string municipio, string estado, string pais, string id_paciente)
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
            sql = "SELECT * FROM rad_remedios where nombre=$$"+ nombre_remedio +"$$";
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

        //Funcion para consultar si la categoria ya esta registrada
        public object Consultar_CategoriasCodigos(string nombre_de_categoria)
        {
            sql = "SELECT count(*) FROM rad_categorias where categoria=$$" + nombre_de_categoria + "$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar(); //Valor del id_paciente lo regresa como objeto
        }

        //Funcion para registrar remedio duplicado
        public void Registrar_Remedio_Duplicado(string id_generado, string nombre, string idpaciente, string nombrepaciente, string idanalisis, string nombreanalisis, DateTime fechac, string codigo)
        {
            sql = "INSERT INTO rad_remedios(idr, nombre, idpaciente, nombrepaciente, idanalisis, nombreanalisis, fechac, codigo) VALUES(@id_generado, @nombre, @idpaciente, @nombrepaciente, @idanalisis, @nombreanalisis, @fechac, @codigo)";

            command = new NpgsqlCommand(sql, conn);

            command.Parameters.AddWithValue("@id_generado", id_generado);
            command.Parameters.AddWithValue("@nombre", nombre);
            command.Parameters.AddWithValue("@idpaciente", idpaciente);
            command.Parameters.AddWithValue("@nombrepaciente", nombrepaciente);
            command.Parameters.AddWithValue("@idanalisis", idanalisis);
            command.Parameters.AddWithValue("@nombreanalisis", nombreanalisis);
            command.Parameters.AddWithValue("@fechac", fechac);
            command.Parameters.AddWithValue("@codigo", codigo);

            command.ExecuteNonQuery();
        }


        //Funcion para registrar categoria nueva
        public void Registrar_CategoriaNueva(string id_generado, string nombre)
        {
            sql = "INSERT INTO rad_categorias(idcategoria,categoria) VALUES($$" + id_generado + "$$,$$" + nombre + "$$)";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        //Funcion para registrar categoria nueva
        public void Registrar_SubCategoriaNueva(string id_generado, string nombre,string idcp)
        {
            sql = "INSERT INTO rad_categorias(idcategoria,categoria,idcp) VALUES($$" + id_generado + "$$,$$" + nombre + "$$,$$" + idcp + "$$)";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        //Funcion para registrar nuevo tratamiento a distancia

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
            sql = "INSERT INTO rad_codigosdetratamientos(idcr,descripcion,tipo) VALUES($$" + idt + "$$,$$" + descripcion +"$$,$$" + tipo + "$$)";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        //Funcion para obtener Id del tratamiento
        public object Obtener_IDTratamiento(string id_paciente,string nombre_paciente,string nombre_tratamiento)
        {
            sql = "SELECT idt FROM rad_tratamientosadistancia where idpaciente=$$" + id_paciente + "$$ and nombrepaciente=$$"+nombre_paciente+"$$ and nombre=$$"+nombre_tratamiento+"$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar(); //Valor del id_paciente lo regresa como objeto
        }

        //Funcion para obtener los codigos de un tratamiento 
        public DataTable CodigosTratamiento(string idt)
        {
            sql = "select * from rad_codigosdetratamientos where idcr=$$"+idt+"$$";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }

        //Funcion para comprobar si el nombre del tratamiento ya esta en uso!...
        public object Comprobar_NombreTratamiento(string nombre_tratamiento)
        {
            sql = "SELECT count(*) FROM rad_tratamientosadistancia where nombre like $$%"+nombre_tratamiento+"%$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar(); //Valor del id_paciente lo regresa como objeto
        }

        //Funcion para eliminar una categoria
        public void Eliminar_Categoria(string nombre)
        {
            sql = "DELETE FROM rad_categorias WHERE categoria=$$"+nombre+"$$";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        //Funcion para eliminar codigos de un diagnostico a modificar
        public void Eliminar_Codigos(string id_analisis)
        {
            sql = "DELETE FROM rad_codigosdeanalisis WHERE ida=$$" + id_analisis + "$$";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        //Funcion para eliminar una sub-categoria
        public void Eliminar_SubCategoria(string nombre,string idcp)
        {
            sql = "DELETE FROM rad_categorias WHERE categoria=$$" + nombre + "$$ and idcp=$$"+idcp+"$$";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        //Funcion para eliminar un código
        public void Eliminar_Codigo(string nombre)
        {
            sql = "DELETE FROM rad_codigos where codigo LIKE'"+nombre+"'";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        //Funcion para guardar tarjeta en categorias
        public void Registrar_Tarjeta_Categorias(string id_generado,string nombre_tarjeta,string codigo_generado)
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

            command.CommandText = "DELETE FROM rad_analisis WHERE idpaciente=$$" + id_paciente + "$$";
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

        //Funcion que modifica el registro del domicilio de un paciente
        public void ModificarDomicilioPaciente(string calle, string numero, string colonia, string CP, string municipio, string estado, string pais, string id_domicilio)
        {
            sql = "UPDATE rad_domicilios SET calle=$$" + calle + "$$, numero=$$" + numero + "$$, colonia=$$" + colonia + "$$, cp=$$" + CP + "$$, municipio=$$" + municipio + "$$, estado=$$" + estado + "$$, pais=$$" + pais + "$$ WHERE iddom=$$" + id_domicilio + "$$";
            // sql = "UPDATE rad_pacientes SET nombre='" + nombre + "',apellido1='" + apellido1 + "',apellido2='" + apellido2 + "',email='" + email + "',sexo='" + sexo + "',profesion='" + profesion + "',titulo='" + titulo + "',fechanacimiento='" + fechanac + "',fpg='" + fpg + "' WHERE idp='" + id_paciente + "'";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        //Funcion que modifica el estado del analisis
        public void Modificar_Estado_Analisis_Analizado(string id_analisis)
        {
            sql = "UPDATE rad_analisis SET analizado = '1' WHERE ida = $$"+id_analisis+"$$";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        //Funcion para eliminar el registro de un domicilio
        public void EliminarDomicilioPaciente(string id_dom)
        {
            sql = "DELETE FROM rad_domicilios WHERE iddom=$$" + id_dom + "$$";
            // sql = "UPDATE rad_pacientes SET nombre='" + nombre + "',apellido1='" + apellido1 + "',apellido2='" + apellido2 + "',email='" + email + "',sexo='" + sexo + "',profesion='" + profesion + "',titulo='" + titulo + "',fechanacimiento='" + fechanac + "',fpg='" + fpg + "' WHERE idp='" + id_paciente + "'";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        //Funcion que modifica el registro del telefono
        public void ModificarTelefonosPaciente(string telefono, string extension, string id_telefono)
        {
            //INSERT INTO rad_telefonos(numero,extension,idobj) VALUES('
            sql = "UPDATE rad_telefonos SET numero=$$" + telefono + "$$,extension=$$" + extension + "$$ WHERE idtel=$$" + id_telefono + "$$";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        //Funcion que elimina el registro del telefono
        public void EliminarTelefonosPaciente(string telefono, string extension)
        {
            //INSERT INTO rad_telefonos(numero,extension,idobj) VALUES('
            sql = "DELETE FROM rad_telefonos WHERE numero=$$" + telefono + "$$ and extension=$$" + extension + "$$";
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
            command.CommandText = "DELETE FROM rad_codigosdeanalisis where ida=$$" + id_analisis.ToString() + "$$";
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

        //Funcion que elimina un remedio elegido por idr (id_remedio)
        public void Eliminar_remedio_Id(string id_rem)
        {
           //Elimina los colores de codigosderemedios
            sql = "DELETE FROM rad_codigosderemedios WHERE idr=$$" + id_rem + "$$";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();

            //Elimina la terapia de remedios
            command.CommandText = "DELETE FROM rad_remedios WHERE idr= $$" + id_rem + "$$";
            command.ExecuteNonQuery();

        }

        //Funcion que modifica el registro del paciente de uno actual
        public void ModificarAntecedentesPaciente()
        {
            // sql = "UPDATE rad_pacientes SET nombre='" + nombre + "',apellido1='" + apellido1 + "',apellido2='" + apellido2 + "',email='" + email + "',sexo='" + sexo + "',profesion='" + profesion + "',titulo='" + titulo + "',fechanacimiento='" + fechanac + "',fpg='" + fpg + "' WHERE idpaciente='" + id_paciente + "'";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        //Funcion para visualizar categorias de los codigos
        public DataTable VisualizarCategoriasCodigos()
        {
            sql = "SELECT * FROM rad_categorias where idcp='' order by categoria";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }

        //Funcion para visualizar categorias de los codigos
        public DataTable VisualizarCategoriasCodigos2()
        {
            sql = "SELECT * FROM rad_categorias where idcp='' or idcp is null order by categoria";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }


        //Funcion para buscar categorias de los codigos
        public object BuscarCategoriasCodigos(string nombre_categoria)
        {
            sql = "SELECT idcategoria FROM rad_categorias where categoria=$$" + nombre_categoria + "$$ order by categoria";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar();
        }

        //Funcion para buscar categorias de los codigos
        public object BuscarCategoriasCodigosSub(string nombre_categoria, string idcp)
        {
            sql = "SELECT idcategoria FROM rad_categorias where categoria=$$" + nombre_categoria + "$$ and idcp=$$" + idcp + "$$ order by categoria";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar();
        }

        //Funcion para visualizar sub-categorias de los codigos
        public DataTable VisualizarSubCategoriasCodigos(string categoria)
        {
            sql = "SELECT categoria,idcp FROM rad_categorias where idcp=$$" + categoria + "$$ order by categoria";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }

        //Funcion para visualizar sub-categorias de los codigos v2
        public DataTable VisualizarSubCategoriasCodigos2(string categoria)
        {
            sql = "SELECT * FROM rad_codigos where idcat=$$" + categoria + "$$ order by nombre";
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
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }

        //Funcion para buscar codigo en base al nombre de la categoria
        public object BuscarCodigoPorNombreCategoria(string nombre_categoria)
        {
            sql = "SELECT codigo FROM rad_codigos where UPPER(nombre)=$$" + nombre_categoria + "$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar();
        }

        //Funcion para visualizar codigos con el genero del paciente y el genero todos
        public DataTable VisualizarSubCategoriasCodigosListadoGenero_Todos(string categoria, string genero_paciente)
        {
            // sql = "SELECT * FROM rad_codigos where idcat='" + categoria + "' order by nombre";
            sql = "SELECT * FROM rad_codigos where idcat=$$" + categoria + "$$ and (genero=$$" + genero_paciente + "$$ or genero='T') order by nombre";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }

        //Funcion para visualizar codigos con el genero del paciente y el genero todos sin categoria en particular
        public DataTable VisualizarCodigosListadoGenero_Todos(string genero_paciente)
        {
            // sql = "SELECT * FROM rad_codigos where idcat='" + categoria + "' order by nombre";
            sql = "SELECT * FROM rad_codigos where (genero=$$" + genero_paciente + "$$ or genero='T') order by nombre";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }

        //Funcion para buscar codigos directamente
        public DataTable BuscarCodigo(string nombre_codigo)
        {
            //sql = "SELECT nombre,codigo FROM rad_codigos WHERE UPPER(nombre) LIKE $$%" + nombre_codigo + "%$$";
            sql = "SELECT rc.codigo, rc.nombre, cat.categoria FROM rad_codigos rc JOIN rad_categorias cat ON rc.idcat = cat.idcategoria WHERE UPPER(rc.nombre) LIKE '%" + nombre_codigo + "%'";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }

        public DataTable BuscarCategoriaCodigo(string nombre_codigo)
        {
            //sql = "SELECT nombre,codigo FROM rad_codigos WHERE UPPER(nombre) LIKE $$%" + nombre_codigo + "%$$";
            sql = "SELECT rc.codigo, rc.nombre, cat.categoria FROM rad_codigos rc JOIN rad_categorias cat ON rc.idcat = cat.idcategoria WHERE UPPER(rc.nombre) LIKE '%" + nombre_codigo + "%'";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }

        public string Categoria(string id)
        {
            string categoria = null;
            sql = "SELECT categoria FROM rad_categorias WHERE idcategoria = '" + id + "'";
            using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
            {
                categoria = cmd.ExecuteScalar()?.ToString();
            }
            return categoria;
        }



        public DataTable BuscarCodigoRem(string nombre_codigo)
        {
            sql = "SELECT nombrecodigo,codigo FROM rad_codigosderemedios WHERE UPPER(nombrecodigo) LIKE $$%" + nombre_codigo + "%$$";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }

        public string ObtenerIdr(string nombre)
        {
            sql = "SELECT idr FROM rad_remedios WHERE nombre LIKE @nombre";
            command = new NpgsqlCommand(sql, conn);
            command.Parameters.AddWithValue("@nombre", nombre);

            // Ejecutar la consulta y obtener el resultado como objeto
            object result = command.ExecuteScalar();

            // Comprobar si el resultado no es nulo y convertirlo a string
            if (result != null && result != DBNull.Value)
            {
                return result.ToString();
            }
            else
            {
                return null; // O un valor predeterminado, dependiendo de tus necesidades
            }
        }
        public DataTable BuscarCodigoRem1(string idr)
        {
            sql = "SELECT nombrecodigo, codigo FROM rad_codigosderemedios WHERE idr LIKE '%' || @idr || '%'";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            da.SelectCommand.Parameters.AddWithValue("@idr", idr);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }


        //Funcion para buscar remedios directamente
        /*public DataTable BuscarRemedioPorNombre(string nombre)
        {
            sql = "SELECT nombre FROM rad_codigos WHERE nombre LIKE '%" + nombre_codigo + "%'";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }*/

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

        //Funcion para visualizar codigos de las sub-categorias
        public DataTable VisualizarCodigos(string categoria)
        {
            sql = "SELECT * FROM (SELECT * FROM rad_codigos where idcatp=$$" + categoria + "$$ and genero='T' and genero=<GENERO DEL PACIENTE> order by nombre) as Tabla where nombre like 'F%' order by nombre";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }

        //Funcion para obtener el id_paciente desde rad_analisis
        public object Obtener_IdPaciente_Analisis(string nombre_analisis)
        {
            sql = "SELECT idpaciente from rad_analisis where nombre=$$" + nombre_analisis + "$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar(); //Valor del id_paciente lo regresa como objeto
        }

        //Funcion para obtener el genero del paciente
        public object Obtener_Genero_Paciente(string id_paciente)
        {
            sql = "SELECT sexo from rad_pacientes where idp=$$" + id_paciente + "$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar(); //Valor del id_paciente lo regresa como objeto
        }

        //Funcion para validar si el analisis ha sido procesado
        public object Validar_Analisis_Analizado(string nombre_analisis)
        {
            sql = "SELECT analizado from rad_analisis where nombre =$$" + nombre_analisis + "$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar(); //Valor del id_paciente lo regresa como objeto
        }

        //Funcion para validar si ya fue reanalizado un analisis en base a su nombre y el campo de reanalizado
        public DataTable Validar_Reanalisis_Hecho(string nombre_analisis)
        {
            sql = "SELECT reanalizar from rad_analisis where nombre =$$" + nombre_analisis + "$$";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }

        //Buscar Id_codigo en base al codigo
        public object Buscar_IdCodigo_Codigo(string codigo)
        {
            sql = "SELECT idcodigo from rad_codigos where codigo=$$" + codigo + "$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar(); //Valor del id_paciente lo regresa como objeto
        }

        //Buscar Id_analisis en base al nombre del analisis
        public object Buscar_IdAnalisis_Nombre(string nombre_analisis)
        {
            sql = "SELECT ida from rad_analisis where nombre=$$" + nombre_analisis + "$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar(); //Valor del id_paciente lo regresa como objeto
        }

        //Funcion para registrar nuevas categorias y subcategorias en el database control
        public void Registrar_Categorias(string id_codigo, string nombre, string idcp)
        {
            sql = "INSERT INTO rad_categorias(idcategoria,categoria,idcp) VALUES($$" + id_codigo + "$$,$$" + nombre + "$$,$$" + idcp + "$$)";
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

        //Funcion para cargar el listado de tratamientos activos
        public DataTable Tratamientos_Inactivos()
        {
            sql = "SELECT * FROM rad_tratamientosadistancia WHERE estado = 0 LIMIT 30";
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
            sql = "select idcategoria from rad_categorias where categoria=$$" + nombre + "$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar();
        }

        //Buscar fecha del remedio en base al nombre del remedio
        public object Buscar_Fecha_Remedio(string nombre)
        {
            sql = "SELECT fechac from rad_remedios where nombre=$$" + nombre + "$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar(); //Valor del id_paciente lo regresa como objeto
        }

        //Buscar genero en base a id_subcategoria y id_categoria 
        public object Buscar_Genero(string id_subcategoria, string id_categoria)
        {
            sql = "select genero from rad_codigos where idcat=$$"+id_subcategoria+"$$ or idcatp=$$"+id_categoria+"$$ limit 1;";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar(); //Valor del id_paciente lo regresa como objeto
        }

        //Guardar el registro en rad_codigosdeanalisis
        public void Registrar_Codigo_de_Analisis(string id_analisis,string id_codigo,string codigo, string nombrecodigo,string valor,string niveles,string sugerido)
        {
            sql = "INSERT INTO rad_codigosdeanalisis(ida,idcodigo,codigo,nombrecodigo,nivel,nivelsugerido,valor) VALUES($$" + id_analisis + "$$,$$" + id_codigo + "$$,$$" + codigo + "$$,$$" + nombrecodigo + "$$,$$" + niveles + "$$,$$" + sugerido + "$$,$$" + valor + "$$)";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        //Funcion que actualiza rad_analisis agregando lo de la informacion biologica
        public void Registrar_Informacion_Biologica(double [] infobio,string nombre_analisis)
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

            sql = "UPDATE rad_analisis SET estatura=$$" + infobio[0].ToString() + "$$, presionsisto=$$" + infobio[1].ToString() + "$$, imc=$$"+ infobio[2].ToString() + "$$, fr=$$"+ infobio[3].ToString() + "$$, ta=$$"+ infobio[4].ToString() + "$$, peso=$$"+ infobio[5].ToString() + "$$, presiondias=$$"+ infobio[6].ToString() + "$$, fc=$$" + infobio[7].ToString() + "$$, valorini=$$" + infobio[8].ToString() + "$$ WHERE nombre=$$" + nombre_analisis + "$$";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        public void Registrar_Padecimiento_Analisis(string padecimiento, string interrogatorio_por_aparatos, string nombre_analisis)
        {
            sql = "UPDATE rad_analisis SET padecimientoactual=$$" + padecimiento + "$$, intporaparatos=$$" + interrogatorio_por_aparatos + "$$ WHERE nombre=$$" + nombre_analisis + "$$";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        //Funcion para visualizar los analisis de acuerdo al sexo del paciente y buscar los codigos de acuerdo a el
        public DataTable VisualizarAnalisisPorGenero()
        {
            sql = "select sexo,rad_analisis.nombre from rad_pacientes INNER JOIN rad_analisis ON cast(idp as text)=idpaciente";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }

        //Funcion para visualizar los analisis de acuerdo al sexo del paciente y buscar los codigos de acuerdo a el
        public DataTable VisualizarAnalisisPorGenero2(string nombre_paciente)
        {
            sql = "select * from (SELECT sexo, CONCAT(nombre,' ', apellido1, ' ', apellido2) as nombrepaciente from rad_pacientes) as tabla where nombrepaciente like $$%"+nombre_paciente+"%$$";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
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
        public DataTable VisualizarRemedios()
        {
            sql = "select * from  rad_remedios Order by nombre";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }

        //Funcion para visualizar remedios directamente dependiendo de la categoria y la letra
        public DataTable CodigoRemedioBuscado(string nombre)
        {
            sql = "select * from rad_remedios where nombre like $$%" + nombre + "%$$";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }

        //Funcion para obtener padecimiento de un analisis
        public DataTable Obtener_InfoPadecimiento(string nombre_analisis)
        {
            sql = "select padecimientoactual,intporaparatos from rad_analisis where nombre=$$"+nombre_analisis+"$$";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }

        //Funcion para obtener la informacion biologica de un analisis
        public DataTable Obtener_InfoBiologica(string nombre_analisis)
        {
            sql = "select estatura, presionsisto, imc, fr, ta, peso, presiondias,fc,valorini from rad_analisis where nombre=$$" + nombre_analisis + "$$";
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

        //Funcion para visualizar remedios directamente dependiendo por el giro
        public DataTable VisualizarRemediosGenerales_Usuarios()
        {
            sql = "select * from rad_remedios where codigo!=''";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }

        //Funcion para visualizar remedios directamente creados por el usuario
        public DataTable VisualizarRemediosCreados_Usuario()
        {
            sql = "select * from rad_remedios where codigo!='' and idpaciente='' and nombrepaciente='' and idanalisis='' and nombreanalisis=''";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }

        //Funcion para obtener el id del remedio en base al nombre
        public object Obtener_IdRemedio(string nombre)
        {
            sql = "SELECT idr from rad_remedios where nombre=$$" + nombre + "$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar(); //Valor del id_paciente lo regresa como objeto
        }

        //Funcion para obtener el id del remedio en base al nombre y a la fecha
        public object Obtener_IdRemedio_ConFecha(string nombre, string fecha)
        {
            sql = "SELECT idr from rad_remedios where nombre=$$" + nombre + "$$ and fechac=$$"+fecha+"$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar(); //Valor del id_paciente lo regresa como objeto
        }

        //Funcion para obtener el id del paciente en base al nombre completo
        public object Obtener_IdPaciente_NombreCompleto(string nombre)
        {
            sql = "select idp from (select idp,concat(nombre,' ',apellido1,' ',apellido2) from rad_pacientes) as tabla where concat like $$"+nombre+"%$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar();
        }

        //Funcion para obtener el id del tratamiento para utilizarlo como Padre
        public object Obtener_IdTratamiento(string id_paciente, string nombre_paciente, string nombre_tratamiento)
        {
            sql = "select idt from rad_tratamientosadistancia where idpaciente=$$"+id_paciente+"$$ and nombrepaciente=$$"+nombre_paciente+"$$ and nombre=$$"+nombre_tratamiento+"$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar();
        }

        //Funcion para visualizar los codigos del remedio directamente dependiendo del id_remedio
        public DataTable VisualizarCodigos_Remedios_IdRemedio(string id_remedio)
        {
            sql = "select codigo,nombrecodigo,potencia,metodo,codigocomplementario,nivel from rad_codigosderemedios where idr=$$"+ id_remedio + "$$";
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
            sql = "select codigo,nombrecodigo,idcodigo,potencia,metodo,codigocomplementario,nivel from rad_codigosderemedios where idr=$$" + id_remedio + "$$";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }


        //Funcion para registrar codigos de remedio de un remedio duplicado
        public void Registrar_CodigosdeRemedios(string idcr, string idr, string codigo, string codigocomplementario, string nombrecodigo, string idcodigo, string potencia, string metodo, string nivel)
        {
            sql = "INSERT INTO rad_codigosderemedios(idcr,idr,codigo,codigocomplementario,nombrecodigo,idcodigo,potencia,metodo,nivel) VALUES($$" + idcr + "$$,$$" + idr + "$$,$$" + codigo + "$$,$$" + codigocomplementario + "$$,$$" + nombrecodigo + "$$,$$" + idcodigo + "$$,$$" + potencia + "$$,$$" + metodo + "$$,$$" + nivel + "$$)";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        //Guardar el registro en rad_remedios
        public void Registrar_Remedio_Diagnostico(string idr, string nombre, string id_paciente, string nombre_paciente, string id_analisis, string nombre_analisis, DateTime fecha, string codigo)
        {
            sql = "INSERT INTO rad_remedios(idr, nombre, idpaciente, nombrepaciente, idanalisis, nombreanalisis, fechac, codigo) VALUES(@idr, @nombre, @id_paciente, @nombre_paciente, @id_analisis, @nombre_analisis, @fecha, @codigo)";

            command = new NpgsqlCommand(sql, conn);

            command.Parameters.AddWithValue("@idr", idr);
            command.Parameters.AddWithValue("@nombre", nombre);
            command.Parameters.AddWithValue("@id_paciente", id_paciente);
            command.Parameters.AddWithValue("@nombre_paciente", nombre_paciente);
            command.Parameters.AddWithValue("@id_analisis", id_analisis);
            command.Parameters.AddWithValue("@nombre_analisis", nombre_analisis);
            command.Parameters.AddWithValue("@fecha", fecha);
            command.Parameters.AddWithValue("@codigo", codigo);

            command.ExecuteNonQuery();
        }

        //Funcion para visualizar los codigos de un analisis en el remedio directamente
        public DataTable VisualizarCodigos_Remedios_de_Analisis(string id_analisis)
        {
            sql = "select codigo,nombrecodigo,nivel from rad_codigosdeanalisis where ida=$$" + id_analisis + "$$";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn);
            ds.Reset();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }

        //Funcion que elimina los codigos resultado de un analisis..
        public void Eliminar_CodigosAnalisis(string id_analisis)
        {
            //INSERT INTO rad_telefonos(numero,extension,idobj) VALUES('
            sql = "DELETE FROM rad_codigosdeanalisis WHERE ida=$$" + id_analisis + "$$";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }


        //Funcion que elimina los codigos de la bd
        public void Eliminar_CodigosCategorias(string nombre_rate, string categoria)
        {
            //INSERT INTO rad_telefonos(numero,extension,idobj) VALUES('
            sql = "DELETE FROM rad_codigos where nombre=$$"+nombre_rate+"$$ and idcat=$$"+categoria+ "$$";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        //Funcion para eliminar los codigos de un remedio sin borrar la raiz
        public void Eliminar_codigos_remedio(string id_remedio)
        {
            //Elimina los colores de codigosderemedios
            sql = "DELETE FROM rad_codigosderemedios WHERE idr=$$" + id_remedio + "$$";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }


        public void Alterar_IdAnalisis(string id_analisis, string nombre_remedio)
        {
            sql = "UPDATE rad_remedios SET idanalisis=$$" + id_analisis + "$$ WHERE nombre=$$" + nombre_remedio + "$$";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }
        public object Obtener_IdRemediosCodigos(string idr)
        {
            sql = "select idanalisis from rad_remedios where idr=$$" + idr + "$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar();
        }

       

        //Guardar remedio para terapia de color
        public void Registrar_Remedio_Color(string idr, string nombre, DateTime fecha)
        {
            sql = "INSERT INTO rad_remedios(idr, nombre, fechac) VALUES(@idr, @nombre, @fecha)";

            command = new NpgsqlCommand(sql, conn);

            command.Parameters.AddWithValue("@idr", idr);
            command.Parameters.AddWithValue("@nombre", nombre);
            command.Parameters.AddWithValue("@fecha", fecha);

            command.ExecuteNonQuery();
        }


        //Obtiene codigo del color en base a tabla y color elegido
        public object Obtener_Color_ParteTerapia(string color)
        {
            sql = "select codigo from rad_colordeterapia where upper(texto)=$$"+color+"$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar(); 
        }

        //Obtiene codigo de la parte elegida en la terapia de color
        public object Obtener_CodigoParte_Color_ParteTerapia(string parte)
        {
            sql = "select codigo from rad_codigos where nombre like $$%"+parte+"%$$ order by length(codigo) desc";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar(); 
        }

        //Funcion para registrar codigo nuevo paciente
        public void Registrar_Codigo_Categorias(string id_codigo,string nombre,string codigo, string description,string id_subcategoria, string id_categoria, string genero)
        {
            sql = "INSERT INTO rad_codigos(idcodigo,nombre,codigo,descripcion,idcat,idcatp,genero) values($$" + id_codigo + "$$,$$" + nombre + "$$,$$" + codigo + "$$,$$" + description + "$$,$$" + id_subcategoria + "$$,$$" + id_categoria + "$$,$$" + genero + "$$)";
            command = new NpgsqlCommand(sql, conn);
            command.ExecuteNonQuery();
        }

        //Visualizar tabla
        public DataTable CategoriasTabla()
        {
            string sql = "";

            // quite complex sql statement
            sql = "SELECT DISTINCT idcp, categoria FROM rad_categorias;";
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


        public static string db = "rad_es";

        //Conectar a la bd
        public bool ConexionBD(string user, string password)
        {
            //Conexion
            constring = String.Format("Server=localhost;Port=5433;" +
            "User Id={0};Password={1};Database={2}", user, password, Database.db);
            conn = new NpgsqlConnection(constring);
            conn.Open();
            return true;
        }

        //Funcion para obtener del remedio
        public object Obtener_Id_Remedio(string nombre)
        {
            sql = "select idr from rad_remedios where nombre=$$" + nombre + "$$";
            command = new NpgsqlCommand(sql, conn);
            return command.ExecuteScalar();
        }

        //Cerrar conexion a la bd
        public void CerrarBD()
        {
            conn.Close();
        }

        //Imagen a bytes (Conversion)
        public byte[] ConvertirImagen(System.Drawing.Image x)
        {
            ImageConverter _imageConverter = new ImageConverter();
            byte[] xByte = (byte[])_imageConverter.ConvertTo(x, typeof(byte[]));
            return xByte; //Regresa imagen convertida
        }

        public static bool ExisteCodigo(string codigo, NpgsqlConnection conn)
        {
            string query = "SELECT COUNT(*) FROM rad_codigos WHERE codigo = @codigo";
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
        public void UploadBackup(string backupFilePath,string message,string title)
        {
            try
            {
              //here, in this code line, we are saving the content from the sql file
              string backupContent = System.IO.File.ReadAllText(backupFilePath);
                    
              //after it, the next line will be executed all neccesary queries, the DB will need to work as well with the system
              using (var cmd = new NpgsqlCommand(backupContent, conn))
              {
                 cmd.ExecuteNonQuery();
                 Console.WriteLine("Backup restaurado exitosamente.");
                 MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information); 
               
                // Remove the file when the precess have been successfull
                System.IO.File.Delete(backupFilePath);
              }     
            
            } catch (Exception ex)
            {
                Console.WriteLine($"Error al restaurar el backup: {ex.Message}");
            }

        }

    }
}
