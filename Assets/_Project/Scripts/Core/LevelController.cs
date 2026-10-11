using System.Collections.Generic;
using UnityEngine;

namespace ColorSortPuzzle
{
    public class LevelController : MonoBehaviour
    {
        public event System.Action OnLevelCompleted;

        private BoardState _boardState;
        private readonly List<Tube> _tubes = new List<Tube>();
        private InputController _input;
        private ColorDefinition _colorDef;

        private int _selectedTubeIndex = -1;
        private bool _isAnimating = false;
        private bool _inputLocked = false;
        private int _remainingExtraTubes;
        private readonly List<MoveRecord> _recordedMoves = new List<MoveRecord>();

        public bool IsAnimating => _isAnimating;
        public bool CanRestart => !_isAnimating && !_inputLocked;
        public bool CanUndo => !_isAnimating && !_inputLocked && _recordedMoves.Count > 0;
        public bool CanAddTube => !_isAnimating && !_inputLocked && _remainingExtraTubes > 0;

        public void Setup(LevelData levelData, ColorDefinition colorDef, InputController input)
        {
            _colorDef = colorDef;
            _input = input;
            _remainingExtraTubes = levelData.ExtraTubeLimit;
            
            _input.OnTubeTapped += HandleTubeTapped;

            List<List<int>> initialTubes = new List<List<int>>();
            foreach (var tubeSetup in levelData.Tubes)
            {
                initialTubes.Add(new List<int>(tubeSetup.balls));
            }

            _boardState = new BoardState(levelData.TubeCapacity, initialTubes);

#if UNITY_EDITOR
            string dump = $"Level Name: {levelData.name}, Capacity: {levelData.TubeCapacity}, Tubes: {_boardState.TubeCount}\n";
            for (int i = 0; i < _boardState.TubeCount; i++)
            {
                var tube = _boardState.GetTube(i);
                dump += $"Tube {i}: [" + string.Join(", ", tube) + "]\n";
            }
            Debug.Log("LEVELCONTROLLER DUMP:\n" + dump);
#endif

            CreateVisuals(levelData);
        }

        public void Cleanup()
        {
            _inputLocked = true;
            if (_input != null)
            {
                _input.OnTubeTapped -= HandleTubeTapped;
                _input = null;
            }
            OnLevelCompleted = null;
        }

        private void OnDestroy()
        {
            Cleanup();
        }

        private void CreateVisuals(LevelData levelData)
        {
            _tubes.Clear();
            for (int i = 0; i < levelData.Tubes.Length; i++)
            {
                CreateTubeVisual(i, levelData.Tubes[i].balls);
            }

            RelayoutAndFitCamera();
        }

        private void CreateTubeVisual(int tubeIndex, int[] initialBalls)
        {
            GameObject tubeGO = new GameObject($"Tube_{tubeIndex}");
            tubeGO.transform.SetParent(transform);

            Tube tube = tubeGO.AddComponent<Tube>();
            tube.Setup(tubeIndex);
            _tubes.Add(tube);

            if (initialBalls == null)
                return;

            for (int j = 0; j < initialBalls.Length; j++)
            {
                GameObject ballGO = new GameObject($"Ball_{tubeIndex}_{j}");
                ballGO.transform.SetParent(transform);

                Ball ball = ballGO.AddComponent<Ball>();
                ball.Setup(initialBalls[j], _colorDef.GetColor(initialBalls[j]));
                tube.AddBallInstant(ball);
            }
        }

        public void RelayoutAndFitCamera()
        {
            if (Camera.main == null) return;

            int tubeCount = _tubes.Count;
            if (tubeCount == 0) return;

            int cols = tubeCount <= 5 ? tubeCount : (tubeCount <= 8 ? 4 : 5);
            int rows = Mathf.CeilToInt((float)tubeCount / cols);

            Sprite tubeSprite = SpriteFactory.GetTubeSprite();
            float tubeWidth = tubeSprite.bounds.size.x;
            float tubeHeight = tubeSprite.bounds.size.y;

            float xSpacing = 1.6f;
            float ySpacing = tubeHeight + 0.8f;
            float startY = (rows - 1) * ySpacing / 2f;

            for (int i = 0; i < tubeCount; i++)
            {
                int row = i / cols;
                int colInRow = i % cols;

                int tubesInThisRow = (row == rows - 1) ? (tubeCount - row * cols) : cols;
                if (tubesInThisRow == 0) tubesInThisRow = cols;

                float rowStartX = -(tubesInThisRow - 1) * xSpacing / 2f;
                Vector3 pos = new Vector3(rowStartX + colInRow * xSpacing, startY - row * ySpacing, 0f);

                _tubes[i].transform.position = pos;
                _tubes[i].RefreshBallPositions();
            }

            float aspect = (float)Screen.width / Screen.height;
            float topY = startY + tubeHeight;
            float bottomY = startY - (rows - 1) * ySpacing;

            float topPadding = 1.8f;
            float bottomMargin = 0.8f;
            float sidePadding = 0.5f;

            float barFraction = UIManager.GetBottomBarScreenFraction();
            float gameplayHeightFraction = Mathf.Max(0.5f, 1f - barFraction);

            float worldHeightNeeded = (topY - bottomY) + topPadding + bottomMargin;
            float requiredSizeY = worldHeightNeeded / (2f * gameplayHeightFraction);

            float totalWidth = (cols - 1) * xSpacing + tubeWidth;
            float requiredSizeX = (totalWidth / 2f + sidePadding) / aspect;

            float cameraSize = Mathf.Max(requiredSizeX, requiredSizeY, 5f);
            Camera.main.orthographicSize = cameraSize;

            float cameraY = (topY + topPadding) - cameraSize;
            Camera.main.transform.position = new Vector3(0, cameraY, -10f);
        }

        private void HandleTubeTapped(int tubeIndex)
        {
            if (_isAnimating || _inputLocked) return;

            if (_selectedTubeIndex == -1)
            {
                if (_boardState.GetBallCount(tubeIndex) > 0)
                {
                    _selectedTubeIndex = tubeIndex;
                    _tubes[tubeIndex].SelectAnimate();
                }
            }
            else if (_selectedTubeIndex == tubeIndex)
            {
                _tubes[_selectedTubeIndex].DeselectAnimate();
                _selectedTubeIndex = -1;
            }
            else
            {
                if (_boardState.CanMove(_selectedTubeIndex, tubeIndex))
                {
                    int topColor = _boardState.PeekTop(_selectedTubeIndex);
                    MoveRecord moveRecord = new MoveRecord(_selectedTubeIndex, tubeIndex, topColor);

                    if (_boardState.Move(_selectedTubeIndex, tubeIndex))
                    {
                        _recordedMoves.Add(moveRecord);

                        _tubes[_selectedTubeIndex].DeselectAnimate();
                        var ball = _tubes[_selectedTubeIndex].RemoveTopBall();

                        _isAnimating = true;
                        _tubes[tubeIndex].AddBallAnimated(ball, () =>
                        {
                            _isAnimating = false;
                            CheckWinCondition();
                        });

                        _selectedTubeIndex = -1;
                    }
                }
                else
                {
                    if (_boardState.GetBallCount(tubeIndex) > 0)
                    {
                        _tubes[_selectedTubeIndex].DeselectAnimate();
                        _selectedTubeIndex = tubeIndex;
                        _tubes[_selectedTubeIndex].SelectAnimate();
                    }
                    else
                    {
                        _tubes[_selectedTubeIndex].DeselectAnimate();
                        _selectedTubeIndex = -1;
                    }
                }
            }
        }

        public bool TryUndo()
        {
            if (!CanUndo)
                return false;

            DeselectCurrentTube();

            MoveRecord lastMove = _recordedMoves[_recordedMoves.Count - 1];
            if (!_boardState.Undo())
                return false;

            _recordedMoves.RemoveAt(_recordedMoves.Count - 1);

            Ball ball = _tubes[lastMove.TargetIndex].RemoveTopBall();
            if (ball != null)
            {
                _tubes[lastMove.SourceIndex].AddBallInstant(ball);
            }

            return true;
        }

        public bool TryAddTube()
        {
            if (!CanAddTube)
                return false;

            DeselectCurrentTube();

            _boardState.AddEmptyTube();
            _remainingExtraTubes--;

            int newTubeIndex = _tubes.Count;
            CreateTubeVisual(newTubeIndex, null);

            RelayoutAndFitCamera();
            return true;
        }

        private void DeselectCurrentTube()
        {
            if (_selectedTubeIndex != -1)
            {
                if (_selectedTubeIndex < _tubes.Count && _tubes[_selectedTubeIndex] != null)
                {
                    _tubes[_selectedTubeIndex].DeselectAnimate();
                }
                _selectedTubeIndex = -1;
            }
        }

        private void CheckWinCondition()
        {
            if (_boardState.IsSolved() && !_inputLocked)
            {
                _inputLocked = true;
                if (_input != null)
                {
                    _input.OnTubeTapped -= HandleTubeTapped;
                }
                
                Debug.Log("LEVEL COMPLETED! Bütün renkler başarıyla ayrıştırıldı.");
#if UNITY_EDITOR
                Debug.Log("MOVES: " + _recordedMoves.Count);
#endif
                StartCoroutine(WinSequenceCoroutine());
            }
        }

        private System.Collections.IEnumerator WinSequenceCoroutine()
        {
            yield return new WaitForSeconds(1.5f);
            OnLevelCompleted?.Invoke();
        }
    }
}
