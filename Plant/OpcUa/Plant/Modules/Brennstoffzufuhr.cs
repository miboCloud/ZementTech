using Opc.UaFx;
using Plant.OpcUa.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Plant.OpcUa.Plant.Modules
{
    public class Brennstoffzufuhr : OpcSimModule
    {
        private OpcDataVariableNode<int> _gasLevel;
        private OpcDataVariableNode<int> _coleLevel;

        public Brennstoffzufuhr(IOpcNode parent, OpcName name, OpcContext context) : base(parent, name, context)
        {
            _gasLevel = new OpcDataVariableNode<int>(this, "GasVorrat", 35);
            _coleLevel = new OpcDataVariableNode<int>(this, "KohleVorrat", 75);
        }
    }
}

