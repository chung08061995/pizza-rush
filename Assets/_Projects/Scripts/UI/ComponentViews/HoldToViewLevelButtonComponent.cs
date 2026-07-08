using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

namespace DraftUtils
{
    /// <summary>
    /// Component xử lý sự kiện nhấn giữ (Hold) trên nút để làm mờ dần CanvasGroup, và khôi phục khi thả ra.
    /// </summary>
    public class HoldToViewLevelButtonComponent : DraftUtils.DraftMonoBehaviour
    {
        [SerializeField] private Button holdButton;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float fadeDuration = 0.5f;

        private bool _isInitialized;

        /// <summary>
        /// Khởi tạo và đăng ký sự kiện nhấn giữ.
        /// </summary>
        public void Init()
        {
            if (_isInitialized)
            {
                return;
            }
            if (holdButton == null || canvasGroup == null)
            {
                return;
            }

            _isInitialized = true;
            EventTrigger trigger = holdButton.gameObject.AddComponent<EventTrigger>();

            var pointerDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            pointerDown.callback.AddListener((data) => { OnHoldStart(); });
            trigger.triggers.Add(pointerDown);

            var pointerUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            pointerUp.callback.AddListener((data) => { OnHoldEnd(); });
            trigger.triggers.Add(pointerUp);
        }

        private void OnHoldStart()
        {
            canvasGroup.DOKill();
            canvasGroup.DOFade(0f, fadeDuration).SetEase(Ease.Linear);
        }

        private void OnHoldEnd()
        {
            canvasGroup.DOKill();
            canvasGroup.alpha = 1f;
        }
    }
}
