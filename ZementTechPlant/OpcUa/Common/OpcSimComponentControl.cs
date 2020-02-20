using Opc.UaFx;
using System;
using System.Collections.Generic;
using System.Text;

namespace Plant.OpcUa.Common
{
    public abstract class OpcSimComponentControl : OpcSimComponent
    {
        public OpcSimComponentControl(IOpcNode parent, OpcName name, OpcContext context) : base(parent, name, context)
        {
        }

        public abstract void SwitchOn();

        public abstract void SwitchOff();
    }
}
