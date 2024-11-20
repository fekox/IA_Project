using System.Collections.Generic;

public enum NodeTravelType
{
    Mine,
    HumanCenter,
    Grass,
    Rocks,
    Water
}
public interface INode
{
    public bool IsBlocked();
    public void SetBlocked(bool value = true);
    public bool IsEqual(INode other);
    public float GetWeight();
    public void SetWeight(float weight);
    public void SetID(int id);
    public int GetID();
    public NodeTravelType GetNodeType();
    public void SetNodeType(NodeTravelType type);
    public void SetPlace(IPlace place);
    public IPlace GetPlace();
}

public interface INode<Coordinate> : INode
{
    public void SetCoordinate(Coordinate coordinateType);
    public void SetNeighbor(INode<Coordinate> tNode);
    public ICollection<INode<Coordinate>> GetNeighbors();
    public Coordinate GetCoordinate();
}