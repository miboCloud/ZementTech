using Opc.UaFx;
using Plant.OpcUa.Common;
using Plant.OpcUa.Helper;
using Plant.OpcUa.Plant.Components;
using System;
using System.Collections.Generic;

namespace Plant.OpcUa.Plant.Modules
{
    public class Kuehler : OpcSimModule
    {

        private List<double> material = new List<double>();
        public List<Motor> Ventilators { get; set; } = new List<Motor>();


        public Kuehler(IOpcNode parent, OpcName name, OpcContext context) : base(parent, name, context)
        {
            Ventilators.Add(new Motor(this, "Ventilator1", Context));
            Ventilators.Add(new Motor(this, "Ventilator2", Context));
            Ventilators.Add(new Motor(this, "Ventilator3", Context));

            Ventilators.ForEach(v => RegisterComponent(v));

            tempSensor = new TemperaturSensor(this, "Temperatur", context);
            tempSensor.UpperLimit = 120.0;

            RegisterComponent(tempSensor);
        }

        public TemperaturSensor tempSensor { get; set; }

        public override void PushMaterial(double volume)
        {
            material.Add(volume);
        }

        public override void Simulate(int clock)
        {
            base.Simulate(clock);

            switch (State)
            {
                case OperationState.Stopped:
                    Ventilators.ForEach(v => v.SwitchOff());
                    tempSensor.SwitchOff();
                    break;

                case OperationState.Started:
                    Ventilators.ForEach(v => v.SwitchOn());
                    tempSensor.SwitchOn();
                    if (Receiver == null)
                    {
                        return;
                    }

                    if (Receiver.ReceiverReady)
                    {
                        try
                        {
                            double mat = material[0];
                            material.RemoveAt(0);

                            if (mat > 0)
                            {
                                Receiver.PushMaterial(mat);
                            }
                        }
                        catch (Exception e)
                        {

                        }
                    }
                    break;

                case OperationState.CriticalStop:
                    Ventilators.ForEach(v => v.SwitchOff());
                    tempSensor.SwitchOff();
                    break;
                default:
                    tempSensor.SwitchOff();
                    Ventilators.ForEach(v => v.SwitchOff());
                    break;
            }
        }


        protected override bool ReceiverReleased()
        {
            return Receiver.ReceiverReady;
        }

    }
}
