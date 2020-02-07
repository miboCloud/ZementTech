using Opc.UaFx;
using Plant.OpcUa.Common;
using Plant.OpcUa.Helper;
using Plant.OpcUa.Plant.Components;
using System.Collections.Generic;

namespace Plant.OpcUa.Plant.Modules
{
    public class MotorDelayLane : OpcSimModule
    {
        private List<Motor> motors { get; set; } = new List<Motor>();
        private DelayQueue queue = new DelayQueue();

        public MotorDelayLane(IOpcNode parent, OpcName name, OpcContext context, int numerOfMotors) : base(parent, name, context)
        {
            for(int i=0; i<numerOfMotors; i++)
            {
                motors.Add(new Motor(this, "Motor" + i, Context));
            }

            motors.ForEach(m => RegisterComponent(m));

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

            
            queue.Delay = 5000;

            switch (State)
            {
                case OperationState.Stopped:
                    motors.ForEach(m => m.SwitchOff());
                    break;

                case OperationState.Started:
                    

                    if (Receiver == null)
                    {
                        motors.ForEach(m => m.SwitchOff());
                        return;
                    }

                    if (Receiver.ReceiverReady)
                    {
                        queue.Tick(clock);
                        motors.ForEach(m => m.SwitchOn());
                        double mat = RetrieveFromQueue();

                        if (mat > 0)
                        {
                            Receiver.PushMaterial(mat);
                        }
                    }
                    break;

                case OperationState.CriticalStop:
                    motors.ForEach(m => m.SwitchOff());
                    break;
                default:
                    motors.ForEach(m => m.SwitchOff());
                    break;
            }
        }
    }
}
