using Opc.UaFx;
using System;
using System.Collections.Generic;
using System.Text;

namespace Plant.OpcUa.Helper
{
    [OpcDataType(id: "OperationMode", namespaceIndex: 2)]
    public enum OperationMode : int
    {
        [OpcEnumMember("Automatik")]
        Automatic,
        [OpcEnumMember("Handbetrieb")]
        Manual
    }
}
