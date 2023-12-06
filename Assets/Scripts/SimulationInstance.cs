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
    private Scene scene;

    // Level
    private readonly Grid levelGrid;
    private readonly Vector2Int entryLocation;
    private readonly Vector2Int exitLocation;

    // Player
    private readonly PlayerController playerController;

    // NOTE: you may need to change this value when you use a different value for
    // physicsTileScale (SimulationInstanceController). 10 is optimised for a
    // physicsTileScale of 10f.
    private readonly int maxInputDuration = 10;

    private readonly float lowerResetRewardBound = -2000;

    // Move left, move right, jump, special, do nothing
    public static readonly int[] actionSpace =
    {
        0,
        1,
        2,
        3,
        4
    };
    private static readonly String[] actionSpaceNames =
    {
        "MOVE_LEFT",
        "MOVE_RIGHT",
        "JUMP",
        "SPECIAL",
        "DO_NOTHING"
    };

    public SimulationInstance(String ID, int levelIndex, int levelGeneratorSeed, int tgmSeed)
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
        Random levelGeneratorRNG = levelGeneratorSeed == 0 ? new Random() : new Random(levelGeneratorSeed);
        Level level = simulationInstanceController.levels[levelIndex];
        level.gameObject.SetActive(true);
        if (level is LevelGenerator levelGenerator)
        {
            levelGenerator.Generate(levelGeneratorRNG);
        }

        // Get level properties relevant for running the simulation
        levelGrid = level.GetComponent<Grid>();
        entryLocation = level.entryLocation;
        exitLocation = level.exitLocation;

        // Instantiate player agent and place at level entry position.
        playerController = simulationInstanceController.playerAgent.GetComponent<PlayerController>();
        ResetPlayer();

        // Create TGM from toggleable properties on level generator and player instance.
        List<Component> componentsWithToggleableProperties = new List<Component>();
        componentsWithToggleableProperties.AddRange(level.componentsWithToggleableProperties);
        componentsWithToggleableProperties.AddRange(playerController.componentsWithToggleableProperties);
        Random tgmRNG = tgmSeed == 0 ? new Random() : new Random(tgmSeed);
        tgm = new ToggleableGameMechanic(componentsWithToggleableProperties, tgmRNG);
    }

    public void ApplyTGM()
    {
        playerController.toggleableGameMechanic = tgm;
        ResetPlayer();
    }

    public async Task UnloadScene()
    {
        Debug.Log($"{ID} SimulationInstance: unloading scene");
        await Awaitable.MainThreadAsync();
        if (scene.IsValid())
        {
            await SceneManager.UnloadSceneAsync(scene);
        }
    }

    public async void ResetPlayer()
    {
        await Awaitable.MainThreadAsync();
        TeleportPlayer(new Vector3(entryLocation.x, entryLocation.y, 0) + new Vector3(0.5f, 0.5f, 0));
        playerController.ResetPlayer();
    }

    private async void TeleportPlayer(Vector3 pos)
    {
        await Awaitable.MainThreadAsync();
        playerController.gameObject.SetActive(true);
        playerController.transform.position = pos;
    }

    public StepResult Step(int action, int iteration)
    {
        WaitForEndOfLastInput().GetAwaiter().GetResult();

        Vector2Int startGridSpace = CurrentGridSpace().GetAwaiter().GetResult();

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

        if (actionTask != null)
        {
            actionTask.GetAwaiter().GetResult();
        }

        Vector2Int resultGridSpace = CurrentGridSpace().GetAwaiter().GetResult();
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

        // Jumps shouldn't not be repeated.
        bool canActionBeRepeated = action != 2;

        return new StepResult(ID, resultGridSpace, action, iteration, reward, isTerminal, canActionBeRepeated);
    }

    public record StepResult
    {
        private readonly String UUID;
        public readonly Vector2Int playerGridPosition;
        private readonly int iteration;
        public readonly float reward;
        public readonly bool isTerminal;
        public readonly int actionTaken;
        public readonly bool canActionBeRepeated;
        public StepResult(String UUID, Vector2Int playerGridPosition, int action, int iteration, float reward, bool isTerminal, bool canActionBeRepeated)
        {
            this.UUID = UUID;
            this.playerGridPosition = playerGridPosition;
            this.iteration = iteration;
            this.reward = reward;
            this.isTerminal = isTerminal;
            actionTaken = action;
            this.canActionBeRepeated = canActionBeRepeated;
        }

        public override String ToString() => $"{UUID}, player grid space: {playerGridPosition}, action: {actionSpaceNames[actionTaken]}, iteration: {iteration}, reward: {reward}, isTerminal: {isTerminal}";

        public override int GetHashCode() =>
            MathUtils.Cantor(MathUtils.HashVector2Int(playerGridPosition), actionTaken, (int) reward);
    }

    private async Task WaitForEndOfLastInput()
    {
        await Awaitable.MainThreadAsync();
        do
        {
            await Awaitable.FixedUpdateAsync();
        } while (playerController.rigidBody.totalForce != Vector2.zero);
    }

    public async Task<Vector2Int> CurrentGridSpace()
    {
        await Awaitable.MainThreadAsync();
        Vector3Int currentGridSpace3D = levelGrid.WorldToCell(playerController.transform.position);
        return new Vector2Int(currentGridSpace3D.x, currentGridSpace3D.y);
    }

    private bool IsTerminal()
    {
        return playerController.hasTouchedExit;
    }

    private float RewardDistanceToExit()
    {
        return Vector2Int.Distance(Vector2Int.zero, Level.levelSize.size) -
               Vector2Int.Distance(exitLocation, CurrentGridSpace().GetAwaiter().GetResult());
    }

    private async Task MoveLeft()
    {
        Vector2Int startGridSpace = await CurrentGridSpace();
        await Awaitable.MainThreadAsync();
        int horizontalMovementInputs = 0;
        do
        {
            await Awaitable.FixedUpdateAsync();
            playerController.rigidBody.AddForce(
                playerController.MoveLeft()
            );
            horizontalMovementInputs++;
        } while (startGridSpace == await CurrentGridSpace() && horizontalMovementInputs < maxInputDuration);
    }

    private async Task MoveRight()
    {
        Vector2Int startGridSpace = await CurrentGridSpace();
        await Awaitable.MainThreadAsync();
        int horizontalMovementInputs = 0;
        do
        {
            await Awaitable.FixedUpdateAsync();
            playerController.rigidBody.AddForce(
                playerController.MoveRight()
            );
            horizontalMovementInputs++;
        } while (startGridSpace == await CurrentGridSpace() && horizontalMovementInputs < maxInputDuration);
    }

    private async Task Jump()
    {
        Vector2Int startGridSpace = await CurrentGridSpace();
        await Awaitable.MainThreadAsync();
        if (playerController.IsGrounded())
        {
            int jumpDuration = 0;
            playerController.rigidBody.AddForce(
                playerController.Jump()
            );
            do
            {
                await Awaitable.FixedUpdateAsync();
                jumpDuration++;
            } while (
                startGridSpace == await CurrentGridSpace() &&
                jumpDuration < maxInputDuration
            );
        }
    }

    private async Task ToggleSpecial()
    {
        await Awaitable.MainThreadAsync();
        playerController.ToggleSpecial();
        await Awaitable.FixedUpdateAsync();
    }

    private async Task DoNothing()
    {
        Vector2Int startGridSpace = await CurrentGridSpace();
        await Awaitable.MainThreadAsync();
        int doNothingUpdates = 0;
        do
        {
            await Awaitable.FixedUpdateAsync();
            doNothingUpdates++;
        } while (startGridSpace == await CurrentGridSpace() && doNothingUpdates < maxInputDuration);
    }
}
