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

    private Thread evolutionThread;
    private GeneticAlgorithm ga;
    private readonly int populationSize = 10;
    private readonly int maxGenerationCount = 10;

    private void Start()
    {
        if (debugLevelMode)
        {
            ToggleableGameMechanic.ToggleGameMechanicGenotype emptyGene = new ToggleableGameMechanic.ToggleGameMechanicGenotype();
            String debugID = Guid.NewGuid().ToString();
            SimulationInstance simulationInstance = new SimulationInstance(debugID);
            simulationInstance.SetTGM(emptyGene);
            GoExplore goExplore = new GoExplore(simulationInstance);
            Thread debugThread = new Thread(() =>
            {
                int cellCount = goExplore.Run();
                Debug.Log($"DEBUG MODE: {debugID} finished after visiting {cellCount} cells");
            });
            debugThread.Start();
        }
        else
        {
            RunEvolution();
        }
    }

    private void OnDestroy()
    {
        if (ga != null)
        {
            ga.Stop();
            evolutionThread.Abort();
        }
    }

    private void RunEvolution()
    {
        EliteSelection selection = new EliteSelection();
        UniformCrossover crossover = new UniformCrossover();
        ReverseSequenceMutation mutation = new ReverseSequenceMutation();
        TGMFitness fitness = new TGMFitness();
        TGMChromosome chromosome = new TGMChromosome(true);
        Population population = new Population(populationSize, populationSize, chromosome);
        ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation)
        {
            Termination = new GenerationNumberTermination(maxGenerationCount),
            TaskExecutor = new ParallelTaskExecutor
            {
                MinThreads = 100,
                MaxThreads = 200
            }
        };
        ga.GenerationRan += delegate
        {
            TGMChromosome bestChromosome = (TGMChromosome) ga.BestChromosome;
            Debug.Log($"GENERATION {ga.GenerationsNumber} - BEST GENE {bestChromosome.ID} ({bestChromosome.genotype}) with a fitness of {bestChromosome.Fitness}");

        };
        evolutionThread = new Thread(() =>
        {
            ga.Start();

            TGMChromosome bestChromosome = (TGMChromosome) ga.BestChromosome;
            Debug.Log($"END genetic algorithm after {ga.GenerationsNumber} - BEST GENE {bestChromosome.ID} ({bestChromosome.genotype}) with a fitness of {bestChromosome.Fitness}");
            if (!ga.IsRunning)
            {
                evolutionThread.Abort();
            }
        });
        evolutionThread.Start();
    }

}
