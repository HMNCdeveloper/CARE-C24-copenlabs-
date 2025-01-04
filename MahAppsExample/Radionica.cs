using System;
using System.Collections.Generic;
using System.IO;

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
            string[] niveles = { "1 - PHYSICAL", "2 - EMOTIONAL", "3 - MENTAL", "4 - SPIRITUAL 1", "5 - SPIRITUAL 2", "6 - SPIRITUAL 3", "7 - SPIRITUAL 4", "8 - SPIRITUAL 5", "9 - SPIRITUAL 6", "10 - SPIRITUAL 7", "11 - SPIRITUAL 8", "12 - SPIRITUAL 9" }; //Niveles  
            return niveles[random.Next(0,11)]; //Regresa valor random (nivel)
        }



        private int getProbabilyNumb(string[] levePotency)
        {
            while (true)
            {
                int num1 = random.Next(1, levePotency.Length -1);
                int num2=random.Next(1, levePotency.Length-1 );

                if(num2 < num1)
                {
                    return num1;
                }
            }
        }


        public string RadionicaSurgerirPotencia()
        {

            string[] levelPotencies = { "1MM", "2MM", "5MM", "10MM", "1cm", "1LM", "2LM", "5LM", "10LM", "50LM", "1M", "2M", "5M", "10M", "50M", "500M", "1MM", "2MM", "5MM", "10MM", "20c", "30c", "10c", "200c", "100c", "3c", "6c", "20c", "40c", "50c", "1x", "3x", "6x", "10x", "2x", "12x", "8x", "24x", "30x", "50x", "100x", "200x" };
            return levelPotencies[getProbabilyNumb(levelPotencies)];
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
