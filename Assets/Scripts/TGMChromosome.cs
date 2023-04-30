using System;
using GeneticSharp.Domain.Chromosomes;

public sealed class TGMChromosome : ChromosomeBase
{
    
    private readonly ToggleableGameMechanic tgm;
    private readonly SimulationInstanceConstructor simulationInstance;
    
    public TGMChromosome() : base(3)
    {
        simulationInstance = new SimulationInstanceConstructor(ID);
        tgm = simulationInstance.tgm;
        CreateGenes();
    }

    public String ID { get; } = Guid.NewGuid().ToString();

    public override Gene GenerateGene(int geneIndex)
    {
        if (geneIndex == 0)
        {
            tgm.SelectComponentProperty();
        } else if (geneIndex == 1) 
        {
            tgm.SelectModifier();
        }
        return new Gene(tgm);
    }

    public override IChromosome CreateNew()
    {
        if (simulationInstance != null)
        {
            simulationInstance.UnloadScene();
        }
        return new TGMChromosome();
    }
}
