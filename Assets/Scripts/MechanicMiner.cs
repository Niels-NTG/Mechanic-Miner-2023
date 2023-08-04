using GeneticSharp.Domain;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using GeneticSharp.Domain.Populations;
using UnityEngine;

public class MechanicMiner : MonoBehaviour
{

    public bool debugLevelMode;

    private void Start()
    {
        if (debugLevelMode)
        {
            GoExplore goExplore = new GoExplore(new SimulationInstance("test1"));
            goExplore.Run();
        }
        else
        {
            RunEvolution();
        }
    }

    private void RunEvolution()
    {
        EliteSelection selection = new EliteSelection();
        UniformCrossover crossover = new UniformCrossover();
        ReverseSequenceMutation mutation = new ReverseSequenceMutation();
        TGMFitness fitness = new TGMFitness();
        TGMChromosome chromosome = new TGMChromosome();
        Population population = new Population(100, 100, chromosome);

        GeneticAlgorithm ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation);
        ga.Termination = new GenerationNumberTermination(10);

        ga.Start();

        Debug.Log($"Best solution found has {ga.BestChromosome.Fitness} fitness");
    }
}
