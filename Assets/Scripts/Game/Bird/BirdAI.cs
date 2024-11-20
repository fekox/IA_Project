using System.Collections.Generic;
using UnityEngine;

public class BirdAI : BirdBase
{
    private List<Obstacle> coins = new List<Obstacle>();
    protected override void OnThink(float dt, BirdBehaviour birdBehaviour, Obstacle obstacle, Obstacle coin)
    {
        float[] inputs = new float[4];
        var obstacleDist = (obstacle.transform.position - birdBehaviour.transform.position);
        obstacleDist.Normalize();
        inputs[0] = obstacleDist.x;
        inputs[1] = obstacleDist.y;
        var coinDist = (coin.transform.position - birdBehaviour.transform.position);
        coinDist.Normalize();
        inputs[2] = coinDist.x;
        inputs[3] = coinDist.y;

        float[] outputs;
        outputs = brain.Synapsis(inputs);
        if (outputs[0] < 0.5f)
        {
            birdBehaviour.Flap();
        }


        if (Vector3.Distance(obstacle.transform.position, birdBehaviour.transform.position) <= 1.0f)
        {
            genome.fitness *= 2;
        }

        if (ObstacleManager.Instance.IsCollidingCoin(this.transform.position,out var obstacleCoin) &&! coins.Contains(obstacleCoin))
        {
            genome.fitness *= 1.2f;
            coins.Add(obstacleCoin);
        }

        genome.fitness += (100.0f - Vector3.Distance(obstacle.transform.position, birdBehaviour.transform.position));
        genome.fitness += (25.0f - Vector3.Distance(coin.transform.position, birdBehaviour.transform.position));
    }

    protected override void OnDead()
    {
    }

    protected override void OnReset()
    {
        genome.fitness = 0.0f;
    }
}