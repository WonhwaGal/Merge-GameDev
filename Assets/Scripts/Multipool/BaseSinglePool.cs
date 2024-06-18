using System.Collections.Generic;
using UnityEngine;

namespace Code.Pools
{
    internal abstract class BaseSinglePool<T> where T : MonoBehaviour, ISpawnable
    {
        protected readonly Stack<T> _inactives = new();
        protected readonly Factory<T> _factory;

        public BaseSinglePool(T prefab)
        {
            _factory = new SingleFactory<T>(prefab);
            var root = new GameObject(prefab.name);
            _factory.RootTransform = root.transform;
        }

        public T Spawn()
        {
            T result;
            if (_inactives.Count > 0)
                result = CheckResult();
            else
                result = _factory.Create();

            OnSpawn(result);
            return result;
        }

        public void Despawn(T prefab)
        {
            if (prefab == null)
                return;
            prefab.gameObject.SetActive(false);
            _inactives.Push(prefab);
        }

        public void ReturnToRoot(T prefab) => prefab.transform.SetParent(_factory.RootTransform);

        private void OnSpawn(T prefab)
        {
            if (_factory.RootTransform != null)
                prefab.transform.SetParent(_factory.RootTransform);
            prefab.gameObject.SetActive(true);
        }

        private T CheckResult()
        {
            var applicant = _inactives.Peek();
            if (applicant.gameObject.activeInHierarchy)
            {
                _inactives.Pop();
                Debug.LogWarning("MERGE POOL: popped result is not deactivated");
                return _factory.Create();
            }
            else
            {
                return _inactives.Pop();
            }
        }

        public void Dispose() => _inactives.Clear();
    }
}