using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CsvHelper;
using System.IO;

namespace AspelViewServer
{
    public partial class NuevoRegistro : Form
    {
        private string rutaCSV;
        
        public NuevoRegistro()
        {
            InitializeComponent();
            rutaCSV = AspelViewServer.Properties.Settings.Default.RUTACSV;
        }

        private void NuevoRegistro_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void BtnRegistrar_Click(object sender, EventArgs e)
        {
            if(!String.IsNullOrWhiteSpace(TxtIp.Text) && !String.IsNullOrWhiteSpace(TxtPuerto.Text) && !String.IsNullOrWhiteSpace(TxtNombre.Text)){
                using (TextWriter fileWriter = File.AppendText(rutaCSV))
                {
                    CvsVO nuevo = new CvsVO() { NombreEquipo = TxtNombre.Text, Usuario = txtUsuario.Text, IpEquipo = TxtIp.Text, PuertoEquipo = TxtPuerto.Text };
                    var csvv = new CsvWriter(fileWriter);
                    csvv.WriteRecord(nuevo);
                    csvv.NextRecord();
                    
                }
                
                this.Dispose();
            }else
                MessageBox.Show("Complete todos los campos");
            
            
        }
    }
}
