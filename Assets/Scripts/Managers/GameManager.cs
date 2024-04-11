using UnityEngine;

namespace Core.Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        

        private void Awake()
        {
            // Create a singleton of the game manager object type.
            if (Instance && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }
    }

    public sealed class StartSessionEvent : GameEvent
    {
    }
    public sealed class EndSessionEvent : GameEvent
    {
    }
}