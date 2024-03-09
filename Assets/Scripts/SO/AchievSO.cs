using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(AchievSO), menuName = "Scriptable/AchievementSO")]
public class AchievSO : ScriptableObject
{
    [SerializeField] private List<AchievBlock> _achievsByType;

    public List<AchievBlock> AchievsByType => _achievsByType;

    public AchievBlock FindAchiev(AchievType type)
    {
        var result = AchievsByType.Find( x => x.AchievementType == type);
        if (result == null)
            Debug.LogError($"{name} : achiev block of type {type} was not found");
        return result;
    }

    [Serializable]
    public class AchievBlock
    {
        public AchievType AchievementType;
        public List<Achievement> Achievements;

        [Serializable]
        public class Achievement
        {
            public string Name;
            public int AchievID;
            public int RewardIndex;
            public bool HasProgress;
            public float ReferenceValue;
            public int Condition;
            public bool IsTotal;

            public bool IsUnlocked { get; set; }
            public int SavedProgress { get; set; }
        }
    }
}