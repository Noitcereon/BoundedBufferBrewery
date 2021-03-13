using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BoundedBufferLib.Models;

namespace BoundedBufferLib
{
    public class BottleFiller
    {
        private readonly BoundedBuffer<Bottle> _fillerQueue = new BoundedBuffer<Bottle>();
        private readonly SemaphoreSlim _fillerLimit = new SemaphoreSlim(6);

        private readonly int _maxDelay;

        public BottleFiller()
        {
            _maxDelay = 2000;
        }

        public BottleFiller(int maxDelay)
        {
            _maxDelay = maxDelay;
        }

        public Bottle FillBottle(Bottle bottle)
        {
            _fillerLimit.Wait(TimeSpan.FromSeconds(5));

            Random random = new Random();

            Reporter.ReportAction(bottle, "being filled");
            Thread.Sleep(random.Next(0, _maxDelay));
            bottle.Status = Enums.BottleStatus.Filled;
            Reporter.ReportAction(bottle, "is filled");

            _fillerQueue.RemoveFromQueue();

            _fillerLimit.Release();
            return bottle;
        }
    }
}
