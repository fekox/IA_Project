using System;

public class TransitionDistance : TransitionToNode
{
    protected float heuristics;

    public TransitionDistance(int destination, float heuristics) : base(destination)
    {
        this.heuristics = heuristics;
    }

    public float GetDistance() => heuristics;
}