using System.Collections.Generic;
using System.Linq;

public abstract class Pathfinder<NodeType, Coordinate> where NodeType : INode<Coordinate>
{
    protected IDistance<NodeType,Coordinate> graph;
    protected bool useManhattan = true;

    public List<NodeType> FindPath(NodeType startNode, NodeType destinationNode, IGraph<NodeType,Coordinate> graph,
        ITraveler traveler)
    {
        Dictionary<NodeType, (NodeType Parent, float AcumulativeCost, float Heuristic)> nodes =
            new Dictionary<NodeType, (NodeType Parent, float AcumulativeCost, float Heuristic)>();

        this.graph = graph;
        foreach (NodeType node in graph.GetNodes())
        {
            nodes.Add(node, (default, 0, 0));
        }


        List<NodeType> openList = new List<NodeType>();
        openList.Add(startNode);


        List<NodeType> closedList = new List<NodeType>();
        while (openList.Count > 0)
        {
            NodeType currentNode = openList[0];
            int currentIndex = 0;

            for (int i = 1; i < openList.Count; i++)
            {
                if (nodes[openList[i]].AcumulativeCost + nodes[openList[i]].Heuristic <
                    nodes[currentNode].AcumulativeCost + nodes[currentNode].Heuristic)
                {
                    currentNode = openList[i];
                    currentIndex = i;
                }
            }

            openList.RemoveAt(currentIndex);
            closedList.Add(currentNode);

            if (NodesEquals(currentNode, destinationNode))
            {
                return GeneratePath(startNode, destinationNode);
            }

            foreach (NodeType neighbor in GetNeighbors(currentNode))
            {
                //
                // NodeType neighbor = graph.ToArray()[tNeighbour.GetDestination()];

                if (!nodes.ContainsKey(neighbor) || IsBloqued(neighbor) ||
                    closedList.Contains(neighbor) || IsImpassable(neighbor, traveler))
                {
                    continue;
                }

                float tentativeNewAcumulatedCost = 0;
                tentativeNewAcumulatedCost += nodes[currentNode].AcumulativeCost;
                tentativeNewAcumulatedCost += MoveToNeighborCost(currentNode, neighbor, traveler);

                if (!openList.Contains(neighbor) || tentativeNewAcumulatedCost < nodes[currentNode].AcumulativeCost)
                {
                    nodes[neighbor] = (currentNode, tentativeNewAcumulatedCost, Distance(neighbor, destinationNode));

                    if (!openList.Contains(neighbor))
                    {
                        openList.Add(neighbor);
                    }
                }
            }
        }

        return null;

        List<NodeType> GeneratePath(NodeType startNode, NodeType goalNode)
        {
            List<NodeType> path = new List<NodeType>();
            NodeType currentNode = goalNode;
            
            while (!NodesEquals(currentNode, startNode))
            {
                path.Add(currentNode);
                currentNode = nodes[currentNode].Parent;
            }
            path.Add(startNode);

            path.Reverse();
            return path;
        }
    }
    public List<NodeType> FindPath(NodeType startNode, NodeType destinationNode, IGraph<NodeType,Coordinate> graph)
    {
        Dictionary<NodeType, (NodeType Parent, float AcumulativeCost, float Heuristic)> nodes =
            new Dictionary<NodeType, (NodeType Parent, float AcumulativeCost, float Heuristic)>();

        this.graph = graph;
        foreach (NodeType node in graph.GetNodes())
        {
            nodes.Add(node, (default, 0, 0));
        }


        List<NodeType> openList = new List<NodeType>();
        openList.Add(startNode);


        List<NodeType> closedList = new List<NodeType>();
        while (openList.Count > 0)
        {
            NodeType currentNode = openList[0];
            int currentIndex = 0;

            for (int i = 1; i < openList.Count; i++)
            {
                if (nodes[openList[i]].AcumulativeCost + nodes[openList[i]].Heuristic <
                    nodes[currentNode].AcumulativeCost + nodes[currentNode].Heuristic)
                {
                    currentNode = openList[i];
                    currentIndex = i;
                }
            }

            openList.RemoveAt(currentIndex);
            closedList.Add(currentNode);

            if (NodesEquals(currentNode, destinationNode))
            {
                return GeneratePath(startNode, destinationNode);
            }

            foreach (NodeType neighbor in GetNeighbors(currentNode))
            {
                //
                // NodeType neighbor = graph.ToArray()[tNeighbour.GetDestination()];

                if (!nodes.ContainsKey(neighbor) || IsBloqued(neighbor) ||
                    closedList.Contains(neighbor) )
                {
                    continue;
                }

                float tentativeNewAcumulatedCost = 0;
                tentativeNewAcumulatedCost += nodes[currentNode].AcumulativeCost;
                tentativeNewAcumulatedCost += MoveToNeighborCost(currentNode, neighbor);

                if (!openList.Contains(neighbor) || tentativeNewAcumulatedCost < nodes[currentNode].AcumulativeCost)
                {
                    nodes[neighbor] = (currentNode, tentativeNewAcumulatedCost, Distance(neighbor, destinationNode));

                    if (!openList.Contains(neighbor))
                    {
                        openList.Add(neighbor);
                    }
                }
            }
        }

        return null;

        List<NodeType> GeneratePath(NodeType startNode, NodeType goalNode)
        {
            List<NodeType> path = new List<NodeType>();
            NodeType currentNode = goalNode;

            while (!NodesEquals(currentNode, startNode))
            {
                path.Add(currentNode);
                currentNode = nodes[currentNode].Parent;
            }
path.Add(startNode);
            path.Reverse();
            return path;
        }
    }

    protected abstract ICollection<NodeType> GetNeighbors(NodeType node);

    protected abstract float Distance(NodeType A, NodeType B);

    protected abstract bool NodesEquals(NodeType A, NodeType B);

    protected abstract float MoveToNeighborCost(NodeType A, NodeType b, ITraveler iTraveler);
    protected abstract float MoveToNeighborCost(NodeType A, NodeType b);

    protected abstract bool IsBloqued(NodeType node);
    protected abstract bool IsImpassable(NodeType node, ITraveler traveler);
}