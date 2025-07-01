using System.Diagnostics;
using System.Numerics;

namespace ParkSimulator
{
    public class SceneTemplates
    {        
        public static void PopulateNewScene()
        {
            Debug.Assert(Simulation.Scene != null, "Scene not assigned");

            //SimulatedObject attractionA;
            //SimulatedObject attractionB;
            //SimulatedObject attractionC;
            //SimulatedObject attractionD;
            //DefaultAttraction attractionComponentA;
            //DefaultAttraction attractionComponentB;
            //DefaultAttraction attractionComponentC;
            //DefaultAttraction attractionComponentD;
            //DefaultAttractionRenderer defaultRendererA;
            //DefaultAttractionRenderer defaultRendererB;
            //DefaultAttractionRenderer defaultRendererC;
            //DefaultAttractionRenderer defaultRendererD;
            //Location locationA;
            //Location locationB;
            //Location locationC;
            //Location locationD;

            //attractionA = new SimulatedObject();
            //attractionA.Name = "AttractionA";

            //attractionComponentA = new DefaultAttraction();
            //attractionComponentA.Type = DefaultAttractionType.rollercoaster;
            //attractionComponentA.Coordinates = new Vector3(-40, 0, -30);
            //attractionComponentA.Capacity = 10;
            //attractionComponentA.Occupation = 10;
            //attractionComponentA.Description = new ResourcePointer("attraction_a_description", "txt");

            //attractionA.AddComponent(attractionComponentA);

            //if(Simulation.Render.GetType() == typeof(DefaultRender))
            //{
            //    defaultRendererA = new DefaultAttractionRenderer();
            //    defaultRendererA.Size = 5.0f;
            //    defaultRendererA.Color = new Vector3(0.5f, 0, 0);

            //    attractionA.AddComponent(defaultRendererA);
            //}


            //locationA = new Location();
            //locationA.Position = attractionComponentA.Coordinates;
            //locationA.Rotation = Vector3.Zero;

            //attractionA.AddComponent(locationA);

            //Simulation.Scene.AddSimulatedObject(attractionA);

            //attractionB = new SimulatedObject();
            //attractionB.Name = "AttractionB";

            //attractionComponentB = new DefaultAttraction();
            //attractionComponentB.Type = DefaultAttractionType.bumperCars;
            //attractionComponentB.Coordinates = new Vector3(80, 0, -80);
            //attractionComponentB.Capacity = 5;
            //attractionComponentB.Occupation = 0;
            //attractionComponentB.Description = new ResourcePointer("attraction_b_description", "txt");

            //attractionB.AddComponent(attractionComponentB);

            //if(Simulation.Render.GetType() == typeof(DefaultRender))
            //{
            //    defaultRendererB = new DefaultAttractionRenderer();
            //    defaultRendererB.Size = 5.0f;
            //    defaultRendererB.Color = new Vector3(0, 0.5f, 0);

            //    attractionB.AddComponent(defaultRendererB);
            //}

            //locationB = new Location();
            //locationB.Position = attractionComponentB.Coordinates;
            //locationB.Rotation = Vector3.Zero;

            //attractionB.AddComponent(locationB);


            //Simulation.Scene.AddSimulatedObject(attractionB);

            //attractionC = new SimulatedObject();
            //attractionC.Name = "AttractionC";

            //attractionComponentC = new DefaultAttraction();
            //attractionComponentC.Type = DefaultAttractionType.carousel;
            //attractionComponentC.Coordinates = new Vector3(20, 0, 40);
            //attractionComponentC.Capacity = 3;
            //attractionComponentC.Occupation = 0;
            //attractionComponentC.Description = new ResourcePointer("attraction_c_description", "txt");

            //attractionC.AddComponent(attractionComponentC);

            //if(Simulation.Render.GetType() == typeof(DefaultRender))
            //{
            //    defaultRendererC = new DefaultAttractionRenderer();
            //    defaultRendererC.Size = 5.0f;
            //    defaultRendererC.Color = new Vector3(0, 0, 0.5f);

            //    attractionC.AddComponent(defaultRendererC);
            //}

            //locationC = new Location();
            //locationC.Position = attractionComponentB.Coordinates;
            //locationC.Rotation = Vector3.Zero;

            //attractionC.AddComponent(locationC);

            //Simulation.Scene.AddSimulatedObject(attractionC);


            //attractionD = new SimulatedObject();
            //attractionD.Name = "AttractionD";

            //attractionComponentD = new DefaultAttraction();
            //attractionComponentD.Type = DefaultAttractionType.ferrisWheel;
            //attractionComponentD.Coordinates = new Vector3(-80, 0, 80);
            //attractionComponentD.Capacity = 5;
            //attractionComponentD.Occupation = 0;
            //attractionComponentD.Description = new ResourcePointer("attraction_d_description", "txt");

            //attractionD.AddComponent(attractionComponentD);

            //if(Simulation.Render.GetType() == typeof(DefaultRender))
            //{
            //    defaultRendererD = new DefaultAttractionRenderer();
            //    defaultRendererD.Size = 5.0f;
            //    defaultRendererD.Color = new Vector3(0.5f, 0.5f, 0);

            //    attractionD.AddComponent(defaultRendererD);
            //}

            //locationD = new Location();
            //locationD.Position = attractionComponentD.Coordinates;
            //locationD.Rotation = Vector3.Zero;

            //attractionC.AddComponent(locationD);

            //Simulation.Scene.AddSimulatedObject(attractionD);

            //attractionComponentA.Neighbour = attractionComponentB;
            //attractionComponentB.Neighbour = attractionComponentC;
            //attractionComponentC.Neighbour = attractionComponentD;
            //attractionComponentD.Neighbour = attractionComponentA;

            //attractionComponentA.Neighbours = new DefaultAttraction[] { attractionComponentB, attractionComponentC };
            //attractionComponentB.Neighbours = new DefaultAttraction[] { attractionComponentA, attractionComponentC };
            //attractionComponentC.Neighbours = new DefaultAttraction[] { attractionComponentB, attractionComponentA };
            //attractionComponentD.Neighbours = new DefaultAttraction[] { attractionComponentA, attractionComponentB };

        }
    }
}
