using Opc.UaFx;
using Plant.OpcUa.Common;
using Plant.OpcUa.Helper;


namespace Plant.OpcUa.Plant.Components
{
    public class TemperaturSensor : OpcSimComponentControl
    {
        private OpcDataVariableNode<double> _upperLimit;
        private OpcDataVariableNode<double> _maxValue;
        private OpcDataVariableNode<string> _unit;
        private OpcDataVariableNode<string> _range;
        private OpcDataVariableNode<double> _tempValue;
        private bool SensorActive = false;
        private OpcDataVariableNode<bool> _overtemp;

        public TemperaturSensor(IOpcNode parent, OpcName name, OpcContext context) : base(parent, name, context)
        {
            _unit = new OpcDataVariableNode<string>(this, "Unit", "°C");
            _range = new OpcDataVariableNode<string>(this, "Range", "0..0");
            _maxValue = new OpcDataVariableNode<double>(this, "MaxValue");
            _tempValue = new OpcDataVariableNode<double>(this, "Value");
            _upperLimit = new OpcDataVariableNode<double>(this, "UpperLimit", 100.0);
            _upperLimit.AccessLevel = OpcAccessLevel.CurrentRead;
            _overtemp = new OpcDataVariableNode<bool>(this, "AlarmÜbertemperatur");
            CreatePT1();
        }

        public override void AcknowledgeAlarm()
        {
            base.AcknowledgeAlarm();
            OverTemp = false;
        }

        public bool OverTemp
        {
            get { return _overtemp.Value; }
            protected set
            {
                if (_overtemp.Value != value)
                {
                    if (value)
                    {
                        OnAlarmEventOccured(new AlarmEventArgs(AlarmType.Error, true));
                    }

                    _overtemp.Value = value;
                    _overtemp.ApplyChanges(Context);
                }
            }
        }

        public PT1_Floating TempPt1 { get; private set; }

        public double MaxValue
        {
            get { return _maxValue.Value; }
            private set { 
                
                _maxValue.Value = value;
                _maxValue.ApplyChanges(Context);
            }
        }

        public double TempValue
        {
            get { return _tempValue.Value; }
            set
            {
                _tempValue.Value = value;
                _tempValue.ApplyChanges(Context);
            }
        }

        public string Unit
        {
            get { return _unit.Value; }
            set
            {
                _unit.Value = value;
                _unit.ApplyChanges(Context);
            }
        }

        public string Range
        {
            get { return _range.Value; }
            set
            {
                _range.Value = value;
                _range.ApplyChanges(Context);
            }
        }

        public double UpperLimit
        {
            get { return _upperLimit.Value; }
            set
            {
                _upperLimit.Value = value;
                _upperLimit.ApplyChanges(Context);
                CreatePT1();
                Range = "0.." + UpperLimit;
            }
        }

        private void CreatePT1()
        {
            TempPt1 = new PT1_Floating(UpperLimit);
            TempPt1.FloatingChangeTime = 5000;
            TempPt1.FloatingTempValue = 5;
            TempPt1.TimeConstant = 30000;
        }

        public override void Simulate(int clock)
        {
            base.Simulate(clock);

            TempValue = TempPt1.GetValue(SensorActive, clock);

            if (MaxValue < TempValue)
            {
                MaxValue = TempValue;
            }
        }

        public override void SwitchOn()
        {
            SensorActive = true;
        }

        public override void SwitchOff()
        {
            SensorActive = false;
        }
    }
}
