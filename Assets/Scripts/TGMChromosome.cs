using System;
using GeneticSharp.Domain.Chromosomes;

public sealed class TGMChromosome : ChromosomeBase
{

    private readonly SimulationInstance simulationInstance;

    public TGMChromosome() : base(3)
    {
        // simulationInstance = new SimulationInstance(ID);
        simulationInstance = UnityMainThreadDispatcher.Dispatch(() => new SimulationInstance(ID));
        CreateGenes();
    }

    private String ID { get; } = Guid.NewGuid().ToString();

    public override Gene GenerateGene(int geneIndex)
    {
        if (geneIndex == 0)
        {
            simulationInstance.tgm.SelectComponentProperty();
        } else if (geneIndex == 1)
        {
            simulationInstance.tgm.SelectModifier();
        }
        return new Gene(simulationInstance);
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
