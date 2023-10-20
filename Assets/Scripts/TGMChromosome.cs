using System;
using System.Threading.Tasks;
using GeneticSharp.Domain.Chromosomes;
using UnityEngine;

public sealed class TGMChromosome : ChromosomeBase
{
    public readonly String ID;
    public SimulationInstance simulationInstance;
    public ToggleableGameMechanic.ToggleGameMechanicGenotype genotype;

    public static int levelIndex;

    public TGMChromosome(bool isSetup) : base(4)
    {
        if (isSetup)
        {
            return;
        }

        ID = Guid.NewGuid().ToString();

        // Create empty gene
        genotype = new ToggleableGameMechanic.ToggleGameMechanicGenotype();

        CreateGenes();
    }

    ~TGMChromosome()
    {
        simulationInstance = null;
    }

    public override Gene GenerateGene(int geneIndex)
    {
        if (simulationInstance == null)
        {
            Debug.Log($"{ID} TGMChromosome.GenerateGene: generate new SimulationInstance");
            Task<SimulationInstance> simulationSceneCreationTask = CreateSimulationInstance();
            simulationInstance = simulationSceneCreationTask.GetAwaiter().GetResult();
        }
        Task<ToggleableGameMechanic.ToggleGameMechanicGenotype> assignAndMutateTGMTask = AssignAndMutateTGM(genotype, geneIndex);
        genotype = assignAndMutateTGMTask.GetAwaiter().GetResult();

        //  0 = game object
        //  1 = component
        //  2 = component field
        //  3 = modifier
        switch (geneIndex)
        {
            case 0:
                return new Gene(genotype.gameObjectName);
            case 1:
                return new Gene(genotype.componentTypeName);
            case 2:
                return new Gene(genotype.fieldName);
            case 3:
                return new Gene(genotype.modifier);
        }
        return new Gene();
    }

    private async Task<SimulationInstance> CreateSimulationInstance()
    {
        await Awaitable.MainThreadAsync();
        return new SimulationInstance(ID, levelIndex);
    }

    private async Task<ToggleableGameMechanic.ToggleGameMechanicGenotype> AssignAndMutateTGM(ToggleableGameMechanic.ToggleGameMechanicGenotype tgmGenotype, int geneIndex)
    {
        await Awaitable.MainThreadAsync();
        simulationInstance.SetTGM(tgmGenotype);
        if (geneIndex == 2)
        {
            simulationInstance.tgm.SelectComponentProperty();
        } else if (geneIndex == 3)
        {
            simulationInstance.tgm.SelectModifier();
        }
        Debug.Log($"{ID} TGMChromosome.AssignAndMutateTGM: gene index {geneIndex} - {simulationInstance.tgm}");
        return simulationInstance.tgm.GetTGMGenotype();
    }

    public override IChromosome CreateNew()
    {
        return new TGMChromosome(false);
    }
}
