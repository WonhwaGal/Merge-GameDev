using System;
using UnityEngine;
using Code.Pools;
using Code.SaveLoad;
using GamePush;
using Code.Achievements;

namespace Code.DropLogic
{
    public sealed class DropService : IService
    {
        private readonly FXMultiPool _fxPool;
        private readonly DropObjectMultipool _pool;
        private readonly UIService _uiService;
        private readonly int _dropableRanks;
        private readonly AchievementService _achievService;
        private readonly KeyBubble _keyBubblePrefab;

        public DropService(DropObjectSO dropSO, EffectList fxList)
        {
            _pool = new(dropSO);
            _fxPool = new(fxList);
            _keyBubblePrefab = dropSO.KeyPrefab;

#if UNITY_EDITOR
            _dropableRanks = Constants.DropableRanks;
#else
            _dropableRanks = GP_Variables.GetInt("DropableRanks");
#endif

            DropQueueHandler.AssignValues(dropSO.TotalNumber, _dropableRanks);
            _uiService = ServiceLocator.Container.RequestFor<UIService>();
            _achievService = ServiceLocator.Container.RequestFor<AchievementService>();
            GameEventSystem.Subscribe<ManageDropEvent>(EndSession);
            _fxPool.Prespawn(PrefabType.PoofEffect, 3);
            WinGameHandler.KeyPrefab = dropSO.KeyPrefab;
        }

        public KeyBubble KeySpawned { get; private set; }

        public void RecreateProgress(ProgressData data)
        {
            for (int i = 0; i < data.SavedDropList.Count; i++)
            {
                //check for Key
                if (data.SavedDropList[i].Rank == Constants.KeyRank)
                    KeySpawned = GameObject.Instantiate(
                        _keyBubblePrefab, data.SavedDropList[i].Position, Quaternion.identity);
                var result = _pool.Spawn(data.SavedDropList[i].Rank);
                SetUpDropObject(result, data.SavedDropList[i].Position, true, true);
            }
            _uiService.SetCurrentScore();
        }

        public DropBase CreateDropObject(Transform transform, bool random)
        {
            DropBase result;
            if (random)
                result = _pool.Spawn(DropQueueHandler.ChooseRank());
            else
                result = _pool.Spawn(Constants.BombRank);
            return SetUpDropObject(result, transform.position, true, false);
        }


        public void MergeDrops(DropBase one, DropBase two)
        {
            var finalRank = one.Rank == DropQueueHandler.MaxRank;
            if (finalRank)
                HandleTopMerge(one, two);
            else
                MergeObjects(one, two);
            MergeCounter.ReceiveMergeInfo(one.Rank);
        }

        public void MergeObjects(DropBase upperOne, DropBase lowerOne)
        {
            var result = _pool.Spawn(upperOne.Rank + 1);
            var middlePos = (upperOne.transform.position + lowerOne.transform.position) / 2;
            AddEffect(PrefabType.PoofEffect, result, middlePos);
            SetUpDropObject(result, middlePos, false, true);
            ReturnPairToPool(upperOne, lowerOne);
            if (upperOne.Rank >= _dropableRanks)
                GameEventSystem.Send(new MergeEvent(upperOne.Rank));
            _achievService.CheckAchievement(AchievType.MergeByRank, lowerOne.Rank);
        }

        private void AddEffect(PrefabType effectType, DropBase result, Vector3 spawnPos)
        {
            var effect = _fxPool.Spawn(effectType);
            _fxPool.OnSpawned(effect, spawnPos);
            effect.transform.localScale = result.transform.localScale;
        }

        private DropBase SetUpDropObject(DropBase result, Vector3 position, bool queueMoved, bool shouldDrop)
        {
            _pool.OnSpawned(result, position);
            result.gameObject.SetActive(true);
            result.OnMerge += MergeDrops;
            GameEventSystem.Send(new CreateDropEvent(queueMoved, result.Rank));
            if (shouldDrop)
                result.Drop();
            return result;
        }

        private void EndSession(ManageDropEvent @event)
        {
            if (@event.WithEffects)
                AddEffect(PrefabType.PoofEffect, @event.Drop, @event.Drop.Pos);
            if (@event.ReturnToPool)
                ReturnToPool(@event.Drop);
        }

        private void HandleTopMerge(DropBase one, DropBase two)
        {
            var mergePoint = (one.transform.position + two.transform.position) / 2;

            //if (!GP_Player.GetBool("has_key"))
                WinGameHandler.SpawnKey(mergePoint);
            ReturnPairToPool(one, two);
            AddEffect(PrefabType.PoofEffect, one, mergePoint);
            GameEventSystem.Send(new CreateDropEvent(false, one.Rank + 1));
            _achievService.CheckAchievement(AchievType.TopMerge, one.Rank);
        }

        private void ReturnPairToPool(DropBase one, DropBase two)
        {
            ReturnToPool(one);
            ReturnToPool(two);
        }

        private void ReturnToPool(DropBase result)
        {
            _pool.Despawn(result.Rank, result);
            result.OnMerge -= MergeDrops;
        }

        public void Dispose()
        {
            GameEventSystem.Subscribe<ManageDropEvent>(EndSession);
            GC.SuppressFinalize(this);
        }
    }
}