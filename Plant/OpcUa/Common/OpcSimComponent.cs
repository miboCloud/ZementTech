using Opc.UaFx;
using Plant.OpcUa.Component;
using Plant.OpcUa.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Plant.OpcUa.Common
{
    public class OpcSimComponent : OpcSim
    {
        private OpcDataVariableNode<bool> _simError;
        private OpcDataVariableNode<bool> _simWarning;
        private OpcDataVariableNode<bool> _warningActive;
        private OpcDataVariableNode<bool> _errorActive;

        public OpcSimComponent(IOpcNode parent, OpcName name, OpcContext context) : base(parent, name, context)
        {
            _warningActive = new OpcDataVariableNode<bool>(this, "WarnungAktiv");
            _errorActive = new OpcDataVariableNode<bool>(this, "ErrorAktiv");
            _simError = new OpcDataVariableNode<bool>(this, "SimStörung");
            _simError.BeforeApplyChanges += _simError_BeforeApplyChanges;
            _simWarning = new OpcDataVariableNode<bool>(this, "SimWarnung");
            _simWarning.BeforeApplyChanges += _simWarning_BeforeApplyChanges;
        }

        public bool WarningActive
        {
            get { return _warningActive.Value; }
            protected set 
            { 
                if(_warningActive.Value != value) {
                    _warningActive.Value = value;
                    _warningActive.ApplyChanges(Context);
                }
                
            }
        }

        public bool ErrorActive
        {
            get { return _errorActive.Value; }
            protected set
            {
                if (_errorActive.Value != value)
                {
                    _errorActive.Value = value;
                    _errorActive.ApplyChanges(Context);
                }

            }
        }

        private void _simWarning_BeforeApplyChanges(object sender, EventArgs e)
        {
            WarningActive = _simWarning.Value;
            OnAlarmEventOccured(new AlarmEventArgs(AlarmType.Warning, _simWarning.Value));
        }

        private void _simError_BeforeApplyChanges(object sender, EventArgs e)
        {
            if (_simError.Value)
            {
                ErrorActive = true;
                OnAlarmEventOccured(new AlarmEventArgs(AlarmType.Error, true));
            }
        }

        public virtual void AcknowledgeAlarm()
        {
            ErrorActive = false;
        }

        public virtual void EnableManualMode(bool on)
        {

        }

        public event EventHandler<AlarmEventArgs> AlarmEventChanged;

        protected virtual void OnAlarmEventOccured(AlarmEventArgs e)
        {
            
            EventHandler<AlarmEventArgs> handler = AlarmEventChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }

    public class AlarmEventArgs : EventArgs
    {
        public AlarmEventArgs(AlarmType type, bool active)
        {
            AlarmType = type;
            Active = active;
        }

        public AlarmType AlarmType { get; }

        public bool Active { get;  }
    }

    public enum AlarmType
    {
        Warning, Error
    }
}
