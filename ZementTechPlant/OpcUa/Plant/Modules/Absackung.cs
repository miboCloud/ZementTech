using Opc.UaFx;
using Plant.OpcUa.Common;
using Plant.OpcUa.Helper;
using Plant.OpcUa.Plant.Components;
using System;

namespace Plant.OpcUa.Plant.Modules
{
    public class Absackung : OpcSimModule
    {
        private DelayQueue queue = new DelayQueue();
        private OpcDataVariableNode<int> _numberOfBags;
        private OpcDataVariableNode<bool> _resetBags;

        public Absackung(IOpcNode parent, OpcName name, OpcContext context) : base(parent, name, context)
        {
            Abfüllung = new Motor(this, "Abfüllung", context);

            _numberOfBags = new OpcDataVariableNode<int>(this, "AnzahlSäcke", 0);
            _numberOfBags.AccessLevel = OpcAccessLevel.CurrentRead;
            _resetBags = new OpcDataVariableNode<bool>(this, "ResetAnzahl");
            _resetBags.BeforeApplyChanges += _resetBags_BeforeApplyChanges;

            RegisterComponent(Abfüllung);
        }

        public Motor Abfüllung { get; set; }

        public override void PushMaterial(double volume)
        {
            queue.Push(volume);
        }

        public int NumberOfBags
        {
            get { return _numberOfBags.Value; }
            set
            {
                if (_numberOfBags.Value != value)
                {
                    _numberOfBags.Value = value;
                    _numberOfBags.ApplyChanges(Context);
                }

            }
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

        double _sum;
        public override void Simulate(int clock)
        {
            base.Simulate(clock);

            queue.Delay = 2000;

            switch (State)
            {
                case OperationState.Stopped:
                    Abfüllung.SwitchOff();
                    break;

                case OperationState.Started:
                    Abfüllung.SwitchOn();
                    queue.Tick(clock);
                    double mat = RetrieveFromQueue();

                    _sum += mat;

                    if (_sum > 25)
                    {
                        while(_sum > 25)
                        {
                            NumberOfBags += 1;
                            _sum -= 25;
                        }
                    }
                   
                    break;

                case OperationState.CriticalStop:
                    Abfüllung.SwitchOff();
                    break;
                default:
                    Abfüllung.SwitchOff();
                    break;
            }
        }

        private void _resetBags_BeforeApplyChanges(object sender, EventArgs e)
        {
            NumberOfBags = 0;
        }

    }
}
