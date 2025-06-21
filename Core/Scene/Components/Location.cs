using System.Numerics;

namespace ParkSimulator
{ 
    public class Location : Component
    {
        public Vector3 Coordinates { get; set; }
        public int Capacity { get; set; }
        public Location? Neighbour { get; set; }
        public ResourcePointer Description { get; set; }

        public Location()
        {
            Coordinates = new Vector3(0, 0, 0);
            Neighbour = null;
        }

    }
}
