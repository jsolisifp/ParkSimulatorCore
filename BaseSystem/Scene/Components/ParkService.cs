using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParkSimulator
{
    public enum SiteType
    {
        attraction,
        restaurant,
        entrance
    }

    public enum VisitorState
    {
        waiting,
        service,
        done
    }

    public struct VisitorData
    {
        public VisitorState state;
        public float timer;
    }

    public class ParkService : Component
    {
        public SiteType Type { get; set; }

        public int InitialVisitors { get; set; }

        public int VisitorsServiceCapacity { get; set; }
        public int VisitorsWaitingCapacity { get; set; }
        public float VisitorWaitTimeMax { get; set; } = 10;
        public float VisitorWaitTimeMin { get; set; } = 1;
        public float VisitorServiceTimeMax { get; set; } = 15;
        public float VisitorServiceTimeMin { get; set; } = 2;

        public float TotalCost { get; set; }
        public float BaseCost { get; set; } = 1;
        public float BaseServiceCost { get; set; } = 1;
        public float PerVisitorServiceCost { get; set; } = 0.01f;

        public ParkService[] Neighbours { get; set; } = new ParkService[0];
        
        List<VisitorData> visitors;

        bool started = false;

        public override void Start()
        {
            visitors = new List<VisitorData>();

            TotalCost = 0;

            for(int i = 0; i < InitialVisitors; i++)
            {
                VisitorData data = new()
                                    { state = VisitorState.waiting,
                                      timer = VisitorWaitTimeMin + Simulation.Random.Next() % (VisitorWaitTimeMax - VisitorWaitTimeMin)
                                    };

                visitors.Add(data);
            }

            started = true;
        }

        public override void Step(float deltaTime)
        {
            int servicingVisitors = GetServiceVisitorOccupation();
            TotalCost += BaseCost * deltaTime;
            if(servicingVisitors > 0)
            {
                TotalCost += BaseServiceCost * deltaTime;
                TotalCost += PerVisitorServiceCost * servicingVisitors * deltaTime;
            }
            

            for(int i = 0; i < visitors.Count; i++)
            {
                VisitorData data = visitors[i];

                data.timer -= deltaTime;

                if(data.timer <= 0)
                {
                    if(data.state == VisitorState.waiting)
                    {
                        if(VisitorsServiceCapacity > GetServiceVisitorOccupation())
                        {
                            data.state = VisitorState.service;
                            data.timer = VisitorServiceTimeMin + Simulation.Random.Next() % (VisitorServiceTimeMax - VisitorServiceTimeMin);
                        }
                    }
                    else // data.state == VisitorState.service
                    {
                        data.state = VisitorState.done;
                    }
                }

                visitors[i] = data;
            }

            int index = visitors.FindIndex(v => v.state == VisitorState.done);
            bool neighboursFull = (Neighbours.Length == 0);
            while(index >= 0 && !neighboursFull)
            {
                bool exited = false;
                int neighbourIndex = 0;
                int triesLeft = Neighbours.Length;

                while(triesLeft > 0 && !exited)
                {
                    if (Neighbours[neighbourIndex].GetVisitorsWaitingOccupation() < Neighbours[neighbourIndex].VisitorsWaitingCapacity)
                    {
                        Neighbours[neighbourIndex].AddVisitor();
                        visitors.RemoveAt(index);
                        exited = true;
                    }
                    else
                    {
                        triesLeft --;

                        neighbourIndex = Simulation.Random.Next() % Neighbours.Length;
                    }
                }

                if(exited)
                {
                    index = visitors.FindIndex(index, v => v.state == VisitorState.done);
                }
                else
                {
                    neighboursFull = true;
                }
            }
        }

        public override void Stop()
        {
            
        }

        public int GetTotalVisitorCapacity()
        {
            if(!started) { Start(); }

            return VisitorsWaitingCapacity + VisitorsServiceCapacity;
        }

        public int GetTotalVisitorOccupation()
        {
            if(!started) { Start(); }

            return visitors.Count;
        }

        public int GetVisitorsWaitingOccupation()
        {
            if(!started) { Start(); }

            int count = 0;
            for(int i = 0; i < visitors.Count; i++) { if (visitors[i].state == VisitorState.waiting) { count ++; } }

            return count;
        }

        public int GetServiceVisitorOccupation()
        {
            if(!started) { Start(); }

            int count = 0;
            for(int i = 0; i < visitors.Count; i++) { if (visitors[i].state == VisitorState.service) { count ++; } }

            return count;
        }

        public void AddVisitor()
        {
            if(!started) { Start(); }

            Debug.Assert(GetVisitorsWaitingOccupation() < VisitorsWaitingCapacity, "Cannot add more visitors");

            VisitorData data = new()
                                { state = VisitorState.waiting,
                                  timer = VisitorWaitTimeMin + Simulation.Random.Next() % (VisitorWaitTimeMax - VisitorWaitTimeMin)
                                };

            visitors.Add(data);
        }
    }
}
