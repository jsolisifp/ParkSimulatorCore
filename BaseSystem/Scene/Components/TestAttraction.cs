using System.Diagnostics;
using System.Numerics;

namespace ParkSimulator
{
    public enum TestAttractionType
    {
        rollercoaster,
        bumperCars,
        carousel,
        ferrisWheel

    };

    public class TestAttraction : Component
    {
        public TestAttractionType Type { get; set; }
        public Vector3 Coordinates { get; set; }
        public int Capacity { get; set; } = 10;
        public int Occupation { get; set; } = 0;
        public TestAttraction? Neighbour { get; set; }
        public ResourcePointer Description { get; set; }

        TestAttraction? neighbourLocation;

        public TestAttraction()
        {
            Coordinates = new Vector3(0, 0, 0);
            Neighbour = null;
        }

        public override void Start()
        {
            neighbourLocation = Neighbour?.GetSimulatedObject()?.GetComponent<TestAttraction>();
        }

        public override void Step()
        {
            Debug.Assert(Simulation.Random != null, "Simulacion no iniciada");

            if(neighbourLocation != null)
            {
                if(Simulation.Random.Next() % 2 == 0 && Occupation > 0 && neighbourLocation.Occupation < neighbourLocation.Capacity)
                {
                    Simulation.Log?.LogMessage(GetSimulatedObject()?.Name + "", "Moving visitor to " + neighbourLocation?.GetSimulatedObject()?.Name);

                    Occupation --;

                    Debug.Assert(neighbourLocation != null);

                    neighbourLocation.Occupation ++;
                }
            }
        }

    }
}
