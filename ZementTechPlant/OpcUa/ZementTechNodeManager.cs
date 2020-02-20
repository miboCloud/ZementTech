using Opc.UaFx;
using Opc.UaFx.Server;
using Plant.OpcUa.Component;
using Plant.OpcUa.Helper;
using Plant.OpcUa.Plant.Modules;
using System;
using System.Collections.Generic;
using System.Text;

namespace Plant.OpcUa
{
    public class ZementTechNodeManager : OpcNodeManager
    {
        public ZementTechNodeManager()
        : base("http://mibo.io/zementtech")
        {

            Opc.UaFx.Licenser.LicenseKey =
               "093oXoIEpOG5TntLTi+LyFFTgmXO7B9W0t7RNCJZo4r6AfPloZuR0j9xPMNzyurVI3UMKN7ayqQ1NRG+KSeSXVg16VUYRS3YJe3qMpGQWSMA06uzF97xM1S03ksFgOw0KxoREaQHNxjlFSSo9IeNxt8eq9wEl+j52bGEUjzFgLNbyA0NrHmiKGG839jBMe4x71hCuYf3soDUZJs5/PnBHElGh1d0UtnbA+KrRVbfM4iwLcsFVEVVj2oJFyWmqvHimIVdYbuDbLzkOUF0xTWn6+Guz6/E4fQciIVx17CsqkPXgj1xPu9XFNny6+k5jFqnjv7yH+/P7nwjgLooavF7osST5Eibe+u36BtxFRF61JwDCZg3lb1DDIKOvXAuVqDB3gymw7yPi8MpKkiAKJzgp117d3CclrPyamka33Jw1BgJMuAaJ2kl6yP76nyGmEolCLfs8EqJvEsF6YtGftyzd29dT0msCig55AkxsuqHj87VdO+CQ5zp2s65lHgorciETM9OXvEw8gBlsM6Q5yb8drJmEqn03ZfImjLFYu2IlDJ4FNQQRwqIlSbotXRazm+bsVztGFBUWK9mKhQtsarz5paCBz7q2y/tvHodIN4oven1ttio9ernHY2ZXMzkj989spZLLmOuE2iJv77kTtUJ9d+rpXHQy9NnL5O2IzPa1nOVueBW01u9le4ZOBkbbfqUFBbCG5u1Sl3cv5J4Mlm0m/EF7vSdm4jSZnF7MSTgetJj05WN2txhFUHNG+QFM5ouBAO5msHFQ+OXZJNzkI8KExCL46ih0bE7bVi0iiQb901cbcDZSqQRJ65bKxAFJvsV/4qGXkrjwGTQidTLAI8ooJ9BlCcuaO/E5ecCM/ksJz5kb59PC+8ixxfE23gLV8PBImtTPpjvT6I7HP3Ab7+hgVJ7ODxFi5vYe00Alwj3KGGQXnb7DWvTeyGefuei6B3EMRXBAfIypG4L/WUfecz/lw2V+Pv6CVzO50uVjR8IGsPH3rM0ms7R0gCaWGV2x2sSiraWB1rPJQL5P3rx1aMeF4yPz4Nf9PX+4e27kFJugmx4jt+IGdBswfRYOQ==";

            Console.WriteLine("Load OPC UA Nodemanager...Done");
        }

        protected override IEnumerable<IOpcNode> CreateNodes(OpcNodeReferenceCollection references)
        {
            var zementTech = new OpcFolderNode(new OpcName("ZementTech", this.DefaultNamespaceIndex));
            references.Add(zementTech, OpcObjectTypes.ObjectsFolder);

            var zementTechConfig = new OpcFolderNode(new OpcName("SimKonfiguration", this.DefaultNamespaceIndex));
            references.Add(zementTechConfig, OpcObjectTypes.ObjectsFolder);

            Context = this.SystemContext;

            AddConfig(zementTechConfig);

            RohMaterialGewinnung rohmaterialgewinnung = new RohMaterialGewinnung(zementTech, "Gewinnung", this.SystemContext);
            MotorDelayLane brecher = new MotorDelayLane(zementTech, "Brecher", this.SystemContext, 2);
            MotorDelayLane transport = new MotorDelayLane(zementTech, "Transport", this.SystemContext, 4);
            Silo mischbett = new Silo(zementTech, "Mischbett", this.SystemContext, 45000.0);
            MotorDelayLane walzmühle = new MotorDelayLane(zementTech, "Mühle", this.SystemContext, 1);
            Silo rohmehlsilo = new Silo(zementTech, "RohmehlSilo", this.SystemContext, 30000.0);

            Brennstoffzufuhr bsz = new Brennstoffzufuhr(zementTech, "Brennstoffzufuhr", this.SystemContext);
            Drehrohrofen drehrohrofen = new Drehrohrofen(zementTech, "Drehrohrofen", this.SystemContext, bsz);
            Zyklon zyklon = new Zyklon(zementTech, "Zyklon", this.SystemContext, drehrohrofen);
  
            Kuehler kuehler = new Kuehler(zementTech, "Kühler", this.SystemContext);
            Silo klinkerSilo = new Silo(zementTech, "KlinkerSilo", this.SystemContext, 20000.0);

            MotorDelayLane zementMühle = new MotorDelayLane(zementTech, "Zementmühle", this.SystemContext, 1);
            Silo zementSilo = new Silo(zementTech, "ZementSilo", this.SystemContext, 35000.0);

            Absackung absackung = new Absackung(zementTech, "Absackung", this.SystemContext);

            rohmaterialgewinnung.Receiver = brecher;
            brecher.Receiver = transport;
            transport.Receiver = mischbett;
            mischbett.Receiver = walzmühle;
            walzmühle.Receiver = rohmehlsilo;
            rohmehlsilo.Receiver = zyklon;
            zyklon.Receiver = drehrohrofen;
            drehrohrofen.Receiver = kuehler;
            kuehler.Receiver = klinkerSilo;
            klinkerSilo.Receiver = zementMühle;
            zementMühle.Receiver = zementSilo;
            zementSilo.Receiver = absackung;

            return new IOpcNode[] 
            { 
                zementTech,
                zementTechConfig,
                new OpcDataTypeNode<OperationState>(),
                new OpcDataTypeNode<OperationMode>(),
                new OpcDataTypeNode<TimeMultiplier>()
            };
        }

        private static OpcDataVariableNode<TimeMultiplier> _timeLapse;
        private static OpcDataVariableNode<int> _clock;

        protected static void AddConfig(IOpcNode node)
        {
            _timeLapse = new OpcDataVariableNode<TimeMultiplier>(node, "Zeitraffer");
            _clock = new OpcDataVariableNode<int>(node, "Clock", 100);
            _clock.Description = "Clock in Millisekunden";
        }

        public static TimeMultiplier TimeLapse
        {
            get { return _timeLapse.Value; }
            set 
            { 
                if(_timeLapse.Value != value)
                {
                    _timeLapse.Value = value;
                    _timeLapse.ApplyChanges(Context);
                }
            }
        }

        public static int Clock
        {
            get { return _clock.Value; }
            set
            {
                if (_clock.Value != value)
                {
                    _clock.Value = value;
                    _clock.ApplyChanges(Context);
                }
            }
        }

        public static OpcContext Context { get; private set; }

        [OpcDataType(id: "TimeMultiplier", namespaceIndex: 2)]
        public enum TimeMultiplier : int
        {
            x1, 
            x10,
            x100
        }
    }

}
