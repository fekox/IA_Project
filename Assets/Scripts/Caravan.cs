using System;
using System.Collections;
using System.Collections.Generic;
using Miner;
using UnityEngine;

public class Caravan : MonoBehaviour, ITraveler, IFlock,IAlarmable
{
    private FSM<MinerStates, MinerFlags> fsm;

    [SerializeField] private float chaseDistance = 0.2f;

    private int food = 0;
    public BoidAgent boid;
    private bool isAlarmOn = false;

    public List<Node<Vector2>> path;
    private Node<Vector2> currentNode;
    private Node<Vector2> currentMine;
    private Node<Vector2> currentObjective;
    private Node<Vector2> humanCenterNode;


    public Action onAlarmRaised = () => { };
    public Action onAlarmStop = () => { };


    private Node<Vector2> startNode;
    private Coroutine startPathFinding;

    public GrapfView grafp;
    private AStarPathfinder<Node<Vector2>, Vector2> Pathfinder =
        new AStarPathfinder<Node<Vector2>, Vector2>();


    private void OnEnable()
    {
        if (startPathFinding != null)
        {
            StopCoroutine(startPathFinding);
        }

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
        boid.objective = grafp.graph.nodes[^1].GetCoordinate();
        PathFinderManager<Node<Vector2>, Vector2>.graph = grafp.graph;
        //path = Pathfinder.FindPath(startNode, destinationNode, grafp.graph, this);

        currentNode = startNode;
        fsm = new FSM<MinerStates, MinerFlags>();

        Action<int> setFood;
        Action<bool> setAlarm;
        Action<Vector3> setObjective;
        Action<List<Node<Vector2>>> setPath;
        Action<Node<Vector2>> setDestinaction;

        Action<Action, bool> setAlarmOnDelegate;
        Action<Action, bool> setAlarmOffDelegate;

        void SetObjective(Vector3 newObjective) => boid.objective = newObjective;

        fsm.AddBehaviour<IdleFoodState>(MinerStates.Idle, onEnterParametes: () => new object[]
            {
                currentNode,
                setAlarmOffDelegate = HandlerToOnStopAlarm,
                setAlarmOnDelegate = HandlerToOnRaisedAlarm,
                setAlarm = value => isAlarmOn = value
            }, onTickParametes: () =>
            {
                return new object[]
                {
                    food, setFood = g => food = g, isAlarmOn
                };
            },
            onExitParametes: () => new object[]
            {
                setAlarmOffDelegate = HandlerToOnStopAlarm,
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
                    SetObjective(path[0].GetCoordinate());
                }
            });
        fsm.SetTransition(MinerStates.Travel, MinerFlags.OnWaitingOnCenter, MinerStates.Idle);
        fsm.SetTransition(MinerStates.Idle, MinerFlags.OnAlarmSound, MinerStates.Travel, () =>
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
                var previousPath = new List<Node<Vector2>>(path);
                path = (humanCenterNode.GetPlace() as HumanCenter2D).GetNewDestination(this,transform.position);
                if (path == null)
                {
                   path= previousPath;
                }
                currentNode = path[^1];
                SetObjective(path[0].GetCoordinate());
                food = 10;
                currentObjective = path[^1];
            }); fsm.SetTransition(MinerStates.Idle, MinerFlags.OnGoingToCenter, MinerStates.Travel,
            () =>
            {
                path = PathFinderManager<Node<Vector2>, Vector2>.GetPath(humanCenterNode, currentNode, this);
                path.Reverse();
                currentNode = path[^1];
                SetObjective(path[0].GetCoordinate());
                food = 10;
                currentObjective = path[^1];
            });
        fsm.SetTransition(MinerStates.Travel, MinerFlags.OnStartMining, MinerStates.Idle);

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
        this.grafp = graph;
    }

    public BoidAgent GetBoid() => boid;
    public void SetActive(bool value = true)
    {
        gameObject.SetActive(value);
    }

    public void InvokeAlarmOn() => onAlarmRaised.Invoke();

    public void InvokeAlarmOff() => onAlarmStop.Invoke();
}