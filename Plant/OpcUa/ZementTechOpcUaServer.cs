using Opc.UaFx;
using Opc.UaFx.Server;
using Plant.OpcUa.Sim;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Plant.OpcUa
{
    public class ZementTechOpcUaServer
    {
        private static string DefaultEndpoint = "opc.tcp://localhost:4841/";
        public OpcServer Server { get; set; }

        public ZementTechOpcUaServer(string endpoint)
        {
            Endpoint = endpoint;
            Server = new OpcServer(Endpoint, ZementTechNodeManager = new ZementTechNodeManager());
            Server.Started += Server_Started;
        }

        public ZementTechOpcUaServer() : this(DefaultEndpoint)
        {
        }

        public ZementTechNodeManager ZementTechNodeManager { get; set; }

        public static string Endpoint { get; protected set; }

        public void Start()
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

            Server.Start();
        }

        private void Server_Started(object sender, EventArgs e)
        {
            Console.WriteLine("Server Endpoint available: " + Endpoint);
            
            Simulator.Instance().ZementTechNodeManager = ZementTechNodeManager;
            
            Thread t = new Thread(Simulator.Instance().Execute);

            t.IsBackground = true;
            t.Start();
            Console.WriteLine("Simulation started.");
            Console.WriteLine("Press any key to shutdown...");
        }

        public void Stop()
        {
            Server.Stop();
        }
    }
}
