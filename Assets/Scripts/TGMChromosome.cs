using System;
using GeneticSharp.Domain.Chromosomes;
using UnityEngine;

public sealed class TGMChromosome : ChromosomeBase
{
    private readonly String ID = Guid.NewGuid().ToString();
    private ToggleableGameMechanic.ToggleGameMechanicGenotype gene;
    private SimulationInstance simulationInstance;

    public TGMChromosome() : base(3)
    {
        // Create empty
        gene = new ToggleableGameMechanic.ToggleGameMechanicGenotype();

        CreateGenes();
    }

    public override Gene GenerateGene(int geneIndex)
    {
        if (simulationInstance == null)
        {
            simulationInstance = new SimulationInstance($"TGM-generator-{ID}");
        }
        simulationInstance.SetTGM(gene);

        if (geneIndex == 0)
        {
            simulationInstance.tgm.SelectComponentProperty();
        } else if (geneIndex == 1)
        {
            simulationInstance.tgm.SelectModifier();
        }
        Debug.Log($"{ID} {simulationInstance.tgm}");
        gene = simulationInstance.tgm.GetTGMGenotype();
        return new Gene(gene);
    }

    public override IChromosome CreateNew()
    {
        return new TGMChromosome();
    }
}
