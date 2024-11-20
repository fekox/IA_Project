using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Debug = System.Diagnostics.Debug;
using Random = UnityEngine.Random;

[Serializable]
public class Vector2Graph<NodeType> : IGraph<Node<Vector2>, UnityEngine.Vector2>
    where NodeType : class, INode<UnityEngine.Vector2>, INode, new()
{
    public List<Node<Vector2>> nodes = new List<Node<Vector2>>();
    public Node<Vector2>[,] nodesMatrix;
    private System.Random random = new System.Random();
    private CaravanFazade _caravanFazade = new();
    public List<Node<Vector2>> mines = new List<Node<Vector2>>();
    public int mineQuantity = 1;
    private VoronoiDiagram diagram;

    public Vector2Graph(int x, int y, float offSet, int mineQuantity, VoronoiDiagram diagram)
    {
        int counter = 0;
        this.mineQuantity = Mathf.Clamp(mineQuantity, 1, (x * y) - y);
        nodesMatrix = new Node<Vector2>[x, y];
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                Node<Vector2> node = new Node<Vector2>();
                node.SetID(counter++);
                node.SetCoordinate(new UnityEngine.Vector2(1 + i * offSet, 1 + j * offSet));
                nodes.Add(node);
                nodesMatrix[i, j] = node;
            }
        }

        this.diagram = diagram;
        PathFinderManager<Node<Vector2>, Vector2>.graph = this;
        SetCardinalConnections(x, y);
        SetRandomType();
        SetRandomHumanCenter();
    }

    private void SetRandomType()
    {
        foreach (Node<Vector2> node in nodes)
        {
            int range = Random.Range(0, Enum.GetValues(typeof(NodeTravelType)).Length);
            NodeTravelType nodeTravelType = (NodeTravelType)range;
            node.SetNodeType(nodeTravelType);
            node.SetWeight(range);
            switch (nodeTravelType)
            {
                case NodeTravelType.Grass:
                    break;
                case NodeTravelType.Rocks:
                    break;
                case NodeTravelType.Water:
                    break;
                default:
                    nodeTravelType = NodeTravelType.Grass;
                    node.SetNodeType(nodeTravelType);
                    node.SetWeight(1);
                    break;
            }

            if (nodeTravelType == NodeTravelType.Water)
            {
                node.SetBlocked();
            }
        }
    }

    private void SetRandomHumanCenter()
    {
        nodes[0].SetNodeType(NodeTravelType.HumanCenter);
        nodes[0].SetWeight(0);
        nodes[0].SetPlace(new HumanCenter2D());
        nodes[0].SetBlocked(false);
        HumanCenter2D humanCenter = (HumanCenter2D)nodes[0].GetPlace();
        humanCenter.SetGraph(this);
        humanCenter.SetNode(nodes[0]);
        for (int i = 0; i < mineQuantity; i++)
        {
            SetRandomMine(humanCenter);
        }
        // SetRandomMine(humanCenter);
        //SetRandomMine(humanCenter);
    }

    private void SetRandomMine(HumanCenter2D humanCenter)
    {
        int randomNode = Random.Range(0, nodes.Count);
        if (nodes[randomNode].GetPlace() is HumanCenter<NodeType, Vector2> || nodes[randomNode].GetPlace() is Mine)
        {
            SetRandomMine(humanCenter);
        }
        else
        {
            if (CalculatePathToMine(humanCenter, nodes[randomNode], out var nodeToMine))
            {
                nodeToMine.SetNodeType(NodeTravelType.Mine);
                nodeToMine.SetWeight(0);
                nodeToMine.SetBlocked(false);
                Mine place = new Mine();
                nodeToMine.SetPlace(place);
                mines.Add(nodeToMine);
                humanCenter.AddGoldNode(nodeToMine,place);
                humanCenter.diagram = diagram;

            }
            else
            {
                SetRandomMine(humanCenter);
            }
        }

        humanCenter.CreateVoronoid();
    }

    private bool CalculatePathToMine(HumanCenter2D humanCenter, Node<Vector2> node, out Node<Vector2> nodeToAdd)
    {
        List<Node<Vector2>> nodeTypes =
            PathFinderManager<Node<Vector2>, Vector2>.GetPath(humanCenter.currentNode, node, _caravanFazade);

        if (nodeTypes != null && nodeTypes.Count > 0)
        {
            nodeToAdd = node;
            return true;
        }
        else
        {
            List<Node<Vector2>> nodesOut = new List<Node<Vector2>>();
            var previousNode = node;
            nodesOut.Add(node);

            int maxIterations = 1000; // Limit the number of iterations to prevent infinite loop
            int iterations = 0;
            bool foundPath = false;

            while ((nodeTypes == null || nodeTypes.Count <= 0) && iterations < maxIterations)
            {
                bool foundNeighbor = false;

                foreach (INode<Vector2> neighbor in previousNode.GetNeighbors())
                {
                    Node<Vector2> nodeToChange = nodes[neighbor.GetID()];

                    if (!nodesOut.Contains(nodeToChange))
                    {
                        nodesOut.Add(nodeToChange);
                        nodeToChange.SetNodeType(NodeTravelType.Grass);
                        nodeToChange.SetWeight(1);
                        nodeToChange.SetBlocked(false);

                        // Attempt to find a new path after modifying the neighbor
                        nodeTypes = PathFinderManager<Node<Vector2>, Vector2>.GetPath(humanCenter.currentNode, nodeToChange,
                            _caravanFazade);

                        if (nodeTypes != null && nodeTypes.Count > 0)
                        {
                            foundPath = true;
                            break;
                        }

                        previousNode = nodeToChange;
                        foundNeighbor = true;
                        break;
                    }
                }

                if (!foundNeighbor)
                {
                    // If no valid neighbor is found, break out of the loop
                    break;
                }

                iterations++;
            }

            if (foundPath)
            {
                nodeToAdd = previousNode; // Add the valid node with a path
                return true;
            }

            // If no valid path is found
            nodeToAdd = null;
            return false;
        }
    }

    private void SetCardinalConnections(int x, int y)
    {
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                if (j + 1 < y)
                {
                    nodesMatrix[i, j].SetNeighbor(nodesMatrix[i, j + 1]);
                }

                if (j - 1 > 0)
                {
                    nodesMatrix[i, j].SetNeighbor(nodesMatrix[i, j - 1]);
                }

                if (i + 1 < x)
                {
                    nodesMatrix[i, j].SetNeighbor(nodesMatrix[i + 1, j]);
                }

                if (i - 1 > 0)
                {
                    nodesMatrix[i, j].SetNeighbor(nodesMatrix[i - 1, j]);
                }
            }
        }
    }

    private void MakeRandomConnection(int index, NodeType current)
    {
        int randomIndex = random.Next(nodes.Count);
        while (randomIndex == index)
        {
            randomIndex = random.Next(nodes.Count);
        }
    }

    public float GetManhattanDistance(Node<Vector2> a, Node<Vector2> b)
    {
        return Mathf.Abs(a.GetCoordinate().x - b.GetCoordinate().x) +
               Mathf.Abs(a.GetCoordinate().y - b.GetCoordinate().y);
    }

    public float GetEuclideanDistance(Node<Vector2> a, Node<Vector2> b)
    {
        return Mathf.Sqrt(Mathf.Pow(a.GetCoordinate().x - b.GetCoordinate().x, 2) +
                          Mathf.Pow(Mathf.Abs(a.GetCoordinate().y - b.GetCoordinate().y), 2));
    }

    public Vector2 GetMediatrix(Node<Vector2> a, Node<Vector2> b)
    {
        
        Vector2 perp = b.GetCoordinate() - a.GetCoordinate();
        Vector2 perpendicular = new Vector2(-perp.y, perp.x);
        //return GetEuclideanDistance(a, b) + perpendicular *0.5f;
        return default;
    }

    public ICollection<Node<Vector2>> GetNodes()
    {
        return nodes;
    }
}