﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CsvHelper;

namespace AspelViewServer
{
    public partial class Modificar : Form
    {
        private string rutaCSV;
        private List<CvsVO> resultCsv;
        public Modificar()
        {
            InitializeComponent();
            rutaCSV = AspelViewServer.Properties.Settings.Default.RUTACSV;
        }

        private void TxtNombre_TextChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void BtnRegistrar_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(TxtIp.Text) && !String.IsNullOrWhiteSpace(TxtPuerto.Text) && !String.IsNullOrWhiteSpace(comboBox1.Text))
            {
                

                var direccionIP = TxtIp.Text;
                var puerto = TxtPuerto.Text;

                var nombreEquipo = comboBox1.SelectedItem.ToString();

                resultCsv[comboBox1.SelectedIndex].IpEquipo = direccionIP;
                resultCsv[comboBox1.SelectedIndex].PuertoEquipo = puerto;

                using (TextWriter fileWriter = File.CreateText(rutaCSV))
                {
                    var csv = new CsvWriter(fileWriter);
                    csv.WriteRecords(resultCsv);
                    this.Dispose();
                } 

                //StreamWriter streamW = File.WriteAllLines(path,)
                                
            }
            else
                MessageBox.Show("Complete todos los campos");
        }

        private void TxtPuerto_TextChanged(object sender, EventArgs e)
        {

        }

        private void TxtIp_TextChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void Modificar_Load(object sender, EventArgs e)
        {
            using (TextReader fileReader = File.OpenText(rutaCSV))
            {
                resultCsv = new List<CvsVO>();
                var csv = new CsvReader(fileReader);
                //csv.Configuration.HasHeaderRecord = true;
                csv.Read();
                while (csv.Read())
                {

                    resultCsv.Add(new CvsVO() { NombreEquipo = csv[0], IpEquipo = csv[1], PuertoEquipo= csv[2]});
                }
                for (int i = 0; i < resultCsv.Count; i++)
                {
                    comboBox1.Items.Add(resultCsv[i].NombreEquipo);    
                }
                
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string direccionIP = resultCsv[comboBox1.SelectedIndex].IpEquipo;
            string puerto = resultCsv[comboBox1.SelectedIndex].PuertoEquipo;

            TxtIp.Text = direccionIP;
            TxtPuerto.Text = puerto;
        }
    }
}
