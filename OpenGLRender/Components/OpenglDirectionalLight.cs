using System.Numerics;

namespace ParkSimulator
{
    public class OpenglDirectionalLight : Component
    {
        public Vector3 Color { get; set; }  = new Vector3(1.0f, 1.0f, 1.0f);
        public float Intensity { get; set; } = 1.0f;

        Location? location;
        OpenglRender? render;

        public override void Pass(int id, object? parameters)
        {

            location ??= GetSimulatedObject()?.GetComponent<Location>();
            render ??= (OpenglRender?)Simulation.Render;

            if(render != null)
            {
                Matrix4x4 model = RenderMathUtils.GetModelMatrix(location.Position, location.Rotation);
                Vector3 direction = RenderMathUtils.TransformDirection(Vector3.UnitY, model);

                render.SetDirectionalLight(direction, Intensity, Color);
            }
            //Vector3 direction = gameObject.transform.TransformDirection(Vector3.UnitZ);
        }

    }
}
