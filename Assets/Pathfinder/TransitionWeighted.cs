public class TransitionWeighted : TransitionDistance
{
    protected float accumulativeWeight;

    public TransitionWeighted(int destination, float heuristics, float accumulativeWeight) : base(destination, heuristics)
    {
        this.accumulativeWeight = accumulativeWeight;
    }

    public float GetAccumulativeWeight() => accumulativeWeight;
}