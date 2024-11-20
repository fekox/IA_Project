using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SeparationSystem : ECSSystem
{
    private ParallelOptions parallelOptions;
    private IDictionary<uint, PositionComponent> positionComponents;
    private IDictionary<uint, RadiusComponent> radiusComponents;
    private IDictionary<uint, SeparationComponent> separationComponents;
    private IEnumerable<uint> queryedEntities;
    private IDictionary<uint, ConcurrentBag<uint>> nearBoids;

    public override void Initialize()
    {
        parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 32 };
        nearBoids = new Dictionary<uint, ConcurrentBag<uint>>();
    }

    protected override void PreExecute(float deltaTime)
    {
        radiusComponents ??= ECSManager.GetComponents<RadiusComponent>();
        separationComponents ??= ECSManager.GetComponents<SeparationComponent>();
        positionComponents ??= ECSManager.GetComponents<PositionComponent>();
        queryedEntities ??=
            ECSManager.GetEntitiesWithComponentTypes(typeof(RadiusComponent), typeof(PositionComponent),
                typeof(SeparationComponent));


        Parallel.ForEach(queryedEntities, parallelOptions, i =>
        {
            ConcurrentBag<uint> insideRadiusBoids = new ConcurrentBag<uint>();

            Parallel.ForEach(queryedEntities, parallelOptions, j =>
            {
                if (positionComponents[i] != positionComponents[j])
                {
                    float distance = Mathf.Abs(positionComponents[i].X - positionComponents[j].X) +
                                     Mathf.Abs(positionComponents[i].Y - positionComponents[j].Y) +
                                     Mathf.Abs(positionComponents[i].Z - positionComponents[j].Z);
                    if (distance < radiusComponents[i].radius)
                    {
                        insideRadiusBoids.Add(j);
                    }
                }
            });


            nearBoids[i] = insideRadiusBoids;
        });
    }

    protected override void Execute(float deltaTime)
    {
        Parallel.ForEach(queryedEntities, parallelOptions, i =>
        {
            separationComponents[i].X = 0;
            separationComponents[i].Y = 0;
            separationComponents[i].Z = 0;
            Parallel.ForEach(nearBoids, parallelOptions, j =>
            {
                separationComponents[i].X += positionComponents[i].X - positionComponents[j.Key].X;
                separationComponents[i].Y += positionComponents[i].Y - positionComponents[j.Key].Y;
                separationComponents[i].Z += positionComponents[i].Z - positionComponents[j.Key].Z;
            });

            Vector3 avg = Vector3.zero;
            avg.x = separationComponents[i].X;
            avg.y = separationComponents[i].Y;
            avg.z = separationComponents[i].Z;

            avg /= nearBoids.Count;
            
            avg *= -1;


            // float magnitude = Mathf.Sqrt(separationComponents[i].X * separationComponents[i].X +
            //                              separationComponents[i].Y * separationComponents[i].Y
            //                              + separationComponents[i].Z + separationComponents[i].Z);
            //
            // magnitude = Normalize(avg)
            // separationComponents[i].X  /= magnitude;
            // separationComponents[i].Y  /= magnitude;
            // separationComponents[i].Z  /= magnitude;
            avg.Normalize();
            separationComponents[i].X = avg.x;
            separationComponents[i].Y = avg.y;
            separationComponents[i].Z = avg.z;
        });
    }

    protected override void PostExecute(float deltaTime)
    {
    }
}