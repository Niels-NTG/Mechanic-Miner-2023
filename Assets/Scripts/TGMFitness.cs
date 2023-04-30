using System;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Fitnesses;

public class TGMFitness : IFitness
{

    public double Evaluate(IChromosome chromosome)
    {
        // TODO implement game playing agent for evaluation
        
        // This is a nonsensical dummy fitness function. To be replaced with a real one.
        Gene gene = chromosome.GetGene(0);
        ToggleableGameMechanic geneValue = (ToggleableGameMechanic)gene.Value;
        String tgmModifier = geneValue.modifier;
        if (tgmModifier == "double")
        {
            return 1f;
        } 
        if (tgmModifier == "half")
        {
            return 0.5f;
        }
        return 0f;
    }
}
