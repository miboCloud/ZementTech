using Opc.UaFx;
using Plant.OpcUa.Helper;
using System;
using System.Collections.Generic;

namespace Plant.OpcUa.Common
{
    /// <summary>
    /// Controls a system unit
    /// </summary>
    public abstract class OpcSimModule : OpcSimOperation, IMaterialSender, IMaterialReceiver
    {
        private OpcDataVariableNode<bool> _start;
        private OpcDataVariableNode<bool> _stop;
        private OpcDataVariableNode<bool> _acknowledge;
        private OpcDataVariableNode<bool> _warningPending;
        private Dictionary<object, bool> _warningList = new Dictionary<object, bool>();
        private OpcDataVariableNode<bool> _receiverReady;


        public OpcSimModule(IOpcNode parent, OpcName name, OpcContext context) : base(parent, name, context)
        {
            _start = new OpcDataVariableNode<bool>(this, "Start");
            _start.Description = "Eine steigende Flanke startet die Anlage.";

            _stop = new OpcDataVariableNode<bool>(this, "Stop");
            _stop.Description = "Eine steigende Flanke stoppt die Anlage.";

            _warningPending = new OpcDataVariableNode<bool>(this, "WarnungAktiv");
            _warningPending.Description = "Zeigt eine anstehende Warnung.";
            _warningPending.AccessLevel = OpcAccessLevel.CurrentRead;

            _acknowledge = new OpcDataVariableNode<bool>(this, "Quittieren");
            _acknowledge.Description = "Eine steigende Flanke quittiert die Anlage.";

            _stop.BeforeApplyChanges += _stop_BeforeApplyChanges;
            _start.BeforeApplyChanges += _start_BeforeApplyChanges;
            _acknowledge.BeforeApplyChanges += _acknowledge_BeforeApplyChanges;

            _receiverReady = new OpcDataVariableNode<bool>(this, "EmpfangenBereit");
            _receiverReady.AccessLevel = OpcAccessLevel.CurrentRead;

        }

        public double UnassignedMaterial { get; protected set; }

        private List<OpcSimComponent> Components { get; set; } = new List<OpcSimComponent>();



        private void _acknowledge_BeforeApplyChanges(object sender, EventArgs e)
        {
            Components.ForEach(c => c.AcknowledgeAlarm());
            State = OperationState.Stopped;
        }


        protected void RegisterComponent(OpcSimComponent component)
        {
            if (!Components.Contains(component))
            {
                component.AlarmEventChanged += OnAlarmEventChanged;
                Components.Add(component);
            }
        }

        protected virtual void OnAlarmEventChanged(object sender, AlarmEventArgs e)
        {
            if (e.Active)
            {
                if (e.AlarmType == AlarmType.Error)
                {
                    OnCriticalStop(sender);
                }

                if (e.AlarmType == AlarmType.Warning)
                {
                    if (_warningList.ContainsKey(sender))
                    {
                        _warningList[sender] = true;
                    }
                    else
                    {
                        _warningList.Add(sender, true);
                    }
                }
            }
            else
            {
                if (e.AlarmType == AlarmType.Warning)
                {
                    _warningList.Remove(sender);
                }
            }
            
            foreach(KeyValuePair<object, bool> item in _warningList)
            {
                if (item.Value)
                {
                    WarningPending = true;
                    return;
                }
            }
            WarningPending = false;
        }

        public bool WarningPending 
        {
            get
            {
                return _warningPending.Value;
            }
            private set
            {
                if(_warningPending.Value != value)
                {
                    _warningPending.Value = value;
                    _warningPending.ApplyChanges(Context);
                }
            }
        }

        public IMaterialReceiver Receiver { get; set; }

        protected override void OnModeChanged(EventArgs e)
        {
            base.OnModeChanged(e);
            State = OperationState.Stopped;
            Components.ForEach(c => c.EnableManualMode(Mode == OperationMode.Manual));
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            
        }

        protected virtual bool ReceiverReleased()
        {
            return true;
        }

        public bool ReceiverReady
        {
            get
            {
                return _receiverReady.Value;
            }
            set
            {
                if (_receiverReady.Value != value)
                {
                    _receiverReady.Value = value;
                    _receiverReady.ApplyChanges(Context);
                }
            }
        }

        protected virtual void OnStart()
        {
            if(State == OperationState.Stopped)
            {
                State = OperationState.Started;
            }
        }

        protected virtual void OnStop()
        {
            if(State == OperationState.Started)
            {
                State = OperationState.Stopped;
            }
        }

        protected virtual void OnCriticalStop(object sender)
        {
            State = OperationState.CriticalStop;
        }

        private void _start_BeforeApplyChanges(object sender, EventArgs e)
        {
            if (_start.Value)
            {
                OnStart();
            }
        }

        private void _stop_BeforeApplyChanges(object sender, EventArgs e)
        {
            if (_stop.Value)
            {
                OnStop();
            }
        }

        public override void Simulate(int clock)
        {
            base.Simulate(clock);

            ReceiverReady = State == OperationState.Started && ReceiverReleased();

        }

        public virtual void PushMaterial(double volume)
        {
            UnassignedMaterial += volume;
        }
    }
}
