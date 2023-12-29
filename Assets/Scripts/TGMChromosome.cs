using System;
using System.Linq;
using System.Threading.Tasks;
using GeneticSharp.Domain.Chromosomes;
using UnityEngine;

public sealed class TGMChromosome : ChromosomeBase
{
    public readonly String ID;
    public readonly String lineageID;

    public SimulationInstance simulationInstance;
    public GoExplore.GoExploreResult goExploreResult;

    public static int levelIndex;
    public static int levelGeneratorSeed;
    public static int tgmGeneratorSeed;

    public TGMChromosome() : base(4) { }

    private TGMChromosome(String previousID) : base(4)
    {
        ID = Guid.NewGuid().ToString();
        lineageID = previousID;
        CreateGenes();
    }

    public TGMChromosome(string previousID, Gene[] predefinedGenes) : base(4)
    {
        ID = Guid.NewGuid().ToString();
        lineageID = previousID;
        ReplaceGenes(0, predefinedGenes);

        Debug.Log($"{ID} TGMChromosome.GenerateGene: generate new SimulationInstance");
        Task<SimulationInstance> simulationSceneCreationTask = CreateSimulationInstance();
        simulationInstance = simulationSceneCreationTask.GetAwaiter().GetResult();

        for (int geneIndex = 0; geneIndex < Length; geneIndex++)
        {
            Gene predefinedGene = predefinedGenes[geneIndex];
            if (predefinedGene.Value == null)
            {
                ReplaceGene(geneIndex, GenerateTGMGene(geneIndex).GetAwaiter().GetResult());
                continue;
            }
            ApplyGeneToTGM(geneIndex, predefinedGene);
        }
        simulationInstance.ApplyTGM();
    }

    public override Gene GenerateGene(int geneIndex)
    {
        if (simulationInstance == null)
        {
            Debug.Log($"{ID} TGMChromosome.GenerateGene: generate new SimulationInstance");
            Task<SimulationInstance> simulationSceneCreationTask = CreateSimulationInstance();
            simulationInstance = simulationSceneCreationTask.GetAwaiter().GetResult();
        }
        Task<Gene> assignAndMutateTGMTask = GenerateTGMGene(geneIndex);
        Gene newGene = assignAndMutateTGMTask.GetAwaiter().GetResult();
        simulationInstance.ApplyTGM();
        return newGene;
    }

    private async Task<SimulationInstance> CreateSimulationInstance()
    {
        await Awaitable.MainThreadAsync();
        return new SimulationInstance(ID, levelIndex, levelGeneratorSeed, tgmGeneratorSeed);
    }

    private async Task<Gene> GenerateTGMGene(int geneIndex)
    {
        await Awaitable.MainThreadAsync();

        //  0 = game object
        //  1 = component
        //  2 = component field
        //  3 = modifier

        switch (geneIndex)
        {
            case 0:
                return new Gene(simulationInstance.tgm.SelectGameObject());
            case 1:
                return new Gene(simulationInstance.tgm.SelectComponent());
            case 2:
                return new Gene(simulationInstance.tgm.SelectComponentProperty());
            case 3:
                return new Gene(simulationInstance.tgm.SelectModifier());
            default:
                return new Gene();
        }
    }

    private async void ApplyGeneToTGM(int geneIndex, Gene existingGene)
    {
        await Awaitable.MainThreadAsync();

        //  0 = game object
        //  1 = component
        //  2 = component field
        //  3 = modifier

        switch (geneIndex)
        {
            case 0:
                ReplaceGene(
                    geneIndex,
                    new Gene(simulationInstance.tgm.SelectGameObject((String)existingGene.Value))
                );
                return;
            case 1:
                ReplaceGene(
                    geneIndex,
                    new Gene(simulationInstance.tgm.SelectComponent((String)existingGene.Value))
                );
                return;
            case 2:
                ReplaceGene(
                    geneIndex,
                    new Gene(simulationInstance.tgm.SelectComponentProperty((String)existingGene.Value))
                );
                return;
            case 3:
                ReplaceGene(
                    geneIndex,
                    new Gene(simulationInstance.tgm.SelectModifier((String)existingGene.Value))
                );
                return;
        }
    }

    public override IChromosome CreateNew()
    {
        return new TGMChromosome(ID);
    }

    public override string ToString()
    {
        return $"{ID} ({String.Join(" - ", GetGenes().Select(g => g.Value))}) with fitness {Fitness}";
    }

    public override int GetHashCode()
    {
        Gene[] genes = GetGenes();
        int gameObjectHash = genes[0].Value == null ? 0 : genes[0].Value.GetHashCode();
        int componentHash = genes[1].Value == null ? 0 : genes[1].Value.GetHashCode();
        int componentPropertyHash = genes[2].Value == null ? 0 : genes[2].Value.GetHashCode();
        int modifierHash = genes[3].Value == null ? 0 : genes[3].Value.GetHashCode();
        return MathUtils.Cantor(gameObjectHash, componentHash, componentPropertyHash, modifierHash);
    }
}
