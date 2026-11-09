using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LightBetweenCultures
{
    public sealed class SceneTransition : MonoBehaviour
    {
        public string WorldReturnScene { get; private set; } = "Prototype";
        public string WorldSelection { get; private set; }
        public void OpenWorld(string destination = null)
        {
            WorldReturnScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "CityMap" ? "CityMap" : "Prototype";
            WorldSelection = destination;
            Go("WorldMap");
        }
        public bool Busy { get; private set; }
        public void Go(string scene)
        {
            if (!Busy) StartCoroutine(Load(scene));
        }
        private IEnumerator Load(string scene)
        {
            if (!Application.CanStreamedLevelBeLoaded(scene)) { Debug.LogError("Scene missing from Build Settings: " + scene); yield break; }
            Busy = true; GameBootstrap.Instance.Persist();
            yield return SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
            Busy = false;
        }
    }
}
