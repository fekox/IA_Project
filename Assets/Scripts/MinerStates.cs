using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Miner
{
    public enum MinerFlags
    {
        OnEmptyEnergy,
        OnStartMining,
        OnGoingToMine,
        OnGoingToCenter,
        OnWaitingOnCenter,
        OnAlarmSound,
        OnAlarmResume
    }

    public enum MinerStates
    {
        Idle,
        Mining,
        Travel
    }

    public class IdleState : State
    {
        private IPlace place;
        private bool isAlarmOn = false;

        public override BehaviourActions GetTickBehaviours(params object[] parameters)
        {
            Action<int> energy = parameters[0] as Action<int>;
            place = (parameters[1] as Node<Vector2>).GetPlace();
            List<Node<Vector2>> path = parameters[2] as List<Node<Vector2>>;
            var traveler = parameters[3] as ITraveler;
            Action<List<Node<Vector2>>> modifyPath = parameters[4] as Action<List<Node<Vector2>>>;
            Action<Vector3> setDestination = parameters[5] as Action<Vector3>;
            Action<Node<Vector2>> setCurrentMine = parameters[6] as Action<Node<Vector2>>;
            isAlarmOn = Convert.ToBoolean(parameters[7]);
            var goldToRetrieve = parameters[8] as Action<int>;
            Vector3 currentPos = (Vector3)parameters[9];
            BehaviourActions behaviour = new BehaviourActions();
            behaviour.SetTransitionBehavior(() =>
            {
                if (place is Mine mine)
                {
                    if (isAlarmOn)
                    {
                        OnFlag.Invoke(MinerFlags.OnAlarmSound);
                    }
                    else if (mine.TryGetFood())
                    {
                        int energyToGet = 3;
                        energy?.Invoke(energyToGet);
                        OnFlag.Invoke(MinerFlags.OnStartMining);
                    }
                }
                else if (place is HumanCenter2D humanCenter)
                {
                    if (!isAlarmOn)
                    {
                        path = humanCenter.GetNewDestination(traveler, currentPos);
                        if (path != null)
                        {
                            setDestination(path[0].GetCoordinate());
                            modifyPath.Invoke(path);
                            setCurrentMine.Invoke(path[^1]);
                            goldToRetrieve.Invoke(0);
                            OnFlag.Invoke(MinerFlags.OnGoingToMine);
                        }
                    }
                }
            });
            return behaviour;
        }

        public override BehaviourActions GetEnterBehaviours(params object[] parameters)
        {
            var onAlarmStop = parameters[0] as Action<Action, bool>;
            var onAlarmResume = parameters[1] as Action<Action, bool>;
            changeAlarmState = parameters[2] as Action<bool>;
            onAlarmStop.Invoke(OnAlarmStoped, true);
            onAlarmResume.Invoke(OnAlarmRaise, true);

            return default;
        }

        Action<bool> changeAlarmState;

        public override BehaviourActions GetExitBehaviours(params object[] parameters)
        {
            var onAlarmStop = parameters[0] as Action<Action, bool>;
            var onAlarmResume = parameters[1] as Action<Action, bool>;
            onAlarmStop.Invoke(OnAlarmStoped, false);
            onAlarmResume.Invoke(OnAlarmRaise, false);
            return default;
        }

        private void OnAlarmStoped()
        {
            isAlarmOn = false;
            changeAlarmState.Invoke(isAlarmOn);
        }

        private void OnAlarmRaise()
        {
            isAlarmOn = false;
            changeAlarmState.Invoke(isAlarmOn);
        }
    }

    public class MiningState : State
    {
        private int gold;
        private int currentGold = 0;
        private int energy;
        private float timer = 0;
        private Mine mine;
        private Node<Vector2> humanCenter;

        public override BehaviourActions GetTickBehaviours(params object[] parameters)
        {
            gold = (int)parameters[0];
            int maxGold = (int)parameters[1];
            energy = (int)parameters[2];
            float deltaTime = (float)parameters[3];
            float timeBetweenGold = (float)parameters[4];
            Action<int> setGold = parameters[5] as Action<int>;
            Action<int> setEnergy = parameters[6] as Action<int>;


            BehaviourActions behaviour = new BehaviourActions();
            behaviour.AddMultiThreadBehaviour(0, () =>
            {
                timer += deltaTime;
                if (timer > timeBetweenGold && energy > 0 && mine.TryGetGold())
                {
                    timer -= timeBetweenGold;
                    gold++;
                    setGold.Invoke(gold);
                    currentGold++;
                    if (currentGold == 3)
                    {
                        energy--;
                        setEnergy.Invoke(energy);
                        currentGold = 0;
                    }
                }
            });

            behaviour.SetTransitionBehavior(() =>
            {
                if (gold >= maxGold)
                {
                    OnFlag.Invoke(MinerFlags.OnGoingToCenter);
                }
                else if (!mine.hasGold)
                {
                    mine.TryGetGold();
                    OnFlag.Invoke(MinerFlags.OnGoingToMine);
                }
                else if (energy <= 0)
                {
                    OnFlag.Invoke(MinerFlags.OnEmptyEnergy);
                }
            });
            return behaviour;
        }

        void OnAlarmRaised()
        {
            OnFlag.Invoke(MinerFlags.OnAlarmSound);
            // List<Node<Vector2>> nodes =
            //     PathFinderManager<Node<Vector2>, Vector2>.GetPath(humanCenter, path[pathCounter], _traveler);
            // nodes.Reverse();
            // modifyPath.Invoke(nodes);
            // pathCounter = 0;
            // setDestination.Invoke(nodes[pathCounter].GetCoordinate());
        }

        public override BehaviourActions GetEnterBehaviours(params object[] parameters)
        {
            mine = (parameters[0] as Node<Vector2>).GetPlace() as Mine;
            humanCenter = parameters[1] as Node<Vector2>;
            var onAlarmRaised = parameters[2] as Action<Action, bool>;
            onAlarmRaised.Invoke(OnAlarmRaised, true);
            return default;
        }

        public override BehaviourActions GetExitBehaviours(params object[] parameters)
        {
            var onAlarmRaised = parameters[0] as Action<Action, bool>;
            onAlarmRaised.Invoke(OnAlarmRaised, false);
            return default;
        }
    }

    public class IdleFoodState : State
    {
        private int food;
        private bool isAlarmOn = false;
        private Action<int> setFood;
        private float timer = 0;
        private IPlace place;


        public override BehaviourActions GetTickBehaviours(params object[] parameters)
        {
            food = (int)parameters[0];
            setFood = parameters[1] as Action<int>;
            isAlarmOn = (bool)parameters[2];

            BehaviourActions behaviour = new BehaviourActions();
            behaviour.AddMultiThreadBehaviour(0, () =>
            {
                if (place is Mine mine)
                {
                    if (!mine.hasFood)
                    {
                        mine.SetFood(food);
                        setFood.Invoke(0);
                    }
                }
            });

            behaviour.SetTransitionBehavior(() =>
            {
                if (place is Mine mine)
                {
                    if (isAlarmOn)
                    {
                        OnFlag.Invoke(MinerFlags.OnAlarmSound);
                    }
                    else if (mine.hasFood)
                    {
                        OnFlag.Invoke(MinerFlags.OnGoingToCenter);
                    }
                }
                else if (place is HumanCenter<Node<Vector2>, Vector2> humanCenter)
                {
                    if (!isAlarmOn)
                    {
                        OnFlag.Invoke(MinerFlags.OnGoingToMine);
                    }
                }
            });
            return behaviour;
        }

        Action<bool> changeAlarmState;

        public override BehaviourActions GetEnterBehaviours(params object[] parameters)
        {
            if (parameters[0] is Node<Vector2> placeProvider)
            {
                place = placeProvider.GetPlace();
            }
            else
            {
                throw new ArgumentNullException(nameof(parameters), "First param must be a place");
            }

            place = (parameters[0] as Node<Vector2>).GetPlace();
            var onAlarmStop = parameters[1] as Action<Action, bool>;
            var onAlarmResume = parameters[2] as Action<Action, bool>;
            changeAlarmState = parameters[3] as Action<bool>;
            onAlarmStop.Invoke(OnAlarmStoped, true);
            onAlarmResume.Invoke(OnAlarmRaise, true);
            return default;
        }

        private void OnAlarmStoped()
        {
            isAlarmOn = false;
            changeAlarmState.Invoke(isAlarmOn);
        }

        private void OnAlarmRaise()
        {
            isAlarmOn = true;
            changeAlarmState.Invoke(isAlarmOn);
        }

        public override BehaviourActions GetExitBehaviours(params object[] parameters)
        {
            var onAlarmStop = parameters[0] as Action<Action, bool>;
            var onAlarmResume = parameters[1] as Action<Action, bool>;
            onAlarmStop.Invoke(OnAlarmStoped, true);
            onAlarmResume.Invoke(OnAlarmRaise, true);
            return default;
        }
    }

    public class TravelState : State
    {
        private int pathCounter = 0;
        Action<List<Node<Vector2>>> modifyPath;
        List<Node<Vector2>> path;
        Node<Vector2> humanCenter;
        Node<Vector2> currentObjective;
        private ITraveler _traveler;

        public override BehaviourActions GetTickBehaviours(params object[] parameters)
        {
            Transform OwnerTransform = parameters[0] as Transform;
            path = parameters[1] as List<Node<Vector2>>;
            Vector3 direction = (Vector3)parameters[2];
            float speed = Convert.ToSingle(parameters[3]);
            float distanceToNode = Convert.ToSingle(parameters[4]);
            Action<Vector3> setDestination = parameters[5] as Action<Vector3>;
            modifyPath = parameters[6] as Action<List<Node<Vector2>>>;
            Action<Node<Vector2>> changeCurrentNode = parameters[7] as Action<Node<Vector2>>;


            BehaviourActions behaviour = new BehaviourActions();


            Vector3 ownerTransformPosition = OwnerTransform.position;
            Vector3 actualTargetPosition = path[pathCounter].GetCoordinate();
            actualTargetPosition.z = ownerTransformPosition.z;
            behaviour.AddMultiThreadBehaviour(0, CalculateDistance());


            behaviour.AddMainThreadBehaviour(1, Move
            );
            behaviour.SetTransitionBehavior(() =>
            {
                if (Vector3.Distance(ownerTransformPosition, path[^1].GetCoordinate()) < distanceToNode)
                {
                    if (path[^1].GetPlace() is Mine)
                    {
                        OnFlag.Invoke(MinerFlags.OnStartMining);
                    }

                    if (path[^1].GetPlace() is HumanCenter2D)
                    {
                        OnFlag.Invoke(MinerFlags.OnWaitingOnCenter);
                    }
                }
            });
            return behaviour;

            Action CalculateDistance()
            {
                return () =>
                {
                    if (Vector3.Distance(ownerTransformPosition, actualTargetPosition) < distanceToNode &&
                        pathCounter + 1 < path.Count)
                    {
                        pathCounter++;
                        setDestination.Invoke(path[pathCounter].GetCoordinate());
                        changeCurrentNode.Invoke(path[pathCounter]);
                    }
                };
            }

            void Move()
            {
                OwnerTransform.right = direction;
                OwnerTransform.position += direction * speed * Time.deltaTime;
            }
        }

        Action<Vector3> setDestination;

        public override BehaviourActions GetEnterBehaviours(params object[] parameters)
        {
            setDestination = parameters[0] as Action<Vector3>;
            List<Node<Vector2>> path = parameters[1] as List<Node<Vector2>>;
            var onAlarmRaised = parameters[2] as Action<Action, bool>;
            var onAlarmStopped = parameters[3] as Action<Action, bool>;
            humanCenter = parameters[4] as Node<Vector2>;
            _traveler = parameters[5] as ITraveler;
            currentObjective = parameters[6] as Node<Vector2>;
            onAlarmStopped.Invoke(OnAlarmStop, true);
            onAlarmRaised.Invoke(OnAlarmRaised, true);
            pathCounter = 0;
            setDestination.Invoke(path[pathCounter].GetCoordinate());
            return default;
        }

        void OnAlarmRaised()
        {
            OnFlag.Invoke(MinerFlags.OnAlarmSound);
        }

        void OnAlarmStop()
        {
            OnFlag.Invoke(MinerFlags.OnAlarmResume);
        }

        public override BehaviourActions GetExitBehaviours(params object[] parameters)
        {
            var onAlarmRaised = parameters[0] as Action<Action, bool>;
            var onAlarmStopped = parameters[1] as Action<Action, bool>;
            onAlarmStopped.Invoke(OnAlarmStop, false);
            onAlarmRaised.Invoke(OnAlarmRaised, false);
            return default;
        }
    }
}