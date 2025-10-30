using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MapSearch
{
    private Map map;
    public List<Tile> path = new List<Tile>();

    public void Init(Map map)
    {
        this.map = map;
    }

    protected int Heuristic(Tile a, Tile b)
    {
        int ax = a.id % map.cols;
        int ay = a.id / map.cols;

        int bx = b.id % map.cols;
        int by = b.id / map.cols;

        return Mathf.Abs(ax - bx) + Mathf.Abs(ay - by);
    }

    public List<Tile> AStarMap(Tile start, Tile goal)
    {
        path.Clear();
        map.ResetTilesPrevious();

        var visited = new HashSet<Tile>();
        var pQueue = new PriorityQueue<Tile, int>();
        var distances = new int[map.tiles.Length];
        var scores = new int[map.tiles.Length];
        for (int i = 0; i < distances.Length; ++i)
        {
            scores[i] = distances[i] = int.MaxValue;
        }

        distances[start.id] = start.Weight;
        scores[start.id] = distances[start.id] + Heuristic(start, goal);
        pQueue.Enqueue(start, scores[start.id]);

        bool success = false;
        while (pQueue.Count > 0)
        {
            var currentTile = pQueue.Dequeue();
            if (visited.Contains(currentTile))
                continue;
            if (currentTile == goal)
            {
                success = true;
                break;
            }

            visited.Add(currentTile);
            foreach (var adjacent in currentTile.adjacents)
            {
                if (adjacent == null)
                    continue;
                if (!adjacent.CanMove || visited.Contains(adjacent))
                {
                    continue;
                }

                var newDistance = distances[currentTile.id] + adjacent.Weight;
                if (distances[adjacent.id] > newDistance)
                {
                    distances[adjacent.id] = newDistance;
                    scores[adjacent.id] = distances[adjacent.id] + Heuristic(adjacent, goal);
                    adjacent.previous = currentTile;

                    pQueue.Enqueue(adjacent, scores[adjacent.id]);
                }
            }
        }

        if (!success)
        {
            return path;
        }

        Tile step = goal;
        while (step != null)
        {
            path.Add(step);
            step = step.previous;
        }

        path.Reverse();
        return path;
    }

    public bool AStarCastle(Tile start, Tile goal)
    {
        path.Clear();
        map.ResetTilesPrevious();

        var visited = new HashSet<Tile>();
        var pQueue = new PriorityQueue<Tile, int>();
        var distances = new int[map.tiles.Length];
        var scores = new int[map.tiles.Length];
        for (int i = 0; i < distances.Length; ++i)
        {
            scores[i] = distances[i] = int.MaxValue;
        }

        distances[start.id] = start.Weight;
        scores[start.id] = distances[start.id] + Heuristic(start, goal);
        pQueue.Enqueue(start, scores[start.id]);

        bool success = false;
        while (pQueue.Count > 0)
        {
            var currentTile = pQueue.Dequeue();
            if (visited.Contains(currentTile))
                continue;
            if (currentTile == goal)
            {
                success = true;
                break;
            }

            visited.Add(currentTile);
            foreach (var adjacent in currentTile.adjacents)
            {
                if (adjacent == null)
                    continue;
                if (!adjacent.CanMove || visited.Contains(adjacent))
                {
                    continue;
                }

                var newDistance = distances[currentTile.id] + adjacent.Weight;
                if (distances[adjacent.id] > newDistance)
                {
                    distances[adjacent.id] = newDistance;
                    scores[adjacent.id] = distances[adjacent.id] + Heuristic(adjacent, goal);
                    adjacent.previous = currentTile;

                    pQueue.Enqueue(adjacent, scores[adjacent.id]);
                }
            }
        }

        if (!success)
        {
            return false;
        }

        Tile step = goal;
        while (step != null)
        {
            path.Add(step);
            step = step.previous;
        }

        path.Reverse();
        return true;
    }
}
