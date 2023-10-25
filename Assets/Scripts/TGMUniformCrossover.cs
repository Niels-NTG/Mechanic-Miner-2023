using System.Collections.Generic;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Randomizations;

public sealed class TGMUniformCrossover : UniformCrossover
{
    protected override IList<IChromosome> PerformCross(IList<IChromosome> parents)
    {
        IChromosome parent1 = parents[0];
        IChromosome parent2 = parents[1];
        TGMChromosome chromosome1 = (TGMChromosome) parent1.CreateNew();
        TGMChromosome chromosome2 = (TGMChromosome) parent2.CreateNew();
        for (int index = 0; index < parent1.Length; index++)
        {
            if (RandomizationProvider.Current.GetDouble() < MixProbability)
            {
                chromosome1.ReplaceGene(index, parent1.GetGene(index));
                chromosome2.ReplaceGene(index, parent2.GetGene(index));
            }
            else if (chromosome1.isSameType(chromosome2))
            {
                chromosome1.ReplaceGene(index, parent2.GetGene(index));
                chromosome2.ReplaceGene(index, parent1.GetGene(index));
            }
        }
        return new List<IChromosome>
        {
            chromosome1,
            chromosome2
        };
    }
}
