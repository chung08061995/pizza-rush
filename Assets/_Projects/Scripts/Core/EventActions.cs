using System;
using UnityEngine;

/// <summary>
/// Component tổng hợp các Action tương ứng với những hàm event (message)
/// phổ biến của Unity.
/// </summary>
namespace DraftUtils
{
    public class EventActions : DraftUtils.DraftMonoBehaviour
    {
        [Header("Lifecycle")]
        public Action onAwakeAction;
        public Action onStartAction;
        public Action onEnableAction;
        public Action onDisableAction;
        public Action onDestroyAction;

        [Header("Update Loop")]
        public Action onUpdateAction;
        public Action onFixedUpdateAction;
        public Action onLateUpdateAction;

        [Header("Collision 3D")]
        public Action<Collision> onCollisionEnterAction;
        public Action<Collision> onCollisionStayAction;
        public Action<Collision> onCollisionExitAction;

        [Header("Trigger 3D")]
        public Action<Collider> onTriggerEnterAction;
        public Action<Collider> onTriggerStayAction;
        public Action<Collider> onTriggerExitAction;

        [Header("Collision 2D")]
        public Action<Collision2D> onCollisionEnter2DAction;
        public Action<Collision2D> onCollisionStay2DAction;
        public Action<Collision2D> onCollisionExit2DAction;

        [Header("Trigger 2D")]
        public Action<Collider2D> onTriggerEnter2DAction;
        public Action<Collider2D> onTriggerStay2DAction;
        public Action<Collider2D> onTriggerExit2DAction;

        [Header("Mouse")]
        public Action onMouseDownAction;
        public Action onMouseUpAction;
        public Action onMouseEnterAction;
        public Action onMouseExitAction;
        public Action onMouseOverAction;
        public Action onMouseDragAction;

        [Header("Application")]
        public Action<bool> onApplicationPauseAction;
        public Action<bool> onApplicationFocusAction;
        public Action onApplicationQuitAction;

        [Header("Visibility")]
        public Action onBecameVisibleAction;
        public Action onBecameInvisibleAction;

        // ---------- Lifecycle ----------
        private void Awake() => onAwakeAction?.Invoke();
        private void Start() => onStartAction?.Invoke();
        private void OnEnable() => onEnableAction?.Invoke();
        private void OnDisable() => onDisableAction?.Invoke();
        private void OnDestroy() => onDestroyAction?.Invoke();

        // ---------- Update loop ----------
        private void Update() => onUpdateAction?.Invoke();
        private void FixedUpdate() => onFixedUpdateAction?.Invoke();
        private void LateUpdate() => onLateUpdateAction?.Invoke();

        // ---------- Collision 3D ----------
        private void OnCollisionEnter(Collision collision) => onCollisionEnterAction?.Invoke(collision);
        private void OnCollisionStay(Collision collision) => onCollisionStayAction?.Invoke(collision);
        private void OnCollisionExit(Collision collision) => onCollisionExitAction?.Invoke(collision);

        // ---------- Trigger 3D ----------
        private void OnTriggerEnter(Collider other) => onTriggerEnterAction?.Invoke(other);
        private void OnTriggerStay(Collider other) => onTriggerStayAction?.Invoke(other);
        private void OnTriggerExit(Collider other) => onTriggerExitAction?.Invoke(other);

        // ---------- Collision 2D ----------
        private void OnCollisionEnter2D(Collision2D collision) => onCollisionEnter2DAction?.Invoke(collision);
        private void OnCollisionStay2D(Collision2D collision) => onCollisionStay2DAction?.Invoke(collision);
        private void OnCollisionExit2D(Collision2D collision) => onCollisionExit2DAction?.Invoke(collision);

        // ---------- Trigger 2D ----------
        private void OnTriggerEnter2D(Collider2D other) => onTriggerEnter2DAction?.Invoke(other);
        private void OnTriggerStay2D(Collider2D other) => onTriggerStay2DAction?.Invoke(other);
        private void OnTriggerExit2D(Collider2D other) => onTriggerExit2DAction?.Invoke(other);

        // ---------- Mouse ----------
        private void OnMouseDown() => onMouseDownAction?.Invoke();
        private void OnMouseUp() => onMouseUpAction?.Invoke();
        private void OnMouseEnter() => onMouseEnterAction?.Invoke();
        private void OnMouseExit() => onMouseExitAction?.Invoke();
        private void OnMouseOver() => onMouseOverAction?.Invoke();
        private void OnMouseDrag() => onMouseDragAction?.Invoke();

        // ---------- Application ----------
        private void OnApplicationPause(bool pauseStatus) => onApplicationPauseAction?.Invoke(pauseStatus);
        private void OnApplicationFocus(bool hasFocus) => onApplicationFocusAction?.Invoke(hasFocus);
        private void OnApplicationQuit() => onApplicationQuitAction?.Invoke();

        // ---------- Visibility ----------
        private void OnBecameVisible() => onBecameVisibleAction?.Invoke();
        private void OnBecameInvisible() => onBecameInvisibleAction?.Invoke();

        public static EventActions Create(Transform parent)
        {
            GameObject go = new GameObject("EventActions");
            go.transform.SetParent(parent, false);
            return go.AddComponent<EventActions>();
        }

        public static EventActions Reuse(EventActions eventActions, Transform parent)
        {
            if (eventActions == null)
            {
                eventActions = Create(parent);
            }
            return eventActions;
        }
    }
}