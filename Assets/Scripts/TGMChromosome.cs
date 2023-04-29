using System;
using System.Collections.Generic;
using GeneticSharp.Domain.Chromosomes;
using UnityEngine;
using Random = System.Random;

public sealed class TGMChromosome : ChromosomeBase
{

    // https://github.com/giacomelli/GeneticSharp/tree/master
    // https://github.com/giacomelli/GeneticSharp/blob/master/src/GeneticSharp.Runner.UnityApp/Assets/_runner/Car/CarVectorPhenotypeEntity.cs#L6
    // https://github.com/giacomelli/GeneticSharp/blob/master/src/GeneticSharp.Runner.UnityApp/Assets/_runner/Car/CarChromosome.cs#L17
    
    private ToggleableGameMechanic tgm;
    public TGMChromosome(List<Component> componentsWithToggleableProperties) : base(10)
    {
        this.componentsWithToggleableProperties = componentsWithToggleableProperties;
        tgm = new ToggleableGameMechanic(componentsWithToggleableProperties, new Random());
        CreateGenes();
    }

    public List<Component> componentsWithToggleableProperties { get; set; }

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
        return new TGMChromosome(componentsWithToggleableProperties);
    }
}
