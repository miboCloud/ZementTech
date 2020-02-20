using Opc.UaFx;
using Plant.OpcUa.Common;
using Plant.OpcUa.Helper;
using Plant.OpcUa.Plant.Components;

namespace Plant.OpcUa.Plant.Modules
{

    public class Silo : OpcSimModule
    {
       
        public Silo(IOpcNode parent, OpcName name, OpcContext context, double capacity) : base(parent, name, context)
        {
            
            Level = new LevelSensor(this, "LevelSensor", Context, capacity);
            Motor = new Motor(this, "Fördermotor", Context);
            Outlet = new Valve(this, "Ausgangsventil", Context);
            RegisterComponent(Motor);
            RegisterComponent(Level);
            RegisterComponent(Outlet);
        }
       
        /// <summary>
        /// Fördermotor
        /// </summary>
        public Motor Motor { get; set; }
        public LevelSensor Level { get; set; }

        public Valve Outlet { get; set; }

        protected override void OnCriticalStop(object sender)
        {
            if (!sender.Equals(Level))
            {
                State = OperationState.CriticalStop;
            }
        }

        protected override bool ReceiverReleased()
        {
            return !Level.ErrorActive;
        }

        public override void Simulate(int clock)
        {
            base.Simulate(clock);

            switch (State)
            {
                case OperationState.Stopped:
                    Motor.SwitchOff();
                    Outlet.SwitchOff();
                    break;

                case OperationState.Started:
                    if(Receiver == null)
                    {
                        Motor.SwitchOff();
                        Outlet.SwitchOff();
                        return;
                    }

                    if (Receiver.ReceiverReady)
                    {
                        Outlet.SwitchOn();
                        double flow = Outlet.GetFlowPerClock(clock);
                        double mat = Level.Retrieve(flow);

                        if(mat > 0)
                        {
                            Receiver.PushMaterial(mat);
                            Motor.SwitchOn();
                        }
                        else
                        {
                            Motor.SwitchOff();
                        }
                    }
                    else
                    {
                        Motor.SwitchOff();
                        Outlet.SwitchOff();
                    }
                    break;

                case OperationState.CriticalStop:
                    Motor.SwitchOff();
                    Outlet.SwitchOff();
                    break;

                default:
                    break;
            }
        }

        public override void PushMaterial(double volume)
        {
            Level.Insert(volume);
        }
    }
}
