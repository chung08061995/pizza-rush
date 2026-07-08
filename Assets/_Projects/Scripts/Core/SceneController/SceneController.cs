using DraftUtils.Ads;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DraftUtils
{
    /// <summary>
    /// Bộ điều khiển quản lý việc tải và hủy tải các Scene trong Unity.
    /// Hỗ trợ quản lý các Scene cơ bản (Base) và các Scene phụ trợ tải thêm (Additional).
    /// </summary>
    [System.Serializable]
    public class SceneController : DraftUtils.DraftMonoBehaviour
    {
        private DraftUtils.FormattedLogger _logger = new(FormattedLogger.CreateFormatForType(typeof(SceneController)));

        /// <summary>
        /// Danh sách các Scene cơ bản luôn được duy trì (ví dụ: các Scene hệ thống, Managers).
        /// </summary>
        [ShowInInspector][ReadOnly] private List<string> _baseScenes = new List<string>();

        /// <summary>
        /// Danh sách các Scene phụ trợ được tải thêm động (ví dụ: Gameplay, Levels).
        /// </summary>
        [ShowInInspector][ReadOnly] private List<string> _additionalScenes = new List<string>();

        /// <summary>
        /// Quyền truy cập chỉ đọc vào danh sách các Scene cơ bản.
        /// </summary>
        public IReadOnlyList<string> BaseScenes => _baseScenes;

        /// <summary>
        /// Quyền truy cập chỉ đọc vào danh sách các Scene phụ trợ đang hoạt động.
        /// </summary>
        public IReadOnlyList<string> AdditionalScenes => _additionalScenes;

        /// <summary>
        /// Khởi tạo và tải additively danh sách các Scene cơ bản.
        /// </summary>
        /// <param name="scenes">Danh sách tên các Scene cơ bản cần tải.</param>
        public void InitializeBaseScenes(List<string> scenes)
        {
            _baseScenes = new List<string>(scenes);
            foreach (var scene in _baseScenes)
            {
                if (!IsSceneLoaded(scene))
                {
                    SceneManager.LoadScene(scene, LoadSceneMode.Additive);
                }
            }
        }

        /// <summary>
        /// Kiểm tra xem một Scene đã được tải vào bộ nhớ hay chưa.
        /// </summary>
        /// <param name="sceneName">Tên của Scene cần kiểm tra.</param>
        /// <returns>True nếu Scene đã được tải, ngược lại là False.</returns>
        private bool IsSceneLoaded(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).name == sceneName)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Tải một Scene đồng bộ.
        /// </summary>
        /// <param name="sceneName">Tên Scene cần tải.</param>
        /// <param name="mode">Chế độ tải: Single (sẽ hủy toàn bộ Scene phụ trợ khác) hoặc Additive (tải thêm).</param>
        public void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (_baseScenes.Contains(sceneName))
            {
                return;
            }

            if (mode == LoadSceneMode.Single)
            {
                foreach (var scene in _additionalScenes)
                {
                    SceneManager.UnloadSceneAsync(scene);
                }
                _additionalScenes.Clear();
            }

            if (!_additionalScenes.Contains(sceneName))
            {
                _additionalScenes.Add(sceneName);
            }
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        }

        /// <summary>
        /// Đặt Scene phụ trợ đầu tiên trong danh sách làm Active Scene.
        /// </summary>
        [Button]
        public void SetActiveFirstAdditionalScene()
        {
            if (_additionalScenes.Count >= 1)
            {
                _logger.Log(_additionalScenes[0]);
                Scene loadedScene = SceneManager.GetSceneByName(_additionalScenes[0]);
                SceneManager.SetActiveScene(loadedScene);
            }
        }

        /// <summary>
        /// Tải một Scene bất đồng bộ và gọi callback sau khi tải xong.
        /// </summary>
        /// <param name="sceneName">Tên Scene cần tải.</param>
        /// <param name="mode">Chế độ tải (Single hoặc Additive).</param>
        /// <param name="onCompletedAction">Hành động callback thực thi sau khi Scene tải xong thành công.</param>
        public void LoadSceneAsync(string sceneName, LoadSceneMode mode, Action onCompletedAction)
        {
            StartCoroutine(LoadSceneAsyncCoroutine(sceneName, mode, onCompletedAction));
        }

        /// <summary>
        /// Coroutine thực hiện việc tải Scene bất đồng bộ.
        /// </summary>
        /// <param name="sceneName">Tên Scene cần tải.</param>
        /// <param name="mode">Chế độ tải.</param>
        /// <param name="onCompletedAction">Callback thực thi sau khi hoàn thành.</param>
        private IEnumerator LoadSceneAsyncCoroutine(string sceneName, LoadSceneMode mode, Action onCompletedAction)
        {
            if (_baseScenes.Contains(sceneName)) yield break;

            if (mode == LoadSceneMode.Single)
            {
                foreach (var scene in _additionalScenes)
                {
                    if (IsSceneLoaded(scene))
                    {
                        yield return SceneManager.UnloadSceneAsync(scene);
                    }
                }
                _additionalScenes.Clear();
            }

            if (!_additionalScenes.Contains(sceneName))
            {
                _additionalScenes.Add(sceneName);
            }

            if (!IsSceneLoaded(sceneName))
            {
                yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            }
            onCompletedAction?.Invoke();
        }

        /// <summary>
        /// Hủy tải bất đồng bộ một Scene phụ trợ ra khỏi bộ nhớ.
        /// </summary>
        /// <param name="sceneName">Tên Scene cần hủy tải.</param>
        public void UnloadSceneAsync(string sceneName)
        {
            if (_baseScenes.Contains(sceneName))
            {
                Debug.LogWarning($"Cannot unload base scene: {sceneName}");
                return;
            }

            if (_additionalScenes.Contains(sceneName))
            {
                _additionalScenes.Remove(sceneName);
            }

            if (IsSceneLoaded(sceneName))
            {
                SceneManager.UnloadSceneAsync(sceneName);
            }
        }
    }
}