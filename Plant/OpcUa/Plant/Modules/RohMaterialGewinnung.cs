using Opc.UaFx;
using Plant.OpcUa.Common;
using Plant.OpcUa.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Plant.OpcUa.Plant.Modules
{
    public class RohMaterialGewinnung : OpcSimModule
    {

        private OpcDataVariableNode<bool> _truckReadyToUnload;
        private OpcDataVariableNode<int> _timeUnloading;
        private OpcDataVariableNode<int> _timeWaitTruck;
        private OpcDataVariableNode<bool> _unloadingActive;
        private OpcDataVariableNode<int> _simTruckIntervalMs;
        private OpcDataVariableNode<double> _simTruckLoadCapacity;
        private OpcDataVariableNode<int> _simTruckUnloadTimeMs;


        public RohMaterialGewinnung(IOpcNode parent, OpcName name, OpcContext context) : base(parent, name, context)
        {

            _truckReadyToUnload = new OpcDataVariableNode<bool>(this, "TruckBereitZumAbladen");
            _truckReadyToUnload.Description = "Truck is ready to unload.";
            _truckReadyToUnload.AccessLevel = OpcAccessLevel.CurrentRead;

            _unloadingActive = new OpcDataVariableNode<bool>(this, "TruckAbladenAktiv");
            _unloadingActive.Description = "Truck is ready to unload.";
            _unloadingActive.AccessLevel = OpcAccessLevel.CurrentRead;

            _timeUnloading = new OpcDataVariableNode<int>(this, "ZeitAbladen");
            _timeUnloading.Description = "Zeit des Abladevorgangs";

            _timeWaitTruck = new OpcDataVariableNode<int>(this, "ZeitWartenAufTruck");
            _timeWaitTruck.Description = "Wartezeit nächster Truck";

            _simTruckIntervalMs = new OpcDataVariableNode<int>(this, "Z_SimTruckIntervallMs", 10000);
            _simTruckLoadCapacity = new OpcDataVariableNode<double>(this, "Z_SimTruckKapazität", 10000.0);
            _simTruckUnloadTimeMs = new OpcDataVariableNode<int>(this, "Z_SimTruckAbladeZeitMs", 8000);
        }

        public int TimeUnloading
        {
            get { return _timeUnloading.Value; }
            private set
            {
                _timeUnloading.Value = value;
                _timeUnloading.ApplyChanges(Context);
            }
        }

        public int TimeWaitTruck
        {
            get { return _timeWaitTruck.Value; }
            private set
            {
                _timeWaitTruck.Value = value;
                _timeWaitTruck.ApplyChanges(Context);
            }
        }


        public bool TruckReadyToUnload
        {
            get { return _truckReadyToUnload.Value; }
            private set 
            {
                _truckReadyToUnload.Value = value;
                _truckReadyToUnload.ApplyChanges(Context);
            }
        }

        public bool UnloadingActive
        {
            get { return _unloadingActive.Value; }
            private set
            {
                if(_unloadingActive.Value != value)
                {
                    _unloadingActive.Value = value;
                    _unloadingActive.ApplyChanges(Context);
                }
                
            }
        }

        // Truck intervall
        public int TruckIntervalMs
        {
            get
            {
                return _simTruckIntervalMs.Value;
            }
            set
            {
                _simTruckIntervalMs.Value = value;
                _simTruckIntervalMs.ApplyChanges(Context);
            }
        }

        /// <summary>
        /// Load in kg
        /// </summary>
        public double TruckLoadCapacity
        {
            get
            {
                return _simTruckLoadCapacity.Value;
            }
            set
            {
                _simTruckLoadCapacity.Value = value;
                _simTruckLoadCapacity.ApplyChanges(Context);
            }
        }

        /// <summary>
        /// Load in kg
        /// </summary>
        public int TruckUnloadTimeMs
        {
            get
            {
                return _simTruckUnloadTimeMs.Value;
            }
            set
            {
                _simTruckUnloadTimeMs.Value = value;
                _simTruckUnloadTimeMs.ApplyChanges(Context);
            }
        }


        public override void Simulate(int clock)
        {
            base.Simulate(clock);

            switch (State)
            {
                case OperationState.Stopped:
                    
                    break;

                case OperationState.Started:

                    if (TruckReadyToUnload)
                    {
                        if (Receiver != null)
                        {
                            if (Receiver.ReceiverReady)
                            {
                                TimeUnloading += clock;
                                UnloadingActive = true;

                                double volumePerCycle = TruckLoadCapacity / (TruckUnloadTimeMs / clock);
                                Receiver.PushMaterial(volumePerCycle);

                                if (TimeUnloading >= TruckUnloadTimeMs)
                                {
                                    TruckReadyToUnload = false;
                                    TimeWaitTruck = 0;
                                    TimeUnloading = 0;
                                    UnloadingActive = false;
                                }
                            }
                            else
                            {
                                UnloadingActive = false;
                            }
                        }
                    }else if (TimeWaitTruck >= TruckIntervalMs)
                    {
                        TruckReadyToUnload = true;
                        TimeUnloading = 0;
                    }
                    else
                    {
                        TimeWaitTruck += clock;
                    }

                    break;

                case OperationState.CriticalStop:
                    
                    break;
                default:
                    
                    break;
            }


        }
    }
}
