using UnityEngine;

public static class DropQueueHandler
{
    public static int MaxRank { get; set; }
    public static int MaxUsableRank { get; set; }
    public static int NextDrop { get; private set; }

    public static void AssignValues(int totalDrops, int usableRanks)
    {
        MaxRank = totalDrops;
        MaxUsableRank = usableRanks;
        NextDrop = Random.Range(1, MaxUsableRank);
    }

    public static int ChooseRank()
    {
        var currentRank  = NextDrop;
        NextDrop = Random.Range(1, MaxUsableRank);
        return currentRank;
    }
}