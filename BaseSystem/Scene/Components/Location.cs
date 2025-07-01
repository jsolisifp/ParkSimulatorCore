using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ParkSimulator
{
    public class Location : Component
    {
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
    }
}
