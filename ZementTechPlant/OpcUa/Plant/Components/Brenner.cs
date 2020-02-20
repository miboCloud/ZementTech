using Opc.UaFx;
using Plant.OpcUa.Common;

namespace Plant.OpcUa.Plant.Components
{
    public class Brenner : Motor
    {
        private OpcDataVariableNode<bool> _fuelMissing;

        public Brenner(IOpcNode parent, OpcName name, OpcContext context) : base(parent, name, context)
        {
            _fuelMissing = new OpcDataVariableNode<bool>(this, "AlarmBrennstoffFehlt");
        }

        public override void AcknowledgeAlarm()
        {
            base.AcknowledgeAlarm();
            FuelMissing = false;
        }

        public bool FuelMissing
        {
            get { return _fuelMissing.Value; }
            protected set
            {
                if (_fuelMissing.Value != value)
                {
                    if (value)
                    {
                        OnAlarmEventOccured(new AlarmEventArgs(AlarmType.Error, true));
                    }

                    _fuelMissing.Value = value;
                    _fuelMissing.ApplyChanges(Context);
                }
            }
        }
    }
}
