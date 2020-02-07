using System;
using System.Collections.Generic;
using System.Text;

namespace Plant.OpcUa.Helper
{
    public class Math2
    {
        public static double PT1(double T, double t, double K)
        {
            return K * (1 - Math.Exp(-t / T));
        }

        public static double PT1_Inverse(double T, double t, double K)
        {
            return K - K * (1 - Math.Exp(-t / T));
        }

        public static double Float(double value, double floatrange)
        {
            var rand = new Random();
            double val = rand.NextDouble();

            if(val >= 0.5)
            {
                return value + val * floatrange;
            }
            else
            {
                return value - val * floatrange;
            }
        }
    }
}
