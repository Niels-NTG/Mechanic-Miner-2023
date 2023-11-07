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

    public static int levelIndex;
    public static int levelGeneratorSeed;
    public static int tgmGeneratorSeed;

    public TGMChromosome(bool isSetup = false, String previousID = null) : base(4)
    {
        if (isSetup)
        {
            return;
        }

        ID = Guid.NewGuid().ToString();

        lineageID = previousID ?? ID;

        CreateGenes();
    }

    public override Gene GenerateGene(int geneIndex)
    {
        if (simulationInstance == null)
        {
            Debug.Log($"{ID} TGMChromosome.GenerateGene: generate new SimulationInstance");
            Task<SimulationInstance> simulationSceneCreationTask = CreateSimulationInstance();
            simulationInstance = simulationSceneCreationTask.GetAwaiter().GetResult();
        }
        Task<Gene> assignAndMutateTGMTask = AssignAndMutateTGM(geneIndex);
        Gene newGene = assignAndMutateTGMTask.GetAwaiter().GetResult();
        simulationInstance.ApplyTGM();
        return newGene;
    }

    private async Task<SimulationInstance> CreateSimulationInstance()
    {
        await Awaitable.MainThreadAsync();
        return new SimulationInstance(ID, levelIndex, levelGeneratorSeed, tgmGeneratorSeed);
    }

    private async Task<Gene> AssignAndMutateTGM(int geneIndex)
    {
        await Awaitable.MainThreadAsync();

        //  0 = game object
        //  1 = component
        //  2 = component field
        //  3 = modifier

        if (geneIndex == 0)
        {
            return new Gene(simulationInstance.tgm.SelectGameObject());
        }
        if (geneIndex == 1)
        {
            return new Gene(simulationInstance.tgm.SelectComponent());
        }
        if (geneIndex == 2)
        {
            return new Gene(simulationInstance.tgm.SelectComponentProperty());
        }
        if (geneIndex == 3)
        {
            return new Gene(simulationInstance.tgm.SelectModifier());
        }

        return new Gene();
    }

    public bool isSameType(TGMChromosome otherChromosome)
    {
        return simulationInstance.tgm.GetFieldValueType() == otherChromosome.simulationInstance.tgm.GetFieldValueType();
    }

    public override IChromosome CreateNew()
    {
        return new TGMChromosome(false, ID);
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
