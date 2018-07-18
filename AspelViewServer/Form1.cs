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
        string respuesta;
        string []respuestaSistemas;
        
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
            respuesta = String.Empty;
            respuestaSistemas = new string[]{"0","0","0"};
        }
        private void LlenarTabla(){
            dataGridView1.DataSource = null;
            ReadInCSV();
            dataGridView1.DataSource = result;
            foreach (DataGridViewColumn columna in dataGridView1.Columns)
            {
                columna.Width = 90;
            }

            dataGridView1.Columns[6].Width = 50;
            dataGridView1.Columns[1].Width = 130;

            dataGridView1.ClearSelection();
           
            for (int i = 0; i < dataGridView1.Columns.Count; i++)
            {
                dataGridView1.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
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
             result.Clear();
             csv.Read();
             while (csv.Read()) {

                 result.Add(new CvsVO() { NombreEquipo = csv[0], Usuario = csv[3], IpEquipo = csv[1], PuertoEquipo = csv[2] });
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
            // primero asignar las columnas como no sortables, osea, no ordenables
            //dataGridView1.Columns[""].SortMode = DataGridViewColumnSortMode.NotSortable;

            // segundo asignarle que se centre el texto de la columna
            
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
                   // respuesta = String.Empty;
                    respuestaSistemas = new String[] { "0", "0", "0" };
                    ipDestino = result[i].IpEquipo;
                    if (Pings.Send(ipDestino, timeout).Status == IPStatus.Success)
                    {
                        puertoDestino = int.Parse(result[i].PuertoEquipo);
                        /*puntoDestino = new IPEndPoint(IPAddress.Parse(ipDestino), puertoDestino);
                        socket.Connect(puntoDestino);

                        dataEnviar = Encoding.Default.GetBytes("" + datoEnviar);

                        socket.SendTo(dataEnviar, puntoDestino); */
                        
                        respuesta = socketAsyc.StartClient(ipDestino,puertoDestino);
                        Console.WriteLine("Enviado a " + ipDestino);
                        if(!String.IsNullOrEmpty(respuesta))
                            respuestaSistemas = respuesta.Split(',');
                       

                        Console.WriteLine(respuestaSistemas[0]);
                        
                        

                       // Thread.Sleep(300);
                        //socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                           
                    }
                    else
                    {
                        Console.WriteLine(ipDestino + " está fuera de red.");   
                    }
                    dataGridView1.Rows[i].Cells[3].Value = respuestaSistemas[0];
                    dataGridView1.Rows[i].Cells[4].Value = respuestaSistemas[1];
                    dataGridView1.Rows[i].Cells[5].Value = respuestaSistemas[2];

                    
                    i++;
                }
               
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ipDestino + " no responde" + ex.Message);
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
            LlenarTabla();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Modificar modificar = new Modificar();
            int filaSeleccionada = dataGridView1.CurrentRow.Index;

           
            modificar.SetSeleccionDGV(filaSeleccionada);
            
            modificar.ShowDialog(this);
            LlenarTabla();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            int j = -1;
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                j++;
                if (dataGridView1.Rows[i].Cells[5].Value != null)
                {
                    if (dataGridView1.Rows[i].Cells[5].Value.ToString() == "True")
                    {
                        Console.WriteLine("Remueve...{0} ", dataGridView1.Rows[i].Cells[5].Value.ToString());
                        Console.WriteLine(i.ToString());
                        result.RemoveAt(j);
                        j--;
                                              
                    }
                        
                }
              
            }
            using (TextWriter fileWriter = File.CreateText(absolutePath))
            {
                var csv = new CsvWriter(fileWriter);
                csv.WriteRecords(result);

            }
            LlenarTabla();
        }

        
    }
}
