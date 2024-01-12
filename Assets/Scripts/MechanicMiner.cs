using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GeneticSharp.Domain;
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

    public int populationSize = 100;
    public int maxGenerationCount = 15;

    [Header("Set to 0 to use random seed. Only applied if Level index is set to 0")]
    public int levelGeneratorSeed;

    [Header("Seed to generate Toggleable Game Mechanics (TGM) with. Set to 0 to use random seed")]
    public int tgmGeneratorSeed;

    public List<int> levelIndexList = new List<int> {3, 4, 5};
    private GeneticAlgorithm ga;

    private void Start()
    {
        if (levelIndexList.Count == 0)
        {
            Debug.LogError("No levels where selected. Stopping…");
            Application.Quit();
            return;
        }

        if (debugLevelMode)
        {
            String debugID = Guid.NewGuid().ToString();
            SimulationInstance simulationInstance = new SimulationInstance(debugID, levelIndexList.First(), levelGeneratorSeed, tgmGeneratorSeed);
            simulationInstance.tgm.GenerateNew();
            simulationInstance.ApplyTGM();
            GoExplore goExplore = new GoExplore(simulationInstance);
            Task.Run(() =>
            {
                Debug.Log(simulationInstance.tgm);
                goExplore.Run();
            });
        }
        else
        {
            RunEvolution(levelIndexList);
        }
    }

    private void OnDestroy()
    {
        if (ga != null)
        {
            ga.Stop();
        }
    }

    private async void RunEvolution(List<int> _levelList)
    {
        if (_levelList.Count == 0)
        {
            return;
        }
        int levelIndex = _levelList.First();
        _levelList.RemoveAt(0);

        Debug.Log($"START genetic algorithm for level {levelIndex}, with a population of {populationSize}");

        StreamWriter writer = new StreamWriter($"Logs/GA log {DateTime.Now:yyyy-MM-dd-T-HH-mm-ss} - {GitCommitUtility.RetrieveCurrentCommitShortHash()} - level {levelIndex} - population {populationSize} (2 % elite selection).csv");
        CsvWriter csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csvWriter.WriteHeader<GeneticAlgorithmLogRow>();
        csvWriter.NextRecord();

        RouletteWheelSelection selection = new TGMSelection();
        TGMUniformCrossover crossover = new TGMUniformCrossover();
        UniformMutation mutation = new UniformMutation(mutableGenesIndexes: new[] {2, 3});
        TGMFitness fitness = new TGMFitness();

        TGMChromosome.levelIndex = levelIndex;
        TGMChromosome.levelGeneratorSeed = levelGeneratorSeed;
        TGMChromosome.tgmGeneratorSeed = tgmGeneratorSeed;
        TGMChromosome chromosome = new TGMChromosome();

        Population population = new TGMPopulation(populationSize, populationSize, chromosome)
        {
            GenerationStrategy = new PerformanceGenerationStrategy(1)
        };

        ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation)
        {
            Termination = new OrTermination(
                new GenerationNumberTermination(maxGenerationCount),
                new FitnessStagnationTermination(maxGenerationCount / 2),
                new FitnessThresholdTermination(1.0)
            ),
            TaskExecutor = new ParallelTaskExecutor
            {
                MinThreads = 10,
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
                    level = levelIndex,
                    levelSeed = levelIndex == 0 ? levelGeneratorSeed : 0,
                    lineageId = currentGenerationChromosome.lineageID,
                    id = currentGenerationChromosome.ID,
                    hash = currentGenerationChromosome.GetHashCode(),
                    fitness = (double) (currentGenerationChromosome.Fitness == null || currentGenerationChromosome.Fitness <= Mathf.Epsilon ? 0.0 : currentGenerationChromosome.Fitness),
                    gameObject = currentGenerationChromosome.GetGene(0).Value.ToString(),
                    component = currentGenerationChromosome.GetGene(1).Value.ToString(),
                    componentField = currentGenerationChromosome.GetGene(2).Value.ToString(),
                    modifier = currentGenerationChromosome.GetGene(3).Value.ToString(),
                    fieldType = currentGenerationChromosome.simulationInstance.tgm.GetFieldValueType().ToString(),
                    iterations = currentGenerationChromosome.goExploreResult.iterations,
                    archiveLength = currentGenerationChromosome.goExploreResult.archive.Length,
                    archive = currentGenerationChromosome.goExploreResult.PrintArchive(),
                    terminalTrajectories = currentGenerationChromosome.goExploreResult.PrintTerminalTrajectories()
                });
                csvWriter.NextRecord();
            }
            csvWriter.Flush();
        };

        await Task.Run(() =>
        {
            ga.Start();

            TGMChromosome bestChromosome = (TGMChromosome) ga.BestChromosome;
            Debug.Log($"Termination: {ga.Termination}");
            Debug.Log($"END genetic algorithm after {ga.GenerationsNumber} - BEST GENE {bestChromosome}");

            csvWriter.Flush();
        });

        RunEvolution(_levelList);
    }

}
