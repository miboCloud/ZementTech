using Opc.UaFx;
using Plant.OpcUa.Component;
using Plant.OpcUa.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Plant.OpcUa.Common
{
    public abstract class OpcSimOperation : OpcSim
    {
        protected OpcDataVariableNode<OperationState> _operationState;
        protected OpcDataVariableNode<OperationMode> _operationMode;

        public OpcSimOperation(IOpcNode parent, OpcName name, OpcContext context) : base(parent, name, context)
        {
            _operationState = new OpcDataVariableNode<OperationState>(this, "State");
            _operationState.BeforeApplyChanges += _operationState_BeforeApplyChanges;
            _operationState.AccessLevel = OpcAccessLevel.CurrentRead;
            _operationMode = new OpcDataVariableNode<OperationMode>(this, "Mode");
            _operationMode.BeforeApplyChanges += _operationMode_BeforeApplyChanges;
        }


        /// <summary>
        /// Operation state
        /// </summary>
        public OperationState State
        {
            get { return _operationState.Value; }
            protected set
            {
                if (State != value)
                {
                    _operationState.Value = value;
                    _operationState.ApplyChanges(Context);
                }
            }
        }

        /// <summary>
        /// Operation mode
        /// </summary>
        public OperationMode Mode
        {
            get { return _operationMode.Value; }

            set
            {
                if (Mode != value)
                {
                    _operationMode.Value = value;
                    _operationMode.ApplyChanges(Context);
                }

                
            }
        }

        /// <summary>
        /// Event when operation state is changing
        /// </summary>
        public event EventHandler StateChanged;

        /// <summary>
        /// Event when mode is changing
        /// </summary>
        public event EventHandler ModeChanged;



        protected virtual void OnStateChanged(EventArgs e)
        {
            EventHandler handler = StateChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnModeChanged(EventArgs e)
        {
            EventHandler handler = ModeChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void _operationState_BeforeApplyChanges(object sender, EventArgs e)
        {
            OnStateChanged(new EventArgs());
        }

        private void _operationMode_BeforeApplyChanges(object sender, EventArgs e)
        {
            OnModeChanged(new EventArgs());
        }
    }

}
