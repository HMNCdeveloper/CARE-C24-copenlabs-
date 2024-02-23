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
    class Machine
    {
        string[] ports; //COMs (Puertos)
        SerialPort port; //Puerto
        private string buffer { get; set; } //Buffer del puerto
        string buffercopy; //Global
        //List<string> Mantras = new List<string>(); //Listado de mantras
        //List<string> Evolution = new List<string>(); //Listado de evolution
        //List<string> Copen = new List<string>(); //Listado Copen

        //Constructor (Machine)
        public Machine()
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

                        port = new SerialPort(puerto, 115200); //Puerto
                        port.ReadTimeout = 5; //Tiempo de respuesta
                        port.DataReceived += new SerialDataReceivedEventHandler(read_serialport); //Manejo de la lectura
                        buffer = String.Empty; //Buffer vacio

                        if (!port.IsOpen) //Sino esta abierto el COM abrelo
                        {
                            port.Open(); //Abre
                            port.Write("W"); // Codigo de Identificacion de la maquina

                            //Le da tiempo al buffer para ser copiado a buffercopy
                            System.Threading.Thread.Sleep(1000);
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

                // MessageBox.Show(buffer);

            }
            catch (IOException) { }

        }

        //Funcion de diagnostico
        public bool Diagnostic()
        {
            //Cierre
            //port.Close();
            //Indicamos inicio|fin de diagnostico
            try {
               /* if (!port.IsOpen)
                {
                    MessageBox.Show("No hay ningún dispositivo conectado!, conecte uno antes de continuar", "Error", MessageBoxButton.OK, MessageBoxImage.Error);                    
                }
                else
                {*/
                    port.Write("A"); //DiagON
                    return true;
                //}
            }
            catch (Exception)
            {
             //   MessageBox.Show("There's no Homoeonic connected, please connect it in order to continue", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
        public void BroadcastOFF()
        {
            port.Write("D");
        }
       
    }
}