using IA_Library;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    private Simulation simulation = new Simulation();

    private void OnEnable()
    {
        simulation.StartSimulation();
    }

    private void Update()
    {
        simulation.UpdateSimulation(Time.deltaTime);
    }

    private void OnDisable()
    {
        simulation.EndSimulation();
    }
}