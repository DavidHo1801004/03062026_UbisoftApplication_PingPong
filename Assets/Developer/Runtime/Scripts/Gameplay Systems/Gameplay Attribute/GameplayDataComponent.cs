using UnityEngine;

namespace Developer.GameplaySystems
{
    /// <summary>
    /// Wrapper class to attach <see cref="UnitGameplayData"/> to game object.
    /// </summary>
    public class GameplayDataComponent : MonoBehaviour
    {
        public readonly UnitGameplayData GameplayData = new();
    }
}
