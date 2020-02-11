using Opc.UaFx.Server;
using Plant.OpcUa;
using Plant.OpcUa.Sim;
using System;
using System.Threading;

namespace PlantConsoleTest
{
    public class Program
    {
        private static string DefaultEndpoint = "opc.tcp://localhost:4841/";

        static void Main(string[] args)
        {
            string endpoint = DefaultEndpoint;

            if (args != null & args.Length > 0)
            {
                if (args[0].Contains("opc.tcp://"))
                {
                    Console.WriteLine("application started with arguments.");
                    endpoint = args[0];
                }
            }

            var app = new OpcServerApplication(endpoint, NodeManager = new ZementTechNodeManager());

            app.Started += Server_Started;

            app.Run();
        }

        private static void Server_Started(object sender, EventArgs e)
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            Console.WriteLine("Version: " + version);
            Console.WriteLine("#####################################################################");
            Console.WriteLine("#                                                                   #");
            Console.WriteLine("#                         ZementTech - Simulation                   #");
            Console.WriteLine("#                                                                   #");
            Console.WriteLine("#                         Autor: FHNW - mbo, 2020                   #");
            Console.WriteLine("#####################################################################");

            Console.WriteLine("Server Endpoint available: " + DefaultEndpoint);

            Simulator.Instance().ZementTechNodeManager = NodeManager;

            Thread t = new Thread(Simulator.Instance().Execute);

            t.IsBackground = true;
            t.Start();
            Console.WriteLine("Simulation started.");
        }

        public static ZementTechNodeManager NodeManager { get; set; }

    }
}
