using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ParkSimulator
{
    public class DefaultParkServiceRenderer : Component
    {
        public Vector3 WaitingColor { get; set; } = new Vector3(0, 0, 1);
        public Vector3 WaitingCapacityColor { get; set; }  = new Vector3(0.3f, 0.3f, 0.3f);

        public Vector3 ServiceColor{ get; set; } = new Vector3(0, 1, 0);
        public Vector3 ServiceCapacityColor { get; set; }  = new Vector3(0.1f, 0.1f, 0.1f);
        public float OccupationScale { get; set; } = 5;

        public float PointSize { get; set; } = 2;
        public Vector3 PointColor { get; set; } = new Vector3(1.0f, 0.0f, 0.0f);

        public Vector3 LinesToNeighboursColor { get; set; } = new Vector3(1.0f, 1.0f, 1.0f);

        DefaultRender? render;
        Location? location;
        ParkService? service;

        float capacityTotal = 0;
        float capacityService = 0;
        float capacityWaiting = 0;

        float ocupationService = 0;
        float occupationWaiting = 0;

        public DefaultParkServiceRenderer()
        {
            render = null;
            location = null;
            service = null;
        }

        public override void Step(float deltaTime)
        {
            service ??= simulatedObject.GetComponent<ParkService>();

            Debug.Assert(service != null, "Falta el componente park service");  

            capacityTotal = service.GetTotalVisitorCapacity();
            capacityService = service.VisitorsServiceCapacity;
            capacityWaiting = service.VisitorsWaitingCapacity;

            ocupationService = service.GetServiceVisitorOccupation();
            occupationWaiting = service.GetVisitorsWaitingOccupation();
        }

        public override void Pass(int id, object? parameters)
        {
            render ??= (DefaultRender)Simulation.Render;
            location ??= simulatedObject.GetComponent<Location>();
            service ??= simulatedObject.GetComponent<ParkService>();

            if(render == null || location == null || service == null) { return; }


            float radius = capacityTotal * OccupationScale;
            Vector2 center = new(location.Position.X, location.Position.Y);
            DefaultRender.Color24 color = new(WaitingCapacityColor);

            render.DrawEllipse(center - Vector2.One * radius, Vector2.One * 2 * radius, color);

            radius = (capacityService + occupationWaiting) * OccupationScale;
            color = new(WaitingColor);

            render.DrawEllipse(center - Vector2.One * radius, Vector2.One * 2 * radius, color);

            radius = (capacityService) * OccupationScale;
            color = new(ServiceCapacityColor);

            render.DrawEllipse(center - Vector2.One * radius, Vector2.One * 2 * radius, color);

            radius = (ocupationService) * OccupationScale;
            color = new(ServiceColor);

            render.DrawEllipse(center - Vector2.One * radius, Vector2.One * 2 * radius, color);

            for(int i = 0; i < service.Neighbours.Length; i++)
            {
                Location neighbourLocation = service.Neighbours[i].GetSimulatedObject().GetComponent<Location>();

                Vector2 p1 = new Vector2(location.Position.X, location.Position.Y);
                Vector2 p2 = new Vector2(neighbourLocation.Position.X, neighbourLocation.Position.Y);

                render.DrawLine(p1, p2, new(LinesToNeighboursColor));
            }
        }
    }
}
