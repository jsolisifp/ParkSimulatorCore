using System.Diagnostics;
using System.Numerics;

namespace ParkSimulator
{
    public class DefaultAttractionRenderer : Component
    {
        public Vector3 Color { get; set; }
        public float Size { get; set; } = 5;
        public float PointSize { get; set; } = 2;
        public Vector3 PointColor { get; set; } = new Vector3(0.0f, 0.0f, 0.0f);


        DefaultRender? render;
        DefaultAttraction? location;

        public DefaultAttractionRenderer()
        {
            render = null;
        }

        public override void Start()
        {
        }

        public override void Pass(int passId)
        {
            Debug.Assert(Simulation.Render != null, "La simulación no está iniciada");
            Debug.Assert(simulatedObject != null, "El componente no está añadido a un objeto");

            render ??= (DefaultRender)Simulation.Render;
            location ??= simulatedObject.GetComponent<DefaultAttraction>();

            Debug.Assert(location != null, "Falta el componente location");

            float size = Size * location.Capacity;
            Vector2 p = new Vector2(location.Coordinates.X, location.Coordinates.Z) - Vector2.One * size / 2;
            render.DrawRect(p, Vector2.One * size, new DefaultRender.Color24() { r = 127, g = 127, b = 127 });

            size = Size * location.Occupation;
            p = new Vector2(location.Coordinates.X, location.Coordinates.Z) - Vector2.One * size / 2;
            render.DrawRect(p, Vector2.One * size, new DefaultRender.Color24(Color));

            DefaultAttraction? neighbourLocation = location?.Neighbour?.GetSimulatedObject()?.GetComponent<DefaultAttraction>();

            if(neighbourLocation != null)
            {
                Debug.Assert(location != null, "Falta el componente location");

                Vector2 p1 = new Vector2(location.Coordinates.X, location.Coordinates.Z);
                Vector2 p2 = new Vector2(neighbourLocation.Coordinates.X, neighbourLocation.Coordinates.Z);
                DefaultRender.Color24 color = new (Color);
                render.DrawLine(p1, p2, color);
            }

            Vector2 ellipseSize = Vector2.One * PointSize * 2;
            Vector2 ellipsePosition = new Vector2(location.Coordinates.X, location.Coordinates.Z) - ellipseSize / 2; 

            render.DrawEllipse(ellipsePosition, ellipseSize, new DefaultRender.Color24(PointColor));
        }

    }
}
