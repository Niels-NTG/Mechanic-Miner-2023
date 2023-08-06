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
    private readonly int maxInputDuration = 240;

    // Move left, move right, jump, special, nothing
    public readonly int[] actionSpace =
    {
        0,
        1,
        2,
        3,
        4
    };

    private readonly int debugSeed = 292921;

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
        TeleportPlayer(new Vector3(entryLocation.x, entryLocation.y, 0) + new Vector3(0.5f, 0.5f, 0));
    }

    public void TeleportPlayer(Vector2Int pos)
    {
        TeleportPlayer(new Vector3(pos.x, pos.y, 0));
    }

    public void TeleportPlayer(Vector3 pos)
    {
        playerController.gameObject.SetActive(true);
        playerController.transform.position = pos;
    }

    public async Task<StepResult> Step(int action, int iteration)
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


        return new StepResult(CurrentGridSpace(), action, iteration, RewardDistanceToExit(), IsTerminal());
    }

    public readonly struct StepResult
    {
        public readonly Vector2Int playerGridPosition;
        private readonly int iteration;
        public readonly float reward;
        public readonly bool isTerminal;
        private readonly int actionTaken;
        public StepResult(Vector2Int playerGridPosition, int action, int iteration, float reward, bool isTerminal)
        {
            this.playerGridPosition = playerGridPosition;
            this.iteration = iteration;
            this.reward = reward;
            this.isTerminal = isTerminal;
            actionTaken = action;
        }

        public override String ToString() => $"player grid space: {playerGridPosition}, action: {actionTaken}, iteration: {iteration}, reward: {reward}, isTerminal: {isTerminal}";

        public override int GetHashCode() => playerGridPosition.GetHashCode() + (int) reward + actionTaken;
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

    private float RewardDistanceToExit()
    {
        return Vector2Int.Distance(Vector2Int.zero, LevelGenerator.levelSize.size) -
               Vector2Int.Distance(exitLocation, CurrentGridSpace());
    }

    private async Task MoveLeft()
    {
        Vector2Int startGridSpace = CurrentGridSpace();
        int horizontalMovementInputs = 0;
        do
        {
            playerController.rigidBody.AddForce(
                playerController.MoveLeft()
            );
            horizontalMovementInputs++;
            await Task.Yield();
        } while (startGridSpace == CurrentGridSpace() && horizontalMovementInputs < maxInputDuration);
    }

    private async Task MoveRight()
    {
        Vector2Int startGridSpace = CurrentGridSpace();
        int horizontalMovementInputs = 0;
        do
        {
            playerController.rigidBody.AddForce(
                playerController.MoveRight()
            );
            horizontalMovementInputs++;
            await Task.Yield();
        } while (startGridSpace == CurrentGridSpace() && horizontalMovementInputs < maxInputDuration);
    }

    private async Task Jump()
    {
        Vector2Int startGridSpace = CurrentGridSpace();
        int jumpWaitTimer = 0;
        playerController.rigidBody.AddForce(
            playerController.Jump()
        );
        do
        {
            jumpWaitTimer++;
            await Task.Yield();
        } while (startGridSpace == CurrentGridSpace() && jumpWaitTimer < maxInputDuration);
    }

    private async Task ToggleSpecial()
    {
        playerController.ToggleSpecial();
        await Task.Yield();
    }

    private async Task DoNothing()
    {
        for (int i = 0; i < maxInputDuration; i++)
        {
            await Task.Yield();
        }
    }
}
