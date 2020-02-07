using Opc.UaFx;
using Plant.OpcUa.Common;
using Plant.OpcUa.Helper;
using Plant.OpcUa.Plant.Components;
using System;
using System.Collections.Generic;

namespace Plant.OpcUa.Plant.Modules
{
    public class Zyklon : OpcSimModule
    {
        public List<TemperaturSensor> tempSensors = new List<TemperaturSensor>();
        private OpcDataVariableNode<bool> exhaustHeat;
        private List<double> material = new List<double>();

        public Zyklon(IOpcNode parent, OpcName name, OpcContext context, Drehrohrofen ofen) : base(parent, name, context)
        {
            Ofen = ofen;
            tempSensors.Add(new TemperaturSensor(this, "Temperatur_1,", Context) { UpperLimit = Ofen.MaxTemp });
            tempSensors.Add(new TemperaturSensor(this, "Temperatur_2,", Context) { UpperLimit = Ofen.MaxTemp });
            tempSensors.Add(new TemperaturSensor(this, "Temperatur_3,", Context) { UpperLimit = Ofen.MaxTemp });
            tempSensors.Add(new TemperaturSensor(this, "Temperatur_4,", Context) { UpperLimit = Ofen.MaxTemp });
            tempSensors.Add(new TemperaturSensor(this, "Temperatur_5,", Context) { UpperLimit = Ofen.MaxTemp });

            tempSensors.ForEach(s => s.TempPt1.TimeConstant = 8000);

            exhaustHeat = new OpcDataVariableNode<bool>(this, "AbwärmeOk", false);
            exhaustHeat.AccessLevel = OpcAccessLevel.CurrentRead;
        }

        public bool HeatOk
        {
            get { return exhaustHeat.Value; }
            set { exhaustHeat.Value = value; exhaustHeat.ApplyChanges(Context); }
        }

        public Drehrohrofen Ofen { get; set; }
        
        public double HeatAverage()
        {
            double totalTemp = 0;
            tempSensors.ForEach(s => totalTemp += s.TempValue);

            return totalTemp / tempSensors.Count;
        }

        protected override bool ReceiverReleased()
        {
            return Receiver.ReceiverReady && HeatOk;
        }

        public override void Simulate(int clock)
        {

            base.Simulate(clock);

            if(tempSensors.Count > 0)
            {
                if (Ofen.MaxTemp != tempSensors[0].UpperLimit)
                {
                    UpdateMaxTemp(Ofen.MaxTemp);
                }
            }
            


            if (Ofen != null)
            {
                TempSensors(Ofen.ValveWasteHeate.Open);
            }
            else
            {
                TempSensors(false);
            }

            HeatOk = HeatAverage() > (Ofen.HeatAverage() * 0.75);


            switch (State)
            {
                case OperationState.Stopped:
                    break;

                case OperationState.Started:;

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
                        }catch(Exception e)
                        {

                        }
                    }
                    break;

                case OperationState.CriticalStop:
                    break;
                default:
                    break;
            }
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

        private void UpdateMaxTemp(double upperLimit)
        {
            tempSensors.ForEach(s => s.UpperLimit = upperLimit);
        }

        public override void PushMaterial(double volume)
        {
            material.Add(volume);
        }
    }
}
