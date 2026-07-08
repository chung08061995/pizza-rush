using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;


namespace DraftUtils
{
    public class DestroyWithDependencies : DraftMonoBehaviour
    {
        private DraftUtils.FormattedLogger _logger = new(FormattedLogger.CreateFormatForType(typeof(DestroyWithDependencies)));
        [ShowInInspector][ReadOnly] private List<GameObject> _dependencies = new();

        public void AddDependency(GameObject obj)
        {
            DraftUtils.Utils.ListUtils.AddIfNotExists(_dependencies, obj);
        }

        public void RemoveDependency(GameObject obj)
        {
            DraftUtils.Utils.ListUtils.RemoveIfExists(_dependencies, obj);
        }

        private void OnDestroy()
        {
            foreach (var obj in _dependencies)
            {
                if (obj == null)
                {
                    continue;
                }
                _logger.Log($"Destroy {obj.name} cùng với {name}");
                Destroy(obj);
            }
            _dependencies.Clear();
        }
        public static DestroyWithDependencies Create(Transform root, string name)
        {
            GameObject go = new GameObject($"{name}_{nameof(DestroyWithDependencies)}_CreatByCode");
            go.transform.SetParent(root);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            return go.AddComponent<DestroyWithDependencies>();
        }
    }
}