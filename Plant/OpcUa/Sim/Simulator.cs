using Plant.OpcUa.Component;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Plant.OpcUa.Sim
{
    public class Simulator
    {
        private static Simulator instance = null;
        private List<OpcSim> list = new List<OpcSim>();

        private Simulator()
        {

        }

        public static Simulator Instance()
        {
            if (instance == null)
            {
                instance = new Simulator();
            }
            return instance;
        }

        public void Register(OpcSim simNode)
        {
            list.Add(simNode);
        }

        public ZementTechNodeManager ZementTechNodeManager { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clock"></param>
        /// <param name="timelapse">1/10</param>
        public void Execute()
        {
            int multiply = 1;
            while (true)
            {
                if(ZementTechNodeManager.TimeLapse == ZementTechNodeManager.TimeMultiplier.x1)
                {
                    multiply = 1;
                }else if (ZementTechNodeManager.TimeLapse == ZementTechNodeManager.TimeMultiplier.x10)
                {
                    multiply = 10;
                }else if (ZementTechNodeManager.TimeLapse == ZementTechNodeManager.TimeMultiplier.x100)
                {
                    multiply = 100;
                }

                    list.ForEach(o => o.Simulate(ZementTechNodeManager.Clock));
                Thread.Sleep(100/ multiply);
            }
        }
    }
}
