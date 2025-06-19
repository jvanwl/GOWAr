using System.Collections;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

public class Unit : MonoBehaviour {
    public UnitType type;
    public Faction faction;
    public HexTile currentTile;
    public int health = 100;
    public int attack = 10;
    public int defense = 5;
    public int movementRange = 3;

    public void MoveTo(HexTile target) {
        StartCoroutine(MoveAlongPath(GameManager.Instance.pathfinder.FindPath(currentTile, target)));
    }

    IEnumerator MoveAlongPath(List<HexTile> path) {
        foreach (HexTile tile in path) {
            if (tile.currentUnit != null) {
                if (tile.currentUnit.faction != faction) {
                    CombatSystem.ResolveCombat(this, tile.currentUnit);
                    yield break;
                }
                continue;
            }

            transform.position = tile.transform.position;
            currentTile.currentUnit = null;
            currentTile = tile;
            tile.currentUnit = this;
            yield return new WaitForSeconds(0.2f);
        }
    }
}

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
        var cameFrom = new NativeArray<int>(nodes.Length, Allocator.Temp);
        for (int i = 0; i < cameFrom.Length; i++) cameFrom[i] = -1;

        var gScore = new NativeArray<float>(nodes.Length, Allocator.Temp);
        for (int i = 0; i < gScore.Length; i++) gScore[i] = float.MaxValue;
        gScore[startNodeIndex] = 0;

        var fScore = new NativeArray<float>(nodes.Length, Allocator.Temp);
        for (int i = 0; i < fScore.Length; i++) fScore[i] = float.MaxValue;
        fScore[startNodeIndex] = HeuristicCostEstimate(startNodeIndex, endNodeIndex);

        openSet.Add(startNodeIndex);

        while (openSet.Length > 0)
        {
            // Find node in openSet with lowest fScore
            int current = openSet[0];
            float lowestF = fScore[current];
            for (int i = 1; i < openSet.Length; i++)
            {
                if (fScore[openSet[i]] < lowestF)
                {
                    current = openSet[i];
                    lowestF = fScore[current];
                }
            }

            if (current == endNodeIndex)
            {
                ReconstructPath(cameFrom, current, path);
                break;
            }

            openSet.RemoveAt(openSet.IndexOf(current));
            closedSet.Add(current);

            foreach (var neighborIndex in nodes[current].neighbors)
            {
                if (!nodes[neighborIndex].walkable || closedSet.Contains(neighborIndex))
                    continue;

                float tentativeG = gScore[current] + nodes[current].CostTo(nodes[neighborIndex]);
                if (!openSet.Contains(neighborIndex))
                    openSet.Add(neighborIndex);
                else if (tentativeG >= gScore[neighborIndex])
                    continue;

                cameFrom[neighborIndex] = current;
                gScore[neighborIndex] = tentativeG;
                fScore[neighborIndex] = gScore[neighborIndex] + HeuristicCostEstimate(neighborIndex, endNodeIndex);
            }
        }

        openSet.Dispose();
        closedSet.Dispose();
        cameFrom.Dispose();
        gScore.Dispose();
        fScore.Dispose();
    }

    float HeuristicCostEstimate(int fromIndex, int toIndex)
    {
        // Example: Manhattan distance (replace with your own heuristic if needed)
        var from = nodes[fromIndex];
        var to = nodes[toIndex];
        return math.abs(from.x - to.x) + math.abs(from.y - to.y);
    }

    void ReconstructPath(NativeArray<int> cameFrom, int current, NativeList<int> outPath)
    {
        outPath.Clear();
        while (current != -1)
        {
            outPath.Add(current);
            current = cameFrom[current];
        }
        // Path is from end to start, reverse if needed
        for (int i = 0, j = outPath.Length - 1; i < j; i++, j--)
        {
            int temp = outPath[i];
            outPath[i] = outPath[j];
            outPath[j] = temp;
        }
    }
}

public struct PathNode
{
    public int index;
    public int x, y;
    public bool walkable;
    public NativeArray<int> neighbors; // Indices of neighbor nodes

    public float CostTo(PathNode other)
    {
        // Example: cost is always 1, customize as needed
        return 1f;
    }
}
