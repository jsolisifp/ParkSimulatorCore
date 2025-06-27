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

    public enum TestAttractionMaintenanceType
    {
        always,
        ifNeeded,
        never
    };

    public class TestAttraction : Component
    {
        public TestAttractionType Type { get; set; }
        public Vector3 Coordinates { get; set; }
        public int Capacity { get; set; } = 10;
        public int Occupation { get; set; } = 0;
        public TestAttraction? Neighbour { get; set; }
        public ResourcePointer Description { get; set; }
        public int[] Valorations { get; set; } = new int[0];
        public TestAttractionMaintenanceType[] MaintenanceSchedule { get; set; } = new TestAttractionMaintenanceType[0];
        public TestAttraction[] Neighbours { get; set; } = new TestAttraction[0];

        public ResourcePointer[] Descriptions { get; set; } = new ResourcePointer[0];

        TestAttraction? neighbourLocation;

        public TestAttraction()
        {
            Coordinates = new Vector3(0, 0, 0);
            Neighbour = null;
            Valorations = new int[] { 5, 4, 3, 2, 3, 4, 5, 6 };
            MaintenanceSchedule = new TestAttractionMaintenanceType[] { TestAttractionMaintenanceType.always,
                                                                        TestAttractionMaintenanceType.ifNeeded,
                                                                        TestAttractionMaintenanceType.always,
                                                                        TestAttractionMaintenanceType.never,
                                                                        TestAttractionMaintenanceType.ifNeeded,
                                                                      };

            Descriptions = new ResourcePointer[] { new ResourcePointer("attraction_a_description", "txt"),
                                                   new ResourcePointer("attraction_b_description", "txt"),
                                                   new ResourcePointer("attraction_c_description", "txt"),
                                                   new ResourcePointer("attraction_d_description", "txt") };
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
