using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using ColorSortPuzzle;

namespace ColorSortPuzzle.Editor
{
    public class LevelGeneratorWindow : EditorWindow
    {
        private int _levelCountToGenerate = 5;
        private int _colorCount = 5;
        private int _tubeCapacity = 4;
        private int _emptyTubes = 2;
        private ColorDefinition _colorDef;

        private const string SAVE_PATH = "Assets/_Project/Resources/Levels";

        [MenuItem("Tools/Color Sort/Generate Levels")]
        public static void ShowWindow()
        {
            GetWindow<LevelGeneratorWindow>("Level Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Seviye Üretici Ayarları", EditorStyles.boldLabel);

            _colorDef = (ColorDefinition)EditorGUILayout.ObjectField("Color Definition", _colorDef, typeof(ColorDefinition), false);
            
            _levelCountToGenerate = EditorGUILayout.IntSlider("Üretilecek Seviye Sayısı", _levelCountToGenerate, 1, 50);
            _colorCount = EditorGUILayout.IntSlider("Kullanılacak Renk Sayısı", _colorCount, 3, 15);
            _tubeCapacity = EditorGUILayout.IntSlider("Tüp Kapasitesi", _tubeCapacity, 3, 6);
            _emptyTubes = EditorGUILayout.IntSlider("Boş Tüp Sayısı", _emptyTubes, 1, 4);

            if (GUILayout.Button("Seviyeleri Üret"))
            {
                GenerateLevels();
            }
        }

        private void GenerateLevels()
        {
            if (_colorDef == null)
            {
                EditorUtility.DisplayDialog("Hata", "Lütfen bir ColorDefinition atayın!", "Tamam");
                return;
            }

            if (_colorCount > _colorDef.ColorCount)
            {
                EditorUtility.DisplayDialog("Hata", $"İstenen renk sayısı ({_colorCount}), ColorDefinition'daki renk sayısından ({_colorDef.ColorCount}) fazla olamaz!", "Tamam");
                return;
            }

            if (!AssetDatabase.IsValidFolder(SAVE_PATH))
            {
                // Create folder recursively
                string[] folders = SAVE_PATH.Split('/');
                string currentPath = "";
                for (int i = 0; i < folders.Length; i++)
                {
                    string parent = currentPath;
                    currentPath = string.IsNullOrEmpty(currentPath) ? folders[i] : currentPath + "/" + folders[i];
                    if (!AssetDatabase.IsValidFolder(currentPath))
                    {
                        AssetDatabase.CreateFolder(parent, folders[i]);
                    }
                }
            }

            int successCount = 0;
            int attemptCount = 0;
            int maxAttempts = _levelCountToGenerate * 10; // Sonsuz döngüyü önlemek için
            
            int startIndex = GetNextAvailableIndex();

            while (successCount < _levelCountToGenerate && attemptCount < maxAttempts)
            {
                attemptCount++;
                LevelData newLevel = GenerateRandomLevelData();
                
                if (newLevel == null) continue; // Başlangıçta çözülmüş tüp kuralına takıldı
                
                // Solver'a gönder
                var initialTubes = new List<List<int>>();
                foreach(var setup in newLevel.Tubes)
                {
                    initialTubes.Add(new List<int>(setup.balls));
                }
                var boardState = new BoardState(_tubeCapacity, initialTubes);
                
                var resultData = Solver.Solve(boardState, 200000);
                
                if (resultData.Result == SolverResult.Solvable)
                {
                    SaveLevelAsset(newLevel, startIndex + successCount);
                    successCount++;
                    Debug.Log($"Seviye {successCount} üretildi. Gezilen Düğüm: {resultData.NodesExplored}");
                }
                else
                {
                    Debug.Log($"Seviye denemesi başarısız oldu. Sonuç: {resultData.Result}, Gezilen Düğüm: {resultData.NodesExplored}");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Sonuç", $"{attemptCount} denemede {successCount} adet çözülebilir seviye üretildi.", "Tamam");
        }

        private LevelData GenerateRandomLevelData()
        {
            // 1. Başlangıçta çözülmüş bir tahta oluştur (Her renk tam dolu, boş tüpler boş)
            List<List<int>> tubes = new List<List<int>>();
            for (int i = 0; i < _colorCount; i++)
            {
                List<int> tube = new List<int>();
                for (int c = 0; c < _tubeCapacity; c++)
                {
                    tube.Add(i);
                }
                tubes.Add(tube);
            }
            for (int i = 0; i < _emptyTubes; i++)
            {
                tubes.Add(new List<int>());
            }

            // 2. Rastgele GERİYE DOĞRU (Reverse) hamleler yaparak karıştır
            // Geriye doğru hamle kuralı: Hedef tüp dolu olmasın yeter. Renk eşleşmesi aranmaz.
            System.Random rng = new System.Random();
            int shuffleMoves = 1000;
            int lastSource = -1;
            int lastTarget = -1;

            for (int m = 0; m < shuffleMoves; m++)
            {
                List<(int s, int t)> possibleMoves = new List<(int, int)>();
                
                for (int s = 0; s < tubes.Count; s++)
                {
                    if (tubes[s].Count == 0) continue;
                    
                    for (int t = 0; t < tubes.Count; t++)
                    {
                        if (s == t) continue;
                        if (tubes[t].Count < _tubeCapacity)
                        {
                            // Bir önceki hamleyi anında geri almayı önle (zengin karıştırma için)
                            if (s == lastTarget && t == lastSource) continue;
                            
                            possibleMoves.Add((s, t));
                        }
                    }
                }

                if (possibleMoves.Count > 0)
                {
                    var move = possibleMoves[rng.Next(possibleMoves.Count)];
                    
                    // Topu taşı
                    int color = tubes[move.s][tubes[move.s].Count - 1];
                    tubes[move.s].RemoveAt(tubes[move.s].Count - 1);
                    tubes[move.t].Add(color);
                    
                    lastSource = move.s;
                    lastTarget = move.t;
                }
            }

            // Oluşan listeyi LevelData'ya çevir
            LevelData levelData = ScriptableObject.CreateInstance<LevelData>();
            SerializedObject so = new SerializedObject(levelData);
            so.FindProperty("tubeCapacity").intValue = _tubeCapacity;
            
            SerializedProperty tubesProp = so.FindProperty("tubes");
            tubesProp.arraySize = _colorCount + _emptyTubes;

            for (int i = 0; i < tubes.Count; i++)
            {
                SerializedProperty element = tubesProp.GetArrayElementAtIndex(i);
                SerializedProperty ballsProp = element.FindPropertyRelative("balls");
                
                ballsProp.arraySize = tubes[i].Count;
                for (int j = 0; j < tubes[i].Count; j++)
                {
                    ballsProp.GetArrayElementAtIndex(j).intValue = tubes[i][j];
                }
            }

            so.ApplyModifiedProperties();
            return levelData;
        }

        private int GetNextAvailableIndex()
        {
            int index = 1;
            while (File.Exists($"{SAVE_PATH}/Level_{index:D3}.asset"))
            {
                index++;
            }
            return index;
        }

        private void SaveLevelAsset(LevelData levelData, int index)
        {
            string fileName = $"Level_{index:D3}.asset";
            string fullPath = $"{SAVE_PATH}/{fileName}";
            AssetDatabase.CreateAsset(levelData, fullPath);
        }
    }
}
