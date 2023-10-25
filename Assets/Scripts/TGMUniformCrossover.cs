using System.Collections.Generic;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Crossovers;

public sealed class TGMUniformCrossover : UniformCrossover
{
    protected override IList<IChromosome> PerformCross(IList<IChromosome> parents)
    {
        IChromosome parent1 = parents[0];
        IChromosome parent2 = parents[1];
        TGMChromosome chromosome1 = (TGMChromosome) parent1.CreateNew();
        TGMChromosome chromosome2 = (TGMChromosome) parent2.CreateNew();
        bool isSameType = chromosome1.isSameType(chromosome2);
        {
            for (int index = 0; index < parent1.Length; index++)
            {
                if (isSameType)
                {
                    chromosome1.ReplaceGene(index, parent2.GetGene(index));
                    chromosome2.ReplaceGene(index, parent1.GetGene(index));
                }
                else
                {
                    chromosome1.ReplaceGene(index, parent1.GetGene(index));
                    chromosome2.ReplaceGene(index, parent2.GetGene(index));
                }
            }
        }
        return new List<IChromosome>
        {
            chromosome1,
            chromosome2
        };
    }
}
