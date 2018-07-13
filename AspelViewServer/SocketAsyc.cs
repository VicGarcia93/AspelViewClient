using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace AspelViewServer
{
    class SocketAsyc
    {
        // ManualResetEvent instances signal completion.  
        private ManualResetEvent connectDone =
            new ManualResetEvent(false);
        private ManualResetEvent sendDone =
            new ManualResetEvent(false);
        private ManualResetEvent receiveDone =
            new ManualResetEvent(false);
        int portClient;
        string direccionIP;
        bool estadoDeConexion = true;
        public SocketAsyc()
        {
           
        }
        // The response from the remote device.  
        private String response = String.Empty;

        public void StartClient(string ip, int puerto)
        {
            
            direccionIP = ip;
            portClient = puerto;
            connectDone = new ManualResetEvent(false);
            sendDone = new ManualResetEvent(false);
            receiveDone = new ManualResetEvent(false);
            // Connect to a remote device.  
            try
            {
                // Establish the remote endpoint for the socket.  
                // The name of the   
                // remote device is "host.contoso.com".  
               // IPHostEntry ipHostInfo = Dns.GetHostEntry("host.contoso.com");
               // IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(direccionIP), portClient);

                // Create a TCP/IP socket.  
                Socket client = new Socket(IPAddress.Parse(direccionIP).AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.  
                client.BeginConnect(remoteEP,
                    new AsyncCallback(ConnectCallback), client);
                
                connectDone.WaitOne();
                Console.WriteLine("Ya se ejecutó BeginConnect");

                if (estadoDeConexion)
                {
                    // Send test data to the remote device.  
                    Send(client, "1");
                    sendDone.WaitOne();

                    Console.WriteLine("Ya se ejecutó Send");
                    // Receive the response from the remote device.  
                    Receive(client);
                    receiveDone.WaitOne();
                    Console.WriteLine("Ya se ejecutó Receive");
                    // Write the response to the console.  
                    Console.WriteLine("Response received : {0}", response);

                    // Release the socket.  
                    //client.Shutdown(SocketShutdown.Both);
                    // client.Close();
                    
                }
                estadoDeConexion = true;             

            }
            catch (Exception e)
            {
                Console.WriteLine("BeginConnect {0}",e.ToString());
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            Socket client = null;
            try
            {
                // Retrieve the socket from the state object.  
                 client = (Socket)ar.AsyncState;

                // Complete the connection.  
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.  
                connectDone.Set();
            }
            catch (Exception e)
            {
                estadoDeConexion = false;
                connectDone.Set();
               
                
            }
        }

        private void Receive(Socket client)
        {
            try
            {
                // Create the state object.  
                StateObject state = new StateObject();
                state.workSocket = client;
             //   Console.WriteLine("Test: {0}", state.workSocket.Available);
                // Begin receiving the data from the remote device.  
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket   
                // from the asynchronous state object.  
                StateObject state = (StateObject)ar.AsyncState;
                
                Socket client = state.workSocket;

                // Read data from the remote device.  
                int bytesRead = client.EndReceive(ar);
                Console.WriteLine(bytesRead.ToString());
                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.  
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                    response = state.sb.ToString();
                    receiveDone.Set();
                   
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void Send(Socket client, String data)
        {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.Default.GetBytes(data);

            // Begin sending the data to the remote device.  
            client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), client);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.  
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        
       
    }

   
 


}
