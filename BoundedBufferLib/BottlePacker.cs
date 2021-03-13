using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BoundedBufferLib.Models;

namespace BoundedBufferLib
{
    public class BottlePacker
    {
        private readonly object _lock = new object();

        public bool PutBottlesInBox(List<Bottle> bottles, int maxDelay)
        {
            if (maxDelay > 1500) maxDelay = 1000;
            Random random = new Random();
            lock (_lock)
            {
                foreach (var bottle in bottles)
                {
                    Thread.Sleep(random.Next(0, maxDelay));
                    Reporter.ReportAction(bottle, "is being boxed.", Enums.ReportMode.Verbose);
                }
            }

            return true;
        }
    }
}
