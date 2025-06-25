using System.Diagnostics;
using System.Numerics;

namespace ParkSimulator
{
    public class SceneTemplates
    {        
        public static void PopulateNewScene()
        {
            Debug.Assert(Simulation.Scene != null, "Scene not assigned");

            SimulatedObject attractionA;
            SimulatedObject attractionB;
            SimulatedObject attractionC;
            SimulatedObject attractionD;
            TestAttraction attractionComponentA;
            TestAttraction attractionComponentB;
            TestAttraction attractionComponentC;
            TestAttraction attractionComponentD;
            TestRenderer rendererComponentA;
            TestRenderer rendererComponentB;
            TestRenderer rendererComponentC;
            TestRenderer rendererComponentD;

            attractionA = new SimulatedObject();
            attractionA.Name = "AttractionA";

            attractionComponentA = new TestAttraction();
            attractionComponentA.Type = TestAttractionType.rollercoaster;
            attractionComponentA.Coordinates = new Vector3(-40, 0, -30);
            attractionComponentA.Capacity = 10;
            attractionComponentA.Occupation = 10;
            attractionComponentA.Description = new ResourcePointer("attraction_a_description", "txt");

            attractionA.AddComponent(attractionComponentA);

            rendererComponentA = new TestRenderer();
            rendererComponentA.Size = 5.0f;
            rendererComponentA.Color = new Vector3(0.5f, 0, 0);

            attractionA.AddComponent(rendererComponentA);

            Simulation.Scene.AddSimulatedObject(attractionA);

            attractionB = new SimulatedObject();
            attractionB.Name = "AttractionB";

            attractionComponentB = new TestAttraction();
            attractionComponentB.Type = TestAttractionType.bumperCars;
            attractionComponentB.Coordinates = new Vector3(80, 0, -80);
            attractionComponentB.Capacity = 5;
            attractionComponentB.Occupation = 0;
            attractionComponentB.Description = new ResourcePointer("attraction_b_description", "txt");

            attractionB.AddComponent(attractionComponentB);

            rendererComponentB = new TestRenderer();
            rendererComponentB.Size = 5.0f;
            rendererComponentB.Color = new Vector3(0, 0.5f, 0);

            attractionB.AddComponent(rendererComponentB);

            Simulation.Scene.AddSimulatedObject(attractionB);

            attractionC = new SimulatedObject();
            attractionC.Name = "AttractionC";

            attractionComponentC = new TestAttraction();
            attractionComponentC.Type = TestAttractionType.carousel;
            attractionComponentC.Coordinates = new Vector3(20, 0, 40);
            attractionComponentC.Capacity = 3;
            attractionComponentC.Occupation = 0;
            attractionComponentC.Description = new ResourcePointer("attraction_c_description", "txt");

            attractionC.AddComponent(attractionComponentC);

            rendererComponentC = new TestRenderer();
            rendererComponentC.Size = 5.0f;
            rendererComponentC.Color = new Vector3(0, 0, 0.5f);

            attractionC.AddComponent(rendererComponentC);

            Simulation.Scene.AddSimulatedObject(attractionC);


            attractionD = new SimulatedObject();
            attractionD.Name = "AttractionD";

            attractionComponentD = new TestAttraction();
            attractionComponentD.Type = TestAttractionType.ferrisWheel;
            attractionComponentD.Coordinates = new Vector3(-80, 0, 80);
            attractionComponentD.Capacity = 5;
            attractionComponentD.Occupation = 0;
            attractionComponentD.Description = new ResourcePointer("attraction_d_description", "txt");

            attractionD.AddComponent(attractionComponentD);

            rendererComponentD = new TestRenderer();
            rendererComponentD.Size = 5.0f;
            rendererComponentD.Color = new Vector3(0.5f, 0.5f, 0);

            attractionD.AddComponent(rendererComponentD);

            Simulation.Scene.AddSimulatedObject(attractionD);

            attractionComponentA.Neighbour = attractionComponentB;
            attractionComponentB.Neighbour = attractionComponentC;
            attractionComponentC.Neighbour = attractionComponentD;
            attractionComponentD.Neighbour = attractionComponentA;


        }
    }
}
