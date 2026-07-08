using UnityEngine;
using TMPro;


namespace DraftUtils
{
    public class FpsCounter : SingletonDontDestroyOnLoadMonoBehaviour<FpsCounter>
    {
        [SerializeField] private TMP_Text fpsText;
        private float updateInterval = 0.1f;

        private float _accum = 0;
        private int _frames = 0;
        private float _timeLeft;
        private float _currentFps;

        private void Start()
        {
            _timeLeft = updateInterval;
        }

        private void Update()
        {
            _timeLeft -= Time.deltaTime;
            _accum += Time.timeScale / Time.deltaTime;
            _frames++;

            if (_timeLeft <= 0.0)
            {
                _currentFps = _accum / _frames;

                if (fpsText != null)
                {
                    fpsText.text = string.Format("{0:F0} FPS", _currentFps);
                }

                _timeLeft = updateInterval;
                _accum = 0.0f;
                _frames = 0;
            }
        }
    }
}