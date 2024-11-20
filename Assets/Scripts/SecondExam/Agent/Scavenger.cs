using System;
using System.Collections.Generic;
using System.Numerics;
using Miner.SecondExam.Agent;

public enum ScavengerStates
{
    Move
}

public enum ScavengerFlags
{
    ToMove
}

public sealed class ScavengerMoveState : SporeMoveState
{
    private float MinEatRadius;
    private float counter;
    private int movesPerTurn = 2;

    public override BehaviourActions GetTickBehaviours(params object[] parameters)
    {
        BehaviourActions behaviour = new BehaviourActions();

        float[] outputs = parameters[0] as float[];
        position = (Vector2)(parameters[1]);
        Vector2 nearFoodPos = (Vector2)parameters[2];
        MinEatRadius = (float)(parameters[3]);
        bool hasEatenFood = (bool)parameters[4];
        Herbivore herbivore = parameters[5] as Herbivore;
        var onMove = parameters[6] as Action<Vector2[]>;
        behaviour.AddMultiThreadBehaviour(0, () =>
        {
            List<Vector2> newPositions = new List<Vector2> { nearFoodPos };
            float distanceFromFood = GetDistanceFrom(newPositions);

            if (distanceFromFood < MinEatRadius && !hasEatenFood)
            {
                counter++;

                brain.FitnessReward += 1;

                if (counter >= 20)
                {
                    brain.FitnessReward += 20;
                    brain.FitnessMultiplier += 0.10f;
                    hasEatenFood = true;
                }
            }
            else if (distanceFromFood > MinEatRadius)
            {
                brain.FitnessMultiplier -= 0.05f;
            }

            Vector2[] direction = new Vector2[movesPerTurn];
            for (int i = 0; i < direction.Length; i++)
            {
                direction[i] = GetDir(outputs[i]);
            }

            foreach (Vector2 dir in direction)
            {
                onMove.Invoke(direction);
            }
        });


        return behaviour;
    }

    public override BehaviourActions GetEnterBehaviours(params object[] parameters)
    {
        brain = parameters[0] as Brain;
        position = (Vector2)(parameters[1]);
        MinEatRadius = (float)(parameters[2]);
        positiveHalf = Neuron.Sigmoid(0.5f, brain.p);
        negativeHalf = Neuron.Sigmoid(-0.5f, brain.p);

        return default;
    }

    public override BehaviourActions GetExitBehaviours(params object[] parameters)
    {
        return default;
    }
}


public class Scavenger : SporeAgent<ScavengerStates, ScavengerFlags>
{
    public Brain flockingBrain;
    float minEatRadius;
    protected Vector2 dir;
    protected float speed;

    public Scavenger(SporeManager populationManager) : base(populationManager) 
    {
        mainBrain = new Brain();
        minEatRadius = 4f;
        
        Action<Vector2> setDir;
        fsm.AddBehaviour<ScavengerMoveState>(ScavengerStates.Move,
            onEnterParametes: () => { return new object[] { mainBrain, position, minEatRadius }; },
            onTickParametes: () =>
            {
                return new object[]
                {
                    mainBrain.outputs, position, minEatRadius, GetNearFoodPos(), hasEaten,GetNearHerbivore(),
                    setDir = MoveTo,
                };
            });

        fsm.ForceState(ScavengerStates.Move);
    }

    public override void DecideState(float[] outputs)
    {

    }

    public override void PreUpdate(float deltaTime)
    {
        var nearFoodPos = GetNearFoodPos();
        mainBrain.inputs = new[] { position.X, position.Y, minEatRadius, nearFoodPos.X, nearFoodPos.Y};
        //Todo: preguntarle a lean
        flockingBrain.inputs = new[] { 0.0f,};
    }

    public override void Update(float deltaTime)
    {
        fsm.Tick();
        Move(deltaTime);
    }

    private void Move(float deltaTime)
    {
        position += dir * speed * deltaTime;
    }

    public Vector2 GetNearFoodPos()
    {
        return GetNearHerbivore().position;
    }

    public Herbivore GetNearHerbivore()
    {
        return populationManager.GetNearHerbivore(position);
    }

    public override void MoveTo(Vector2 dir)
    {
        this.dir = dir;
    }
}