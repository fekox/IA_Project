using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Traveler : MonoBehaviour , ITraveler
{
    public GrapfView grapfView;

      private AStarPathfinder<Node<Vector2Int>,Vector2Int> Pathfinder = new();
    //private DijstraPathfinder<Node<Vector2Int>> Pathfinder;
    // private DepthFirstPathfinder<Node<Vector2Int>, Vector2Int> Pathfinder =
    //     new DepthFirstPathfinder<Node<Vector2Int>, Vector2Int>();
    //private BreadthPathfinder<Node<Vector2Int>> Pathfinder;

    private Node<Vector2Int> startNode;
    private Node<Vector2Int> destinationNode;

    void OnEnable()
    {
        startNode = new Node<Vector2Int>();
        destinationNode = new Node<Vector2Int>();

    }

    public IEnumerator Move(List<Node<Vector2Int>> path)
    {
        foreach (Node<Vector2Int> node in path)
        {
            transform.position = new Vector3(node.GetCoordinate().x, node.GetCoordinate().y);
            yield return new WaitForSeconds(1.0f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
            return;


        Gizmos.color = Color.blue;

        Vector3 currentNodeCoordinate = new Vector3(startNode.GetCoordinate().x, startNode.GetCoordinate().y);
        Gizmos.DrawWireSphere(currentNodeCoordinate, 0.1f);
        currentNodeCoordinate = new Vector3(destinationNode.GetCoordinate().x, destinationNode.GetCoordinate().y);
        Gizmos.DrawWireSphere(currentNodeCoordinate, 0.1f);
    }

    public bool CanTravelNode(NodeTravelType type)
    {
        return true;
    }

    public float GetNodeCostToTravel(NodeTravelType type)
    {

        return 0;
    }

    public void SetGraph(GrapfView graph)
    {
        
    }
}