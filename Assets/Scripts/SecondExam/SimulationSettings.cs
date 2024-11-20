using System.Collections.Generic;

public class SimulationSettings
{
    private int currentTurn = 0;
    bool isRunning = false;
    GameSettings gameSettings;
    AgentManager agentManager;

    public SimulationSettings(GameSettings gameSettings)
    {
        this.gameSettings = gameSettings;
        currentTurn = 0;
        isRunning = true;
    }

    public void Tick(float deltaTime)
    {
        if (!isRunning)
            return;
        currentTurn++;
        agentManager.Tick();
        if (currentTurn >= gameSettings.turnCount)
        {
            EndGeneration();
        }
    }

    public void PauseSimulation() => isRunning = !isRunning;

    private void EndGeneration()
    {
        throw new System.NotImplementedException();
    }
}



public class AgentManager
{
    private BrainData brainData;
    
    public int generation { get; private set; }

    public float bestFitness { get; private set; }

    public float avgFitness { get; private set; }

    public float worstFitness { get; private set; }

    #region Fitness

    private float getBestFitness(List<Genome> population)
    {
        float fitness = 0;
        foreach (Genome g in population)
        {
            if (fitness < g.fitness)
                fitness = g.fitness;
        }

        return fitness;
    }

    private float getAvgFitness(List<Genome> population)
    {
        float fitness = 0;
        foreach (Genome g in population)
        {
            fitness += g.fitness;
        }

        return fitness / population.Count;
    }

    private float getWorstFitness(List<Genome> population)
    {
        float fitness = float.MaxValue;
        foreach (Genome g in population)
        {
            if (fitness > g.fitness)
                fitness = g.fitness;
        }

        return fitness;
    }

    #endregion

    public void Tick()
    {
       
    }
}