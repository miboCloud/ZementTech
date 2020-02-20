using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Plant.OpcUa.Helper
{
    public class DelayQueue
    {
        private List<DelayItem> items = new List<DelayItem>();

        public DelayQueue()
        {

        }

        public int Clock { get; set; }

        public int Delay { get; set; }

        public void Tick(int relativeClock)
        {
            Clock += relativeClock;
        }

        public void Reset()
        {
            Clock = 0;
        }



        public void Push(double kg)
        {
            items.Add(new DelayItem(Clock, Delay, kg));
        }

        public IEnumerable Retrieve()
        {
            List<DelayItem> deleteItems = new List<DelayItem>();
            foreach(DelayItem i in items)
            {
                if (i.Expired(Clock))
                {
                    deleteItems.Add(i);
                    yield return i;
                }  
            }

            deleteItems.ForEach(d => items.Remove(d));
        }

    }

    public class DelayItem
    {
        
        public DelayItem(int currentClock, int delayMs, double kg)
        {
            DelayMs = delayMs;
            Kg = kg;
            CurrentClock = currentClock;
        }

        public double Kg { get; set; }

        public int DelayMs { get; private set; }

        public int CurrentClock { get; private set; }

        public bool Expired(int clock)
        {
            return clock >= (CurrentClock + DelayMs);
        }
 
    }
}
