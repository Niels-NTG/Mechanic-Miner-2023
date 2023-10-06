using System;
using System.Threading.Tasks;
using GeneticSharp.Domain.Chromosomes;
using UnityEngine;

public sealed class TGMChromosome : ChromosomeBase
{
    private readonly String ID = Guid.NewGuid().ToString();
    private ToggleableGameMechanic.ToggleGameMechanicGenotype gene;
    private SimulationInstance simulationInstance;

    public readonly struct GeneStruct
    {
        public readonly String ID;
        public readonly ToggleableGameMechanic.ToggleGameMechanicGenotype gene;
        public readonly SimulationInstance simulationInstance;

        public GeneStruct(
            String ID,
            ToggleableGameMechanic.ToggleGameMechanicGenotype gene,
            SimulationInstance simulationInstance
        )
        {
            this.ID = ID;
            this.gene = gene;
            this.simulationInstance = simulationInstance;
        }
    }

    public TGMChromosome(bool isSetup) : base(3)
    {
        if (isSetup)
        {
            return;
        }
        // Create empty
        gene = new ToggleableGameMechanic.ToggleGameMechanicGenotype();

        CreateGenes();
    }

    public override Gene GenerateGene(int geneIndex)
    {
        if (simulationInstance == null)
        {
            Debug.Log($"{ID} TGMChromosome.GenerateGene: attempt to generate new SimulationInstance");
            Task<SimulationInstance> simulationSceneCreationTask = CreateSimulationInstance();
            simulationInstance = simulationSceneCreationTask.GetAwaiter().GetResult();
        }
        Debug.Log($"{ID} TGMChromosome.GenerateGene: attempt to assign TGM to simulation instance");
        Task<GeneStruct> assignAndMutateTGMTask = AssignAndMutateTGM(gene, geneIndex);
        GeneStruct geneStruct = assignAndMutateTGMTask.GetAwaiter().GetResult();
        return new Gene(geneStruct);
    }

    private async Task<SimulationInstance> CreateSimulationInstance()
    {
        await Awaitable.MainThreadAsync();
        return new SimulationInstance(ID);
    }

    private async Task<GeneStruct> AssignAndMutateTGM(ToggleableGameMechanic.ToggleGameMechanicGenotype toggleGameMechanicGenotype, int geneIndex)
    {
        await Awaitable.MainThreadAsync();
        simulationInstance.SetTGM(toggleGameMechanicGenotype);
        if (geneIndex == 0)
        {
            simulationInstance.tgm.SelectComponentProperty();
        } else if (geneIndex == 1)
        {
            simulationInstance.tgm.SelectModifier();
        }
        Debug.Log($"{ID} TGMChromosome.AssignAndMutateTGM: {simulationInstance.tgm}");
        return new GeneStruct(ID, gene, simulationInstance);
    }

    public override IChromosome CreateNew()
    {
        return new TGMChromosome(false);
    }
}
