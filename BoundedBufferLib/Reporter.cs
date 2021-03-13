using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BoundedBufferLib.Models;
using static BoundedBufferLib.Enums;

namespace BoundedBufferLib
{
    public static class Reporter
    {
        private static readonly ReportMode Mode = ReportMode.Verbose;

        private static readonly object Locker = new object();

        public static void ReportAction(Bottle bottle, string message)
        {
            if (Mode == ReportMode.Silent) return;
            lock (Locker)
            {
                Console.WriteLine($"Bottle ID {bottle.Id}: {message}");
            }
        }
        public static void ReportAction(Bottle bottle, string message, ReportMode mode)
        {
            if (mode == ReportMode.Silent) return;
            lock (Locker)
            {
                Console.WriteLine($"Bottle ID {bottle.Id}: {message}");
            }
        }

        public static void ReportAction(string messsage)
        {
            if(Mode == ReportMode.Silent) return;
            lock (Locker)
            {
                Console.WriteLine(messsage);
            }
        }

        public static void ReportTime(TimeSpan timeElapsed)
        {
            if (Mode != ReportMode.Everything) return;
            lock (Locker)
            {
                Console.WriteLine(timeElapsed);
            }
        }
    }
}
