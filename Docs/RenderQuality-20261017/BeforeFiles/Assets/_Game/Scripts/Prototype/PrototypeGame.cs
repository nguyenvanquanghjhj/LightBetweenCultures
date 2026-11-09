using UnityEngine;
namespace LightBetweenCultures
{
    // Compatibility entry point for older scenes. Gameplay is owned by the scene controllers.
    public sealed class PrototypeGame : MonoBehaviour
    {
        private void Start() { GameBootstrap.Ensure(); if (GetComponent<MainMenu>() == null) gameObject.AddComponent<MainMenu>(); }
    }
}
