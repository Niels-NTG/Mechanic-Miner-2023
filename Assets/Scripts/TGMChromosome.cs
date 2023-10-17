using System;
using System.Threading.Tasks;
using GeneticSharp.Domain.Chromosomes;
using UnityEngine;

public sealed class TGMChromosome : ChromosomeBase
{
    public readonly String ID;
    public SimulationInstance simulationInstance;
    public ToggleableGameMechanic.ToggleGameMechanicGenotype gene;

    public TGMChromosome(bool isSetup) : base(3)
    {
        if (isSetup)
        {
            return;
        }

        ID = Guid.NewGuid().ToString();

        // Create empty gene
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

    private async Task<GeneStruct> AssignAndMutateTGM(ToggleableGameMechanic.ToggleGameMechanicGenotype tgmGenotype, int geneIndex)
    {
        await Awaitable.MainThreadAsync();
        simulationInstance.SetTGM(tgmGenotype);
        if (geneIndex == 0)
        {
            simulationInstance.tgm.SelectComponentProperty();
        } else if (geneIndex == 1)
        {
            simulationInstance.tgm.SelectModifier();
        }
        gene = simulationInstance.tgm.GetTGMGenotype();
        Debug.Log($"{ID} TGMChromosome.AssignAndMutateTGM: {simulationInstance.tgm}");
        return new GeneStruct(ID, simulationInstance.tgm, simulationInstance);
    }

    public override IChromosome CreateNew()
    {
        return new TGMChromosome(false);
    }

    public readonly struct GeneStruct
    {
        public readonly String ID;
        public readonly ToggleableGameMechanic.ToggleGameMechanicGenotype tgmGenotype;
        public readonly SimulationInstance simulationInstance;

        public GeneStruct(
            String ID,
            ToggleableGameMechanic tgmInstance,
            SimulationInstance simulationInstance
        )
        {
            this.ID = ID;
            tgmGenotype = tgmInstance.GetTGMGenotype();
            this.simulationInstance = simulationInstance;
        }
    }
}
