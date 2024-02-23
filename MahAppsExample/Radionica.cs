using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MahAppsExample
{
    //Clase del motor de radionica
    class Radionica
    {
        Random random = new Random();

        //Motor RANDOM
        public string RandomDigits(int length)
        {
            string s = string.Empty;
            for (int i = 0; i < length; i++) //Genera Numero Random en base a la cantidad de digitos
                s = String.Concat(s, random.Next(10).ToString());
            return s;
        }

        //Analisis cuantitativos y cualitativos

        //-100/100
        public string ValorSugerido()
        {
            return (random.Next(-100, 100).ToString());
        }

        //Pronunciamiento
        public string Pronunciamiento()
        {
            //SI = 1
            //NO = 0
            string[] valores = { "1", "0" };
            return (valores[random.Next(0, 1)]);
        }

        //Polaridad
        public string Polaridad()
        {
            //POSITIVO = 1
            //NEUTRO = 0
            //NEGATIVO = -1
            string[] valores = { "1", "-1", "0" };
            return (valores[random.Next(0, 2)]);
        }

        //Probabilidad
        public string Probabilidad()
        {
            //ALTA = 100
            //MEDIA = 50
            //BAJA = 25
            // NULA = 0
            string[] valores = { "100", "50", "25", "0" };
            return (valores[random.Next(0, 3)]);
        }

        //Porcentaje
        public string Porcentaje()
        {
            return (random.Next(0, 100).ToString());
        }

        //Sugerir niveles - Metodo PRO
        public string SugerirNiveles()
        {
            int cantidad_niveles = random.Next(0, 11); //Cantidad de veces que recorrera el arreglo

            string[] niveles = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" }; //Niveles
            //Lista donde se guardaran los niveles
            List<string> randomniveles = new List<string>();
            string nivelesconcat = ""; //Niveles concatenados

            //Cantidad de niveles
            for (int i = 0; i <= cantidad_niveles; i++)
            {
                int valor = random.Next(-10, 10);
                if (valor >= 0)
                {
                    nivelesconcat = nivelesconcat + " " + niveles[i];
                }
            }

            if (nivelesconcat == "")
            {
                nivelesconcat = "-";
            }

            return nivelesconcat;
        }

        //Radionico - Sugerir niveles
        public string RadionicoSugerirNiveles()
        {
            int cantidad_niveles = random.Next(0, 11); //Cantidad de veces que recorrera el arreglo

            string[] niveles = { "01 - FISICO", "02 - EMOCIONAL", "03 - MENTAL", "04 - ESPIRITUAL 1", "05 - ESPIRITUAL 2", "06 - ESPIRITUAL 3", "07 - ESPIRITUAL 4", "08 - ESPIRITUAL 5", "09 - ESPIRITUAL 6", "10 - ESPIRITUAL 7", "11 - ESPIRITUAL 8", "12 - ESPIRITUAL 9" }; //Niveles
           
            return niveles[random.Next(0,11)]; //Regresa valor random (nivel)
        }

        //Codigos complementarios
        public string CodigoComplementario(string codigo)
        {
            int longitud_codigo = codigo.Length;
            string[] codigocomplementario = new string[longitud_codigo];

            for(int i = 0; i <= longitud_codigo - 1; i++)
            {
                int casteo_digito = Int32.Parse(codigo[i].ToString());

                if(casteo_digito==0)
                {
                    codigocomplementario[i] = "0";
                }
                else
                {
                    int redondeo = Math.Abs(casteo_digito - 10);
                    codigocomplementario[i] = redondeo.ToString();
                }
            }

            string numero_compl = "";
            //Junto el array y lo devuelvo
            for(int p = 0; p <= codigocomplementario.Length - 1; p++)
            {
                numero_compl = numero_compl + codigocomplementario[p];
            }

            return numero_compl;
        }

        public string Generar_Id()
        {
            Random rdm = new Random();

            string id = rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() +
                     rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() +
                      rdm.Next(0, 9).ToString() + "-" + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() +
                     rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() + rdm.Next(0, 9).ToString() +
                      rdm.Next(0, 9).ToString();

            return id;
        }

        //Funcion para copiar el directorio de radionica_db (Images) del 4.9
        public void Copiar_Directorio(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "No se ha encontrado la carpeta origen a copiar: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    Copiar_Directorio(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
    }
}
