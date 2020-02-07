using Opc.UaFx;
using Plant.OpcUa.Sim;
using System.Collections.Generic;

namespace Plant.OpcUa.Component
{
    public abstract class OpcSim : OpcObjectNode
    {
        protected List<OpcSim> simChildren = new List<OpcSim>();

        public OpcContext Context { get; private set; }

        public OpcSim(IOpcNode parent, OpcName name, OpcContext context) : base(parent, name)
        {
            Context = context;
            Simulator.Instance().Register(this);
            //LoadChildren(simChildren, Context);
            //RegisterChildren();

        }

        //protected virtual void LoadChildren(List<OpcSim> childrenList, OpcContext context)
        //{

        //}

        //private void RegisterChildren()
        //{
        //    Simulator.Instance().Register(this);
        //    foreach (OpcSim sim in simChildren)
        //    {
        //        this.AddChild(Context, sim);
        //        Simulator.Instance().Register(sim);
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clock">timeinterval ms</param>
        public virtual void Simulate(int clock)
        {
            //foreach(OpcSim sim in simChildren)
            //{
            //    sim.Simulate(clock);
            //}
        }
    }
}
