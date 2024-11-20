using System.Collections.Generic;

using System.Threading.Tasks;
using UnityEngine;

public class DirectionSystem : ECSSystem
{
    private ParallelOptions parallelOptions;
    private IDictionary<uint, PositionComponent> positionComponents;
    private IDictionary<uint, DirectionComponent> directionComponents;
    private IDictionary<uint, ObjectiveComponent> objectiveComponents;
    private IEnumerable<uint> queryedEntities;


    public override void Initialize()
    {
        parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 32 };
    }

    protected override void PreExecute(float deltaTime)
    {
        positionComponents ??= ECSManager.GetComponents<PositionComponent>();
        directionComponents ??= ECSManager.GetComponents<DirectionComponent>();
        objectiveComponents ??= ECSManager.GetComponents<ObjectiveComponent>();
        queryedEntities ??=
            ECSManager.GetEntitiesWithComponentTypes(typeof(ObjectiveComponent), typeof(PositionComponent),
                typeof(DirectionComponent));
    }

    protected override void Execute(float deltaTime)
    {
        Parallel.ForEach(queryedEntities, parallelOptions, i =>
        {
            directionComponents[i].X = objectiveComponents[i].X - positionComponents[i].X;
            directionComponents[i].Y = objectiveComponents[i].Y - positionComponents[i].Y;
            directionComponents[i].Z = objectiveComponents[i].Z - positionComponents[i].Z;

            Vector3 avg = new Vector3(directionComponents[i].X,
                directionComponents[i].Y,
                directionComponents[i].Z);
            avg.Normalize();
            directionComponents[i].X = avg.x;
            directionComponents[i].Y = avg.y;
            directionComponents[i].Z = avg.z;
        });
    }

    protected override void PostExecute(float deltaTime)
    {
    }
}