using UnityEngine;
using UnityEngine.Tilemaps;

public class SceneManager : MonoBehaviour
{
    public Population population;
    
    public int initialPopulationSize = 1;
    
    public GameObject playerAgentPrefab;
    public LevelGenerator levelGenerator;

    private void Awake()
    {
        levelGenerator.Generate();
        population.CreatePopulation(
            initialPopulationSize, 
            playerAgentPrefab,
            levelGenerator,
            levelGenerator.entryLocation,
            levelGenerator.exitTilemap.GetComponent<TilemapCollider2D>()
        );
    }
}
