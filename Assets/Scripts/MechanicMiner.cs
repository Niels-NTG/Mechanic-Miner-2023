using System;
using System.Threading;
using GeneticSharp.Domain;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Infrastructure.Framework.Threading;
using UnityEngine;

public class MechanicMiner : MonoBehaviour
{

    public bool debugLevelMode;

    private GeneticAlgorithm ga;

    private void Start()
    {
        if (debugLevelMode)
        {
            GoExplore goExplore = new GoExplore(new SimulationInstance(Guid.NewGuid().ToString()));
            Thread debugThread = new Thread(() =>
            {
                goExplore.Run();
            });
            debugThread.Start();
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
        Population population = new Population(10, 10, chromosome);
        ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation);
        ga.Termination = new GenerationNumberTermination(10);
        ga.TaskExecutor = new ParallelTaskExecutor
        {
            MinThreads = 100,
            MaxThreads = 200
        };
        ga.Start();
        Debug.Log($"Best solution found has {ga.BestChromosome.Fitness} fitness");
    }

    private void OnDestroy()
    {
        if (ga != null)
        {
            ga.Stop();
        }
    }
}
