using System.Numerics;

namespace ParkSimulator
{
    public class OpenglCamera : Component
    {
        public Vector3 BackgroundColor { get; set; }  = new Vector3(0, 0, 1);
        public float Fov { get; set; } = 60.0f;
        public float ZNear { get; set; } = 0.1f;
        public float ZFar { get; set; } = 1000.0f;

        OpenglRender? render;
        Location? location;

        public override void Pass(int id, object? parameters)
        {
            render ??= (OpenglRender)Simulation.Render;
            location ??= (Location)GetSimulatedObject().GetComponent<Location>();

            if(render != null && location != null)
            {
                render.SetClearColor(BackgroundColor);
                render.SetView(location.Position, location.Rotation, Fov, ZNear, ZFar);
            }
        }
    }
}
