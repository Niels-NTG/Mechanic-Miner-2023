using System.Threading;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Fitnesses;
using UnityEngine;

public class TGMFitness : IFitness
{

    public double Evaluate(IChromosome chromosome)
    {
        Gene gene = chromosome.GetGene(0);



        object simulationResult = null;
        Thread simulationThread = new Thread(() =>
        {
            GoExplore goExplore = new GoExplore((SimulationInstance) gene.Value);
            simulationResult = goExplore.Run();
            Debug.Log($"{goExplore.ID}: Go explore terminated in TGMFitness with {goExplore.iteration} iterations");
        });
        simulationThread.Start();
        simulationThread.Join();
        Debug.Log($"simulation thread joined with result {simulationResult}");

        double fitnessValue = (bool) simulationResult ? 1.0 : 0.0;
        return fitnessValue;
    }
}
