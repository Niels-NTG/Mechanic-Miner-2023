using System;
using System.Collections.Generic;
using System.Linq;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Selections;

public class TGMSelection : RouletteWheelSelection
{
    protected override IList<IChromosome> PerformSelectChromosomes(int number, Generation generation)
    {
        // number = population.MinSize
        int numberOfElites = (int) Math.Round(generation.Chromosomes.Count * 0.1);
        int numberOfNewChromosomes = (int) Math.Round(generation.Chromosomes.Count * 0.1);

        IList<IChromosome> chromosomesSortedByFitness =
            generation.Chromosomes.OrderByDescending(c => c.Fitness).ToList();

        // Take the 10% best chromosomes.
        IList<IChromosome> eliteChromosomes = chromosomesSortedByFitness.Take(numberOfElites).ToList();

        // For the worst 10%, replace it with new chromosomes.
        IList<IChromosome> newChromosomes = new List<IChromosome>(numberOfNewChromosomes);
        for (int i = 0; i < numberOfNewChromosomes; i++)
        {
            newChromosomes.Add(new TGMChromosome());
        }

        // For top 10%-90%, do roulette wheel selection.
        IList<IChromosome> chromosomesForRouletteWheelSelection = chromosomesSortedByFitness.Skip(numberOfElites).Take(
            generation.Chromosomes.Count - (numberOfElites + numberOfNewChromosomes)
        ).ToList();
        IList<IChromosome> selectedByRouletteWheelSelection = base.PerformSelectChromosomes(
            chromosomesForRouletteWheelSelection.Count,
            new Generation(chromosomesForRouletteWheelSelection.Count, chromosomesForRouletteWheelSelection)
        );

        return eliteChromosomes.Concat(selectedByRouletteWheelSelection).Concat(newChromosomes).ToList();
    }
}
