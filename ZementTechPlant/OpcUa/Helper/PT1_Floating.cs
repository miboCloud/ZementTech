using System;

namespace Plant.OpcUa.Helper
{
    enum State
    {
        Stop, Starting, Started
    }

    public class PT1_Floating
    {
        private int time = 0;
        private State state;

        public PT1_Floating(double upperLimit)
        {
            UpperLimit = upperLimit;
       

        }

        public double Value { get; set; }

        public double TimeConstant { get; set; } = 60000;

        public double FloatingChangeTime { get; set; } = 5000;

        public double FloatingTempValue { get; set; } = 5;


        public double UpperLimit { get; set; }

        public double GetValue(bool enable, int clock)
        {
            time += clock;

            switch (state)
            {
                case State.Stop:

                    if (enable)
                    {
                        time = 0;
                        state = State.Starting;
                        return Value;
                    }
                    else { 
                        Value = Math2.PT1_Inverse(TimeConstant, time, Value);
                        return Value = Math.Round(Value, 1);
                    }

                case State.Starting:

                    Value = Math2.PT1(TimeConstant, time, UpperLimit);
                    Value = Math.Round(Value, 1);

                    if (Value >= UpperLimit - FloatingTempValue / 2)
                    {
                        state = State.Started;
                        return Value;
                    }

                    if (!enable)
                    {
                       time = 0;
                       state = State.Stop;
                        return Value;
                    }
                    return Value;

                case State.Started:

                    if (time > FloatingChangeTime)
                    {
                        time = 0;
                        Value = Math2.Float(UpperLimit, FloatingTempValue);
                        Value = Math.Round(Value, 1);
                    }

                    if (!enable)
                    {
                        time = 0;
                        state = State.Stop;
                    }
                    return Value;

                default:
                    return 0;
            }
        }
    }
}
