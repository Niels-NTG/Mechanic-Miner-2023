using System.Threading.Tasks;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Fitnesses;

public class TGMFitness : IFitness
{

    public double Evaluate(IChromosome chromosome)
    {
        Gene gene = chromosome.GetGene(0);

        GoExplore goExplore = new GoExplore((SimulationInstance) gene.Value);

        Task<bool> goExploreTask = Task.Run(goExplore.Run);
        goExploreTask.Start();
        goExploreTask.Wait();
        bool simulationResult = goExploreTask.GetAwaiter().GetResult();

        double fitnessValue = simulationResult ? 1.0 : 0.0;
        return fitnessValue;
    }
}
