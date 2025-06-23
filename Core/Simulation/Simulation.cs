using System;
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
        public static Render? Render { get { return render; } }
        public static Random Random { get { return random; } } 


        static SimulationState state = SimulationState.uninitialized;
        static SimulatedScene? scene;
        static string? sceneResourceId;
        static Storage? storage;
        static Render? render;
        static Config? config;
        static Random? random;

        static string? playingTemporarySceneResourceId;
        static SimulatedScene? playingPreviousScene;

        public static void Init(Config _config, Storage? _storage = null, Render? _rendering = null)
        {
            Debug.Assert(state == SimulationState.uninitialized, "Simulation is already initialized");

            storage = _storage;
            config = _config;
            render = _rendering;
            state = SimulationState.stopped;

            storage?.Init(config);
            render?.Init(config);

            scene = new SimulatedScene();
            sceneResourceId = null;
            scene.Link();

            playingTemporarySceneResourceId = null;
        }

        public static void NewScene()
        {
            Debug.Assert(state == SimulationState.stopped, "Simulation is not stopped");
            Debug.Assert(storage != null, "You must create a storage");

            if(scene != null) { scene.Unlink(); }
            if(sceneResourceId != null) { storage.RemoveReference(sceneResourceId, Storage.typeIdScene); sceneResourceId = null; }

            scene = new SimulatedScene();
            sceneResourceId = null;

            scene.Link();
        }

        public static void LoadScene(string resourceId)
        {
            Debug.Assert(state == SimulationState.stopped, "Simulation is not stopped");
            Debug.Assert(scene != null, "You must create a new scene or load one from storage");
            Debug.Assert(storage != null, "You must create a storage");

            if(scene != null) { scene.Unlink(); }
            if(sceneResourceId != null) { storage.RemoveReference(sceneResourceId, Storage.typeIdScene); sceneResourceId = null; }

            sceneResourceId = resourceId;
            storage.AddReference(resourceId, Storage.typeIdScene);
            scene = storage.GetLoadedResource<SimulatedScene>(resourceId);

            Debug.Assert(scene != null, "Cannot load scene resource");

            scene.Link();
        }

        public static void SaveScene(ref string resourceId)
        {
            Debug.Assert(state == SimulationState.stopped, "Simulation is not stopped");
            Debug.Assert(scene != null, "You must create a new scene or load one from storage");
            Debug.Assert(storage != null, "You must create a storage");

            scene.Unlink();

            if(sceneResourceId != null) { storage.RemoveReference(sceneResourceId, Storage.typeIdScene); sceneResourceId = null; }

            storage.SaveResource(ref resourceId, Storage.typeIdScene, scene);

            storage.AddReference(resourceId, Storage.typeIdScene);
            scene = storage.GetLoadedResource<SimulatedScene>(resourceId);
            sceneResourceId = resourceId;

            Debug.Assert(scene != null, "Cannot load scene resource");

            scene.Link();

        }

        //public static bool IsSceneSaved()
        //{
        //    return sceneResourceId != null;
        //}
        

        public static void Play()
        {
            Debug.Assert(scene != null, "You must create a new scene or load one from storage before playing");
            Debug.Assert(state == SimulationState.stopped, "Simulation is not stopped");
            Debug.Assert(scene != null, "You must create a new scene or load one from storage");
            //Debug.Assert(sceneResourceId != null, "Scene must be saved before ");

            scene.Unlink();

            playingPreviousScene = scene;
            playingTemporarySceneResourceId = Guid.NewGuid().ToString();
            storage.SaveResource(ref playingTemporarySceneResourceId, Storage.typeIdScene, scene);

            storage.AddReference(playingTemporarySceneResourceId, Storage.typeIdScene);
            scene = storage.GetLoadedResource<SimulatedScene>(playingTemporarySceneResourceId);
            scene.Link();

            random = new Random(scene.Seed);

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
            Debug.Assert(playingTemporarySceneResourceId != null, "Temporary playing scene not saved");

            scene.Stop();

            scene.Unlink();

            storage.RemoveReference(playingTemporarySceneResourceId, Storage.typeIdScene);

            scene = playingPreviousScene;

            scene.Link();

            state = SimulationState.stopped;
        }

        public static void Finish()
        {
            Debug.Assert(state == SimulationState.stopped, "Stop the simulation first");
            Debug.Assert(scene != null, "You must create a new scene or load one from storage");
            Debug.Assert(storage != null, "You must create a storage");

            scene.Unlink();
            scene = null;

            storage?.Finish();
            storage = null;

            render?.Finish();
            render = null;

            state = SimulationState.uninitialized;

        }

        public static void RenderFrame()
        {
            Debug.Assert(scene != null, "You must create a new scene or load one from storage");

            render?.RenderFrame();

        }



    }
}
