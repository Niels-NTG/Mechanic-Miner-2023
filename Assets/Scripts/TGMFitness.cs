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
        int levelCellCount = LevelGenerator.levelSize.width * LevelGenerator.levelSize.height;
        double archiveToLevelSizeRation = Math.Clamp(1.0 - (double) goExploreCellCount / levelCellCount, 0.0, 1.0);

        double fitnessValue = archiveToLevelSizeRation;
        return fitnessValue;
    }
}
