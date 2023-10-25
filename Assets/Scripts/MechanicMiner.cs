using System;
using System.Globalization;
using System.IO;
using System.Threading;
using GeneticSharp.Domain;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Infrastructure.Framework.Threading;
using UnityEngine;
using CsvHelper;

// TODO I'm using GeneticSharp 2.6.0, consider upgrading to 3.1.4 https://github.com/giacomelli/GeneticSharp/releases

public class MechanicMiner : MonoBehaviour
{

    public bool debugLevelMode;

    [Range(0, 4)] public int levelIndex;

    public int populationSize = 100;
    public int maxGenerationCount = 15;

    [Header("Set to 0 to use random seed. Only applied if Level index is set to 0")]
    public int levelGeneratorSeed;

    [Header("Seed to generate Toggleable Game Mechanics (TGM) with. Set to 0 to use random seed")]
    public int tgmGeneratorSeed;

    private Thread evolutionThread;
    private GeneticAlgorithm ga;

    private void Start()
    {
        if (debugLevelMode)
        {
            String debugID = Guid.NewGuid().ToString();
            SimulationInstance simulationInstance = new SimulationInstance(debugID, levelIndex, levelGeneratorSeed, tgmGeneratorSeed);
            simulationInstance.tgm.GenerateNew();
            simulationInstance.ApplyTGM();
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
        TGMUniformCrossover crossover = new TGMUniformCrossover();
        UniformMutation mutation = new UniformMutation(mutableGenesIndexes: new[] {2, 3});
        TGMFitness fitness = new TGMFitness();

        TGMChromosome.levelIndex = levelIndex;
        TGMChromosome.levelGeneratorSeed = levelGeneratorSeed;
        TGMChromosome.tgmGeneratorSeed = tgmGeneratorSeed;
        TGMChromosome chromosome = new TGMChromosome(true);

        Population population = new Population(populationSize, populationSize, chromosome);

        ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation)
        {
            Termination = new GenerationNumberTermination(maxGenerationCount),
            TaskExecutor = new ParallelTaskExecutor
            {
                MinThreads = 16,
                MaxThreads = 100
            }
        };
        ga.GenerationRan += delegate
        {
            TGMChromosome bestChromosome = (TGMChromosome) ga.BestChromosome;
            Debug.Log($"GENERATION {ga.GenerationsNumber} - BEST GENE {bestChromosome}");

            // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
            foreach (TGMChromosome currentGenerationChromosome in ga.Population.CurrentGeneration.Chromosomes)
            {
                csvWriter.WriteRecord(new GeneticAlgorithmLogRow{
                    generation = ga.GenerationsNumber,
                    id = currentGenerationChromosome.ID,
                    fitness = currentGenerationChromosome.Fitness ?? 0.0,
                    gameObject = currentGenerationChromosome.GetGene(0).Value.ToString(),
                    component = currentGenerationChromosome.GetGene(1).Value.ToString(),
                    componentField = currentGenerationChromosome.GetGene(2).Value.ToString(),
                    modifier = currentGenerationChromosome.GetGene(3).Value.ToString()
                });
                csvWriter.NextRecord();
            }
            csvWriter.Flush();
        };
        evolutionThread = new Thread(() =>
        {
            ga.Start();

            TGMChromosome bestChromosome = (TGMChromosome) ga.BestChromosome;
            Debug.Log($"END genetic algorithm after {ga.GenerationsNumber} - BEST GENE {bestChromosome}");
            if (!ga.IsRunning)
            {
                evolutionThread.Abort();
            }
            csvWriter.Flush();
        });
        evolutionThread.Start();
    }

}
