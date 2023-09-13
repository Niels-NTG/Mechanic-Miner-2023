using System;
using GeneticSharp.Domain.Chromosomes;
using UnityEngine;

public sealed class TGMChromosome : ChromosomeBase
{

    private readonly String ID = Guid.NewGuid().ToString();
    private readonly SimulationInstance simulationInstance;
    private readonly ToggleableGameMechanic.ToggleGameMechanicGenotype gene;

    public TGMChromosome() : base(3)
    {
        // Create empty
        gene = new ToggleableGameMechanic.ToggleGameMechanicGenotype();

        simulationInstance = UnityMainThreadDispatcher.Dispatch(() => new SimulationInstance(ID, gene));
        CreateGenes();
    }

    public override Gene GenerateGene(int geneIndex)
    {
        if (geneIndex == 0)
        {
            simulationInstance.tgm.SelectComponentProperty();
        } else if (geneIndex == 1)
        {
            simulationInstance.tgm.SelectModifier();
        }
        Debug.Log($"{ID} {simulationInstance.tgm}");
        return new Gene(simulationInstance);
    }

    public override IChromosome CreateNew()
    {
        return new TGMChromosome();
    }
}
