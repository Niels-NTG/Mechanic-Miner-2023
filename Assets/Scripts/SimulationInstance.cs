using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = System.Random;

public class SimulationInstance
{
    private readonly String ID;
    public readonly ToggleableGameMechanic tgm;
    private readonly Scene scene;

    // Level
    private readonly Grid levelGrid;
    private readonly Vector2Int entryLocation;
    private readonly Vector2Int exitLocation;

    // Player
    private readonly PlayerController playerController;
    private readonly int inputDuration = 20;

    // Move left, move right, jump, special, nothing
    public readonly int[] actionSpace =
    {
        0,
        1,
        2,
        3,
        4
    };

    private readonly int debugSeed = 83982;

    public SimulationInstance(String ID)
    {
        this.ID = ID;

        // Create new scene
        CreateSceneParameters createSceneParameters = new CreateSceneParameters(LocalPhysicsMode.Physics2D);
        scene = SceneManager.CreateScene(ID, createSceneParameters);

        // Add scene controller to scene
        GameObject sceneControllerPrefab = Resources.Load<GameObject>("Prefabs/SimulationSceneController");
        GameObject sceneController = (GameObject) PrefabUtility.InstantiatePrefab(sceneControllerPrefab, scene);

        // Get instance controller component
        SimulationInstanceController simulationInstanceController = sceneController.GetComponent<SimulationInstanceController>();

        // Generate level
        LevelGenerator levelGenerator = simulationInstanceController.levelGenerator;
        levelGenerator.Generate(new Random(debugSeed));

        // Get level properties relevant for running the simulation
        levelGrid = levelGenerator.GetComponent<Grid>();
        entryLocation = levelGenerator.entryLocation;
        exitLocation = levelGenerator.exitLocation;

        // Instantiate player agent and place at level entry position.
        playerController = simulationInstanceController.playerAgent.GetComponent<PlayerController>();
        ResetPlayer();

        // Create TGM from toggleable properties on level generator and player instance.
        List<Component> componentsWithToggleableProperties = new List<Component>();
        componentsWithToggleableProperties.AddRange(levelGenerator.componentsWithToggleableProperties);
        componentsWithToggleableProperties.AddRange(playerController.componentsWithToggleableProperties);
        tgm = new ToggleableGameMechanic(componentsWithToggleableProperties, new Random(debugSeed));

        playerController.toggleableGameMechanic = tgm;

    }

    public void UnloadScene()
    {
        SceneManager.UnloadSceneAsync(scene);
    }

    public void ResetPlayer()
    {
        playerController.gameObject.SetActive(true);
        playerController.transform.position =
            new Vector3(entryLocation.x, entryLocation.y, 0) + new Vector3(0.5f, 0.5f, 0);
    }

    public async Task<StepResult> Step(int action)
    {
        Task actionTask = null;
        switch (action)
        {
            case 0:
                actionTask = MoveLeft();
                break;
            case 1:
                actionTask = MoveRight();
                break;
            case 2:
                actionTask = Jump();
                break;
            case 3:
                actionTask = ToggleSpecial();
                break;
            case 4:
                actionTask = DoNothing();
                break;
        }

        await actionTask;
        return new StepResult(CurrentGridSpace(), action, Time.frameCount, DistanceToExit(), IsTerminal());
    }
    
    public readonly struct StepResult
    {
        public readonly Vector2Int playerGridPosition;
        public readonly int frameNumber;
        public readonly float reward;
        public readonly bool isTerminal;
        private readonly int actionTaken;
        public StepResult(Vector2Int playerGridPosition, int action, int frameNumber, float reward, bool isTerminal)
        {
            this.playerGridPosition = playerGridPosition;
            this.frameNumber = frameNumber;
            this.reward = reward;
            this.isTerminal = isTerminal;
            actionTaken = action;
        }

        public override String ToString() => $"player grid space: {playerGridPosition}, action: {actionTaken}, frame: {frameNumber}, reward: {reward}, isTerminal: {isTerminal}";

        public override int GetHashCode()
        {
            return playerGridPosition.GetHashCode() + (int) reward + actionTaken;
        }
    }

    private Vector2Int CurrentGridSpace()
    {
        Vector3Int currentGridSpace3D = levelGrid.WorldToCell(playerController.transform.position);
        return new Vector2Int(currentGridSpace3D.x, currentGridSpace3D.y);
    }

    private bool IsTerminal()
    {
        if (!playerController.gameObject.activeSelf)
        {
            return true;
        }
        // TODO consider adding timeout if player has spend too long to reach the exit.
        return false;
    }

    private float DistanceToExit()
    {
        return 0 - Vector2Int.Distance(CurrentGridSpace(), exitLocation);
    }

    private async Task MoveLeft()
    {
        Vector2Int startGridSpace = CurrentGridSpace();
        int horizontalMovementInputs = 0;
        while (
            startGridSpace == CurrentGridSpace() ||
            horizontalMovementInputs < inputDuration
        )
        {
            playerController.rigidBody.AddForce(
                playerController.MoveLeft()
            );
            horizontalMovementInputs++;
            await Task.Yield();
        }
    }

    private async Task MoveRight()
    {
        Vector2Int startGridSpace = CurrentGridSpace();
        int horizontalMovementInputs = 0;
        while (
            startGridSpace == CurrentGridSpace() ||
            horizontalMovementInputs < inputDuration
        )
        {
            playerController.rigidBody.AddForce(
                playerController.MoveRight()
            );
            horizontalMovementInputs++;
            await Task.Yield();
        }
    }

    private async Task Jump()
    {
        Vector2Int startGridSpace = CurrentGridSpace();
        int jumpWaitTimer = 0;
        playerController.rigidBody.AddForce(
            playerController.Jump()
        );
        while (
            startGridSpace == CurrentGridSpace() ||
            jumpWaitTimer < inputDuration
        )
        {
            jumpWaitTimer++;
            await Task.Yield();
        }
    }

    private async Task ToggleSpecial()
    {
        playerController.ToggleSpecial();
        await Task.Yield();
    }

    private async Task DoNothing()
    {
        for (int i = 0; i < inputDuration; i++)
        {
            await Task.Yield();
        }
    }
}
