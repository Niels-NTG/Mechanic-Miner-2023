using System.Threading.Tasks;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Fitnesses;
using UnityEngine;

public class TGMFitness : IFitness
{

    public double Evaluate(IChromosome chromosome)
    {
        // TODO implement game playing agent for evaluation

        Gene gene = chromosome.GetGene(0);
        GoExplore goExplore = new GoExplore((SimulationInstance)gene.Value);

        Task<bool> task = Task.Run(goExplore.Run);
        task.Wait();
        bool goExploreResult = task.GetAwaiter().GetResult();
        Debug.Log($"{goExplore.ID}: Go explore terminated in TGMFitness with {goExplore.iteration}");

        double fitnessValue = goExploreResult ? 1.0 : 0.0;
        return fitnessValue;
    }
}
