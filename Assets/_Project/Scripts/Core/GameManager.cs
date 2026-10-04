using UnityEngine;

namespace ColorSortPuzzle
{
    /// <summary>
    /// Oyun durumunu yönetir. Faz 1 iskeleti.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public enum GameState
        {
            Menu,
            Playing,
            LevelComplete
        }

        private GameState _currentState = GameState.Menu;

        public GameState CurrentState => _currentState;

        public void SetState(GameState newState)
        {
            _currentState = newState;
        }
    }
}
