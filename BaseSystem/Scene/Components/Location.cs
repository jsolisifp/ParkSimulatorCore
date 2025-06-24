using System.Diagnostics;
using System.Numerics;

namespace ParkSimulator
{
    public class Location : Component
    {
        public Vector3 Coordinates { get; set; }
        public int Capacity { get; set; } = 10;
        public int Occupation { get; set; } = 0;
        public Location? Neighbour { get; set; }
        public ResourcePointer Description { get; set; }

        Location? neighbourLocation;

        public Location()
        {
            Coordinates = new Vector3(0, 0, 0);
            Neighbour = null;
        }

        public override void Start()
        {
            neighbourLocation = Neighbour?.GetSimulatedObject()?.GetComponent<Location>();
        }

        public override void Step()
        {
            Debug.Assert(Simulation.Random != null, "Simulacion no iniciada");

            if(neighbourLocation != null)
            {
                if(Simulation.Random.Next() % 2 == 0 && Occupation > 0 && neighbourLocation.Occupation < neighbourLocation.Capacity)
                {
                    Occupation --;
                    neighbourLocation.Occupation ++;
                }
            }
        }

    }
}
