using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BoundedBufferLib.Models;
using static BoundedBufferLib.Enums;

namespace BoundedBufferLib
{
    public class BottleWasher
    {
        private readonly BoundedBuffer<Bottle> _washer = new BoundedBuffer<Bottle>();
        private readonly int _maxDelay;

        public BottleWasher()
        {
            _maxDelay = 2000;
        }

        public BottleWasher(int maxDelay)
        {
            _maxDelay = maxDelay;
        }

        public Bottle WashBottle(Bottle bottle)
        {
            _washer.AddToQueue(bottle);
            Random random = new Random();

            Reporter.ReportAction(bottle, "washing");
            Thread.Sleep(random.Next(0, _maxDelay));
            bottle.Status = BottleStatus.Washed;
            Reporter.ReportAction(bottle, "has been washed");
            
            _washer.RemoveFromQueue();
            return bottle;
        }
    }
}
