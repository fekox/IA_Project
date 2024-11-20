using UnityEngine;

public class Tank : TankBase
{
    float fitness = 0;
    protected override void OnReset()
    {
        fitness = 1;
    }

    protected override void OnThink(float dt)
    {
        Vector3 dirToMine = GetDirToMine(nearMine);

        inputs[0] = dirToMine.x;
        inputs[1] = dirToMine.z;
        inputs[2] = transform.forward.x;
        inputs[3] = transform.forward.z;
        inputs[4] = nearMine.IsGoodMine() ? 1f : -1f;

        float[] output = brain.Synapsis(inputs);

        SetForces(output[0], output[1], dt);
    }

    protected override void OnTakeMine(IMinable mine)
    {
        if (mine.IsGoodMine())
        {
            fitness *= FitnessReward * FitnessMultiplyer;
            genome.fitness = fitness;
            FitnessMultiplyer = 1.0f;
        }
        else
        {
            fitness *= 0.1f;
            FitnessMultiplyer = 0.0f;
            genome.fitness = fitness;
        }
        
    }
}