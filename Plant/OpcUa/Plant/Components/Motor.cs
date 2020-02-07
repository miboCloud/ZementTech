using Opc.UaFx;
using Plant.OpcUa.Common;
using Plant.OpcUa.Helper;

namespace Plant.OpcUa.Plant.Components
{
    public class Motor : OpcSimComponentControl
    {
        private OpcDataVariableNode<double> _speed;
        private OpcDataVariableNode<double> _power;
        private OpcDataVariableNode<double> _current;
        private OpcDataVariableNode<bool> _running;
        private OpcDataVariableNode<bool> _overcurrent;

        public Motor(IOpcNode parent, OpcName name, OpcContext context) : base(parent, name, context)
        {
            tempSensor = new TemperaturSensor(this, "Temperatur", context);
            tempSensor.AlarmEventChanged += TempSensor_AlarmEventChanged;
            tempSensor.UpperLimit = 70.0;
            
            _speed = new OpcDataVariableNode<double>(this, "Speed");
            _current = new OpcDataVariableNode<double>(this, "Current");
            _power = new OpcDataVariableNode<double>(this, "Power");
            _running = new OpcDataVariableNode<bool>(this, "Running");
            _overcurrent = new OpcDataVariableNode<bool>(this, "AlarmÜberlast");

            ManualControl = new ManualControl(this, "Handbetrieb", Context);

            CreateSpeedPt1();
            PowerPt1 = new PT1_Floating(5000);
            CurrentPt1 = new PT1_Floating(12.5);
            CurrentPt1.TimeConstant = 200;
        }

        public TemperaturSensor tempSensor { get; set; }

        private ManualControl ManualControl { get; set; }

        private void TempSensor_AlarmEventChanged(object sender, AlarmEventArgs e)
        {
            OnAlarmEventOccured(e);
        }

        public override void AcknowledgeAlarm()
        {
            base.AcknowledgeAlarm();
            Overcurrent = false;
        }

        public bool Overcurrent
        {
            get { return _overcurrent.Value; }
            protected set
            {
                if (_overcurrent.Value != value)
                {
                    if (value)
                    {
                        OnAlarmEventOccured(new AlarmEventArgs(AlarmType.Error, true));
                    }
                    
                    _overcurrent.Value = value;
                    _overcurrent.ApplyChanges(Context);
                }
            }
        }

        public PT1_Floating SpeedPt1
        {
            get; private set;
        }

        public PT1_Floating PowerPt1
        {
            get; private set;
        }

        public PT1_Floating CurrentPt1
        {
            get; private set;
        }

        public double Speed
        {
            get { return _speed.Value; }
            private set
            {
                if (_speed.Value != value) { 
                    _speed.Value = value;
                    _speed.ApplyChanges(Context);
                }
            }
        }

        public double Power
        {
            get { return _power.Value; }
            private set
            {

                _power.Value = value;
                _power.ApplyChanges(Context);
            }
        }

        public double Current
        {
            get { return _current.Value; }
            private set
            {
                _current.Value = value;
                _current.ApplyChanges(Context);
            }
        }

        public bool Running
        {
            get { return _running.Value; }
            private set
            {
                _running.Value = value;
                _running.ApplyChanges(Context);
            }
        }

        private double _maxSpeed = 3000;
        public double MaxSpeed
        {
            get { return _maxSpeed; }
            set
            {
                _maxSpeed = value;
                CreateSpeedPt1();
            }
        }

        private void CreateSpeedPt1()
        {
            SpeedPt1 = new PT1_Floating(MaxSpeed);
            SpeedPt1.TimeConstant = 1000;
            
        }

        public override void EnableManualMode(bool on)
        {
            base.EnableManualMode(on);

            ManualControl.Enable = on;
        }

        public override void Simulate(int clock)
        {
            base.Simulate(clock);

            if (ManualControl.Enable)
            {
                Running = ManualControl.Start;
            }

            Speed = SpeedPt1.GetValue(Running, clock);

            Power = PowerPt1.GetValue(Running, clock);

            Current = CurrentPt1.GetValue(Running, clock);
        }

        public override void SwitchOn()
        {
            Running = true;
        }

        public override void SwitchOff()
        {
            Running = false;
        }
    }
}
