using System.Collections.Generic;
using GeneticSharp.Domain;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using GeneticSharp.Domain.Populations;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
    
    public GameObject playerAgentPrefab;
    public LevelGenerator levelGenerator;

    private void Awake()
    {
        // TODO initialize a 100 "gym" environments for each TGM. Each create their own player agent,
        //  which should be added to componentsWithToggleableProperties
        // TODO Use Pyshics Layers https://docs.unity3d.com/Manual/LayerBasedCollision.html
        // TODO of nog beter, maak meerdere scenes aan https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager.html

        levelGenerator.Generate();
        
        List<Component> componentsWithToggleableProperties = new List<Component>();
        componentsWithToggleableProperties.AddRange(levelGenerator.componentsWithToggleableProperties);

        EliteSelection selection = new EliteSelection();
        UniformCrossover crossover = new UniformCrossover();
        ReverseSequenceMutation mutation = new ReverseSequenceMutation();
        TGMFitness fitness = new TGMFitness();
        TGMChromosome chromosome = new TGMChromosome(componentsWithToggleableProperties);
        Population population = new Population(100, 100, chromosome);

        GeneticAlgorithm ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation);
        ga.Termination = new GenerationNumberTermination(10);

        ga.Start();
        
        Debug.Log($"Best solution found has {ga.BestChromosome.Fitness} fitness");
        
    }
}
