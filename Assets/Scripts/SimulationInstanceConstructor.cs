using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = System.Random;

public class SimulationInstanceConstructor
{
    public readonly ToggleableGameMechanic tgm;
    private readonly Scene newScene;

    public SimulationInstanceConstructor(String ID)
    {
        // Create new scene
        CreateSceneParameters createSceneParameters = new CreateSceneParameters(LocalPhysicsMode.Physics2D);
        newScene = SceneManager.CreateScene(ID, createSceneParameters);

        // Add scene controller to scene
        GameObject sceneControllerPrefab = Resources.Load<GameObject>("Prefabs/SimulationSceneController");
        GameObject sceneController = (GameObject) PrefabUtility.InstantiatePrefab(sceneControllerPrefab, newScene);

        // Get instance controller component
        SimulationInstanceController simulationInstanceController =
            sceneController.GetComponent<SimulationInstanceController>();
        
        // Generate level
        LevelGenerator levelGenerator = simulationInstanceController.levelGenerator;
        levelGenerator.Generate();
        
        // Instantiate player agent and place at level entry position.
        simulationInstanceController.playerAgent.transform.position =
            new Vector3(levelGenerator.entryLocation.x, levelGenerator.entryLocation.y, 0) + new Vector3(0.5f, 0.5f, 0);
        PlayerController playerController = simulationInstanceController.playerAgent.GetComponent<PlayerController>();
        
        // Create TGM from toggleable properties on level generator and player instance.
        List<Component> componentsWithToggleableProperties = new List<Component>();
        componentsWithToggleableProperties.AddRange(levelGenerator.componentsWithToggleableProperties);
        componentsWithToggleableProperties.AddRange(playerController.componentsWithToggleableProperties);
        tgm = new ToggleableGameMechanic(componentsWithToggleableProperties, new Random());

        playerController.toggleableGameMechanic = tgm;
    }

    public void UnloadScene()
    {
        SceneManager.UnloadSceneAsync(newScene);
    }
}
