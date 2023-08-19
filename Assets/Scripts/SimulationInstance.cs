using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = System.Random;

public class SimulationInstance
{
    public readonly String ID;
    public readonly ToggleableGameMechanic tgm;
    private readonly Scene scene;

    // Level
    private readonly Grid levelGrid;
    private readonly Vector2Int entryLocation;
    private readonly Vector2Int exitLocation;

    // Player
    private readonly PlayerController playerController;
    private readonly int maxInputDuration = 240;

    private readonly float lowerResetRewardBound = -2000;

    // Move left, move right, jump, special
    public readonly int[] actionSpace =
    {
        0,
        1,
        2,
        3
    };

    private readonly int debugSeed = 93;

    public SimulationInstance(String ID)
    {
        this.ID = ID;

        Random rng = debugSeed == 0 ? new Random() : new Random(debugSeed);

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
        levelGenerator.Generate(rng);

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
        tgm = new ToggleableGameMechanic(componentsWithToggleableProperties, rng);
        playerController.toggleableGameMechanic = tgm;
    }

    public void UnloadScene()
    {
        UnityMainThreadDispatcher.Dispatch(() => SceneManager.UnloadSceneAsync(scene));
    }

    private void ResetPlayer()
    {
        TeleportPlayer(new Vector3(entryLocation.x, entryLocation.y, 0) + new Vector3(0.5f, 0.5f, 0));
        playerController.ResetPlayer();
    }

    public void TeleportPlayer(Vector2Int pos)
    {
        TeleportPlayer(new Vector3(pos.x, pos.y, 0));
    }

    private void TeleportPlayer(Vector3 pos)
    {
        playerController.gameObject.SetActive(true);
        playerController.transform.position = pos;
    }


    public StepResult Step(int action, int iteration)
    {
        Vector2Int startGridSpace = CurrentGridSpace();

        Task actionTask = null;
        switch (action)
        {
            case 0:
                actionTask = UnityMainThreadDispatcher.DispatchAsync(MoveLeft);
                break;
            case 1:
                actionTask = UnityMainThreadDispatcher.DispatchAsync(MoveRight);
                break;
            case 2:
                actionTask = UnityMainThreadDispatcher.DispatchAsync(Jump);
                break;
            case 3:
                actionTask = UnityMainThreadDispatcher.DispatchAsync(ToggleSpecial);
                break;
            case 4:
                actionTask = UnityMainThreadDispatcher.DispatchAsync(DoNothing);
                break;
        }

        if (actionTask != null)
        {
            actionTask.Wait();
        }

        Vector2Int resultGridSpace = CurrentGridSpace();
        float reward = RewardDistanceToExit();
        bool isTerminal = IsTerminal();

        // Kill and reset player to start position if it has touched spikes or has gone too far from the level goal.
        if (playerController.hasTouchedSpikes || reward < lowerResetRewardBound)
        {
            reward = float.MinValue;
            ResetPlayer();
        } else if (playerController.hasTouchedExit)
        {
            reward = float.MaxValue;
        } else if (resultGridSpace == startGridSpace)
        {
            reward = 0f;
        }


        return new StepResult(resultGridSpace, action, iteration, reward, isTerminal);
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
        Vector3Int currentGridSpace3D = UnityMainThreadDispatcher.Dispatch(() => levelGrid.WorldToCell(playerController.transform.position));
        return new Vector2Int(currentGridSpace3D.x, currentGridSpace3D.y);
    }

    private bool IsTerminal()
    {
        return playerController.hasTouchedExit;
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
