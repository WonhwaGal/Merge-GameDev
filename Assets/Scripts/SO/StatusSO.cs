using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(StatusSO), menuName = "Scriptable/StatusSO")]

public class StatusSO : ScriptableObject
{
    [SerializeField] private List<PlayerStatus> _statuses;

    public List<PlayerStatus> Statuses => _statuses;

    public bool TryGetByType(int enumNumber, out PlayerStatus playerStatus)
    {
        var status = (PlayerStatusType)enumNumber;
        playerStatus = Statuses.Find(x => x.Type == status);

        if (playerStatus is not null)
            return true;

        return false;
    }

    [Serializable]
    public class PlayerStatus
    {
        public PlayerStatusType Type;
        public string RussianName;
        public string EnglishName;
        public int Goal;
        public PlayerStatusType Next;
    }
}
