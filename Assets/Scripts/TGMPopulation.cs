using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Populations;

public class TGMPopulation : Population
{
    public TGMPopulation(int minSize, int maxSize, IChromosome adamChromosome) : base(minSize, maxSize, adamChromosome)
    {
    }

    public override void EndCurrentGeneration()
    {
        base.EndCurrentGeneration();
        foreach (var chromosome in CurrentGeneration.Chromosomes)
        {
            TGMChromosome tgmChromosome = (TGMChromosome) chromosome;
            tgmChromosome.simulationInstance.UnloadScene().GetAwaiter().GetResult();
        }
    }
}
