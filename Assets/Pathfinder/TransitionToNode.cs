using System;
using UnityEngine;

[Serializable]
public class TransitionToNode
{
    [SerializeField] protected int destinationID;

    public TransitionToNode(int destination)
    {
        this.destinationID = destination;
    }

    public int GetDestination() => destinationID;
}