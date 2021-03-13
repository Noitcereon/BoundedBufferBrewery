using System;
using System.Collections.Generic;
using System.Text;
using static BoundedBufferLib.Enums;

namespace BoundedBufferLib.Models
{
    public class Bottle
    {
        public int Id { get; init; }
        public BottleStatus Status { get; set; }

        public Bottle()
        {
            
        }
        public Bottle(int id, BottleStatus status)
        {
            Id = id;
            Status = status;
        }

        public override string ToString()
        {
            return $"Bottle ID: {Id} Status: {Status}";
        }
    }
}
