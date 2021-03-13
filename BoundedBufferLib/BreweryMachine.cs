using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BoundedBufferLib.Models;
using static BoundedBufferLib.Enums;

namespace BoundedBufferLib
{
    public class BreweryMachine
    {
        private readonly ConcurrentBag<Bottle> _bottlesToPrepare = new ConcurrentBag<Bottle>();
        private readonly BoundedBuffer<Bottle> _washedBottles = new BoundedBuffer<Bottle>();
        private readonly BoundedBuffer<Bottle> _filledBottles = new BoundedBuffer<Bottle>();
        private readonly BoundedBuffer<Bottle> _cappedBottles = new BoundedBuffer<Bottle>();
        private readonly BoundedBuffer<List<Bottle>> _boxesWithBottles = new BoundedBuffer<List<Bottle>>();
        private readonly SemaphoreSlim _boxPackerLimit = new SemaphoreSlim(2, 2);

        private readonly object _lock = new object();

        public BreweryMachine()
        {
            GenerateBottles(50);
        }
        public BreweryMachine(int amountOfBottles)
        {
            GenerateBottles(amountOfBottles);
        }

        public void Start(int maxDelay = 1000)
        {
            Console.WriteLine("Starting brewery simulation.");
            int unboxedBottles = _bottlesToPrepare.Count + _washedBottles.Count + _filledBottles.Count + _cappedBottles.Count;
            bool boxingNotDone = false;
            while (unboxedBottles > 24 || boxingNotDone)
            {
                // Take bottles from the line and insert into 3 washers
                #region Washer solution 1
                //BottleWasher washer = new BottleWasher(maxDelay);
                //BottleWasher washer2 = new BottleWasher(maxDelay);
                //BottleWasher washer3 = new BottleWasher(maxDelay);

                //var timer = new Stopwatch();

                //Task washers = Task.Run(() =>
                //{
                //    while (!_bottlesToPrepare.IsEmpty)
                //    {
                //        timer.Start();
                //        Task<bool> wash1 = Task.Run(() =>
                //        {
                //            _bottlesToPrepare.TryTake(out Bottle washedBottle);
                //            return _washedBottles.AddToQueue(washer.WashBottle(washedBottle));
                //        });
                //        Task<bool> wash2 = Task.Run(() =>
                //        {
                //            _bottlesToPrepare.TryTake(out Bottle washedBottle);
                //            return _washedBottles.AddToQueue(washer2.WashBottle(washedBottle));
                //        });
                //        Task<bool> wash3 = Task.Run(() =>
                //        {
                //            _bottlesToPrepare.TryTake(out Bottle washedBottle);
                //            return _washedBottles.AddToQueue(washer3.WashBottle(washedBottle));
                //        });
                //        Task.WaitAll(wash1, wash2, wash3);
                //        Console.WriteLine(_washedBottles.Count);
                //        timer.Stop();
                //        Console.WriteLine(timer.Elapsed);
                //        timer.Reset();
                //    }
                //});
                #endregion

                #region Washer solution 2
                BottleWasher washer = new BottleWasher(maxDelay);

                Task wash = Task.Run(() =>
                {
                    while (!_bottlesToPrepare.IsEmpty)
                    {
                        var timer = new Stopwatch();
                        timer.Start();

                        // Wash 3 bottles at a time
                        Parallel.For(0, 3, i =>
                            {
                                if (_bottlesToPrepare.Count <= 0) return;
                                _bottlesToPrepare.TryTake(out Bottle bottle);
                                _washedBottles.AddToQueue(washer.WashBottle(bottle));
                                Reporter.ReportAction($"Washed bottles: {_washedBottles.Count}");
                            });

                        timer.Stop();
                        Reporter.ReportTime(timer.Elapsed);
                        timer.Reset();
                    }
                });
                #endregion

                Task fillAndCap = Task.Run(() =>
                {
                    Parallel.For(0, 6, i =>
                    {
                        try
                        {
                            BottleCapper capper = new BottleCapper(maxDelay);

                            if (capper.IsReady) // Wait for a cap machine to be free
                            {
                                BottleFiller filler = new BottleFiller(maxDelay);

                                // Fill up to 6 bottles at a time (locked by Semaphore in BottleFiller.cs)
                                var fillBottle = Task.Run(() => FillBottle(filler));
                            }

                            // Caps up to 6 bottles at a time (locked by Semaphore in BottleCapper.cs)
                            var capBottle = Task.Run(() => CapBottle(capper));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            throw;
                        }
                    });
                });

                // packing
                Task packing = Task.Run(async () =>
                {
                    // Wait for 24 bottles
                    if (_cappedBottles.Count >= 24 && _boxPackerLimit.CurrentCount > 0)
                    {
                        Reporter.ReportAction($"Capped bottles: {_cappedBottles.Count}");
                        await _boxPackerLimit.WaitAsync();
                        BoxBottles(maxDelay);
                        _boxPackerLimit.Release();
                    }
                    else
                    {
                        Thread.Sleep(2000);
                    }
                });
               
                unboxedBottles = _bottlesToPrepare.Count + _washedBottles.Count + _filledBottles.Count +
                                 _cappedBottles.Count;
                boxingNotDone = _boxPackerLimit.CurrentCount < 2;
            }
            
            
            Reporter.ReportAction($"Bottles left when done: {unboxedBottles}");
            Console.WriteLine("Finished brewery simulation.");
        }

        private bool CapBottle(BottleCapper capper)
        {
            if (_filledBottles.Count <= 0) return false;

            Bottle filledBottle = _filledBottles.TakeFromQueue();
            return _cappedBottles.AddToQueue(capper.CapBottle(filledBottle));
        }

        private bool FillBottle(BottleFiller filler)
        {
            if (_washedBottles.Count <= 0) { return false; }

            Bottle washedBottle = _washedBottles.TakeFromQueue();
            return _filledBottles.AddToQueue(filler.FillBottle(washedBottle));
        }

        private bool BoxBottles(int maxDelay)
        {
            List<Bottle> bottlesToBox = new List<Bottle>();
            for (int i = 0; i < 24; i++)
            {
                lock (_lock)
                {
                    bottlesToBox.Add(_cappedBottles.TakeFromQueue());
                }
            }
            BottlePacker packer = new BottlePacker();
            bool success = packer.PutBottlesInBox(bottlesToBox, maxDelay);
            if (success) _boxesWithBottles.AddToQueue(bottlesToBox);
            Reporter.ReportAction($"Boxes ready: {_boxesWithBottles.Count}");
            return success;
        }

        private void GenerateBottles(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                _bottlesToPrepare.Add(new Bottle(GetUniqueId(), BottleStatus.Dirty));
            }
        }

        private int GetUniqueId()
        {
            return !_bottlesToPrepare.IsEmpty ? _bottlesToPrepare.Max(x => x.Id) + 1 : 1;
        }
    }
}
