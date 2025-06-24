using System.Diagnostics;
using System.Globalization;
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
            resource,
            simulation,
            views
        };

        // Constants for menu options

        const int MenuOptionBackOrQuit = 0;

        const int MenuMainOptionScene= 1;
        const int MenuMainOptionSelection = 2;
        const int MenuMainOptionObject = 3;
        const int MenuMainOptionResource = 4;
        const int MenuMainOptionSimulation = 5;
        const int MenuMainOptionRender = 6;
        const int MenuMainOptionViews= 7;

        const int MenuMainOptionsCount = 8;

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
        const int MenuObjectOptionSetFieldValue = 5;

        const int MenuObjectOptionsCount = 5;

        const int MenuResourceOptionDelete = 1;

        const int MenuResourceOptionsCount = 1;

        const int MenuSimulationOptionPlay = 1;
        const int MenuSimulationOptionStep = 2;
        const int MenuSimulationOptionStop = 3;
        const int MenuSimulationBatchPlay = 4;

        const int MenuSimulationOptionsCount = 4;

        const int MenuViewOptionObjects = 1;
        const int MenuViewOptionResources = 2;

        const int MenuViewOptionsCount = 2;


        static void Main(string[] args)
        { 
            // Initialize simulator

            Config config = new MemoryConfig();

            Storage storage = new FileStorage();
            Render rendering = new PictureRender();
            Log log = new FileLog();

            Simulation.Init(config, storage, rendering, log);

            // Create initial scene

            SceneTemplates.PopulateNewScene();

            // Selection

            SimulatedObject? selectedObject = null;

            // Save

            string? savedSceneFileName = null;

            // Views

            bool viewObjectsEnabled = true;
            bool viewResourcesEnabled = true;

            // Menu

            MenuId menu = MenuId.main;
            int option = -1;
            bool quit = false;

            // Error

            bool error = false;
            string errorMessage = "";

            // Main loop

            while(!quit)
            {

                Console.Clear();

                ShowHeader();

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
                    else if (option == MenuMainOptionResource) { menu = MenuId.resource; }
                    else if (option == MenuMainOptionSimulation) { menu = MenuId.simulation; }
                    else if (option == MenuMainOptionRender) { Simulation.RenderFrame(); }
                    else if (option == MenuMainOptionViews) { menu = MenuId.views; }
                    else if (option == MenuOptionBackOrQuit) { quit = true; }

                }
                else if(menu == MenuId.scene)
                {
                    bool saveAs = false;

                    if (option == MenuSceneOptionNew)
                    {   Simulation.NewScene();
                        SceneTemplates.PopulateNewScene();
                        selectedObject = null;
                        menu = MenuId.main;
                    }
                    else if (option == MenuSceneOptionSave)
                    {   if(savedSceneFileName == null) { saveAs = true; }
                        else { Simulation.SaveScene(ref savedSceneFileName); menu = MenuId.main; }
                        menu = MenuId.main;
                    }
                    else if (option == MenuSceneOptionLoad)
                    {   selectedObject = null;
                        string fileName = AskText("File name (without extension)");
                        Simulation.LoadScene(fileName);
                        menu = MenuId.main;
                    }
                    else if (option == MenuSceneOptionSaveAs) { saveAs = true; }
                    else if (option == MenuOptionBackOrQuit) { menu = MenuId.main; }

                    if(saveAs)
                    {   string fileName = AskText("File name (without extension)");
                        Simulation.SaveScene(ref fileName);
                        savedSceneFileName = fileName;
                        menu = MenuId.main;
                    }
                }
                else if (menu == MenuId.selection)
                {
                    if (option == MenuSelectionOptionSelect)
                    {   selectedObject = AskObject("Object");
                        menu = MenuId.main;
                    }
                    else if (option == MenuSelectionOptionClear)
                    {   selectedObject = null;
                        menu = MenuId.main;
                    }
                    else if (option == MenuOptionBackOrQuit) { menu = MenuId.main; }
                }
                else if (menu == MenuId.@object)
                {
                    if (option == MenuObjectOptionCreate)
                    {   Debug.Assert(Simulation.Scene != null, "Scene not assigned");
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
                            {   Simulation.Scene.RemoveSimulatedObject(target);
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
                        if(targetObject != null && component != null) { targetObject.AddComponent(component); }

                        menu = MenuId.main;
                    }
                    else if (option == MenuObjectOptionRemoveComponent)
                    {
                        SimulatedObject? targetObject = AskObject("Object");

                        if(targetObject != null)
                        {    Component? targetComponent = AskObjectComponent(targetObject, "Component");
                            if(targetComponent != null) { targetObject.RemoveComponent(targetComponent); }
                        }

                        menu = MenuId.main;
                    }
                    else if (option == MenuObjectOptionSetFieldValue)
                    {
                        SimulatedObject? targetObject = AskObject("Object");

                        if(targetObject != null)
                        {
                            Component? targetComponent = AskObjectComponent(targetObject, "Component");
                            if(targetComponent != null)
                            {
                                ComponentFieldInfo? field = AskComponentField(targetComponent, "Field");

                                if(field.HasValue)
                                {   object? value = AskFieldValue(field.Value, "Value");
                                    targetComponent.SetFieldValue<object>(field.Value.name, value);
                                }
                            }
                        }

                        menu = MenuId.main;
                    }
                    else if (option == MenuOptionBackOrQuit) { menu = MenuId.main; }
                }
                else if (menu == MenuId.resource)
                {
                    if (option == MenuResourceOptionDelete)
                    {
                        Debug.Assert(Simulation.Storage != null, "Storage not assigned");

                        ResourcePointer? p = AskResource("Resource");
                        if(p.HasValue) { Simulation.Storage.DeleteResource(p.Value.ResourceId, p.Value.TypeId); }
                        menu = MenuId.main;

                    }
                    else if (option == MenuOptionBackOrQuit) { menu = MenuId.main; }
                }
                else if (menu == MenuId.simulation)
                {
                    if (option == MenuSimulationOptionPlay) { Simulation.Play(); Simulation.RenderFrame(); menu = MenuId.main; }
                    else if (option == MenuSimulationOptionStep) { Simulation.Step(); Simulation.RenderFrame(); }
                    else if(option == MenuSimulationOptionStop) { Simulation.Stop(); menu = MenuId.main; }
                    else if(option == MenuSimulationBatchPlay)
                    {
                        int steps = AskInt("Steps");
                        Simulation.Play();
                        Simulation.RenderFrame();
                        for(int i = 0; i < steps; i++)
                        {   Simulation.Step();
                            Simulation.RenderFrame();
                        }
                        Simulation.Stop();
                        menu = MenuId.main;
                    }
                    else if (option == MenuOptionBackOrQuit) { menu = MenuId.main; }
                }
                else if(menu == MenuId.views)
                {   if (option == MenuViewOptionObjects) { viewObjectsEnabled = !viewObjectsEnabled; menu = MenuId.main; }
                    else if (option == MenuViewOptionResources) { viewResourcesEnabled = !viewResourcesEnabled; menu = MenuId.main; }
                    else if (option == MenuOptionBackOrQuit) { menu = MenuId.main; }

                }

                if(error) { ShowError(errorMessage); }

                error = false;

            }

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
                { Console.WriteLine("[" + (i + 1) + "]:" + components[i].name); }

                Console.Write(message + "?>");
                string? line = Console.ReadLine();

                if (Int32.TryParse(line, out index))
                {
                    if (index >= 1 && index <= components.Count)
                    {   done = true;
                        r = components[index - 1].name;
                    }
                    else { Console.WriteLine("Please enter a numeric index between " + 1 + " and " + components.Count + " (both included)"); }
                }
                else { Console.WriteLine("Please enter a numeric index");  }
            }

            return r;
        }

        static int AskInt(string message = "", int minValue = Int32.MinValue, int maxValue = Int32.MaxValue, int defaultValue = -1)
        {
            int r = defaultValue;

            bool done = false;

            while(!done)
            {
                Console.Write(message + "?>");
                string? line = Console.ReadLine();

                if (Int32.TryParse(line, out r))
                {   if (r >= minValue && r <= maxValue) { done = true; }
                    else { Console.WriteLine("Please enter a numeric value between " + minValue + " and " + maxValue + " (both included)"); }
                }
                else { Console.WriteLine("Please enter a numeric value");  }
            }

            return r;
        }

        static float AskSingle(string message = "", float minValue = Single.MinValue, float maxValue = Single.MaxValue, float defaultValue = -1)
        {
            float r = defaultValue;

            bool done = false;

            while(!done)
            {
                Console.Write(message + "?>");
                string? line = Console.ReadLine();

                if (Single.TryParse(line, CultureInfo.InvariantCulture, out r))
                {   if (r >= minValue && r <= maxValue) { done = true; }
                    else { Console.WriteLine("Please enter a numeric value between " + minValue + " and " + maxValue + " (both included)"); }
                }
                else { Console.WriteLine("Please enter a numeric value");  }
            }

            return r;
        }

        static bool AskBool(string message, bool defaultValue = false)
        {
            bool r = defaultValue;
            bool done = false;

            while(!done)
            {
                Console.Write(message + "?[true|false]>");
                string? line = Console.ReadLine();

                if(line.Trim().ToLower() == "true")
                {   r = true;
                    done = true;
                }
                else if(line.Trim().ToLower() == "false")
                {   r = false;
                    done = true;
                }
                else { Console.WriteLine("Please enter true or false"); }
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
                {   line = line.Trim();
                    if(line.Length >= minLength)
                    {   r = line;
                        done = true;
                    }
                    else { Console.WriteLine("Please enter a text with a minimum length of " + minLength + " characters");  }
                }
                else
                {   r = "";
                    done = true;
                }
            }

            return r;
        }

        static ComponentFieldInfo? AskComponentField(Component component, string message)
        {
            Debug.Assert(Simulation.Scene != null, "Scene not assigned");

            ComponentFieldInfo? r = null;
            int index;
            bool done = false;
            while(!done)
            {
                var fields = component.GetFieldsInfo();

                for(int i = 0; i < fields.Count; i++)
                { Console.WriteLine("[" + (i + 1) + "]:" + fields[i].name + " : " + FormatFieldValue(component, fields[i])); }

                Console.Write(message + "?>");
                string? line = Console.ReadLine();

                if (Int32.TryParse(line, out index))
                {   if (index >= 1 && index <= fields.Count) { done = true; r = fields[index - 1]; }
                    else { Console.WriteLine("Please enter a numeric index between " + 1 + " and " + fields.Count + " (both included)"); }
                }
                else { Console.WriteLine("Please enter a numeric index");  }
            }

            return r;

        }

        static object? AskFieldValue(ComponentFieldInfo field, string message)
        {
            object? r = null;

            if(field.type == "Vector3")
            {   float x = AskSingle(message + " X");
                float y = AskSingle(message + " Y");
                float z = AskSingle(message + " Z");

                r = (object?)(new Vector3(x, y, z));
            }
            else if(field.type == "Boolean") { r = (object?)AskBool(message); }
            else if(field.type == "Int32") { r = (object?)AskInt(message); }
            else if(field.type == "Single") { r = (object?)AskSingle(message); }
            else if(field.isResourcePointer) { r = (object?)AskResource(message); }
            else if(field.isComponent)
            {
                SimulatedObject? simObject = AskObject(message + " referenced object", true);
                if(simObject != null)
                { r = AskObjectComponent(simObject, message + " referenced component"); }
            }

            return r;

        }


        static Component? AskObjectComponent(SimulatedObject simObject, string message)
        {
            Debug.Assert(Simulation.Scene != null, "Scene not assigned");

            Component? r = null;
            int index;
            bool done = false;
            while(!done)
            {
                var components = simObject.GetComponents();

                for(int i = 0; i < components.Count; i++)
                {
                    Console.WriteLine("[" + (i + 1) + "]:" + components[i].GetType().Name);
                }

                Console.Write(message + "?>");
                string? line = Console.ReadLine();

                if (Int32.TryParse(line, out index))
                {
                    if (index >= 1 && index <= components.Count) { done = true; r = components[index - 1]; }
                    else { Console.WriteLine("Please enter a numeric index between " + 1 + " and " + components.Count + " (both included)"); }
                }
                else { Console.WriteLine("Please enter a numeric index");  }
            }

            return r;

        }

        static SimulatedObject? AskObject(string message, bool askNull = false)
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

                if(askNull) { Console.WriteLine("[" + (objects.Count + 1) + "]: null"); }

                Console.Write(message + "?>");
                string? line = Console.ReadLine();

                if (Int32.TryParse(line, out index))
                {
                    if (index >= 1 && index <= objects.Count + (askNull ? 1 : 0))
                    {
                        done = true;
                        if(index <= objects.Count) { r = objects[index - 1]; }
                        else { r = null; }
                    }
                    else { Console.WriteLine("Please enter a numeric index between " + 1 + " and " + objects.Count + " (both included)"); }
                }
                else { Console.WriteLine("Please enter a numeric index");  }
            }

            return r;

        }

        static ResourcePointer? AskResource(string message)
        {
            Debug.Assert(Simulation.Scene != null, "Scene not assigned");
            Debug.Assert(Simulation.Storage != null, "Storage not assigned");

            ResourcePointer? r = null;
            bool done = false;
            int index = -1;

            while(!done)
            {
                var pointers = Simulation.Storage.GetResourcePointers();

                for(int i = 0; i < pointers.Count; i++)
                {
                    Console.WriteLine("[" + (i + 1) + "]:" + pointers[i].ResourceId);
                }

                Console.Write(message + "?>");
                string? line = Console.ReadLine();

                if (Int32.TryParse(line, out index))
                {
                    if (index >= 1 && index <= pointers.Count) { done = true; r = pointers[index - 1]; }
                    else { Console.WriteLine("Please enter a numeric index between " + 1 + " and " + pointers.Count + " (both included)"); }
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
            else if (menu == MenuId.resource) { numOptions = MenuResourceOptionsCount + 1; }
            else if (menu == MenuId.simulation) { numOptions = MenuSimulationOptionsCount + 1; }
            else // menu == MenuId.views
            { numOptions = MenuViewOptionsCount + 1; }

            Console.WriteLine("");
            result = AskInt("", 0, numOptions - 1);

            return result;
        }

        static void ShowHeader()
        {
            Console.WriteLine(",*******************************************,");
            Console.WriteLine("|                                           |");
            Console.WriteLine("|          Park simulator console           |");
            Console.WriteLine("|                                           |");
            Console.WriteLine("´*******************************************´");
        }

        static void ShowSimulationState()
        {
            Console.WriteLine(",--------------------,");
            Console.WriteLine("|     Simulation     |");
            Console.WriteLine("`-------------------`");

            Console.WriteLine(" State : " + Simulation.State);
        }

        static void ShowProperties(SimulatedObject? simuObject)
        {
            Console.WriteLine(",--------------------,");
            Console.WriteLine("|     Properties      |");
            Console.WriteLine("`-------------------`");

            if(simuObject == null) { Console.WriteLine(" None"); }
            else
            {
                Console.WriteLine(" Name : " + simuObject.Name);
                Console.WriteLine(" Active : " + simuObject.Active);

                var components = simuObject.GetComponents();
                for(int i = 0; i < components.Count; i++)
                {
                    Component c = components[i];
                    Console.WriteLine(" === " + c.GetType().Name + " ===");
                    var fieldsInfo = c.GetFieldsInfo();
                    
                    for(int j = 0; j < fieldsInfo.Count; j++)
                    {   ComponentFieldInfo f = fieldsInfo[j];
                        string value = FormatFieldValue(c, f);
                        Console.WriteLine("  " + f.name + " : " + f.type + " : " + value + (!f.isWritable ? " : [READONLY]" : ""));
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
                Console.WriteLine(" [Main]");
                Console.WriteLine("  [1]Scene");
                Console.WriteLine("  [2]Selection");
                Console.WriteLine("  [3]Object");
                Console.WriteLine("  [4]Resource");
                Console.WriteLine("  [5]Simulation");
                Console.WriteLine("  [6]Render");
                Console.WriteLine("  [7]Views");
                Console.WriteLine("  [0]Exit");
            }
            else if (menu == MenuId.scene)
            {
                Console.WriteLine(" [Main][Scene]");
                Console.WriteLine("  [1]New scene");
                Console.WriteLine("  [2]Load scene");
                Console.WriteLine("  [3]Save scene");
                Console.WriteLine("  [4]Save scene as");
                Console.WriteLine("  [0]Back");
            }
            else if (menu == MenuId.selection)
            {
                Console.WriteLine(" [Main][Selection]");
                Console.WriteLine("  [1]Select object");
                Console.WriteLine("  [2]Clear selection");
                Console.WriteLine("  [0]Back");
            }
            else if (menu == MenuId.@object)
            {
                Console.WriteLine(" [Main][Object]");
                Console.WriteLine("  [1]Create object");
                Console.WriteLine("  [2]Delete object");
                Console.WriteLine("  [3]Add component");
                Console.WriteLine("  [4]Remove component");
                Console.WriteLine("  [5]Set field value");
                Console.WriteLine("  [0]Back");
            }
            else if (menu == MenuId.resource)
            {
                Console.WriteLine(" [Main][Resource]");
                Console.WriteLine("  [1]Delete resource");
                Console.WriteLine("  [0]Back");
            }
            else if (menu == MenuId.simulation)
            {
                Console.WriteLine(" [Main][Simulation]");
                Console.WriteLine("  [1]Play");
                Console.WriteLine("  [2]Step");
                Console.WriteLine("  [3]Stop");
                Console.WriteLine("  [4]Batch play");
                Console.WriteLine("  [0]Back");
            }
            else if (menu == MenuId.views)
            {
                Console.WriteLine(" [Main][Views]");
                Console.WriteLine("  [1]Objects");
                Console.WriteLine("  [2]Resources");
                Console.WriteLine("  [0]Back");
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
                Console.WriteLine(" : " + objects[i].Name + (objects[i].Active ? "[v]" : "[x]"));
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
            {   string? id = resources[i].ResourceId;
                string? type = resources[i].TypeId;

                Debug.Assert(id != null, "Resource pointer id is null");
                Debug.Assert(type != null, "Resource pointer type is null");

                int referenceCount = Simulation.Storage.GetReferenceCount(id);

                Console.WriteLine(" : " + id + " : " + type + (referenceCount > 0 ? " : [LOADED] : refCount " + referenceCount : ""));
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

        static string FormatFieldValue(Component c, ComponentFieldInfo f)
        {
            string value = "*Cannot display*";

            if(f.type == "Vector3")
            {   Vector3? v = c.GetFieldValue<Vector3>(f.name);
                if(v.HasValue) { value = "[" + v.Value.X + ", " + v.Value.Y + ", " + v.Value.Z + "]"; }
            }
            else if(f.type == "Boolean")
            {   bool? v = c.GetFieldValue<bool>(f.name);
                if(v.HasValue) { value = v.Value.ToString(); }
            }
            else if(f.type == "Int32")
            {   int? v = c.GetFieldValue<int>(f.name);
                if(v.HasValue) { value = v.Value.ToString(); }
            }
            else if(f.type == "Single")
            {   float? v = c.GetFieldValue<float>(f.name);
                if(v.HasValue) { value = v.Value.ToString(); }
            }
            else if(f.isResourcePointer)
            {   ResourcePointer v = c.GetFieldValue<ResourcePointer>(f.name);
                if(v.ResourceId != null) { value = "[" + v.ResourceId + "]"; }
                else { value = "[none]"; }
            }
            else if(f.isComponent)
            {
                Component? v = c.GetFieldValue<Component>(f.name);
                if(v != null)
                {   SimulatedObject? so = v.GetSimulatedObject();
                    Debug.Assert(so != null, "Component not attached to simulated object");
                    value = "[" + so.Name + "]";
                }
                else { value = "[none]"; }
            }

            return value;

        }
    }
}
