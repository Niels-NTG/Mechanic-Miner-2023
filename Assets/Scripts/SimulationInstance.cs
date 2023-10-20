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
    private readonly int maxInputDuration = 10;

    private readonly float lowerResetRewardBound = -2000;

    // Move left, move right, jump, special
    public readonly int[] actionSpace =
    {
        0,
        1,
        2,
        3
    };

    private readonly int tgmSeed = 0;
    private readonly int levelGeneratorSeed = 877;

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
        Random levelGeneratorRNG = levelGeneratorSeed == 0 ? new Random() : new Random(levelGeneratorSeed);
        LevelGenerator levelGenerator = simulationInstanceController.levelGenerator;
        levelGenerator.Generate(levelGeneratorRNG);

        // Get level properties relevant for running the simulation
        levelGrid = levelGenerator.GetComponent<Grid>();
        entryLocation = levelGenerator.entryLocation;
        exitLocation = levelGenerator.exitLocation;

        // Instantiate player agent and place at level entry position.
        playerController = simulationInstanceController.playerAgent.GetComponent<PlayerController>();
        ResetPlayer();

        // Make UUID label visible on player in the scene
        simulationInstanceController.playerDebugLabel.text = ID;

        // Create TGM from toggleable properties on level generator and player instance.
        List<Component> componentsWithToggleableProperties = new List<Component>();
        componentsWithToggleableProperties.AddRange(levelGenerator.componentsWithToggleableProperties);
        componentsWithToggleableProperties.AddRange(playerController.componentsWithToggleableProperties);
        Random tgmRNG = tgmSeed == 0 ? new Random() : new Random(tgmSeed);
        tgm = new ToggleableGameMechanic(componentsWithToggleableProperties, tgmRNG);
    }

    ~SimulationInstance()
    {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        UnloadScene();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
    }

    public void SetTGM(ToggleableGameMechanic.ToggleGameMechanicGenotype toggleGameMechanicGenotype)
    {
        // Generate new TGM if genotype data structure is empty
        if (Equals(toggleGameMechanicGenotype, default(ToggleableGameMechanic.ToggleGameMechanicGenotype)))
        {
            tgm.GenerateNew();
            Debug.Log($"{ID} SimulationInstance: created new TGM {tgm}");
        }
        else
        {
            tgm.GenerateFromGenotype(toggleGameMechanicGenotype);
            Debug.Log($"{ID} SimulationInstance: generated TGM from genotype {tgm}");
        }
        playerController.toggleableGameMechanic = tgm;

        ResetPlayer();
    }

    private async Task UnloadScene()
    {
        Debug.Log($"{ID} SimulationInstance: unloading scene");
        await Awaitable.MainThreadAsync();
        await SceneManager.UnloadSceneAsync(scene);
    }

    public async void ResetPlayer()
    {
        await Awaitable.MainThreadAsync();
        TeleportPlayer(new Vector3(entryLocation.x, entryLocation.y, 0) + new Vector3(0.5f, 0.5f, 0));
        playerController.ResetPlayer();
    }

    public void TeleportPlayer(Vector2Int pos)
    {
        TeleportPlayer(new Vector3(pos.x, pos.y, 0));
    }

    private async void TeleportPlayer(Vector3 pos)
    {
        await Awaitable.MainThreadAsync();
        playerController.gameObject.SetActive(true);
        playerController.transform.position = pos;
    }


    public StepResult Step(int action, int iteration)
    {
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
            actionTask.Wait();
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

        return new StepResult(ID, resultGridSpace, action, iteration, reward, isTerminal);
    }

    public readonly struct StepResult
    {
        private readonly String UUID;
        public readonly Vector2Int playerGridPosition;
        private readonly int iteration;
        public readonly float reward;
        public readonly bool isTerminal;
        private readonly int actionTaken;
        public StepResult(String UUID, Vector2Int playerGridPosition, int action, int iteration, float reward, bool isTerminal)
        {
            this.UUID = UUID;
            this.playerGridPosition = playerGridPosition;
            this.iteration = iteration;
            this.reward = reward;
            this.isTerminal = isTerminal;
            actionTaken = action;
        }

        public override String ToString() => $"{UUID}, player grid space: {playerGridPosition}, action: {actionTaken}, iteration: {iteration}, reward: {reward}, isTerminal: {isTerminal}";

        public override int GetHashCode() => playerGridPosition.GetHashCode() + (int) reward + actionTaken;
    }

    private async Task<Vector2Int> CurrentGridSpace()
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
        return Vector2Int.Distance(Vector2Int.zero, LevelGenerator.levelSize.size) -
               Vector2Int.Distance(exitLocation, CurrentGridSpace().GetAwaiter().GetResult());
    }

    private async Task MoveLeft()
    {
        Vector2Int startGridSpace = await CurrentGridSpace();
        await Awaitable.MainThreadAsync();
        int horizontalMovementInputs = 0;
        do
        {
            playerController.rigidBody.AddForce(
                playerController.MoveLeft()
            );
            horizontalMovementInputs++;
            await Awaitable.FixedUpdateAsync();
        } while (startGridSpace == await CurrentGridSpace() && horizontalMovementInputs < maxInputDuration);
    }

    private async Task MoveRight()
    {
        Vector2Int startGridSpace = await CurrentGridSpace();
        await Awaitable.MainThreadAsync();
        int horizontalMovementInputs = 0;
        do
        {
            playerController.rigidBody.AddForce(
                playerController.MoveRight()
            );
            horizontalMovementInputs++;
            await Awaitable.FixedUpdateAsync();
        } while (startGridSpace == await CurrentGridSpace() && horizontalMovementInputs < maxInputDuration);
    }

    private async Task Jump()
    {
        Vector2Int startGridSpace = await CurrentGridSpace();
        await Awaitable.MainThreadAsync();
        int jumpWaitTimer = 0;
        playerController.rigidBody.AddForce(
            playerController.Jump()
        );
        do
        {
            jumpWaitTimer++;
            await Awaitable.FixedUpdateAsync();
        } while (startGridSpace == await CurrentGridSpace() && jumpWaitTimer < maxInputDuration);
    }

    private async Task ToggleSpecial()
    {
        await Awaitable.MainThreadAsync();
        playerController.ToggleSpecial();
        await Awaitable.FixedUpdateAsync();
    }

    private async Task DoNothing()
    {
        for (int i = 0; i < maxInputDuration; i++)
        {
            await Awaitable.FixedUpdateAsync();
        }
    }
}
