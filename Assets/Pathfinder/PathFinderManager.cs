using System.Collections.Generic;

public static class PathFinderManager<NodeType, Coordinate>
    where NodeType : class, INode<Coordinate>
{
    private static AStarPathfinder<NodeType, Coordinate> a = new AStarPathfinder<NodeType, Coordinate>();
    public static IGraph<NodeType,Coordinate> graph;

    public static List<NodeType> GetPath(NodeType currentNode, NodeType destinationNode,
        ITraveler traveler)
    {
        return a.FindPath(currentNode, destinationNode, graph, traveler);
    }  public static List<NodeType> GetPath(NodeType currentNode, NodeType destinationNode,IGraph<NodeType,Coordinate> graph,
        ITraveler traveler)
    {
        return a.FindPath(currentNode, destinationNode, graph, traveler);
    }
}