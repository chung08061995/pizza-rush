
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DraftUtils
{

    [Serializable]
    public class ObjectCreator<T> where T : Component
    {
        [SerializeField]
        private bool isParentAssignedByCode;

        [SerializeField]
        private bool isItemAssignedByCode;

        [Header("Pool Settings")]
        [ShowIf("@!isParentAssignedByCode", true)]
        [SerializeField]
        private Transform parent;

        [ShowIf("@!isItemAssignedByCode", true)]
        [SerializeField]
        private T prefab;

        [SerializeField]
        [ReadOnly]
        private List<T> inactiveItems = new List<T>();

        [SerializeField]
        [ReadOnly]
        private List<T> activeItems = new List<T>();

        public IPoolObjectFactory<T> Factory { get; set; }

        public IReadOnlyList<T> ActiveItems => activeItems;

        public List<T> PublicActiveItems => activeItems;

        public IReadOnlyList<T> InactiveItems => inactiveItems;

        public void SetItem(T prefab)
        {
            this.prefab = prefab;
        }

        public void SetParent(Transform parent)
        {
            this.parent = parent;
        }

        public void EnsureParentExists(Transform root)
        {
            if (parent == null)
            {
                GameObject gameObject = new GameObject(typeof(T).Name + " Pool");
                gameObject.transform.SetParent(root);
                parent = gameObject.transform;
            }
        }

        public T Spawn()
        {
            //EnsureFactory();
            T val = UnityEngine.Object.Instantiate(prefab, parent);
            activeItems.Add(val);
            return val;
        }

        public void Despawn(T instance)
        {
            if (!(instance == null))
            {
                //EnsureFactory();
                if (activeItems.Remove(instance))
                {
                    //DeactivateInstance(instance);
                    //inactiveItems.Add(instance);
                }
            }
        }

        public void DespawnAll()
        {
            foreach (T item in new List<T>(activeItems))
            {
                Despawn(item);
            }
        }

        public void Prewarm(int totalCount)
        {
            EnsureFactory();
            int num = inactiveItems.Count + activeItems.Count;
            int num2 = totalCount - num;
            if (num2 > 0)
            {
                for (int i = 0; i < num2; i++)
                {
                    T val = CreateNewInstance();
                    DeactivateInstance(val);
                    inactiveItems.Add(val);
                }
            }
        }

        private T GetFromInactivePool()
        {
            if (inactiveItems.Count == 0)
            {
                return null;
            }

            T result = inactiveItems[0];
            inactiveItems.RemoveAt(0);
            return result;
        }

        private T CreateNewInstance()
        {
            return Factory.Create(prefab, parent);
        }

        private void ActivateInstance(T instance)
        {
            Factory.OnGet(instance);
        }

        private void DeactivateInstance(T instance)
        {
            Factory.OnRelease(instance);
        }

        private void EnsureFactory()
        {
            if (Factory == null)
            {
                throw new InvalidOperationException("ObjectPool<" + typeof(T).Name + "> chưa được gán Factory.");
            }
        }

        public IEnumerable<T> CreateItemsWithAction<TData>(IEnumerable<TData> data, Action<T, TData> onCreate)
        {
            foreach (TData datum in data)
            {
                T val = Spawn();
                onCreate(val, datum);
                yield return val;
            }
        }

        public IEnumerable<T> CreateItemsAtPosition(IEnumerable<Vector3> positions)
        {
            Action<T, Vector3> setPositionAction = delegate (T item, Vector3 position)
            {
                item.transform.position = position;
            };
            foreach (Vector3 position in positions)
            {
                T val = Spawn();
                setPositionAction(val, position);
                yield return val;
            }
        }
    }
}