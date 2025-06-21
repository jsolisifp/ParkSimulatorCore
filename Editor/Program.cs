using System.Diagnostics;
using System.Numerics;

namespace ParkSimulator
{

    internal class Program
    {
        // Menus

        enum MenuId
        {
            main,
            scene,
            selection,
            @object,
            simulation,
            views
        };

        // Constants for menu options

        const int MenuOptionBackOrQuit = 0;

        const int MenuMainOptionScene= 1;
        const int MenuMainOptionSelection = 2;
        const int MenuMainOptionObject = 3;
        const int MenuMainOptionSimulation = 4;
        const int MenuMainOptionViews= 5;

        const int MenuMainOptionsCount = 5;

        const int MenuSceneOptionNew = 1;
        const int MenuSceneOptionLoad = 2;
        const int MenuSceneOptionSave = 3;
        const int MenuSceneOptionSaveAs = 4;

        const int MenuSceneOptionsCount = 4;

        const int MenuSelectionOptionSelect = 1;
        const int MenuSelectionOptionClear = 2;

        const int MenuSelectionOptionsCount = 2;

        const int MenuObjectOptionCreate = 1;
        const int MenuObjectOptionDelete = 2;
        const int MenuObjectOptionAddComponent = 3;
        const int MenuObjectOptionRemoveComponent = 4;

        const int MenuObjectOptionsCount = 4;

        const int MenuSimulationOptionPlay = 1;
        const int MenuSimulationOptionStep = 2;
        const int MenuSimulationOptionStop = 3;

        const int MenuSimulationOptionsCount = 4;

        const int MenuViewOptionObjects = 1;
        const int MenuViewOptionResources = 2;

        const int MenuViewOptionsCount = 2;


        static void Main(string[] args)
        { 

            SimulatedObject? selectedObject = null;
            string? savedSceneFileName = null;

            Config config = new MemoryConfig();
            config.SetTextValue("basePath", "Resources\\");

            Storage storage = new FileStorage();
            storage.Init(config);

            Simulation.Init(config, storage);

            CreateNewSceneDefaultObjects();

            MenuId menu = MenuId.main;
            int option = -1;
            bool quit = false;

            bool viewObjectsEnabled = true;
            bool viewResourcesEnabled = true;

            bool error = false;
            string errorMessage = "";

            while(!quit)
            {

                Console.Clear();

                Console.WriteLine(",*******************************************,");
                Console.WriteLine("|          Park simulator console           |");
                Console.WriteLine("´*******************************************´");

                if(viewObjectsEnabled) { ShowObjectsView(); }
                if(viewResourcesEnabled) { ShowResourcesView(); }

                ShowProperties(selectedObject);

                ShowSimulationState();

                ShowMenu(menu);
                option = AskMenuOption(menu);

                if(menu == MenuId.main)
                {
                    if (option == MenuMainOptionScene) { menu = MenuId.scene; }
                    else if (option == MenuMainOptionSelection) { menu = MenuId.selection; }
                    else if (option == MenuMainOptionObject) { menu = MenuId.@object; }
                    else if (option == MenuMainOptionSimulation) { menu = MenuId.simulation; }
                    else if (option == MenuMainOptionViews) { menu = MenuId.views; }
                    else if (option == MenuOptionBackOrQuit) { quit = true; }

                }
                else if(menu == MenuId.scene)
                {
                    bool saveAs = false;

                    if (option == MenuSceneOptionNew)
                    {
                        Simulation.NewScene();
                        CreateNewSceneDefaultObjects();
                        selectedObject = null;
                        menu = MenuId.main;
                    }
                    else if (option == MenuSceneOptionSave)
                    {
                        if(savedSceneFileName == null) { saveAs = true; }
                        else { Simulation.SaveScene(ref savedSceneFileName, "scene"); menu = MenuId.main; }
                        menu = MenuId.main;
                    }
                    else if (option == MenuSceneOptionLoad)
                    {
                        selectedObject = null;
                        string fileName = AskText("File name");
                        Simulation.LoadScene(fileName, "scene");
                        menu = MenuId.main;
                    }
                    else if (option == MenuSceneOptionSaveAs)
                    {
                        saveAs = true;
                    }
                    else if (option == MenuOptionBackOrQuit) { menu = MenuId.main; }

                    if(saveAs)
                    {
                        string fileName = AskText("File name");
                        Simulation.SaveScene(ref fileName, "scene");
                        menu = MenuId.main;
                    }
                }
                else if (menu == MenuId.selection)
                {
                    if (option == MenuSelectionOptionSelect)
                    {
                        selectedObject = AskObject("Object");
                        menu = MenuId.main;
                    }
                    else if (option == MenuSelectionOptionClear)
                    {
                        selectedObject = null;
                        menu = MenuId.main;
                    }
                    else if (option == MenuOptionBackOrQuit) { menu = MenuId.main; }
                }
                else if (menu == MenuId.@object)
                {
                    if (option == MenuObjectOptionCreate)
                    {
                        Debug.Assert(Simulation.Scene != null, "Scene not assigned");
                        SimulatedObject o = new();
                        o.Name = AskText("Name");
                        Simulation.Scene.AddSimulatedObject(o);

                        menu = MenuId.main;

                    }
                    else if (option == MenuObjectOptionDelete)
                    {
                        Debug.Assert(Simulation.Scene != null, "Scene not assigned");
                        if(Simulation.Scene.GetSimulatedObjects().Count > 0)
                        { 
                            SimulatedObject? target = AskObject("Object");
                            if(target != null)
                            {
                                Simulation.Scene.RemoveSimulatedObject(target);
                                if(selectedObject == target) { selectedObject = null; }
                            }
                            
                        }
                        menu = MenuId.main;
                    }
                    else if (option == MenuObjectOptionAddComponent)
                    {
                        string name = AskComponentName("Component");
                        Component? component = SimulatedObject.CreateComponentByName(name);

                        SimulatedObject? targetObject = AskObject("Object");

                        if(component != null && targetObject != null) { targetObject.AddComponent(component); }

                        menu = MenuId.main;
                    }
                    else if (option == MenuObjectOptionRemoveComponent) { menu = MenuId.main; }
                    else if (option == MenuOptionBackOrQuit) { menu = MenuId.main; }
                }
                else if (menu == MenuId.simulation)
                {
                    if (option == MenuSimulationOptionPlay) { Simulation.Play(); menu = MenuId.main; }
                    else if (option == MenuSimulationOptionStep) { Simulation.Step(); }
                    else if(option == MenuSimulationOptionStop) { Simulation.Stop(); menu = MenuId.main; }
                    else if (option == MenuOptionBackOrQuit) { menu = MenuId.main; }
                }
                else if(menu == MenuId.views)
                {
                    if (option == MenuViewOptionObjects) { viewObjectsEnabled = !viewObjectsEnabled; menu = MenuId.main; }
                    else if (option == MenuViewOptionResources) { viewResourcesEnabled = !viewResourcesEnabled; menu = MenuId.main; }
                    else if (option == MenuOptionBackOrQuit) { menu = MenuId.main; }

                }

                if(error)
                {
                    ShowError(errorMessage);
                }

                error = false;

            }

        }

        public static void CreateNewSceneDefaultObjects()
        {
            Debug.Assert(Simulation.Scene != null, "Scene not assigned");

            SimulatedObject rollerCoasterA;
            SimulatedObject rollerCoasterB;
            Location locationComponentA;
            Location locationComponentB;

            rollerCoasterA = new SimulatedObject();
            rollerCoasterA.Name = "RollercoasterA";

            locationComponentA = new Location();
            locationComponentA.Coordinates = new Vector3(-10, -10, 0);
            locationComponentA.Description = new ResourcePointer("rollercoaster_a_description.txt", "txt");

            rollerCoasterA.AddComponent(locationComponentA);

            Simulation.Scene.AddSimulatedObject(rollerCoasterA);

            rollerCoasterB = new SimulatedObject();
            rollerCoasterB.Name = "RollercoasterB";

            locationComponentB = new Location();
            locationComponentB.Coordinates = new Vector3(10, -10, 0);
            locationComponentB.Description = new ResourcePointer("rollercoaster_b_description.txt", "txt");

            rollerCoasterB.AddComponent(locationComponentB);

            Simulation.Scene.AddSimulatedObject(rollerCoasterB);

            locationComponentA.Neighbour = locationComponentB;
            locationComponentB.Neighbour = locationComponentA;
            
            
        }

        static string AskComponentName(string message)
        {
            Debug.Assert(Simulation.Scene != null, "Scene not assigned");

            string r = "";
            bool done = false;
            int index = -1;

            while(!done)
            {
                var components = SimulatedObject.GetComponentsInfo();

                for(int i = 0; i < components.Count; i++)
                {
                    Console.WriteLine("[" + (i + 1) + "]:" + components[i].name);
                }

                Console.Write(message + "?>");
                string? line = Console.ReadLine();

                if (Int32.TryParse(line, out index))
                {
                    if (index >= 1 && index <= components.Count) { done = true; r = components[index - 1].name; }
                    else { Console.WriteLine("Please enter a numeric index between " + 1 + " and " + components.Count + " (both included)"); }
                }
                else { Console.WriteLine("Please enter a numeric index");  }
            }

            return r;
        }

        static int AskInt(string message = "", int minValue = 0, int maxValue = Int32.MaxValue, int defaultValue = -1)
        {
            int r = defaultValue;

            bool done = false;

            while(!done)
            {
                Console.Write(message + "?>");
                string? line = Console.ReadLine();

                if (Int32.TryParse(line, out r))
                {
                    if (r >= minValue && r <= maxValue) { done = true; }
                    else { Console.WriteLine("Please enter a numeric value between " + minValue + " and " + maxValue + " (both included)"); }
                }
                else { Console.WriteLine("Please enter a numeric value");  }
            }

            return r;
        }

        static string AskText(string message = "", int minLength = 0, string defaultValue = "")
        {
            string r = defaultValue;
            bool done = false;
            while(!done)
            {
                Console.Write(message + "?>");
                string? line = Console.ReadLine();

                if(!string.IsNullOrEmpty(line))
                {
                    line = line.Trim();
                    if(line.Length >= minLength)
                    {   r = line;
                        done = true;
                    }
                    else { Console.WriteLine("Please enter a text with a minimum length of " + minLength + " characters");  }
                }
                else
                {
                    r = "";
                    done = true;
                }
            }

            return r;
        }

        static SimulatedObject? AskObject(string message)
        {
            Debug.Assert(Simulation.Scene != null, "Scene not assigned");

            SimulatedObject? r = null;
            bool done = false;
            int index = -1;

            while(!done)
            {
                var objects = Simulation.Scene.GetSimulatedObjects();

                for(int i = 0; i < objects.Count; i++)
                {
                    Console.WriteLine("[" + (i + 1) + "]:" + objects[i].Name);
                }

                Console.Write(message + "?>");
                string? line = Console.ReadLine();

                if (Int32.TryParse(line, out index))
                {
                    if (index >= 1 && index <= objects.Count) { done = true; r = objects[index - 1]; }
                    else { Console.WriteLine("Please enter a numeric index between " + 1 + " and " + objects.Count + " (both included)"); }
                }
                else { Console.WriteLine("Please enter a numeric index");  }
            }

            return r;

        }

        static int AskMenuOption(MenuId menu)
        {
            int numOptions;
            int result;
            if (menu == MenuId.main) { numOptions = MenuMainOptionsCount + 1; }
            else if (menu == MenuId.scene) { numOptions = MenuSceneOptionsCount + 1; }
            else if (menu == MenuId.selection) { numOptions = MenuSelectionOptionsCount + 1; }
            else if (menu == MenuId.@object) { numOptions = MenuObjectOptionsCount + 1; }
            else if (menu == MenuId.simulation) { numOptions = MenuSimulationOptionsCount + 1; }
            else // menu == MenuId.views
            { numOptions = MenuViewOptionsCount + 1; }

            Console.WriteLine("");
            result = AskInt("", 0, numOptions - 1);

            return result;
        }

        static void ShowSimulationState()
        {
            Console.WriteLine(",--------------------,");
            Console.WriteLine("|     Simulation     |");
            Console.WriteLine("`-------------------`");

                Console.WriteLine("State : " + Simulation.State);
        }

        static void ShowProperties(SimulatedObject? simuObject)
        {
            Console.WriteLine(",--------------------,");
            Console.WriteLine("|     Properties      |");
            Console.WriteLine("`-------------------`");

            if(simuObject == null) { Console.WriteLine("None"); }
            else
            {
                Console.WriteLine("Name : " + simuObject.Name);
                Console.WriteLine("Active : " + simuObject.Active);

                var components = simuObject.GetComponents();
                for(int i = 0; i < components.Count; i++)
                {
                    Component c = components[i];

                    Console.WriteLine("=== " + c.GetType().Name + " ===");

                    var fieldsInfo = c.GetFieldsInfo();
                    
                    for(int j = 0; j < fieldsInfo.Count; j++)
                    {
                        ComponentFieldInfo f = fieldsInfo[j];
                        string value = "*Cannot display value*";

                        if(f.type == "Vector3")
                        {
                            Vector3? v = c.GetFieldValue<Vector3>(f.name);
                            if(v.HasValue) { value = "[" + v.Value.X + ", " + v.Value.Y + ", " + v.Value.Z + "]"; }
                        }
                        else if(f.type == "Boolean")
                        {
                            bool? v = c.GetFieldValue<bool>(f.name);
                            if(v.HasValue) { value = v.Value.ToString(); }
                        }
                        else if(f.type == "Int32")
                        {
                            int? v = c.GetFieldValue<int>(f.name);
                            if(v.HasValue) { value = v.Value.ToString(); }
                        }
                        else if(f.type == "Single")
                        {
                            float? v = c.GetFieldValue<float>(f.name);
                            if(v.HasValue) { value = v.Value.ToString(); }
                        }
                        else if(f.isResourcePointer)
                        {
                            ResourcePointer v = c.GetFieldValue<ResourcePointer>(f.name);
                            if(v.resourceId != null) { value = "[" + v.resourceId + "]"; }
                            else { value = "[none]"; }
                        }
                        else if(f.isComponent)
                        {
                            Component? v = c.GetFieldValue<Component>(f.name);
                            if(v != null)
                            {
                                SimulatedObject? so = v.GetAttachedSimulatedObject();
                                Debug.Assert(so != null, "Component not attached to simulated object");
                                value = "[" + so.Name + "]";
                            }
                            else { value = "[none]"; }
                        }

                        Console.WriteLine(" " + f.name + " : " + f.type + " : " + value + (!f.isWritable ? " : [READONLY]" : ""));
                    }
                }
            }
        }

        static void ShowMenu(MenuId menu)
        {
            Console.WriteLine(",-------------------,");
            Console.WriteLine("|       Menu        |");
            Console.WriteLine("`-------------------`");

            if (menu == MenuId.main)
            {
                Console.WriteLine("[Main]");
                Console.WriteLine(" [1]Scene");
                Console.WriteLine(" [2]Selection");
                Console.WriteLine(" [3]Object");
                Console.WriteLine(" [4]Simulation");
                Console.WriteLine(" [5]Views");
                Console.WriteLine(" [0]Exit");
            }
            else if (menu == MenuId.scene)
            {
                Console.WriteLine("[Main][Scene]");
                Console.WriteLine(" [1]New scene");
                Console.WriteLine(" [2]Load scene");
                Console.WriteLine(" [3]Save scene");
                Console.WriteLine(" [4]Save scene as");
                Console.WriteLine(" [0]Back");
            }
            else if (menu == MenuId.selection)
            {
                Console.WriteLine("[Main][Selection]");
                Console.WriteLine(" [1]Select object");
                Console.WriteLine(" [2]Clear selection");
                Console.WriteLine(" [0]Back");
            }
            else if (menu == MenuId.@object)
            {
                Console.WriteLine("[Main][Object]");
                Console.WriteLine(" [1]Create object");
                Console.WriteLine(" [2]Delete object");
                Console.WriteLine(" [3]Add component");
                Console.WriteLine(" [4]Remove component");
                Console.WriteLine(" [0]Back");
            }
            else if (menu == MenuId.simulation)
            {
                Console.WriteLine("[Main][Simulation]");
                Console.WriteLine(" [1]Play");
                Console.WriteLine(" [2]Step");
                Console.WriteLine(" [3]Stop");
                Console.WriteLine(" [0]Back");
            }
            else if (menu == MenuId.views)
            {
                Console.WriteLine("[Main][Views]");
                Console.WriteLine(" [1]Objects");
                Console.WriteLine(" [2]Resources");
                Console.WriteLine(" [0]Back");
            }

        }

        static void ShowObjectsView()
        {
            Debug.Assert(Simulation.Scene != null, "Scene not assigned");

            Console.WriteLine(",-------------------,");
            Console.WriteLine("| Simulated Objects |");
            Console.WriteLine("`-------------------`");

            var objects = Simulation.Scene.GetSimulatedObjects();

            for(int i = 0; i < objects.Count; i++)
            {
                Console.WriteLine(":" + objects[i].Name + (objects[i].Active ? "[v]" : "[x]"));
            }

        }

        static void ShowResourcesView()
        {
            Debug.Assert(Simulation.Storage != null, "Storage not assigned");

            Console.WriteLine(",-------------------,");
            Console.WriteLine("|      Resources    |");
            Console.WriteLine("`-------------------`");

            var resources = Simulation.Storage.GetResourcePointers();

            for(int i = 0; i < resources.Count; i++)
            {
                string? id = resources[i].resourceId;

                Debug.Assert(id != null, "Resource pointer id is null");

                bool isLoaded = Simulation.Storage.IsResourceLoaded(id);
                Console.WriteLine(":" + id + (isLoaded ? " : [LOADED]" : ""));
            }

        }

        static void ShowError(string error)
        {
            Console.Clear();
            Console.WriteLine(",-------------------,");
            Console.WriteLine("|      Error!       |");
            Console.WriteLine("`-------------------`");
            Console.WriteLine(" " + error);
            Thread.Sleep(3000);
                
        }
    }
}
