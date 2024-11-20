using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public abstract class HumanCenterBase
{
}

public abstract class HumanCenter<NodeType, Coordinate> : HumanCenterBase, IPlace
    where NodeType : class, INode<Coordinate>
    where Coordinate : IEquatable<Vector2>
{
    public NodeType currentNode;
    protected List<Villager> _villagers = new List<Villager>();
    protected IGraph<NodeType, Coordinate> graph;
    protected AStarPathfinder<NodeType, Coordinate> a = new AStarPathfinder<NodeType, Coordinate>();
    protected List<NodeType> goldMines = new List<NodeType>();
    public VoronoiDiagram diagram;


    protected Dictionary<Mine, NodeType> id = new();

    public void SetGraph(IGraph<NodeType, Coordinate> graph)
    {
        this.graph = graph;
    }

    public virtual void AddGoldNode(NodeType node, Mine mine)
    {
        goldMines.Add(node);
        id.Add(mine, node);
        mine.OnGoldEmpty += OnGoldEnd;
    }

    public abstract void OnGoldEnd(Mine currentMine);

    public void SetNode(NodeType node)
    {
        currentNode = node;
    }

    public void SpawnVillager()
    {
    }

    public void ActionOnPlace()
    {
    }

    public List<NodeType> GetNewDestination(ITraveler traveler, Vector2 position)
    {
  
        foreach (ThiessenPolygon2D<SegmentVec2, Vector2> poli in diagram.GetPoly)
        {
            if (poli.IsInside(position))
            {
                var a = goldMines.Where(p => p.GetCoordinate().Equals(poli.itemSector));
                return PathFinderManager<NodeType, Coordinate>.GetPath(currentNode, a.ToArray()[0], traveler);
            }
        }

        return PathFinderManager<NodeType, Coordinate>.GetPath(currentNode, currentNode , traveler);
    }

    public List<NodeType> GetNewDestination(ITraveler traveler, NodeType currentPosition, Vector2 position)
    {

        foreach (ThiessenPolygon2D<SegmentVec2, Vector2> poli in diagram.GetPoly)
        {
            if (poli.IsInside(position))
            {
                var a = goldMines.Where(p => p.GetCoordinate().Equals(poli.itemSector));
                if (a.ToArray().Length > 0)
                {
                    return PathFinderManager<NodeType, Coordinate>.GetPath(currentPosition, a.ToArray()[0],graph, traveler);
                }
            }
        }

        return PathFinderManager<NodeType, Coordinate>.GetPath(currentNode, currentPosition,graph, traveler);
    }
}

public class HumanCenter2D : HumanCenter<Node<Vector2>, Vector2>, IPlace
{
    public override void OnGoldEnd(Mine currentMine)
    {
        // diagram.RemoveItem(id[currentMine].GetCoordinate());
        goldMines.Remove(id[currentMine]);
        currentMine.OnGoldEmpty -= OnGoldEnd;
        diagram.pointsToCheck = new List<Vector2>();
        foreach (Node<Vector2> mine in goldMines)
        {
            diagram.pointsToCheck.Add(mine.GetCoordinate());
        }

        diagram.create = true;
    }

    public void CreateVoronoid()
    {
        diagram.pointsToCheck = new List<Vector2>();
        foreach (Node<Vector2> mine in goldMines)
        {
            diagram.pointsToCheck.Add(mine.GetCoordinate());
        }

        diagram.CreateSegments();
    }
}