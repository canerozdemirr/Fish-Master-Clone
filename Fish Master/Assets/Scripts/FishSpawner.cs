using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    [SerializeField] private Fish fishPrefab;
    [SerializeField] private Fish.FishType[] fishTypes;
    private void Awake()
    {
        foreach (var t in fishTypes)
        {
            var fishNumber = 0;
            while (fishNumber < t.fishCount)
            {
                var fish = Instantiate(fishPrefab);
                fish.fishType = t;
                fish.RefreshFish();
                fishNumber++;
            }
        }
    }
}
