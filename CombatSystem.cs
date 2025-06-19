using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

public static class CombatSystem {
    public static void ResolveCombat(Unit attacker, Unit defender) {
        float terrainBonus = defender.currentTile.defenseBonus * 0.1f;
        float typeAdvantage = GetTypeAdvantage(attacker.type, defender.type);
        int damage = Mathf.RoundToInt(attacker.attack * typeAdvantage - defender.defense * (1 + terrainBonus));

        defender.health -= Mathf.Max(1, damage);
        if (defender.health <= 0) {
            Object.Destroy(defender.gameObject);
            attacker.MoveTo(defender.currentTile);
        }
    }

    static float GetTypeAdvantage(UnitType attacker, UnitType defender) {
        if (attacker == UnitType.Cavalry && defender == UnitType.Infantry) return 1.5f;
        if (attacker == UnitType.Infantry && defender == UnitType.Archer) return 1.5f;
        if (attacker == UnitType.Archer && defender == UnitType.Cavalry) return 1.5f;
        return 1.0f;
    }
}

/// <summary>
/// Burst-compiled A* pathfinding job for hex grids.
/// </summary>
[BurstCompile]
public struct PathfindingJob : IJob
{
    [ReadOnly] public NativeArray<PathNode> nodes;
    [ReadOnly] public int startNodeIndex;
    [ReadOnly] public int endNodeIndex;
    public NativeList<int> path; // Output: indices of nodes in the path
    public NativeArray<bool> foundPath; // Output: true if path found

    public void Execute()
    {
        var openSet = new NativeList<int>(Allocator.Temp);
        var closedSet = new NativeHashSet<int>(nodes.Length, Allocator.Temp);
        var cameFrom = new NativeArray<int>(nodes.Length, Allocator.Temp, NativeArrayOptions.ClearMemory);

        var gScore = new NativeArray<float>(nodes.Length, Allocator.Temp, NativeArrayOptions.ClearMemory);
        var fScore = new NativeArray<float>(nodes.Length, Allocator.Temp, NativeArrayOptions.ClearMemory);

        for (int i = 0; i < nodes.Length; i++)
        {
            gScore[i] = float.MaxValue;
            fScore[i] = float.MaxValue;
            cameFrom[i] = -1;
        }

        gScore[startNodeIndex] = 0;
        fScore[startNodeIndex] = HeuristicCostEstimate(startNodeIndex, endNodeIndex);

        openSet.Add(startNodeIndex);

        bool pathFound = false;

        while (openSet.Length > 0)
        {
            // Find node in openSet with lowest fScore
            int current = openSet[0];
            float lowestF = fScore[current];
            for (int i = 1; i < openSet.Length; i++)
            {
                int idx = openSet[i];
                if (fScore[idx] < lowestF)
                {
                    current = idx;
                    lowestF = fScore[idx];
                }
            }

            if (current == endNodeIndex)
            {
                ReconstructPath(cameFrom, current, path);
                pathFound = true;
                break;
            }

            openSet.RemoveAt(openSet.IndexOf(current));
            closedSet.Add(current);

            var node = nodes[current];
            for (int n = 0; n < node.neighborCount; n++)
            {
                int neighborIndex = node.GetNeighbor(n);
                if (neighborIndex < 0 || neighborIndex >= nodes.Length)
                    continue;
                if (!nodes[neighborIndex].walkable || closedSet.Contains(neighborIndex))
                    continue;

                float tentativeG = gScore[current] + node.CostTo(nodes[neighborIndex]);
                if (!openSet.Contains(neighborIndex))
                    openSet.Add(neighborIndex);
                else if (tentativeG >= gScore[neighborIndex])
                    continue;

                cameFrom[neighborIndex] = current;
                gScore[neighborIndex] = tentativeG;
                fScore[neighborIndex] = tentativeG + HeuristicCostEstimate(neighborIndex, endNodeIndex);
            }
        }

        foundPath[0] = pathFound;

        openSet.Dispose();
        closedSet.Dispose();
        cameFrom.Dispose();
        gScore.Dispose();
        fScore.Dispose();
    }

    /// <summary>
    /// Hex grid heuristic: cube distance.
    /// </summary>
    float HeuristicCostEstimate(int fromIndex, int toIndex)
    {
        var from = nodes[fromIndex];
        var to = nodes[toIndex];
        int dx = from.x - to.x;
        int dy = from.y - to.y;
        int dz = (-from.x - from.y) - (-to.x - to.y);
        return (math.abs(dx) + math.abs(dy) + math.abs(dz)) / 2f;
    }

    void ReconstructPath(NativeArray<int> cameFrom, int current, NativeList<int> outPath)
    {
        outPath.Clear();
        while (current != -1)
        {
            outPath.Add(current);
            current = cameFrom[current];
        }
        // Reverse path
        for (int i = 0, j = outPath.Length - 1; i < j; i++, j--)
        {
            int temp = outPath[i];
            outPath[i] = outPath[j];
            outPath[j] = temp;
        }
    }
}

/// <summary>
/// Blittable struct for pathfinding nodes.
/// neighbors: fixed-size array of neighbor indices (e.g., length 6 for hex).
/// neighborCount: actual number of neighbors.
/// </summary>
public unsafe struct PathNode
{
    public int index;
    public int x, y;
    public bool walkable;
    public fixed int neighbors[6]; // For hex grid, 6 neighbors max
    public int neighborCount;

    public int GetNeighbor(int i)
    {
        fixed (int* n = neighbors)
            return n[i];
    }

    public float CostTo(PathNode other)
    {
        // Example: cost is always 1, customize as needed
        return 1f;
    }
}
