using System.Collections.Generic;
using System.Threading.Tasks;

public sealed class RotationSystem : ECSSystem
{
    private ParallelOptions parallelOptions;

    private IDictionary<uint, RotationComponent> rotationComponents;
    private IDictionary<uint, VelocityComponent> velocityComponents;
    private IEnumerable<uint> queryedEntities;

    public override void Initialize()
    {
        parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 32 };
    }

    protected override void PreExecute(float deltaTime)
    {
        rotationComponents??= ECSManager.GetComponents<RotationComponent>();
        velocityComponents??= ECSManager.GetComponents<VelocityComponent>();
        queryedEntities??= ECSManager.GetEntitiesWithComponentTypes(typeof(RotationComponent), typeof(VelocityComponent));
    }

    protected override void Execute(float deltaTime)
    {
        Parallel.ForEach(queryedEntities, parallelOptions, i =>
        {
            rotationComponents[i].X += velocityComponents[i].directionX  * 200 * deltaTime;
            rotationComponents[i].Y += velocityComponents[i].directionY  * 200 * deltaTime;
            rotationComponents[i].Z += velocityComponents[i].directionZ  * 200 * deltaTime;
        });
    }

    protected override void PostExecute(float deltaTime)
    {
    }
}