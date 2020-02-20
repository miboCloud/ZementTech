using Opc.UaFx;
using Plant.OpcUa.Common;
using Plant.OpcUa.Plant.Components;
using System.Collections.Generic;

namespace Plant.OpcUa.Plant.Modules
{
    public class Brecher : OpcSimModule
    {
        private List<Motor> motors { get; set; } = new List<Motor>();

        public Brecher(IOpcNode parent, OpcName name, OpcContext context) : base(parent, name, context)
        {
            motors.ForEach(m => RegisterComponent(m));
        }


        public override void Simulate(int clock)
        {
            base.Simulate(clock);

          
        }
    }
}
