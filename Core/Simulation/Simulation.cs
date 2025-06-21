using System.Diagnostics;
using static System.Formats.Asn1.AsnWriter;

namespace ParkSimulator
{
    public enum SimulationState
    {
        uninitialized,
        stopped,
        playing
    };

    public class Simulation
    {
        public static SimulationState  State { get { return state; } }
        public static SimulatedScene? Scene { get { return scene; } }
        public static Storage? Storage { get { return storage; } }

        static SimulationState state = SimulationState.uninitialized;
        static SimulatedScene? scene;
        static string? sceneResourceId;
        static Storage? storage;
        static Config? config;

        public static void Init(Config _config, Storage _storage)
        {
            Debug.Assert(state == SimulationState.uninitialized, "Simulation is already initialized");

            storage = _storage;
            config = _config;
            state = SimulationState.stopped;

            scene = new SimulatedScene();
            sceneResourceId = null;

            scene.Load();
        }

        public static void NewScene()
        {
            Debug.Assert(state == SimulationState.stopped, "Simulation is not stopped");
            Debug.Assert(storage != null, "You must create a storage");

            if(scene != null) { scene.Unload(); }
            if(sceneResourceId != null) { storage.RemoveReference(sceneResourceId, Storage.typeIdScene); sceneResourceId = null; }

            scene = new SimulatedScene();
            sceneResourceId = null;

            scene.Load();
        }

        public static void LoadScene(string resourceId)
        {
            Debug.Assert(state == SimulationState.stopped, "Simulation is not stopped");
            Debug.Assert(scene != null, "You must create a new scene or load one from storage");
            Debug.Assert(storage != null, "You must create a storage");

            if(scene != null) { scene.Unload(); }
            if(sceneResourceId != null) { storage.RemoveReference(sceneResourceId, Storage.typeIdScene); sceneResourceId = null; }

            sceneResourceId = resourceId;
            storage.AddReference(resourceId, Storage.typeIdScene);
            scene = storage.GetLoadedResource<SimulatedScene>(resourceId);

            Debug.Assert(scene != null, "Cannot load scene");

            scene.Load();
        }

        public static void SaveScene(ref string resourceId)
        {
            Debug.Assert(state == SimulationState.stopped, "Simulation is not stopped");
            Debug.Assert(scene != null, "You must create a new scene or load one from storage");
            Debug.Assert(storage != null, "You must create a storage");

            storage.SaveResource(ref resourceId, Storage.typeIdScene, scene);
        }
        

        public static void Play()
        {
            Debug.Assert(scene != null, "You must create a new scene or load one from storage before playing");
            Debug.Assert(state == SimulationState.stopped, "Simulation is not stopped");
            Debug.Assert(scene != null, "You must create a new scene or load one from storage");

            scene.Start();

            state = SimulationState.playing;

        }

        public static void Step()
        {
            Debug.Assert(state == SimulationState.playing, "Simulation is not playing");
            Debug.Assert(scene != null, "You must create a new scene or load one from storage");

            scene.Step();

        }

        public static void Stop()
        {
            Debug.Assert(state == SimulationState.playing, "Simulation is not playing");
            Debug.Assert(scene != null, "You must create a new scene or load one from storage");

            scene.Stop();

            state = SimulationState.stopped;
        }

        public static void Finish()
        {
            Debug.Assert(state == SimulationState.stopped, "Stop the simulation first");
            Debug.Assert(scene != null, "You must create a new scene or load one from storage");
            Debug.Assert(storage != null, "You must create a storage");

            scene.Unload();
            storage.Finish();
            storage = null;

        }



    }
}
