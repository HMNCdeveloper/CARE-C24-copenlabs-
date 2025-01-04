using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;


namespace MahAppsExample
{
    //Uso de la maquina
    public class MachineP
    {
        string[] ports; //COMs (Puertos)
        SerialPort port; //Puerto

        private readonly object bufferLock = new object();
        private string buffer { get; set; } //Buffer del puerto
        string buffercopy; //Global
        private SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        //Constructor (Machine)
        public MachineP()
        {
            //Obtiene todos los puertos COM
            ports = SerialPort.GetPortNames();
        }

        //Funcion revisar COMS para la deteccion de la maquina (Mantra, MXP, MXD, etc)
        public string Machine_Detection(string puerto)
        {

            //Reconoce que hay maquina COM conectada
            if (ports.Length == 0)
            {

                MessageBoxResult result = MessageBox.Show("There's no instrument connected!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                if (result == MessageBoxResult.OK)
                {
                    //Cierra la aplicacion
                    var window = Application.Current.Windows[0];
                    window.Close();
                }
            }
            else //Si si hay COM's
            {
                try
                {


                    port = new SerialPort(puerto, 9600, Parity.None, 8, StopBits.One); //Puerto
                    port.ReadTimeout = 5; //Tiempo de respuesta
                    port.DataReceived += new SerialDataReceivedEventHandler(read_serialport); //Manejo de la lectura
                    buffer = String.Empty; //Buffer vacio


                    if (!port.IsOpen) //Sino esta abierto el COM abrelo
                    {
                        port.Open(); //Abre
                        port.DiscardInBuffer();
                        port.DiscardOutBuffer();
                        port.Write("W"); // Codigo de Identificacion de la maquina
                        System.Threading.Thread.Sleep(1800);
                    }

                }
                catch (Exception)
                {
                    MessageBox.Show("There was an issue with a COM Port!, If the problem persists, Disconnect the machine from the USB cable");
                }
            }

            return buffercopy; //Regresa codigo de identificacion

        }

        public void closePort()
        {
            port.Close();
        }

        //Funcion de lectura de serial
        public void read_serialport(object sender, SerialDataReceivedEventArgs e)
        {

            try
            {
                lock (bufferLock)
                {
                    buffer = port.ReadExisting(); //Lee el buffer del puerto
                    Console.WriteLine(buffer);  
                    Console.WriteLine("prueba en linea 89 en al archivo machinep" + string.IsNullOrEmpty(buffer).ToString());
                    buffercopy += buffer;
                }

            }
            catch (Exception)
            {
                buffer = String.Empty;
            }
        }


        public bool isOpen()
        {
            try
            {
                if (port.IsOpen)
                {

                    return true; // La escritura fue exitosa
                }
                else
                {
                    return false; // El puerto no está abierto
                }
            }
            catch (IOException)
            {
                return false; // Hubo un problema con el puerto
            }
            catch (UnauthorizedAccessException)
            {
                return false; // Hubo un problema con el acceso al puerto
            }
            catch (InvalidOperationException)
            {
                return false; // Hubo un problema con la operación en el puerto
            }
            catch (TimeoutException)
            {
                return false; // Hubo un problema con el tiempo de espera del puerto
            }
            catch (Exception)
            {
                return false; // Hubo algún otro problema
            }
        }


        public async Task<string> checkPing()
        {
            await semaphore.WaitAsync();
            try
            {

                if (!port.IsOpen)
                {
                    port.Open();
                }

                lock (bufferLock)
                {
                    buffer = string.Empty; // Limpia el buffer antes de la escritura
                }

                port.Write("O");
                await Task.Delay(2000);


                lock (bufferLock)
                {
                    Console.WriteLine("se ejecuto la funcion" + buffer);
                    return buffer; // Devuelve el contenido del buffer
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return string.Empty;
            }
            finally
            {
                semaphore.Release();
            }

        }


        //Funcion de diagnostico
        public bool Diagnostic()
        {
            try
            {
                if (!port.IsOpen)
                {
                    port.Open();
                }

                port.Write("A");

                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        //Funcion de guardado en tarjeta
        public void Save()
        {

            try
            {
                if (!port.IsOpen)
                    port.Open();


                port.Write("V");
            }
            catch (Exception) { }
        }

        //Funcion para Autosimile
        public void Similie()
        {

            try
            {
                if (!port.IsOpen)
                {
                    port.Open();
                }

                port.Write("S");

            }
            catch (Exception) { }
        }

        //Funcion para Neutralizar
        public void Neutralizando()
        {

            try
            {
                if (!port.IsOpen)
                {
                    port.Open();
                }

                //port.Write("Z");

                port.Write("E");

            }
            catch (Exception) { }
        }

        //Funcion para Imprint
        public void Imprint()
        {

            try
            {
                if (!port.IsOpen)
                {
                    port.Open();
                }

                port.Write("P");
            }
            catch (Exception) { }
        }

        //Funcion para Copiar
        public void Copy()
        {

            try
            {
                if (!port.IsOpen)
                {
                    port.Open();
                }

                port.Write("Y");
            }
            catch (Exception) { }
        }

        //Funcion para Borrar
        public void Erase()
        {

            try
            {
                if (!port.IsOpen)
                {
                    port.Open();
                }

                port.Write("E");
            }
            catch (Exception) { }

        }

        //Funcion para Broadcast ON
        public void BroadcastON()
        {

            try
            {
                if (!port.IsOpen)
                {
                    port.Open();
                }

                port.Write("I");


            }
            catch (Exception) { }

        }

        //Funcion para Broadcast OFF
        public bool BroadcastOFF()
        {

            try
            {

                if (!port.IsOpen)
                {
                    port.Open();
                }


                port.Write("D");
                return true; // La escritura fue exitosa
            }
            catch (IOException)
            {
                return false; // Hubo un problema con el puerto
            }
            catch (UnauthorizedAccessException)
            {
                return false; // Hubo un problema con el acceso al puerto
            }
            catch (InvalidOperationException)
            {
                return false; // Hubo un problema con la operación en el puerto
            }
            catch (TimeoutException)
            {
                return false; // Hubo un problema con el tiempo de espera del puerto
            }
            catch (Exception)
            {
                return false; // Hubo algún otro problema
            }
        }

    }
}