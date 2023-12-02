using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class GoExplore
{
    private readonly SimulationInstance env;

    private static readonly CellStats Weights = new CellStats(0.1f, 0, 0.3f);
    private static readonly CellStats Powers = new CellStats(0.5f, 0.5f, 0.5f);
    private static readonly double e1 = 0.001;
    private static readonly double e2 = 0.00001;

    private int iteration;
    private readonly Dictionary<int, Cell> archive = new Dictionary<int, Cell>();
    private readonly int maxTrajectoryLength = 30;
    private readonly int maxAttempts = 10;

    private readonly Random rng;

    public GoExplore(SimulationInstance env)
    {
        this.env = env;
        rng = new Random();
    }

    public int Run()
    {
        // Reset player to starting position and save this state as a cell to the archive.
        env.ResetPlayer();
        Vector2Int initialPlayerPosition = env.CurrentGridSpace().GetAwaiter().GetResult();
        Cell initialStateCell = new Cell(initialPlayerPosition, env.Step(-1, 0).reward);
        archive[initialStateCell.GetHashCode()] = initialStateCell;

        bool isTerminal = false;
        for (int i = 0; i < maxAttempts; i++)
        {
            // Debug.Log($"{env.ID} GoExplore: start attempt {i + 1}");

            // Restore state from archived cell, selected by weighted distribution.
            Restore();

            // Continue playing even if terminal state has been found, since we want to find how much of level
            // the agent can explore using the current TGM.
            if (RolloutAction())
            {
                isTerminal = true;
            }
        }

        int archiveCount = isTerminal ? archive.Count : int.MinValue;
        Debug.Log(isTerminal
            ? $"{env.ID} GoExplore: Ended running GoExplore by finding level exit after {iteration} iterations visiting {archiveCount} cells"
            : $"{env.ID} GoExplore: Ended running GoExplore without finding level exit after {iteration} iterations"
        );
        return archiveCount;
    }

    private bool RolloutAction()
    {
        // Hash of previous selected action. For the purposes of comparing to prevent certain types of actions
        // from being repeated. The hash takes the player's position, action type and reward as arguments.
        int lastActionResultHash = 0;

        // List of actions taken thus far.
        List<int> trajectory = new List<int>();

        // Initial selected action. See SimulationInstance for a list of possible actions the player can take.
        int action = SelectRandomAction();

        // Keep taking actions until a maximum rollout is reached or when the player reaches a terminal state.
        while (trajectory.Count < maxTrajectoryLength)
        {
            iteration++;

            SimulationInstance.StepResult actionResult = env.Step(action, iteration);
            // Debug.Log($"{env.ID} GoExplore: step result {actionResult}");

            trajectory.Add(action);

            Cell cell = new Cell(actionResult.playerGridPosition, actionResult.reward, trajectory);
            // Add cell to archive if there isn't an entry for this location yet, or if the current cell is better than
            // the existing one at the same location in the level.
            if (!archive.ContainsKey(cell.GetHashCode()) || cell.IsBetterThan(archive[cell.GetHashCode()]))
            {
                archive[cell.GetHashCode()] = cell;
            }
            // Increment the visit count by one.
            cell.Visit();

            // Return if player reaches a terminal state.
            if (actionResult.isTerminal)
            {
                return true;
            }

            // Select a different random action if the current type of action cannot be repeated (as defined in
            // SimulationInstance), the reward is zero or less (caused by if player dies by touching spikes or went
            // too far outside of the level bounds, the result of the action hashes to the same value as the previous
            // taken action, or a 5% random chance.
            if (
                !actionResult.canActionBeRepeated ||
                actionResult.reward <= 0f ||
                actionResult.GetHashCode() == lastActionResultHash ||
                rng.NextDouble() > 0.95
            )
            {
                action = SelectRandomAction(action);
            }

            lastActionResultHash = actionResult.GetHashCode();
        }

        return false;
    }

    private void Restore()
    {
        Cell restoreCell = SelectCellToRestore(archive, rng);
        if (restoreCell != null)
        {
            // Debug.Log($"{env.ID} Restore state. Cell: {restoreCell}");
            restoreCell.Choose();

            // Reset player to start position of the level.
            env.ResetPlayer();

            // Replay all actions in trajectory in order.
            // This way velocity and state of the player is retained when reaching the end of the recorded trajectory.
            foreach (int trajectoryAction in restoreCell.trajectory)
            {
                env.Step(trajectoryAction, iteration);
            }
        }
    }

    private int SelectRandomAction(int excludeAction = -1)
    {
        int result;
        do
        {
            result = SimulationInstance.actionSpace[rng.Next(0, SimulationInstance.actionSpace.Length)];
        } while (result == excludeAction);
        return result;
    }

    private class Cell
    {
        private readonly Vector2Int gridPosition;
        private readonly double reward;

        public readonly List<int> trajectory;

        private CellStats cellStats;

        public Cell(Vector2Int playerGridPosition, double reward, List<int> trajectory = null)
        {
            gridPosition = playerGridPosition;
            this.reward = reward;

            this.trajectory = trajectory ?? new List<int>();

            cellStats = new CellStats(0, 0, 0);
        }

        private double CNTScore(double w, double p, double v)
        {
            return Math.Pow(w / (v + e1), p + e2);
        }

        private double CNTScoreTimesChosen()
        {
            return CNTScore(
                Weights.timesChosen,
                Powers.timesChosen,
                cellStats.timesChosen
            );
        }

        private double CNTScoreTimesChosenSinceNew()
        {
            return CNTScore(
                Weights.timesChosenSinceNew,
                Powers.timesChosenSinceNew,
                cellStats.timesChosenSinceNew
            );
        }

        private double CNTScoreTimesSeen()
        {
            return CNTScore(
                Weights.timesSeen,
                Powers.timesSeen,
                cellStats.timesSeen
            );
        }

        public double CellScore()
        {
            return CNTScoreTimesChosen() + CNTScoreTimesChosenSinceNew() + CNTScoreTimesSeen() + 1;
        }

        public void Visit()
        {
            cellStats.timesSeen += 1;
        }

        public void Choose()
        {
            cellStats.timesChosen += 1;
            cellStats.timesChosenSinceNew += 1;
        }

        public bool IsBetterThan(Cell otherCell)
        {
            return reward > otherCell.reward ||
                   (Math.Abs(reward - otherCell.reward) < e2 && trajectory.Count < otherCell.trajectory.Count);
        }

        public override String ToString() => $"Cell: player grid space: {gridPosition}, reward: {reward}, visited: {cellStats.timesChosen}, trajectory size: {trajectory.Count}";

        public override int GetHashCode() => MathUtils.HashVector2Int(gridPosition);
    }

    private struct CellStats
    {
        internal double timesChosen;
        internal double timesChosenSinceNew;
        internal double timesSeen;

        public CellStats(double timesChosen, double timesChosenSinceNew, double timesSeen)
        {
            this.timesChosen = timesChosen;
            this.timesChosenSinceNew = timesChosenSinceNew;
            this.timesSeen = timesSeen;
        }
    }

    private static double CellSummedScores(List<Cell> cellList)
    {
        double sum = 0;
        foreach (Cell cell in cellList)
        {
            sum += cell.CellScore();
        }
        return sum;
    }

    private static Cell SelectCellToRestore(Dictionary<int, Cell> archive, Random rng)
    {
        List<Cell> cellList = archive.Values.ToList();
        if (cellList.Count == 1)
        {
            return cellList.First();
        }

        double cellSummedScores = CellSummedScores(cellList);
        double value = rng.NextDouble();
        double cumulative = 0.0;
        foreach (Cell cell in cellList)
        {
            double cellProbability = cell.CellScore() / cellSummedScores;
            cumulative += cellProbability;
            if (value < cumulative)
            {
                return cell;
            }
        }

        return null;
    }
}
