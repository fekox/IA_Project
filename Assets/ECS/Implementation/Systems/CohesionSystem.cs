using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CohesionSystem : ECSSystem
{
    private ParallelOptions parallelOptions;

    private IDictionary<uint, PositionComponent> positionComponents;
    private IDictionary<uint, RadiusComponent> radiusComponents;
    private IDictionary<uint, CohesionComponent> cohesionComponents;
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
        cohesionComponents ??= ECSManager.GetComponents<CohesionComponent>();
        positionComponents ??= ECSManager.GetComponents<PositionComponent>();
        queryedEntities ??=
            ECSManager.GetEntitiesWithComponentTypes(typeof(RadiusComponent), typeof(PositionComponent),
                typeof(CohesionComponent));


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
            cohesionComponents[i].X = 0;
            cohesionComponents[i].Y = 0;
            cohesionComponents[i].Z = 0;
            Parallel.ForEach(nearBoids, parallelOptions, j =>
            {
                cohesionComponents[i].X += positionComponents[j.Key].X;
                cohesionComponents[i].Y += positionComponents[j.Key].Y;
                cohesionComponents[i].Z += positionComponents[j.Key].Z;
            });

            Vector3 avg = Vector3.zero;
            
            avg.x = cohesionComponents[i].X;
            avg.y = cohesionComponents[i].Y;
            avg.z = cohesionComponents[i].Z;
            
            avg /= nearBoids.Count;
            
            avg.x -= positionComponents[i].X;
            avg.y -= positionComponents[i].Y;
            avg.z -= positionComponents[i].Z;
            avg.Normalize();
            
            cohesionComponents[i].X = avg.x;
            cohesionComponents[i].Y = avg.y;
            cohesionComponents[i].Z = avg.z;
        });
    }

    protected override void PostExecute(float deltaTime)
    {
    }
}