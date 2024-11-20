using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class RadiusBoidSystem : ECSSystem
{
    private ParallelOptions parallelOptions;
    private IDictionary<uint, RadiusComponent> nearBoidsComponents;
    private IDictionary<uint, PositionComponent> positionComponents;
    private IEnumerable<uint> queryedEntities;

    public override void Initialize()
    {
        parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 32 };
    }

    protected override void PreExecute(float deltaTime)
    {
        nearBoidsComponents ??= ECSManager.GetComponents<RadiusComponent>();
        positionComponents ??= ECSManager.GetComponents<PositionComponent>();
        queryedEntities ??=
            ECSManager.GetEntitiesWithComponentTypes(typeof(RadiusComponent), typeof(PositionComponent));

    }



    protected override void Execute(float deltaTime)
    {
    
    }

    protected override void PostExecute(float deltaTime)
    {
       
    }
}