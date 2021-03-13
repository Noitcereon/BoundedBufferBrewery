using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BoundedBufferLib;

namespace BrewerySimulation
{
    public class BreweryWorker
    {
        public void Start()
        {
          

            BreweryMachine machine = new BreweryMachine();
            machine.Start();
        }
    }
}
