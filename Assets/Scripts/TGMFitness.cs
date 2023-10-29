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

        Debug.Log($"{tgmChromosome.ID} TGMFitness: START FITNESS EVALUATION");
        double fitnessValue = RunSimulation(tgmChromosome.simulationInstance).GetAwaiter().GetResult();
        Debug.Log($"{tgmChromosome.ID} TGMFitness: FITNESS {fitnessValue}");
        return fitnessValue;
    }

    private async Task<double> RunSimulation(SimulationInstance simulationInstance)
    {
        await Awaitable.BackgroundThreadAsync();
        GoExplore goExplore = new GoExplore(simulationInstance);
        int goExploreCellCount = goExplore.Run();
        int levelInnerCellCount = (Level.levelSize.width - 1) * (Level.levelSize.height - 1);

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
