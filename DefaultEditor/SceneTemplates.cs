using System.Diagnostics;
using System.Numerics;

namespace ParkSimulator
{
    public class SceneTemplates
    {        
        public static void PopulateNewScene()
        {
            Debug.Assert(Simulation.Scene != null, "Scene not assigned");

            SimulatedObject rollerCoasterA;
            SimulatedObject rollerCoasterB;
            SimulatedObject rollerCoasterC;
            SimulatedObject rollerCoasterD;
            Location locationComponentA;
            Location locationComponentB;
            Location locationComponentC;
            Location locationComponentD;
            PictureRenderer rendererComponentA;
            PictureRenderer rendererComponentB;
            PictureRenderer rendererComponentC;
            PictureRenderer rendererComponentD;

            rollerCoasterA = new SimulatedObject();
            rollerCoasterA.Name = "RollercoasterA";

            locationComponentA = new Location();
            locationComponentA.Coordinates = new Vector3(-40, 0, -30);
            locationComponentA.Capacity = 10;
            locationComponentA.Occupation = 10;
            locationComponentA.Description = new ResourcePointer("rollercoaster_a_description", "txt");

            rollerCoasterA.AddComponent(locationComponentA);

            rendererComponentA = new PictureRenderer();
            rendererComponentA.Size = 5.0f;
            rendererComponentA.Color = new Vector3(0.5f, 0, 0);

            rollerCoasterA.AddComponent(rendererComponentA);

            Simulation.Scene.AddSimulatedObject(rollerCoasterA);

            rollerCoasterB = new SimulatedObject();
            rollerCoasterB.Name = "RollercoasterB";

            locationComponentB = new Location();
            locationComponentB.Coordinates = new Vector3(80, 0, -80);
            locationComponentB.Capacity = 5;
            locationComponentB.Occupation = 0;
            locationComponentB.Description = new ResourcePointer("rollercoaster_b_description", "txt");

            rollerCoasterB.AddComponent(locationComponentB);

            rendererComponentB = new PictureRenderer();
            rendererComponentB.Size = 5.0f;
            rendererComponentB.Color = new Vector3(0, 0.5f, 0);

            rollerCoasterB.AddComponent(rendererComponentB);

            Simulation.Scene.AddSimulatedObject(rollerCoasterB);

            rollerCoasterC = new SimulatedObject();
            rollerCoasterC.Name = "RollercoasterC";

            locationComponentC = new Location();
            locationComponentC.Coordinates = new Vector3(20, 0, 40);
            locationComponentC.Capacity = 3;
            locationComponentC.Occupation = 0;
            locationComponentC.Description = new ResourcePointer("rollercoaster_c_description", "txt");

            rollerCoasterC.AddComponent(locationComponentC);

            rendererComponentC = new PictureRenderer();
            rendererComponentC.Size = 5.0f;
            rendererComponentC.Color = new Vector3(0, 0, 0.5f);

            rollerCoasterC.AddComponent(rendererComponentC);

            Simulation.Scene.AddSimulatedObject(rollerCoasterC);


            rollerCoasterD = new SimulatedObject();
            rollerCoasterD.Name = "RollercoasterD";

            locationComponentD = new Location();
            locationComponentD.Coordinates = new Vector3(-80, 0, 80);
            locationComponentD.Capacity = 5;
            locationComponentD.Occupation = 0;
            locationComponentD.Description = new ResourcePointer("rollercoaster_d_description", "txt");

            rollerCoasterD.AddComponent(locationComponentD);

            rendererComponentD = new PictureRenderer();
            rendererComponentD.Size = 5.0f;
            rendererComponentD.Color = new Vector3(0.5f, 0.5f, 0);

            rollerCoasterD.AddComponent(rendererComponentD);

            Simulation.Scene.AddSimulatedObject(rollerCoasterD);

            locationComponentA.Neighbour = locationComponentB;
            locationComponentB.Neighbour = locationComponentC;
            locationComponentC.Neighbour = locationComponentD;
            locationComponentD.Neighbour = locationComponentA;


        }
    }
}
