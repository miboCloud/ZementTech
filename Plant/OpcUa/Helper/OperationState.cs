using Opc.UaFx;
using System;
using System.Collections.Generic;
using System.Text;

namespace Plant.OpcUa.Helper
{
    [OpcDataType(id: "OperationState", namespaceIndex: 2)]
    public enum OperationState : int
    {
        [OpcEnumMember("Gestoppt")]
        Stopped,
        [OpcEnumMember("Gestartet")]
        Started,
        [OpcEnumMember("Störung")]
        CriticalStop
    }
}
