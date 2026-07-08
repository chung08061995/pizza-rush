using System;
using UnityEngine;

public static class TimeoutUtils
{
    public static DraftUtils.TimeoutMonoBehaviour CreateTimeout(float delay, Action callback, Transform root)
    {
        GameObject go = new GameObject("TimeoutMonoBehaviour");
        if (root != null)
        {
            go.transform.SetParent(root);
        }
        var timeout = go.AddComponent<DraftUtils.TimeoutMonoBehaviour>();
        timeout.SetDuration(delay);
        timeout.SetOnCompleteAction(() =>
        {
            callback?.Invoke();
            if (go != null)
            {
                UnityEngine.Object.Destroy(go);
            }
        });
        timeout.StartTimeout();
        return timeout;
    }
}
