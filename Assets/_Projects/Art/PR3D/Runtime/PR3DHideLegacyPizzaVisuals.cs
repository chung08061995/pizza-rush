using UnityEngine;

namespace PizzaRush.PR3D
{
    /// <summary>
    /// Removes the legacy placeholder pizza meshes embedded in the production-line
    /// FBX while leaving the gameplay Production component and the PR3D visual
    /// child intact. This is an additive art-only compatibility shim.
    /// </summary>
    public sealed class PR3DHideLegacyPizzaVisuals : MonoBehaviour
    {
        private int _scanFrames;

        private void Awake()
        {
            HideLegacy();
        }

        private void LateUpdate()
        {
            // LevelFactory can instantiate the line visuals after this object
            // awakens, so perform one deferred scan as well.
            if (_scanFrames++ < 120) HideLegacy();
        }

        private void HideLegacy()
        {
            foreach (var child in GetComponentsInChildren<Transform>(true))
            {
                if (child == transform) continue;
                if (child.name == "pizza1" || child.name == "pizza2" ||
                    child.name == "belt1" || child.name == "belt2" || child.name == "belt2 (1)")
                {
                    child.gameObject.SetActive(false);
                }
            }
            foreach (var child in FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (child.name == "pizza1" || child.name == "pizza2" ||
                    child.name == "belt1" || child.name == "belt2" || child.name == "belt2 (1)")
                {
                    child.gameObject.SetActive(false);
                }
            }
            foreach (var renderer in FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (renderer.transform.name == "pizza1" || renderer.transform.name == "pizza2" ||
                    renderer.transform.name == "belt1" || renderer.transform.name == "belt2" ||
                    renderer.transform.name == "belt2 (1)")
                    renderer.enabled = false;
            }
        }
    }
}
