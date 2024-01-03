using System;
using System.Threading.Tasks;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Fitnesses;
using UnityEngine;

public class TGMFitness : IFitness
{

    public double Evaluate(IChromosome chromosome)
    {
        TGMChromosome tgmChromosome = chromosome as TGMChromosome;
        if (tgmChromosome == null)
        {
            return 0.0;
        }

        double fitnessValue = RunSimulation(tgmChromosome).GetAwaiter().GetResult();
        Debug.Log($"{tgmChromosome}{fitnessValue}");
        return fitnessValue;
    }

    private async Task<double> RunSimulation(TGMChromosome tgmChromosome)
    {
        SimulationInstance simulationInstance = tgmChromosome.simulationInstance;
        await Awaitable.BackgroundThreadAsync();
        GoExplore goExplore = new GoExplore(simulationInstance);
        GoExplore.GoExploreResult goExploreResult = goExplore.Run();
        tgmChromosome.goExploreResult = goExploreResult;

        int goExploreCellCount = goExploreResult.isTerminal ? goExploreResult.archive.Length : int.MinValue;
        int levelInnerCellCount = Level.levelSize.width * Level.levelSize.height;

        // Reward population members that explore a larger part of the level
        double archiveToLevelSizeRation = Math.Clamp(
            (double) goExploreCellCount / levelInnerCellCount,
            // Do not use 0 for minimum value, since this can break RouletteWheelSelection
            Mathf.Epsilon, 1.0
        );

        double fitnessValue = archiveToLevelSizeRation;
        return fitnessValue;
    }
}
