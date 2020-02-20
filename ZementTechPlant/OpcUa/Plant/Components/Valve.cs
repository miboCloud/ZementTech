using Opc.UaFx;
using Plant.OpcUa.Common;
using Plant.OpcUa.Helper;
using System;

namespace Plant.OpcUa.Plant.Components
{
    public class Valve : OpcSimComponentControl
    {
        private OpcAnalogItemNode<double> _flow;
        private OpcAnalogItemNode<double> _maxFlow;
        private OpcDataVariableNode<bool> _open;
        private OpcDataVariableNode<bool> _movementAlarm;

        public Valve(IOpcNode parent, OpcName name, OpcContext context) : base(parent, name, context)
        {
            _maxFlow = new OpcAnalogItemNode<double>(this, "MaxDurchfluss", 600.0);
            _maxFlow.BeforeApplyChanges += _maxFlow_BeforeApplyChanges;

            _flow = new OpcAnalogItemNode<double>(this, "Durchfluss");
            _flow.InstrumentRange = new OpcValueRange(400.0, 0);
            _flow.EngineeringUnit = new OpcEngineeringUnitInfo(4666675, "m3/min", "Kubikmeter pro Minute");
            _flow.EngineeringUnitRange = new OpcValueRange(400.0, 0);
            _flow.Description = "Kubikmeter pro Minute";
            
            _open = new OpcDataVariableNode<bool>(this, "Offen");
            _open.Description = "True = offen, False = zu";
            _movementAlarm = new OpcDataVariableNode<bool>(this, "AlarmPosition");
            ManualControl = new ManualControl(this, "Handbetrieb", Context);
        }

        public override void AcknowledgeAlarm()
        {
            base.AcknowledgeAlarm();
            MovementAlarm = false;
        }

        public override void EnableManualMode(bool on)
        {
            base.EnableManualMode(on);

            ManualControl.Enable = on;
        }

        public bool MovementAlarm
        {
            get { return _movementAlarm.Value; }
            protected set
            {
                if (_movementAlarm.Value != value)
                {
                    if (value)
                    {
                        OnAlarmEventOccured(new AlarmEventArgs(AlarmType.Error, true));
                    }

                    _movementAlarm.Value = value;
                    _movementAlarm.ApplyChanges(Context);
                }
            }
        }

        private ManualControl ManualControl { get; set; }

        public double GetFlowPerClock(int clock)
        {
            return Flow / 60000.0 * clock;
        }

        private void _maxFlow_BeforeApplyChanges(object sender, EventArgs e)
        {
            FlowPt1 = new PT1_Floating(MaxFlow);
            FlowPt1.FloatingChangeTime = 2500;
            FlowPt1.FloatingTempValue = 0.4;
            FlowPt1.TimeConstant = 10000;

            _flow.InstrumentRange = new OpcValueRange(MaxFlow, 0);
            _flow.EngineeringUnitRange = new OpcValueRange(MaxFlow, 0);
        }

        public PT1_Floating FlowPt1 { get; private set; }

        public double MaxFlow
        {
            get { return _maxFlow.Value; }
            set
            {
                if (_maxFlow.Value != value)
                {
                    _maxFlow.Value = value;
                    _maxFlow.ApplyChanges(Context);
                }
            }
        }

        public bool Open
        {
            get { return _open.Value; }
            private set 
            { 
                if(_open.Value != value)
                {
                    _open.Value = value;
                    _open.ApplyChanges(Context);
                }
            }
        }

        public double Flow
        {
            get { return _flow.Value; }
            private set
            {
                if (_flow.Value != value)
                {
                    _flow.Value = value;
                    _open.ApplyChanges(Context);
                }
            }
        }

        public override void SwitchOff()
        {
            Open = false;
        }

        public override void SwitchOn()
        {
            Open = true;
        }

        public override void Simulate(int clock)
        {
            base.Simulate(clock);

            if (ManualControl.Enable)
            {
                Open = ManualControl.Start;
            }

            Flow = FlowPt1.GetValue(Open, clock);
        }
    }
}
