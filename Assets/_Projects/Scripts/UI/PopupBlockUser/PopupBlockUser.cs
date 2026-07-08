using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class PopupBlockUser : DraftUtils.DraftMonoBehaviour
{
    private float timeout = 1;
    public void SetData(float timeout)
    {
        this.timeout = timeout;
    }
    private void Update()
    {
        if (timeout < 0)
        {
            gameObject.SetActive(false);
            return;
        }
        timeout -= Time.deltaTime;
    }
}
