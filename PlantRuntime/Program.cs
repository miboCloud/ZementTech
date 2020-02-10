using Plant.OpcUa;
using System;
using System.Threading;

namespace PlantConsoleTest
{
    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args != null & args.Length > 0)
                {
                    if (args[0].Contains("opc.tcp://")){
                        Console.WriteLine("Application started with arguments.");
                        Server = new ZementTechOpcUaServer(args[0]);
                    }
                }

                if(Server == null)
                {
                    Console.WriteLine("Application started default.");
                    Server = new ZementTechOpcUaServer();
                }
                
                Server.Start();

                Console.ReadKey();

            }
            catch(Exception e)
            {
                Console.WriteLine("Exception during execution of OPC UA Server.");
                Console.WriteLine("Press any key to stop...");
                Console.ReadKey();
            }
            finally
            {
                Console.WriteLine("Shutdown simulation... please wait");
                Server?.Stop();
            }
        }

        public static ZementTechOpcUaServer Server { get; set; }

    }
}
