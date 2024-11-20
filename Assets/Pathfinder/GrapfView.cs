using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GrapfView : MonoBehaviour
{
    [FormerlySerializedAs("grapf")] public Vector2Graph<Node<Vector2>> graph;
    public VoronoiDiagram diagram;
    [SerializeField] private Sprite grass;
    [SerializeField] private Sprite rock;
    [SerializeField] private Sprite mine;
    [SerializeField] private Sprite water;
    [SerializeField] private Sprite humanCenter;

    [SerializeField] private List<Transform> limits;

    private CaravanFazade _caravanFazade = new CaravanFazade();
    private List<GameObject> _tiles = new List<GameObject>();
    public GameObject tile;
    public int mines = 3;
    public int nodesX = 3;
    public int nodesY = 3;
    public float offset = 3;

    [ContextMenu("Generate Map")]
    void OnEnable()
    {
        CreateGraph();

        DrawMap(graph);
    }

    private void CreateGraph()
    {
        graph = new Vector2Graph<Node<Vector2>>(nodesX, nodesY, offset, mines, diagram);
        // AStarPathfinder<Node<Vector2>, Vector2> test = new AStarPathfinder<Node<Vector2>, Vector2>();
        // List<Node<Vector2>> findPath = test.FindPath(graph.nodes[0],graph.nodes[^1],graph,_caravanFazade);
        // if (findPath == null||findPath.Count < 0)
        // {
        //     CreateGraph();
        // }
        //Node scale * quatity + (sepation* cuantity -1)
        limits[0].position = new Vector3(0, 1 + nodesY * offset, 0);
        limits[1].position = new Vector3(1 + nodesX * offset, 1 + nodesY * offset, 0);
        limits[2].position = new Vector3(1 + nodesX * offset, 0, 0);
        limits[3].position = new Vector3(0, 0, 0);
    }

    private void DrawMap(Vector2IntGrapf<Node<Vector2Int>> vector2IntGrapf)
    {
    }


    private void DrawMap(Vector2Graph<Node<Vector2>> vector2IntGraph)
    {
        if (_tiles.Count > 0)
        {
            foreach (GameObject var in _tiles)
            {
                if (Application.isEditor && !Application.isPlaying)
                {
                    DestroyImmediate(var);
                }
                else
                {
                    Destroy(var);
                }
            }

            _tiles.Clear();
        }

        foreach (Node<Vector2> node in graph.nodes)
        {
            var position = new Vector3(node.GetCoordinate().x, node.GetCoordinate().y);
            var newTile = Instantiate(tile, position, Quaternion.identity);
            newTile.transform.SetParent(this.transform);
            newTile.GetComponent<SpriteRenderer>().sprite = GetSpriteType(node.GetNodeType());
            _tiles.Add(newTile);
        }
    }

    private Sprite GetSpriteType(NodeTravelType getNodeType)
    {
        return getNodeType switch
        {
            NodeTravelType.Mine => mine,
            NodeTravelType.HumanCenter => humanCenter,
            NodeTravelType.Grass => grass,
            NodeTravelType.Rocks => rock,
            NodeTravelType.Water => water,
            _ => throw new ArgumentOutOfRangeException(nameof(getNodeType), getNodeType, null)
        };
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;
        // foreach (Node<Vector2> node in grapf.nodes)
        // {
        //     if (node.IsBlocked())
        //         Gizmos.color = Color.red;
        //     else
        //         Gizmos.color = Color.green;
        //
        //     Vector3 currentNodeCoordinate = new Vector3(node.GetCoordinate().x, node.GetCoordinate().y);
        //     Gizmos.DrawWireSphere(currentNodeCoordinate, 0.1f);
        //     foreach (INode<Vector2Int> neighborConnections in node.GetNeighbors())
        //     {
        //         Vector2Int vector2Int = neighborConnections.GetCoordinate();
        //         
        //         Vector3 nodePos = new Vector3(vector2Int.x, vector2Int.y);
        //         Gizmos.color = Color.yellow;
        //         Gizmos.DrawLine(currentNodeCoordinate, nodePos);
        //     }
        // }
    }
}

public class CaravanFazade : ITraveler
{
    public virtual bool CanTravelNode(NodeTravelType type)
    {
        return !(type == NodeTravelType.Rocks);
    }

    public float GetNodeCostToTravel(NodeTravelType type)
    {
        return type switch
        {
            NodeTravelType.Mine => 0,
            NodeTravelType.HumanCenter => 0,
            NodeTravelType.Grass => 2,
            NodeTravelType.Rocks => 2,
            NodeTravelType.Water => 5,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public void SetGraph(GrapfView graph)
    {
    }
}