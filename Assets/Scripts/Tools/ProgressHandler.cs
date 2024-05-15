using Code.DropLogic;
using System.Collections.Generic;
using UnityEngine;
using static Code.SaveLoad.ProgressData;

namespace Code.SaveLoad
{
    public class ProgressHandler
    {
        private readonly List<DropSave> _drops = new();

        public List<DropSave> Drops => _drops;

        public void FillData(DropBase drop)
        {
            for(int i = 0; i < _drops.Count; i++)
            {
                if (_drops[i].Rank == drop.Rank && _drops[i].Position == drop.Pos)
                    return;
            }
            _drops.Add(new DropSave(drop.Rank, drop.transform.position));
        }

        public void AddKey(Transform transform) => 
            _drops.Add(new DropSave(Constants.KeyRank, transform.position));

        public void Clear() => _drops.Clear();
    }
}