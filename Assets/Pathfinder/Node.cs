using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Node<Coordinate> : INode<Coordinate>, IEquatable<Node<Coordinate>>
    where Coordinate : IEquatable<Coordinate>
{
    private Coordinate coordinate;
    private float weight;
    private IPlace place { get; set; }
    public bool isBlocked = false;
    public NodeTravelType type;
    public int id;
    [SerializeField] private List<INode<Coordinate>>neighbours = new();

    public void SetCoordinate(Coordinate coordinate)
    {
        this.coordinate = coordinate;
    }

    public Coordinate GetCoordinate()
    {
        return coordinate;
    }

    public bool IsBlocked()
    {
        return isBlocked;
    }

    public void SetBlocked(bool value = true)
    {
        isBlocked = value;
    }

    public bool IsEqual(INode other)
    {
        return coordinate.Equals(((Node<Coordinate>)other).GetCoordinate());
    }
    

    public float GetWeight() => weight;
    public void SetWeight(float weight)
    {
        this.weight = weight;
    }

    public void SetID(int id) => this.id = id;
    public int GetID()
    {
        return this.id;
    }

    public NodeTravelType GetNodeType()
    {
        return type;
    }

    public void SetNodeType(NodeTravelType type)
    {
        this.type = type;
    }

    public void SetPlace(IPlace place)
    {
        this.place = place;
    }

    public IPlace GetPlace()
    {
        return place;
    }

    public void SetNeighbor(INode<Coordinate> tNode)
    {
        if (!neighbours.Contains(tNode))
        {
            neighbours.Add(tNode);
        }
    }

    public ICollection<INode<Coordinate>> GetNeighbors()
    {
        return neighbours;
    }

    public bool Equals(Node<Coordinate> other)
    {
        return isBlocked == other.isBlocked && coordinate.Equals(other.coordinate) && neighbours == other.neighbours;
    }
}