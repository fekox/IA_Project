using System.Collections.Generic;

public class DijkstraPathfinder<NodeType, Coordinate> : Pathfinder<NodeType, Coordinate>
    where NodeType : class, INode<Coordinate>
{
    protected override float Distance(NodeType A, NodeType B)
    {
        return useManhattan
            ? graph.GetManhattanDistance(A, B)
            : graph.GetEuclideanDistance(A, B);
    }

    protected override ICollection<NodeType> GetNeighbors(NodeType node)
    {
        ICollection<INode<Coordinate>> neighbors = node.GetNeighbors();
        List<NodeType> neighborsList = new List<NodeType>();

        foreach (var neighbor in neighbors)
        {
            neighborsList.Add(neighbor as NodeType);
        }

        return neighborsList;
    }

    protected override float MoveToNeighborCost(NodeType A, NodeType b)
    {
        return useManhattan
            ? graph.GetManhattanDistance(A, b)
            : graph.GetEuclideanDistance(A, b);
    }

    protected override bool IsBloqued(NodeType node)
    {
        return node.IsBlocked();
    }

    protected override bool IsImpassable(NodeType node, ITraveler traveler)
    {
        throw new System.NotImplementedException();
    }

    protected override float MoveToNeighborCost(NodeType A, NodeType b, ITraveler iTraveler)
    {
        return useManhattan
            ? graph.GetManhattanDistance(A, b)
            : graph.GetEuclideanDistance(A, b);
    }

    protected override bool NodesEquals(NodeType A, NodeType B)
    {
        return A.Equals(B);
    }
}