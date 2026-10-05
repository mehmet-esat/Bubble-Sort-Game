using System.Collections.Generic;
using UnityEngine;

namespace ColorSortPuzzle
{
    public class LevelController : MonoBehaviour
    {
        public event System.Action OnLevelCompleted;

        private BoardState _boardState;
        private Tube[] _tubes;
        private InputController _input;
        private ColorDefinition _colorDef;

        private int _selectedTubeIndex = -1;
        private bool _isAnimating = false;
        private bool _inputLocked = false;
        private List<string> _recordedMoves = new List<string>();

        public void Setup(LevelData levelData, ColorDefinition colorDef, InputController input)
        {
            _colorDef = colorDef;
            _input = input;
            
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
            var solveResult = Solver.Solve(new BoardState(_boardState));
            Debug.Log($"LEVELCONTROLLER SOLVER CHECK: Result = {solveResult.Result}, Nodes = {solveResult.NodesExplored}");
#endif

            CreateVisuals(levelData);
        }

        private void OnDestroy()
        {
            if (_input != null)
                _input.OnTubeTapped -= HandleTubeTapped;
        }

        private void CreateVisuals(LevelData levelData)
        {
            int tubeCount = levelData.Tubes.Length;
            _tubes = new Tube[tubeCount];

            int cols = tubeCount <= 5 ? tubeCount : (tubeCount <= 8 ? 4 : 5);
            int rows = Mathf.CeilToInt((float)tubeCount / cols);
            
            // Tüp boyutlarını hardcoded değerler yerine Sprite'tan oku
            Sprite tubeSprite = SpriteFactory.GetTubeSprite();
            float tubeWidth = tubeSprite.bounds.size.x;
            float tubeHeight = tubeSprite.bounds.size.y;
            
            float xSpacing = 1.6f;
            float ySpacing = tubeHeight + 0.8f; // Tüp yüksekliği + 0.8 boşluk
            
            float startY = (rows - 1) * ySpacing / 2f; 

            for (int i = 0; i < tubeCount; i++)
            {
                int row = i / cols;
                int colInRow = i % cols;
                
                // Son satırda kolon sayısından az tüp varsa yatayda ortala
                int tubesInThisRow = (row == rows - 1) ? (tubeCount - row * cols) : cols;
                if (tubesInThisRow == 0) tubesInThisRow = cols; // Tam bölünme durumu için koruma
                
                float rowStartX = -(tubesInThisRow - 1) * xSpacing / 2f;
                
                Vector3 pos = new Vector3(rowStartX + colInRow * xSpacing, startY - row * ySpacing, 0);
                
                GameObject tubeGO = new GameObject($"Tube_{i}");
                tubeGO.transform.SetParent(transform);
                tubeGO.transform.position = pos;
                
                Tube tube = tubeGO.AddComponent<Tube>();
                tube.Setup(i);
                _tubes[i] = tube;

                var initialBalls = levelData.Tubes[i].balls;
                for (int j = 0; j < initialBalls.Length; j++)
                {
                    GameObject ballGO = new GameObject($"Ball_{i}_{j}");
                    ballGO.transform.SetParent(transform);
                    
                    Ball ball = ballGO.AddComponent<Ball>();
                    ball.Setup(initialBalls[j], _colorDef.GetColor(initialBalls[j]));
                    tube.AddBallInstant(ball);
                }
            }

            AdjustCameraToFit(cols, rows, xSpacing, ySpacing, tubeWidth, tubeHeight, startY);
        }

        private void AdjustCameraToFit(int cols, int rows, float xSpacing, float ySpacing, float tubeWidth, float tubeHeight, float startY)
        {
            if (Camera.main == null) return;

            float aspect = (float)Screen.width / Screen.height;
            
            // Tüplerin gerçek (dünya koordinatındaki) sınırları
            // Pivot alt orta olduğu için en alt Y pozisyonu kendi pozisyonudur.
            float topY = startY + tubeHeight; 
            float bottomY = startY - (rows - 1) * ySpacing;
            
            // Üst ve alt HUD payları (Alt pay daha büyük, alt UI barı için)
            float topPadding = 2.0f;
            float bottomPadding = 4.0f; 
            float sidePadding = 0.5f;

            // Kameranın görmesi gereken hedef Y sınırları
            float desiredTopEdge = topY + topPadding;
            float desiredBottomEdge = bottomY - bottomPadding;
            
            // Y ekseni için gereken yarı yükseklik (Orthographic Size)
            float requiredSizeY = (desiredTopEdge - desiredBottomEdge) / 2f;
            
            // X ekseni için en geniş satır cols kadar tüp içerir
            float totalWidth = (cols - 1) * xSpacing + tubeWidth;
            float requiredSizeX = (totalWidth / 2f + sidePadding) / aspect;
            
            // Kamera Y konumu (hedef sınırların tam ortası)
            float cameraY = (desiredTopEdge + desiredBottomEdge) / 2f;

            // Kamerayı en büyük gereksinime göre ayarla
            Camera.main.orthographicSize = Mathf.Max(requiredSizeX, requiredSizeY, 5f);
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
                    _recordedMoves.Add($"{_selectedTubeIndex}>{tubeIndex}");
                    _boardState.Move(_selectedTubeIndex, tubeIndex);
                    
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
                Debug.Log("MOVES: " + string.Join(",", _recordedMoves));
                StartCoroutine(WinSequenceCoroutine());
            }
        }

        private System.Collections.IEnumerator WinSequenceCoroutine()
        {
            // Kazanma sonrası bekleme
            yield return new WaitForSeconds(1.5f);
            OnLevelCompleted?.Invoke();
        }
    }
}
