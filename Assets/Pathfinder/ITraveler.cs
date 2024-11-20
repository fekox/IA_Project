public interface ITraveler
{
    public bool CanTravelNode(NodeTravelType type);
    public float GetNodeCostToTravel(NodeTravelType type);
    public void SetGraph(GrapfView graph);
}

public interface IPlace
{
    public void ActionOnPlace();
}
public interface IFlock
{
    public BoidAgent GetBoid();
    public void SetActive(bool value = true);
}
public interface IAlarmable
{
    public void InvokeAlarmOn();
    public void InvokeAlarmOff();
}