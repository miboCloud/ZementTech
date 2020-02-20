using Opc.UaFx;
using Plant.OpcUa.Component;

namespace Plant.OpcUa.Plant.Components
{
    public class ManualControl : OpcSim
    {

        public OpcDataVariableNode<bool> _start;
        public OpcDataVariableNode<bool> _manualActive;

        private bool _enable;

        

        public bool Start
        {
            get { return _start.Value; }
            set { _start.Value = value; _start.ApplyChanges(Context); }
        }


        public bool Enable
        {
            get { return _enable; }
            set { 
                _enable = value;
                _manualActive.Value = _enable;


                if (_enable)
                {
                    _start.AccessLevel = OpcAccessLevel.CurrentReadOrWrite;
                }
                else
                {
                    Start = false;
                    _start.AccessLevel = OpcAccessLevel.CurrentRead;
                    
                }
                _manualActive.ApplyChanges(Context);
                _start.ApplyChanges(Context);
            }
        }

        public ManualControl(IOpcNode parent, OpcName name, OpcContext context) : base(parent, name, context)
        {

            _start = new OpcDataVariableNode<bool>(this, "Ein");
            _start.AccessLevel = OpcAccessLevel.CurrentRead;
            _manualActive = new OpcDataVariableNode<bool>(this, "Erlaubt");
            _manualActive.AccessLevel = OpcAccessLevel.CurrentRead;



        }
    }
}
