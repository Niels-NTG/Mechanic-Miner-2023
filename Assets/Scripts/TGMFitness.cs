using System.Threading.Tasks;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Fitnesses;
using UnityEngine;

public class TGMFitness : IFitness
{

    public double Evaluate(IChromosome chromosome)
    {
        Gene gene = chromosome.GetGene(0);

        SimulationInstance env = (SimulationInstance) gene.Value;
        env.ResetPlayer();

        GoExplore goExplore = new GoExplore(env);

        Task<int> goExploreTask = Task.Run(() => goExplore.Run());
        goExploreTask.Start();
        goExploreTask.Wait();
        int simulationResult = goExploreTask.GetAwaiter().GetResult();

        Vector2Int levelEntry = env.entryLocation;
        Vector2Int levelExit = env.exitLocation;
        int levelCellCount = LevelGenerator.levelSize.width * LevelGenerator.levelSize.height;
        double archiveToLevelSizeRatio = 1.0 - (double)simulationResult / levelCellCount;

        double fitnessValue = archiveToLevelSizeRatio;
        return fitnessValue;
    }
}
