﻿using System;
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
using static MahAppsExample.MainWindow;

namespace HS5
{
    /// <summary>
    /// Interaction logic for Remedio.xaml
    /// </summary>
    public partial class Remedio : Window
    {
        public Remedio(DataTable CodigosdeRemedios)
        {
            InitializeComponent();
            for (int i = 0; i <= CodigosdeRemedios.Rows.Count - 1; i++)
            {
                ListaRemedios.Items.Add(new nuevoRemedio { nombrecodigo = CodigosdeRemedios.Rows[i][1].ToString(), codigo = CodigosdeRemedios.Rows[i][0].ToString(), potencia = CodigosdeRemedios.Rows[i][2].ToString(), metodo = CodigosdeRemedios.Rows[i][3].ToString(), codigocomplementario = CodigosdeRemedios.Rows[i][4].ToString(), nivel = CodigosdeRemedios.Rows[i][5].ToString() });
            }
        }
    }
}