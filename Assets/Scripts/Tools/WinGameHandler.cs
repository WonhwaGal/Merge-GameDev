using UnityEngine;

public static class WinGameHandler
{
    public static KeyBubble KeyPrefab { get; set; }

    public static void SpawnKey(Vector3 spawnPos)
    {
        var key = GameObject.Instantiate(KeyPrefab, spawnPos, Quaternion.identity);
        key.gameObject.SetActive(true);
    }
}