using Opc.UaFx;
using Plant.OpcUa.Common;
using Plant.OpcUa.Helper;
using Plant.OpcUa.Plant.Components;
using System.Collections.Generic;

namespace Plant.OpcUa.Plant.Modules
{
    public class Drehrohrofen : OpcSimModule
    {
        public List<TemperaturSensor> tempSensors = new List<TemperaturSensor>();
        private OpcDataVariableNode<int> _delayLengthMs;
        private OpcDataVariableNode<bool> _heatOk;
        private DelayQueue queue = new DelayQueue();
        public double MaxTemp = 1450;


        public Drehrohrofen(IOpcNode parent, OpcName name, OpcContext context, Brennstoffzufuhr bsz) : base(parent, name, context)
        {
            Brennstoffzufuhr = bsz;
            _heatOk = new OpcDataVariableNode<bool>(this, "TemperaturOk");
            _delayLengthMs = new OpcDataVariableNode<int>(this, "SimDelayLength", 10000);
            _delayLengthMs.Description = "Simulierte Verzögerung von Einlauf zum Auslauf von Material";
            Brenner = new Brenner(this, "Brenner", Context);
            Motor = new Motor(this, "Hauptmotor", Context);
            ValveWasteHeate = new Valve(this, "VentilAbwärme", Context);



            tempSensors.Add(new TemperaturSensor(this, "Temperatur_1,", Context) { UpperLimit = MaxTemp });
            tempSensors.Add(new TemperaturSensor(this, "Temperatur_2,", Context) { UpperLimit = MaxTemp });
            tempSensors.Add(new TemperaturSensor(this, "Temperatur_3,", Context) { UpperLimit = MaxTemp });
            tempSensors.Add(new TemperaturSensor(this, "Temperatur_4,", Context) { UpperLimit = MaxTemp });
            tempSensors.Add(new TemperaturSensor(this, "Temperatur_5,", Context) { UpperLimit = MaxTemp });
            tempSensors.Add(new TemperaturSensor(this, "Temperatur_4,", Context) { UpperLimit = MaxTemp });
            tempSensors.Add(new TemperaturSensor(this, "Temperatur_5,", Context) { UpperLimit = MaxTemp });

            tempSensors.ForEach(s => s.TempPt1.TimeConstant = 14000);

            RegisterComponent(Brenner);
            RegisterComponent(Motor);
            RegisterComponent(ValveWasteHeate);

            tempSensors.ForEach(s => RegisterComponent(s));
        }

        public int SimDelayMs
        {
            get { return _delayLengthMs.Value; }
            set
            {
                _delayLengthMs.Value = value;
                _delayLengthMs.ApplyChanges(Context);
            }
        }

        public Brennstoffzufuhr Brennstoffzufuhr { get; set; }

        public Brenner Brenner { get; set; }

        public Motor Motor { get; set;  }

        public Valve ValveWasteHeate { get; set; }

        public bool HeatOk
        {
            get { return _heatOk.Value; }
            set 
            { 
                if(_heatOk.Value != value)
                {
                    _heatOk.Value = value;
                    _heatOk.ApplyChanges(Context);
                }
            }
        }

        protected override bool ReceiverReleased()
        {
            return HeatOk && Receiver.ReceiverReady;
        }

        public override void PushMaterial(double volume)
        {
            queue.Push(volume);
        }

        protected double RetrieveFromQueue()
        {
            double sum = 0;
            foreach (DelayItem x in queue.Retrieve())
            {
                sum += x.Kg;
            }

            return sum;
        }

        public override void Simulate(int clock)
        {
            base.Simulate(clock);

            HeatOk = HeatAverage() > (MaxTemp * 0.8);

           
            queue.Delay = SimDelayMs;

            TempSensors(Brenner.Running);

            switch (State)
            {
                case OperationState.Stopped:
                    Brenner.SwitchOff();
                    Motor.SwitchOff();
                    ValveWasteHeate.SwitchOff();
                    break;

                case OperationState.Started:
                    Brenner.SwitchOn();
                    Motor.SwitchOn();

                    if (HeatOk)
                    {
                        ValveWasteHeate.SwitchOn();
                    }

                    if (Receiver == null)
                    {
                        return;
                    }

                    if (Receiver.ReceiverReady)
                    {
                        queue.Tick(clock);
                        double mat = RetrieveFromQueue();

                        if (mat > 0)
                        {
                            Receiver.PushMaterial(mat);
                        }
                    }
                    break;

                case OperationState.CriticalStop:
                    Brenner.SwitchOff();
                    Motor.SwitchOff();
                    ValveWasteHeate.SwitchOff();
                    break;
                default:
                    Brenner.SwitchOff();
                    Motor.SwitchOff();
                    ValveWasteHeate.SwitchOff();
                    break;
            }

        }

        public double HeatAverage()
        {
            double totalTemp = 0;
            tempSensors.ForEach(s => totalTemp += s.TempValue);

            return totalTemp / tempSensors.Count;
        }

        private void TempSensors(bool on)
        {
            if (on)
            {
                tempSensors.ForEach(s => s.SwitchOn());
            }
            else
            {
                tempSensors.ForEach(s => s.SwitchOff());
            }
        }
    }
}
