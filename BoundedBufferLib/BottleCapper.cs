using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BoundedBufferLib.Models;

namespace BoundedBufferLib
{
    public class BottleCapper
    {
        private readonly BoundedBuffer<Bottle> _capperQueue = new BoundedBuffer<Bottle>();
        private readonly SemaphoreSlim _capperLimit = new SemaphoreSlim(6);

        private readonly int _maxDelay;

        public bool IsReady => _capperLimit.CurrentCount > 0;

        public BottleCapper()
        {
            _maxDelay = 2000;
        }

        public BottleCapper(int maxDelay)
        {
            _maxDelay = maxDelay;
        }

        public Bottle CapBottle(Bottle bottle)
        {
            _capperLimit.Wait(TimeSpan.FromSeconds(5));
            _capperQueue.AddToQueue(bottle);
            Random random = new Random();

            Reporter.ReportAction(bottle, "being capped");
            
            Thread.Sleep(random.Next(0, _maxDelay));
            bottle.Status = Enums.BottleStatus.Capped;
            Reporter.ReportAction(bottle, "is capped");

            _capperQueue.RemoveFromQueue();
            _capperLimit.Release();
            return bottle;
        }
    }
}
