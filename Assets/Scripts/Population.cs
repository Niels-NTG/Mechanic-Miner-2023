using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = System.Random;

public class Population : MonoBehaviour
{
    private List<ToggleableGameMechanic> population;
    [SerializeField] private int seed = 38387298;

    public void CreatePopulation(int populationSize, GameObject playerAgentPrefab, Vector2Int playerEntryLocation, TilemapCollider2D exitCollider)
    {
        Random rng = new Random(seed);

        population = new List<ToggleableGameMechanic>();
        for (int i = 0; i < populationSize; i++)
        {
            GameObject newPlayerAgent = Instantiate(playerAgentPrefab, new Vector3(playerEntryLocation.x, playerEntryLocation.y, 0), Quaternion.identity);
            PlayerController playerController = newPlayerAgent.GetComponent<PlayerController>();
            Component selectedComponent = ToggleableGameMechanic.SelectComponent(playerController, rng);
            String selectedComponentField = ToggleableGameMechanic.SelectComponentProperty(selectedComponent, rng);
            ToggleableGameMechanic toggleableGameMechanic = new ToggleableGameMechanic(
                selectedComponent,
                selectedComponentField,
                rng
            );
            playerController.toggleableGameMechanic = toggleableGameMechanic;
            population.Add(toggleableGameMechanic);
        }
    }
}
