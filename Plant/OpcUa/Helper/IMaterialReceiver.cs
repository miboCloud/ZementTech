using System;
using System.Collections.Generic;
using System.Text;

namespace Plant.OpcUa.Helper
{
    public interface IMaterialReceiver
    {
       void PushMaterial(double volume);
       bool ReceiverReady { get; }
    }
}
