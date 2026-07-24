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
        private const int VisiblePizzaCountPerLine = 4;
        private static int _lastGlobalScanFrame = -1;

        private void Awake()
        {
            HideLegacy();
        }

        private void LateUpdate()
        {
            // Production lines are pooled and shift while the level is running.
            // Keep the visual window in sync without changing any Production data.
            HideLegacy();
        }

        private void HideLegacy()
        {
            // This component is present on the scene root and production-line
            // prefabs. Only the first instance in a frame performs the global scan.
            if (_lastGlobalScanFrame == Time.frameCount) return;
            _lastGlobalScanFrame = Time.frameCount;

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

            // Level 301 intentionally owns several long queues. The original
            // placeholders were visually clipped by their line housing, while the
            // replacement mesh remained visible far beyond it and crossed the HUD.
            // Limit only the replacement child to the readable segment nearest the
            // board. Pooled Production objects and their indices remain untouched.
            foreach (var production in FindObjectsByType<Production>(
                         FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var shouldShow = production.gameObject.activeInHierarchy &&
                                 production.CurrentIndex >= 0 &&
                                 production.CurrentIndex < VisiblePizzaCountPerLine;
                foreach (var visualRenderer in production.GetComponentsInChildren<Renderer>(true))
                {
                    if (visualRenderer.transform.name != "PR3D_VisualPizza") continue;
                    if (visualRenderer.enabled != shouldShow)
                        visualRenderer.enabled = shouldShow;
                }
            }
        }
    }
}
