using System;
using System.Globalization;
using System.IO;
using System.Threading;
using GeneticSharp.Domain;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Infrastructure.Framework.Threading;
using UnityEngine;
using CsvHelper;

public class MechanicMiner : MonoBehaviour
{

    public bool debugLevelMode;

    [Range(0, 3)] public int levelIndex;

    public int populationSize = 10;
    public int maxGenerationCount = 10;

    private Thread evolutionThread;
    private GeneticAlgorithm ga;

    private void Start()
    {
        if (debugLevelMode)
        {
            ToggleableGameMechanic.ToggleGameMechanicGenotype emptyGene = new ToggleableGameMechanic.ToggleGameMechanicGenotype();
            String debugID = Guid.NewGuid().ToString();
            SimulationInstance simulationInstance = new SimulationInstance(debugID, levelIndex);
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
        StreamWriter writer = new StreamWriter($"Logs/GA log {DateTime.Now:yyyy-MM-dd-T-HH-mm-ss}.csv");
        CsvWriter csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csvWriter.WriteHeader<GeneticAlgorithmLogRow>();
        csvWriter.NextRecord();

        EliteSelection selection = new EliteSelection();
        UniformCrossover crossover = new UniformCrossover();
        ReverseSequenceMutation mutation = new ReverseSequenceMutation();
        TGMFitness fitness = new TGMFitness();
        TGMChromosome.levelIndex = levelIndex;
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

            // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
            foreach (TGMChromosome currentGenerationChromosome in ga.Population.CurrentGeneration.Chromosomes)
            {
                csvWriter.WriteRecord(new GeneticAlgorithmLogRow
                {
                    generation = ga.GenerationsNumber,
                    id = currentGenerationChromosome.ID,
                    fitness = currentGenerationChromosome.Fitness ?? 0.0,
                    gameObject = currentGenerationChromosome.GetGene(0).Value as String,
                    component = currentGenerationChromosome.GetGene(1).Value as String,
                    componentField = currentGenerationChromosome.GetGene(2).Value as String,
                    modifier = currentGenerationChromosome.GetGene(3).Value as String
                });
                csvWriter.NextRecord();
            }
            csvWriter.Flush();
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
            csvWriter.Flush();
        });
        evolutionThread.Start();
    }

}
