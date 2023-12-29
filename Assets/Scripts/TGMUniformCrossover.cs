using System.Collections.Generic;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Randomizations;

public sealed class TGMUniformCrossover : UniformCrossover
{
    protected override IList<IChromosome> PerformCross(IList<IChromosome> parents)
    {
        TGMChromosome parent1 = (TGMChromosome) parents[0];
        TGMChromosome parent2 = (TGMChromosome) parents[1];

        Gene[] chromosomeGenes1 = new Gene[parent1.Length];
        Gene[] chromosomeGenes2 = new Gene[parent2.Length];

        // Only do crossover when the parents share the same component type (1) and component field (2).
        bool isSameType =
            parent1.GetGene(1).Value == parent2.GetGene(1).Value &&
            parent1.GetGene(2).Value == parent2.GetGene(2).Value;

        for (int index = 0; index < parent1.Length; index++)
        {
            if (isSameType && RandomizationProvider.Current.GetDouble() > MixProbability)
            {
                chromosomeGenes1[index] = new Gene(parent2.GetGene(index).Value);
                chromosomeGenes2[index] = new Gene(parent1.GetGene(index).Value);
            }
            else
            {
                chromosomeGenes1[index] = new Gene(parent1.GetGene(index).Value);
                chromosomeGenes2[index] = new Gene(parent2.GetGene(index).Value);
            }
        }

        return new List<IChromosome>
        {
            new TGMChromosome(parent1.ID, chromosomeGenes1),
            new TGMChromosome(parent2.ID, chromosomeGenes2)
        };
    }
}
