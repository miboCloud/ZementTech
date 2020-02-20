using System;
using System.Collections.Generic;
using System.Text;

namespace Plant.OpcUa.Helper
{
    public interface IMaterialSender
    {
        IMaterialReceiver Receiver { get; }
    }
}
