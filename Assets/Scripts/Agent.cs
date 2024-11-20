using System;
using System.Collections;
using System.Collections.Generic;
using Miner;
using UnityEngine;


public class Agent : MonoBehaviour, ITraveler ,IFlock,IAlarmable
{
    private FSM<MinerStates, MinerFlags> fsm;

    [SerializeField] private float chaseDistance = 0.5f;

    private int gold = 0;
    private int energy = 3;
    private int maxGold = 15;
    private float timeBetweenGold = 1.0f;
    public BoidAgent boid;
    private bool isAlarmOn = false;

    public List<Node<Vector2>> path;
    private Node<Vector2> currentNode;
    private Node<Vector2> currentMine;
    private Node<Vector2> currentObjective;
    private Node<Vector2> humanCenterNode;


    public Action onAlarmRaised = () => { };
    public Action onAlarmStop = () => { };
    private Matrix4x4 drawMatrix;

    private Node<Vector2> startNode;
    private Coroutine startPathFinding;

    public GrapfView grafp;
    private AStarPathfinder<Node<Vector2>, Vector2> Pathfinder =
        new AStarPathfinder<Node<Vector2>, Vector2>();
    public GameObject prefab;
    private Mesh prefabMesh;
    private Material prefabMaterial;


    private void OnEnable()
    {
        if (startPathFinding != null)
        {
            StopCoroutine(startPathFinding);
        }
        prefabMesh = prefab.GetComponent<MeshFilter>().sharedMesh;
        prefabMaterial = prefab.GetComponent<MeshRenderer>().sharedMaterial;
        startPathFinding = StartCoroutine(StartVillager());
    }

    public IEnumerator StartVillager()
    {
        boid = new BoidAgent()
        {
            parent = transform
        };
        yield return null;
        yield return null;
        yield return null;
        startNode = grafp.graph.nodes[0];
        humanCenterNode = startNode;
        transform.position = humanCenterNode.GetCoordinate();
        boid.objective = grafp.graph.nodes[^1].GetCoordinate();
        boid.parent = transform;
        PathFinderManager<Node<Vector2>, Vector2>.graph = grafp.graph;
        //path = Pathfinder.FindPath(startNode, destinationNode, grafp.graph, this);

        currentNode = startNode;
        fsm = new FSM<MinerStates, MinerFlags>();

        Action<int> setGold;
        Action<int> setEnergy;
        Action<bool> setAlarm;
        Action<Vector3> setObjective;
        Action<List<Node<Vector2>>> setPath;
        Action<Node<Vector2>> setDestinaction;
        Action<Node<Vector2>> setNodeObjective;

        Action<Action, bool> setAlarmOnDelegate;
        Action<Action, bool> setAlarmOffDelegate;

        void SetObjective(Vector3 newObjective) => boid.objective = newObjective;

        fsm.AddBehaviour<IdleState>(MinerStates.Idle, onEnterParametes: () => new object[]
            {
                setAlarmOffDelegate = HandlerToOnStopAlarm,
                setAlarmOnDelegate = HandlerToOnRaisedAlarm,
                setAlarm = value => isAlarmOn = value
                
            }, onTickParametes: () =>
            {
                return new object[]
                {
                    setEnergy = a => energy = a, currentNode, path, this, setPath = list => path = list,
                    setObjective = SetObjective, setNodeObjective = node => currentObjective = node, isAlarmOn,
                    setGold = g => gold = g,
                    transform.position
                };
            },
            onExitParametes: () => new object[]
            {
                setAlarmOffDelegate = HandlerToOnStopAlarm,
                setAlarmOnDelegate = HandlerToOnRaisedAlarm,
            });
        fsm.AddBehaviour<MiningState>(MinerStates.Mining, onTickParametes: () => new object[]
        {
            gold, maxGold, energy, Time.deltaTime, timeBetweenGold,
            setGold = value => gold = value,
            setEnergy = a => energy = a,
            setObjective = SetObjective
        }, onEnterParametes: () => new object[]
        {
            currentNode,
            humanCenterNode,
            setAlarmOnDelegate = HandlerToOnRaisedAlarm,
        }, onExitParametes: () => new object[]
        {
            setAlarmOnDelegate = HandlerToOnRaisedAlarm,
        });
        fsm.AddBehaviour<TravelState>(MinerStates.Travel, onTickParametes: () => new object[]
            {
                this.transform, path, boid.ACS(), boid.speed, chaseDistance,
                setObjective = SetObjective,
                setPath = list => path = list,
                setDestinaction = node => currentNode = node,
            },
            onEnterParametes: () => new object[]
            {
                setObjective = SetObjective,
                path,
                setAlarmOnDelegate = HandlerToOnRaisedAlarm,
                setAlarmOffDelegate = HandlerToOnStopAlarm,
                humanCenterNode,
                this,
                currentObjective
            }, onExitParametes: () => new object[]
            {
                setAlarmOnDelegate = HandlerToOnRaisedAlarm,
                setAlarmOffDelegate = HandlerToOnStopAlarm
            });

        fsm.SetTransition(MinerStates.Travel, MinerFlags.OnStartMining, MinerStates.Mining);
        fsm.SetTransition(MinerStates.Travel, MinerFlags.OnAlarmSound, MinerStates.Travel,
            () =>
            {
                path = PathFinderManager<Node<Vector2>, Vector2>.GetPath(humanCenterNode, currentNode, this);
                path.Reverse();
                SetObjective(path[0].GetCoordinate());
                isAlarmOn = true;
            });
        fsm.SetTransition(MinerStates.Travel, MinerFlags.OnAlarmResume, MinerStates.Travel,
            () =>
            {
                isAlarmOn = false;
                if (currentObjective.GetPlace() is Mine)
                {
                    path = PathFinderManager<Node<Vector2>, Vector2>.GetPath(currentNode, currentObjective, this);
                    //path.Reverse();
                    SetObjective(path[0].GetCoordinate());
                }
            });
        fsm.SetTransition(MinerStates.Travel, MinerFlags.OnWaitingOnCenter, MinerStates.Idle);
        fsm.SetTransition(MinerStates.Idle, MinerFlags.OnStartMining, MinerStates.Mining,
            () => { currentNode = path[^1]; });
        fsm.SetTransition(MinerStates.Mining, MinerFlags.OnGoingToCenter, MinerStates.Travel,
            () =>
            {
                path = PathFinderManager<Node<Vector2>, Vector2>.GetPath(humanCenterNode, currentNode, this);
                path.Reverse();
                // boid.objective = path[^1].GetCoordinate();
            }); 
        fsm.SetTransition(MinerStates.Mining, MinerFlags.OnGoingToMine, MinerStates.Travel,
            () =>
            {
                path = ((HumanCenter2D)humanCenterNode.GetPlace()).GetNewDestination(this,currentNode, transform.position);
                // path = PathFinderManager<Node<Vector2>, Vector2>.GetPath(humanCenterNode, currentNode, this);
                path.Reverse();
                // boid.objective = path[^1].GetCoordinate();
            });
        fsm.SetTransition(MinerStates.Mining, MinerFlags.OnEmptyEnergy, MinerStates.Idle);
        fsm.SetTransition(MinerStates.Mining, MinerFlags.OnAlarmSound, MinerStates.Travel, () =>
        {
            path =
                PathFinderManager<Node<Vector2>, Vector2>.GetPath(humanCenterNode, currentNode, this);
            path.Reverse();
            currentObjective = path[0];
            isAlarmOn = true;
        });
        fsm.SetTransition(MinerStates.Idle, MinerFlags.OnGoingToMine, MinerStates.Travel,
            () =>
            {
                currentNode = path[^1];
                // boid.objective = path[^1].GetCoordinate();
            });
        fsm.SetTransition(MinerStates.Idle, MinerFlags.OnGoingToMine, MinerStates.Travel,
            () =>
            {
                currentNode = path[^1];
                // boid.objective = path[^1].GetCoordinate();
            });

        fsm.ForceState(MinerStates.Idle);
        yield break;

        void HandlerToOnRaisedAlarm(Action actionToAdd, bool value)
        {
            if (value)
            {
                onAlarmRaised += actionToAdd;
            }
            else
            {
                onAlarmRaised -= actionToAdd;
            }
        }

        void HandlerToOnStopAlarm(Action actionToAdd, bool value)
        {
            if (value)
            {
                onAlarmStop += actionToAdd;
            }
            else
            {
                onAlarmStop -= actionToAdd;
            }
        }
    }


    private void Update()
    {
        fsm?.Tick();
    }
    private void LateUpdate()
    {
        for (int j = 0; j < prefabMesh.subMeshCount; j++)
        { 
            drawMatrix.SetTRS(transform.position, transform.rotation, prefab.transform.localScale);
            Graphics.DrawMesh(prefabMesh, drawMatrix, prefabMaterial, 0, null, j);
        }
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, chaseDistance);
            Gizmos.color = Color.blue;
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
        this.grafp = graph;
    }

    public BoidAgent GetBoid()
    {
        return boid;
    }

    public void SetActive(bool value = true)
    {
        gameObject.SetActive(value);
    }

    public void InvokeAlarmOn() => onAlarmRaised.Invoke();

    public void InvokeAlarmOff() => onAlarmStop.Invoke();
}
