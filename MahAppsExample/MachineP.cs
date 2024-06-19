using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;
using System.Windows;
using System.Windows.Threading;
using System.Threading;

namespace MahAppsExample
{
    //Uso de la maquina
    public class MachineP
    {
        string[] ports; //COMs (Puertos)
        SerialPort port; //Puerto
        private string buffer { get; set; } //Buffer del puerto
        string buffercopy; //Global
        //List<string> Mantras = new List<string>(); //Listado de mantras
        //List<string> Evolution = new List<string>(); //Listado de evolution
        //List<string> Copen = new List<string>(); //Listado Copen

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
               MessageBoxResult result= MessageBox.Show("There's no instrument connected!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                if (result == MessageBoxResult.OK)
                {
                    //Cierra la aplicacion
                    var window = Application.Current.Windows[0];
                    window.Close();
                }
            }
            else //Si si hay COM's
            {
                //Muestra los puertos
                //for (int i = 0; i <= ports.Length - 1; i++)
                //{
                //    //Prueba los puertos
                    try
                    {

                        port = new SerialPort(puerto, 2400); //Puerto
                        port.ReadTimeout = 5; //Tiempo de respuesta
                        port.DataReceived += new SerialDataReceivedEventHandler(read_serialport); //Manejo de la lectura
                        buffer = String.Empty; //Buffer vacio

                        if (!port.IsOpen) //Sino esta abierto el COM abrelo
                        {
                            port.Open(); //Abre
                            port.Write("W"); // Codigo de Identificacion de la maquina

                            //Le da tiempo al buffer para ser copiado a buffercopy
                            System.Threading.Thread.Sleep(1800);
                        }
                        //port.Close(); //Cierra
                        //MessageBox.Show(ports.Length.ToString());
                        //MessageBox.Show(buffercopy);

                    }
                    catch (IOException)
                    {
                        MessageBox.Show("There was an issue with a COM Port!");
                    }
                //}
            }

            return buffercopy; //Regresa codigo de identificacion

        }

     
        //Funcion de lectura de serial
        public void read_serialport(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                buffer += port.ReadExisting(); //Lee el buffer del puerto
                //MessageBox.Show(buffer);
                buffercopy = buffer;
                //Determina que tipo de maquina es.. (Identificacion)
                //Cacheo de la respuesta

                 //MessageBox.Show(buffer);

            }
            catch (IOException) { }

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

        //Funcion de diagnostico
        public bool Diagnostic()
        {
            //Cierre
            try {
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
            port.Write("V");
        }

        //Funcion para Autosimile
        public void Similie()
        {
            port.Write("S");
        }
        
        //Funcion para Neutralizar
        public void Neutralizando()
        {
            port.Write("Z");
        }

        //Funcion para Imprint
        public void Imprint()
        {
            port.Write("P");
        }

        //Funcion para Copiar
        public void Copy()
        {
            port.Write("Y");
        }

        //Funcion para Borrar
        public void Erase()
        {
            port.Write("E");
        }

        //Funcion para Broadcast ON
        public void BroadcastON()
        {
            port.Write("I");
        }

        //Funcion para Broadcast OFF
        public bool BroadcastOFF()
        {
            port.Write("D");
            try
            {
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