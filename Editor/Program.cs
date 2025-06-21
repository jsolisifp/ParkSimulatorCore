using System.Diagnostics;

namespace ParkSimulator
{

    internal class Program
    {
        enum MenuId
        {
            main,
            scene,
            selection,
            @object,
            simulation,
            views
        };

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

            Storage storage = new FileStorage();
            storage.Init("Resources\\");

            Simulation.Init(storage);

            MenuId menu = MenuId.main;
            int numOptions = 0;
            int option = -1;
            bool quit = false;

            bool viewObjectsEnabled = false;
            bool viewResourcesEnabled = false;


            while(!quit)
            {
                Console.Clear();
                Console.WriteLine(",------------------------,");
                Console.WriteLine("| Park simulator console |");
                Console.WriteLine("´------------------------´");

                if(viewObjectsEnabled) { ShowObjectsView(); }
                if(viewResourcesEnabled) { ShowResourcesView(); }

                if (menu == MenuId.main)
                {
                    Console.WriteLine("[Main]");
                    Console.WriteLine(" [1]Scene");
                    Console.WriteLine(" [2]Selection");
                    Console.WriteLine(" [3]Object");
                    Console.WriteLine(" [4]Simulation");
                    Console.WriteLine(" [5]Views");
                    Console.WriteLine(" [0]Exit");
                    numOptions = MenuMainOptionsCount + 1;
                }
                else if (menu == MenuId.scene)
                {
                    Console.WriteLine("[Main][Scene]");
                    Console.WriteLine(" [1]New scene");
                    Console.WriteLine(" [2]Load scene");
                    Console.WriteLine(" [3]Save scene");
                    Console.WriteLine(" [4]Save scene as");
                    Console.WriteLine(" [0]Back");
                    numOptions = MenuSceneOptionsCount + 1;
                }
                else if (menu == MenuId.selection)
                {
                    Console.WriteLine("[Main][Selection]");
                    Console.WriteLine(" [1]Select object");
                    Console.WriteLine(" [2]Clear selection");
                    Console.WriteLine(" [0]Back");
                    numOptions = MenuSelectionOptionsCount + 1;
                }
                else if (menu == MenuId.simulation)
                {
                    Console.WriteLine("[Main][Object]");
                    Console.WriteLine(" [1]Create object");
                    Console.WriteLine(" [2]Delete object");
                    Console.WriteLine(" [3]Add component");
                    Console.WriteLine(" [4]Remove component");
                    Console.WriteLine(" [0]Back");
                    numOptions = MenuObjectOptionsCount + 1;
                }
                else if (menu == MenuId.simulation)
                {
                    Console.WriteLine("[Main][Simulation]");
                    Console.WriteLine(" [1]Play");
                    Console.WriteLine(" [2]Step");
                    Console.WriteLine(" [3]Stop");
                    Console.WriteLine(" [0]Back");
                    numOptions = MenuSimulationOptionsCount + 1;
                }
                else if (menu == MenuId.views)
                {
                    Console.WriteLine("[Main][Views]");
                    Console.WriteLine(" [1]Objects");
                    Console.WriteLine(" [2]Resources");
                    Console.WriteLine(" [0]Back");
                    numOptions = MenuSimulationOptionsCount + 1;
                }

                Console.WriteLine("");
                option = AskInt("", 0, numOptions - 1);

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

                    if (option == MenuSceneOptionNew) { Simulation.NewScene(); menu = MenuId.main; }
                    else if (option == MenuSceneOptionSave)
                    {
                        if(savedSceneFileName == null) { saveAs = true; }
                        else { Simulation.SaveScene(savedSceneFileName); menu = MenuId.main; }
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
                        Simulation.SaveScene(fileName);
                        menu = MenuId.main;
                    }
                }
                else if (menu == MenuId.selection)
                {
                    if (option == MenuSelectionOptionSelect) { selectedObject = AskObject("Object"); menu = MenuId.main; }
                    else if (option == MenuSelectionOptionClear) { selectedObject = null; menu = MenuId.main; }
                    else if (option == MenuOptionBackOrQuit) { menu = MenuId.main; }
                }
                else if (menu == MenuId.@object)
                {
                    if (option == MenuObjectOptionCreate)
                    {
                        Debug.Assert(Simulation.Scene != null, "Scene not assigned");
                        SimulatedObject o = new();
                        Simulation.Scene.AddSimulatedObject(o);
                        selectedObject = o;

                        menu = MenuId.main;

                    }
                    else if (option == MenuObjectOptionDelete)
                    {
                        Debug.Assert(Simulation.Scene != null, "Scene not assigned");
                        if(selectedObject != null) { Simulation.Scene.RemoveSimulatedObject(selectedObject); }
                        menu = MenuId.main;
                    }
                    else if (option == MenuObjectOptionAddComponent)
                    {
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

            }

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

        static void ShowObjectsView()
        {
            Debug.Assert(Simulation.Scene != null, "Scene not assigned");

            var objects = Simulation.Scene.GetSimulatedObjects();

            for(int i = 0; i < objects.Count; i++)
            {
                Console.WriteLine(":" + objects[i].Name);
            }

        }

        static void ShowResourcesView()
        {
            Debug.Assert(Simulation.Storage != null, "Storage not assigned");

            var resources = Simulation.Storage.GetResourcesInfo();

            for(int i = 0; i < resources.Count; i++)
            {
                Console.WriteLine(":" + resources[i].id);
            }

        }
    }
}
