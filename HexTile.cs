using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Burst-compiled A* pathfinding job for hex grids.
/// Assumes PathNode.neighbors is a fixed-size array of neighbor indices (e.g., length 6 for hex).
/// </summary>
[BurstCompile]
public struct PathfindingJob : IJob
{
    [ReadOnly] public NativeArray<PathNode> nodes;
    [ReadOnly] public int startNodeIndex;
    [ReadOnly] public int endNodeIndex;
    public NativeList<int> path; // Output: indices of nodes in the path

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
                break;
            }

            openSet.RemoveAt(openSet.IndexOf(current));
            closedSet.Add(current);

            var node = nodes[current];
            for (int n = 0; n < node.neighborCount; n++)
            {
                int neighborIndex = node.neighbors[n];
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
        // Cube coordinates for hex grids
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
public struct PathNode
{
    public int index;
    public int x, y;
    public bool walkable;
    public fixed int neighbors[6]; // For hex grid, 6 neighbors max
    public int neighborCount;

    public float CostTo(PathNode other)
    {
        // Example: cost is always 1, customize as needed
        return 1f;
    }
}

public class HexTile : MonoBehaviour {
    public int x, y;
    public BiomeType biome;
    public Faction owner;
    public Unit currentUnit;
    public int defenseBonus;

    public void Init(int x, int y, BiomeType biome) {
        this.x = x;
        this.y = y;
        this.biome = biome;
        defenseBonus = (biome == BiomeType.Mountain) ? 2 : 0;
        GetComponent<SpriteRenderer>().color = GetBiomeColor();
    }

    Color GetBiomeColor() {
        switch(biome) {
            case BiomeType.Forest: return new Color(0.1f, 0.6f, 0.1f);
            case BiomeType.Mountain: return Color.gray;
            case BiomeType.Desert: return new Color(0.9f, 0.8f, 0.5f);
            default: return new Color(0.6f, 0.9f, 0.6f);
        }
    }

    void OnMouseDown() {
        if (GameManager.Instance.selectedUnit != null) {
            GameManager.Instance.MoveSelectedUnit(this);
        }
    }
}
