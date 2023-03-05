using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class Population
{
    private List<ToggleableGameMechanic> population;
    public readonly int populationSize;

    Population(int populationSize, PlayerController player)
    {
        this.populationSize = populationSize;

        Random rng = new Random();
        
        population = new List<ToggleableGameMechanic>();
        for (int i = 0; i < populationSize; i++)
        {
            Component selectedComponent = ToggleableGameMechanic.SelectComponent(player, rng);
            String selectedComponentField = ToggleableGameMechanic.SelectComponentField(selectedComponent, rng);
            population.Add(
                new ToggleableGameMechanic(
                    selectedComponent,
                    selectedComponentField,
                    rng
                )
            );
        }
    }
}