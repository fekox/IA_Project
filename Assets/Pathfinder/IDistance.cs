using System.Collections.Generic;

public interface IDistance<NodeType,Coordinate> where NodeType : INode<Coordinate>
{
    public float GetManhattanDistance(NodeType a, NodeType b);
    public float GetEuclideanDistance(NodeType a, NodeType b);
    public Coordinate GetMediatrix(NodeType a, NodeType b);
}
public interface IGraph<NodeType,Coordinate>: IDistance<NodeType,Coordinate> where NodeType : INode<Coordinate>
{
    public ICollection<NodeType> GetNodes();

}
// public interface IDistance<NodeType,Coordinate> where NodeType : INode<Coordinate> 
// {
//     public Coordinate GetMediatrix(NodeType a, NodeType b, float distance);
// }
