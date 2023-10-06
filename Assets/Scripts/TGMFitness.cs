using System.Threading.Tasks;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Fitnesses;
using UnityEngine;

public class TGMFitness : IFitness
{

    public double Evaluate(IChromosome chromosome)
    {
        Gene gene = chromosome.GetGene(0);

        TGMChromosome.GeneStruct geneStruct = (TGMChromosome.GeneStruct) gene.Value;

        Debug.Log($"{geneStruct.ID} TGMFitness: START FITNESS EVALUATION {geneStruct.ID}");
        double fitnessValue = RunSimulation(geneStruct).GetAwaiter().GetResult();
        Debug.Log($"{geneStruct.ID} TGMFitness: FITNESS {fitnessValue}");
        return fitnessValue;
    }

    private async Task<double> RunSimulation(TGMChromosome.GeneStruct geneStruct)
    {
        await Awaitable.BackgroundThreadAsync();
        GoExplore goExplore = new GoExplore(geneStruct.simulationInstance);
        int goExploreCellCount = goExplore.Run();
        int levelCellCount = LevelGenerator.levelSize.width * LevelGenerator.levelSize.height;
        double archiveToLevelSizeRation = 1.0 - (double) goExploreCellCount / levelCellCount;

        double fitnessValue = archiveToLevelSizeRation;
        return fitnessValue;
    }
}
