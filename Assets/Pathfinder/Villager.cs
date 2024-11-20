using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Villager : MonoBehaviour, ITraveler
{
    public GrapfView grafp;
    private AStarPathfinder<Node<Vector2>, Vector2> Pathfinder =
        new AStarPathfinder<Node<Vector2>, Vector2>();
    private Node<Vector2> startNode;
    private Node<Vector2> destinationNode;
    private Coroutine startPathFinding;

    private Func<int> gold;
    private Func<int> energy;

    private void OnEnable()
    {
        if (startPathFinding != null)
        {
            StopCoroutine(startPathFinding);
        }

        startPathFinding = StartCoroutine(StartVillager());
    }

    public IEnumerator StartVillager()
    {
        yield return null;
        startNode = grafp.graph.nodes[0];
        destinationNode = grafp.graph.nodes[^1];

        List<Node<Vector2>> path = Pathfinder.FindPath(startNode, destinationNode, grafp.graph, this);
     
    }

    public IEnumerator Move(List<Node<Vector2>> path)
    {
        transform.position = new Vector3(path[0].GetCoordinate().x, path[0].GetCoordinate().y);
        foreach (Node<Vector2> node in path)
        {
            transform.position = new Vector3(node.GetCoordinate().x, node.GetCoordinate().y);
            yield return new WaitForSeconds(1.0f);
        }
    }

    public virtual bool CanTravelNode(NodeTravelType type)
    {
        return true;
    }

    public float GetNodeCostToTravel(NodeTravelType type)
    {
        return type switch
        {
            NodeTravelType.Mine => 0,
            NodeTravelType.HumanCenter => 0,
            NodeTravelType.Grass => 1,
            NodeTravelType.Rocks => 2,
            NodeTravelType.Water => 10,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public void SetGraph(GrapfView graph)
    {
        
    }
}