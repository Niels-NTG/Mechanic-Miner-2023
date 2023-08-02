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
    private static readonly float e1 = 0.001f;
    private static readonly float e2 = 0.00001f;

    private int frameCount;
    private int iterations;
    private float highScore;
    private float score;
    private int action;
    private List<int> trajectory = new List<int>();
    private readonly Dictionary<int, Cell> archive = new Dictionary<int, Cell>();
    private Cell restoreCell;

    private readonly int debugSeed = 383823008;
    private readonly Random rng;

    public GoExplore(SimulationInstance env)
    {
        this.env = env;
        rng = new Random(debugSeed);
        action = SelectRandomAction();
    }

    private int SelectRandomAction()
    {
        return env.actionSpace[rng.Next(0, env.actionSpace.Length)];
    }

    public async void Run()
    {
        int lastActionHash = 0;
        for (int i = 0; i < 100; i++)
        {
            SimulationInstance.StepResult result = await env.Step(action);
            Debug.Log(result);
            if (result.GetHashCode() == lastActionHash)
            {
                action = SelectRandomAction();
                continue;
            }
            if (rng.NextDouble() > 0.95)
            {
                action = SelectRandomAction();
                continue;
            }

            trajectory.Add(action);

            score += result.reward;
            frameCount += result.frameNumber;

            if (score > highScore)
            {
                highScore = score;
            }

            if (result.isTerminal)
            {
                break;
            }

            Cell cell = new Cell(result.playerGridPosition);
            archive[cell.GetHashCode()] = cell;
            bool isFirstVisit = cell.Visit();
            if (isFirstVisit || score >= cell.reward && trajectory.Count < cell.trajectory.Count)
            {
                cell.reward = score;
                cell.trajectory = new List<int>(trajectory);
                cell.cellStats.timesChosen = 0;
                cell.cellStats.timesChosenSinceNew = 0;
            }

            lastActionHash = result.GetHashCode();
        }

        restoreCell = SelectCellToRestore(archive, rng);
        if (restoreCell != null)
        {
            restoreCell.Choose();
            trajectory = restoreCell.trajectory;
            score = restoreCell.reward;
            env.ResetPlayer();
            // Replay all actions in trajectory in order.
            foreach (int trajectoryAction in trajectory)
            {
                await env.Step(trajectoryAction);
            }
        }

        iterations++;
    }

    private class Cell
    {
        public CellStats cellStats;
        public float reward;
        public List<int> trajectory = new List<int>();

        private readonly Vector2Int gridPosition;

        public Cell(Vector2Int playerGridPosition)
        {
            cellStats = new CellStats(0, 0, 0);

            gridPosition = playerGridPosition;
        }

        private float CNTScore(float w, float p, float v)
        {
            return MathF.Pow(w / (v + e1), p + e2);
        }

        private float CNTScoreTimesChosen()
        {
            return CNTScore(
                Weights.timesChosen,
                Powers.timesChosen,
                cellStats.timesChosen
            );
        }

        private float CNTScoreTimesChosenSinceNew()
        {
            return CNTScore(
                Weights.timesChosenSinceNew,
                Powers.timesChosenSinceNew,
                cellStats.timesChosenSinceNew
            );
        }

        private float CNTScoreTimesSeen()
        {
            return CNTScore(
                Weights.timesSeen,
                Powers.timesSeen,
                cellStats.timesSeen
            );
        }

        public float CellScore()
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

        public override int GetHashCode()
        {
            return gridPosition.GetHashCode();
        }
    }

    private struct CellStats
    {
        public float timesChosen;
        public float timesChosenSinceNew;
        public float timesSeen;

        public CellStats(float timesChosen, float timesChosenSinceNew, float timesSeen)
        {
            this.timesChosen = timesChosen;
            this.timesChosenSinceNew = timesChosenSinceNew;
            this.timesSeen = timesSeen;
        }
    }

    private static float CellSummedScores(List<Cell> cellList)
    {
        float sum = 0;
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

        float cellSummedScores = CellSummedScores(cellList);
        List<Cell> shuffledList = cellList.OrderBy(_ => rng.Next()).ToList();
        foreach (Cell cell in shuffledList)
        {
            double cellProbability = cell.reward / cellSummedScores;
            double value = rng.NextDouble();
            if (value < cellProbability)
            {
                return cell;
            }
        }

        return null;
    }
}
