using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CsvHelper.Expressions;
using CsvHelper;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Net.NetworkInformation;

namespace AspelViewServer
{
    public partial class Form1 : Form
    {
        string absolutePath;
        List<CvsVO> result;
        Socket socket;
        EndPoint puntoLocal;
        EndPoint puntoDestino;
        Int16 datoEnviar;
        string ipDestino;
        int puertoDestino;
        byte[] buffer;
        byte[] dataEnviar;
        int contadorEnvio;
        bool corriendo = false;
        DataTable dataT;
        SocketAsyc socketAsyc;
        
        public Form1()
        {
            InitializeComponent();
            absolutePath = AspelViewServer.Properties.Settings.Default.RUTACSV;
            result = new List<CvsVO>();
            socket = null;
            datoEnviar = 1;
            buffer = new byte[100];
            dataEnviar = null;
            dataT = new DataTable();
            socketAsyc = new SocketAsyc();
        }
        private void LlenarTabla(){
            
            result.Clear();
            ReadInCSV();
            dataGridView1.DataSource = result;
            foreach (DataGridViewColumn columna in dataGridView1.Columns)
            {
                columna.Width = 118;
            }
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            LlenarTabla();
            BuscarServers();
                  
        }

      
        public void ReadInCSV() {
            
             using (TextReader fileReader = File.OpenText(absolutePath)) {
             var csv = new CsvReader(fileReader);
             //csv.Configuration.HasHeaderRecord = true;
             csv.Read();
             while (csv.Read()) {

                 result.Add(new CvsVO() { NombreEquipo = csv[0], IpEquipo = csv[1], PuertoEquipo = csv[2] });
             }
           }
       

        }

    

        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.AutoGenerateColumns = false;
            //DataRow nuevaFila = dataT.NewRow();
            dataT.Columns.Add("Equipo");
            dataT.Columns.Add("IP");
            dataT.Columns.Add("Puerto");
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
           // new Thread(Escuchador).Start();
            
            LlenarTabla();
           // BuscarServers();
            

        }
        private void BuscarServers()
        {
            Ping Pings = new Ping();
            int timeout = 10;

           
            
            try
            {
                int i = 0;
                while (i < result.Count)
                {
                    
                    ipDestino = result[i].IpEquipo;
                    if (Pings.Send(ipDestino, timeout).Status == IPStatus.Success)
                    {
                        puertoDestino = int.Parse(result[i].PuertoEquipo);
                        /*puntoDestino = new IPEndPoint(IPAddress.Parse(ipDestino), puertoDestino);
                        socket.Connect(puntoDestino);

                        dataEnviar = Encoding.Default.GetBytes("" + datoEnviar);

                        socket.SendTo(dataEnviar, puntoDestino); */
                        
                        socketAsyc.StartClient(ipDestino,puertoDestino);
                        Console.WriteLine("Enviado a " + ipDestino);
                       // Thread.Sleep(300);
                        //socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                           
                    }
                    else
                    {
                        Console.WriteLine(ipDestino + " está fuera de red.");   
                    }
                    i++;
                }
               
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ipDestino + " no responde");
            }
        }

        private void Escuchador()
        {
            
           // ipRemota = new IPEndPoint(IPAddress.Any, 0);
            byte[] buffer = new byte[1024];
            corriendo = true;
            Console.WriteLine("Escuchando...");
            Console.Write("IP Destino: ");
            while (corriendo)
            {
                if (socket.Available == 0)
                {
                    Thread.Sleep(200);
                    continue;
                }
                int contadorLeido = socket.Receive(buffer,0,buffer.Length,0);
                string datosRecibidos = Encoding.Default.GetString(buffer, 0, contadorLeido);
                Console.WriteLine("Recibí: " + datosRecibidos);
                //MessageBox.Show(datosRecibidos);
                //socket.Close(200);
            }

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            corriendo = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            NuevoRegistro nuevoRegistro = new NuevoRegistro();
            nuevoRegistro.ShowDialog();
            ReadInCSV();
            dataGridView1.EndEdit();
            dataGridView1.Refresh();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Modificar modificar = new Modificar();
            modificar.ShowDialog();
        }

        private void button3_Click(object sender, EventArgs e)
        {

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (dataGridView1.Rows[i].Cells[5].Value != null)
                {
                    if (dataGridView1.Rows[i].Cells[5].Value.ToString() == "True")
                    {
                        result.RemoveAt(i);
                        i--;
                        
                       
                    }
                        
                }
              
            }
            using (TextWriter fileWriter = File.CreateText(absolutePath))
            {
                var csv = new CsvWriter(fileWriter);
                csv.WriteRecords(result);

            }
            dataGridView1.EndEdit();

            dataGridView1.Refresh();
            
        }
    }
}
