using Opc.UaFx;
using Plant.OpcUa.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Plant.OpcUa.Plant.Components
{
    public class LevelSensor : OpcSimComponent
    {
      
        private OpcAnalogItemNode<double> _maxLevelKg;
        private OpcAnalogItemNode<double> _levelKg;
        private OpcDataVariableNode<double> _levelPercent;
        private OpcDataVariableNode<bool> _alarmLevelHeigh;
        private OpcDataVariableNode<bool> _warningLevelHeigh;

        public LevelSensor(OpcSimModule parent, OpcName name, OpcContext context, double maxLevel ) : base(parent, name, context)
        {
          
            _maxLevelKg = new OpcAnalogItemNode<double>(this, "MaxLevelKg", maxLevel);
            _maxLevelKg.AccessLevel = OpcAccessLevel.CurrentRead;

            _levelKg = new OpcAnalogItemNode<double>(this, "LevelKg");
            _levelKg.InstrumentRange = new OpcValueRange(MaxLevelKg, 0);
            _levelKg.EngineeringUnit = new OpcEngineeringUnitInfo(4933453, "Kg", "Kilogramm");
            _levelKg.EngineeringUnitRange = new OpcValueRange(MaxLevelKg, 0);
            _levelKg.Description = "Füllstand in Kilogramm";
            _levelKg.BeforeApplyChanges += _levelKg_BeforeApplyChanges;

            _levelPercent = new OpcDataVariableNode<double>(this, "LevelPercent");
            _levelPercent.Description = "Füllstand in Prozent";
            _levelPercent.AccessLevel = OpcAccessLevel.CurrentRead;

            _alarmLevelHeigh = new OpcDataVariableNode<bool>(this, "AlarmFüllstandHoch");
            _alarmLevelHeigh.Description = "Füllstand > 95%";

            _warningLevelHeigh = new OpcDataVariableNode<bool>(this, "WarnungFüllstandHoch");
            _warningLevelHeigh.Description = "Füllstand > 80%";

            //Random r = new Random();
            //LevelKg = r.NextDouble() * 5000.0;
        }

        /// <summary>
        /// Add Material
        /// </summary>
        /// <param name="volumeKg"></param>
        /// <returns>Return amount of not added material (0 means everything could be inserted)</returns>
        public virtual double Insert(double volumeKg)
        {
            if (MaxLevelKg < (LevelKg + volumeKg))
            {
                double rest = (LevelKg + volumeKg) - MaxLevelKg;
                LevelKg = MaxLevelKg;
                return rest;
            }
            else
            {
                LevelKg += volumeKg;
                return 0;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="volumeKg"></param>
        /// <returns>Returns retrieved material</returns>
        public  virtual double Retrieve(double requestedVolumeKg)
        {
            if (LevelKg < requestedVolumeKg)
            {
                double rest = LevelKg;
                LevelKg = 0;
                return rest;
            }
            else
            {
                LevelKg -= requestedVolumeKg;
                return requestedVolumeKg;
            }
        }


        /// <summary>
        /// Module handles this amount of material in kg
        /// </summary>
        public double LevelKg
        {
            get { return _levelKg.Value; }
            protected set
            {
                if(_levelKg.Value != value){
                    _levelKg.Value = value;

                    _levelKg.ApplyChanges(Context);
                }
            }
        }

        /// <summary>
        /// Percentage view of material
        /// </summary>
        public double LevelPercent
        {
            get { return _levelPercent.Value; }
            protected set
            {
                if(_levelPercent.Value != value)
                {
                    _levelPercent.Value = value;
                    _levelPercent.ApplyChanges(Context);
                }
            }
        }

        public override void AcknowledgeAlarm()
        {
            base.AcknowledgeAlarm();
            AlarmLevelHeigh = false;
        }

        public bool AlarmLevelHeigh
        {
            get { return _alarmLevelHeigh.Value; }
            protected set
            {
                if(_alarmLevelHeigh.Value != value)
                {
                    if (value)
                    {
                        OnAlarmEventOccured(new AlarmEventArgs(AlarmType.Error, true));
                    }
                    
                    _alarmLevelHeigh.Value = value;
                    _alarmLevelHeigh.ApplyChanges(Context);
                }
            }
        }

        public bool WarningLevelHeigh
        {
            get { return _warningLevelHeigh.Value; }
            protected set
            {
                if(_warningLevelHeigh.Value != value)
                {
                    OnAlarmEventOccured(new AlarmEventArgs(AlarmType.Warning, value));
                    _warningLevelHeigh.Value = value;
                    _warningLevelHeigh.ApplyChanges(Context);
                }
            }
        }


        private void _levelKg_BeforeApplyChanges(object sender, EventArgs e)
        {
            LevelPercent = 100.0 / MaxLevelKg * LevelKg;

            if (LevelPercent >= 95)
            {
                AlarmLevelHeigh = true;
            }
            else
            {
                AlarmLevelHeigh = false;
            }

            if (LevelPercent >= 80)
            {
                WarningLevelHeigh = true;
            }
            else
            {
                WarningLevelHeigh = false;
            }


        }

        /// <summary>
        /// Maximum capacity
        /// </summary>
        public double MaxLevelKg
        {
            get { return _maxLevelKg.Value; }
             set 
            { 
                if(_maxLevelKg.Value != value)
                {
                    _maxLevelKg.Value = value;
                    _maxLevelKg.ApplyChanges(Context);
                }
            }
        }

    }
}
