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
    private double highScore;
    private double score;
    private int action;
    private List<int> trajectory = new List<int>();
    private readonly Dictionary<int, Cell> archive = new Dictionary<int, Cell>();
    private Cell restoreCell;
    private readonly int maxTrajectoryLength = 100;
    private readonly int maxAttempts = 100;

    private readonly Random rng;

    public GoExplore(SimulationInstance env)
    {
        this.env = env;
        rng = new Random();
        action = SelectRandomAction();
    }

    private int SelectRandomAction()
    {
        return env.actionSpace[rng.Next(0, env.actionSpace.Length)];
    }

    public int Run()
    {
        bool isTerminal = false;
        for (int i = 0; i < maxAttempts; i++)
        {
            isTerminal = RolloutAction();
            if (isTerminal)
            {
                break;
            }
        }

        int archiveCount = isTerminal ? archive.Count : int.MaxValue;
        Debug.Log(isTerminal
            ? $"{env.ID} Ended running GoExplore by finding level exit after {iteration} iterations visiting {archiveCount} cells"
            : $"{env.ID} Ended running GoExplore without finding level exit after {iteration} iterations"
        );
        return archiveCount;
    }

    private bool RolloutAction()
    {
        int lastActionResultHash = 0;
        for (int i = 0; i < maxTrajectoryLength; i++)
        {
            iteration++;

            SimulationInstance.StepResult actionResult = env.Step(action, iteration);
            Debug.Log($"actionResult {actionResult}");

            trajectory.Add(action);

            score += actionResult.reward;

            if (score > highScore)
            {
                highScore = score;
            }

            if (actionResult.isTerminal)
            {
                return true;
            }

            Cell cell = new Cell(actionResult.playerGridPosition);
            archive[cell.GetHashCode()] = cell;
            bool isFirstVisit = cell.Visit();
            if (isFirstVisit || score >= cell.reward && trajectory.Count < cell.trajectory.Count)
            {
                cell.reward = score;
                cell.trajectory = new List<int>(trajectory);
                cell.cellStats.timesChosen = 0;
                cell.cellStats.timesChosenSinceNew = 0;
            }

            if (actionResult.GetHashCode() == lastActionResultHash)
            {
                action = SelectRandomAction();
            } else if (rng.NextDouble() > 0.95)
            {
                action = SelectRandomAction();
            }

            lastActionResultHash = actionResult.GetHashCode();
        }

        restoreCell = SelectCellToRestore(archive, rng);
        if (restoreCell != null)
        {
            // Debug.Log($"{env.ID} Restore state. Cell: {restoreCell}");
            restoreCell.Choose();
            trajectory = restoreCell.trajectory;
            score = restoreCell.reward;
            env.TeleportPlayer(restoreCell.gridPosition);

            // Replay all actions in trajectory in order.
            foreach (int trajectoryAction in trajectory)
            {
                env.Step(trajectoryAction, iteration);
            }
        }

        return false;
    }

    private class Cell
    {
        public CellStats cellStats;
        public double reward;
        public List<int> trajectory = new List<int>();

        public readonly Vector2Int gridPosition;

        public Cell(Vector2Int playerGridPosition)
        {
            cellStats = new CellStats(0, 0, 0);
            gridPosition = playerGridPosition;
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

        public bool Visit()
        {
            cellStats.timesSeen += 1;
            return (int)cellStats.timesSeen == 1;
        }

        public void Choose()
        {
            cellStats.timesChosen += 1;
            cellStats.timesChosenSinceNew += 1;
        }

        public override String ToString() => $"player grid space {gridPosition}, trajectory size: {trajectory.Count}";

        public override int GetHashCode() => gridPosition.GetHashCode();
    }

    private struct CellStats
    {
        public double timesChosen;
        public double timesChosenSinceNew;
        public double timesSeen;

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
