using System.Diagnostics;
using System.Numerics;

namespace ParkSimulator
{
    public enum DefaultAttractionType
    {
        rollercoaster,
        bumperCars,
        carousel,
        ferrisWheel

    };

    public enum DefaultAttractionMaintenanceType
    {
        always,
        ifNeeded,
        never
    };

    public class DefaultAttraction : Component
    {
        public DefaultAttractionType Type { get; set; }
        public Vector3 Coordinates { get; set; }
        public int Capacity { get; set; } = 10;
        public int Occupation { get; set; } = 0;
        public DefaultAttraction? Neighbour { get; set; }
        public ResourcePointer Description { get; set; }
        public int[] Valorations { get; set; } = new int[0];
        public DefaultAttractionMaintenanceType[] MaintenanceSchedule { get; set; } = new DefaultAttractionMaintenanceType[0];
        public DefaultAttraction[] Neighbours { get; set; } = new DefaultAttraction[0];

        public ResourcePointer[] Descriptions { get; set; } = new ResourcePointer[0];

        DefaultAttraction? neighbourLocation;

        public DefaultAttraction()
        {
            Coordinates = new Vector3(0, 0, 0);
            Neighbour = null;
            Valorations = new int[] { 5, 4, 3, 2, 3, 4, 5, 6 };
            MaintenanceSchedule = new DefaultAttractionMaintenanceType[] { DefaultAttractionMaintenanceType.always,
                                                                        DefaultAttractionMaintenanceType.ifNeeded,
                                                                        DefaultAttractionMaintenanceType.always,
                                                                        DefaultAttractionMaintenanceType.never,
                                                                        DefaultAttractionMaintenanceType.ifNeeded,
                                                                      };

            Descriptions = new ResourcePointer[] { new ResourcePointer("attraction_a_description", "txt"),
                                                   new ResourcePointer("attraction_b_description", "txt"),
                                                   new ResourcePointer("attraction_c_description", "txt"),
                                                   new ResourcePointer("attraction_d_description", "txt") };
        }

        public override void Start()
        {
            neighbourLocation = Neighbour?.GetSimulatedObject()?.GetComponent<DefaultAttraction>();
        }

        public override void Step(float deltaTime)
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
