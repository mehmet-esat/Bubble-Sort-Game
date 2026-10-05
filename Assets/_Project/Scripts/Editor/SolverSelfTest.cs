using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace ColorSortPuzzle.Editor
{
    public class SolverSelfTest : EditorWindow
    {
        [MenuItem("Tools/Color Sort/Solver Self-Test")]
        public static void RunTests()
        {
            Debug.Log("--- SOLVER SELF-TEST BAŞLIYOR ---");
            TestA();
            TestB();
            TestC();
            TestD();
            TestE();
            Debug.Log("--- SOLVER SELF-TEST BİTTİ ---");
        }

        private static void TestA()
        {
            List<List<int>> tubes = new List<List<int>>
            {
                new List<int> { 0, 1 },
                new List<int> { 1, 0 },
                new List<int>()
            };
            BoardState board = new BoardState(2, tubes);
            var result = Solver.Solve(board);
            Debug.Log($"Test A (Basit): Sonuç = {result.Result}, Ziyaret Edilen Düğüm = {result.NodesExplored}");
        }

        private static void TestB()
        {
            var levelData = Resources.Load<LevelData>("Levels/Level_01");
            if (levelData == null) levelData = Resources.Load<LevelData>("Levels/Level_001");
            if (levelData == null) return;

            List<List<int>> tubes = new List<List<int>>();
            foreach (var t in levelData.Tubes) tubes.Add(new List<int>(t.balls));
            BoardState board = new BoardState(levelData.TubeCapacity, tubes);
            
            var result = Solver.Solve(board, 1000000);
            Debug.Log($"Test B (Level_001 Normal): Sonuç = {result.Result}, Ziyaret Edilen Düğüm = {result.NodesExplored}");
        }

        private static void TestC()
        {
            int colorCount = 5;
            int capacity = 4;
            int emptyCount = 2;

            List<List<int>> tubes = new List<List<int>>();
            for (int i = 0; i < colorCount; i++)
            {
                var tube = new List<int>();
                for (int c = 0; c < capacity; c++) tube.Add(i);
                tubes.Add(tube);
            }
            for (int i = 0; i < emptyCount; i++) tubes.Add(new List<int>());

            BoardState board = new BoardState(capacity, tubes);
            System.Random rng = new System.Random(42); 

            int shuffleMoves = 30;
            for (int m = 0; m < shuffleMoves; m++)
            {
                List<(int s, int t)> possibleReverseMoves = new List<(int, int)>();
                for (int s = 0; s < board.TubeCount; s++)
                {
                    if (board.GetBallCount(s) == 0) continue;
                    for (int t = 0; t < board.TubeCount; t++)
                    {
                        if (s == t) continue;
                        if (board.GetBallCount(t) < board.TubeCapacity)
                        {
                            int topColor = board.PeekTop(s);
                            bool sWillBeEmpty = board.GetBallCount(s) == 1;
                            bool sNextMatches = false;
                            
                            if (!sWillBeEmpty)
                            {
                                var sTube = board.GetTube(s);
                                sNextMatches = (sTube[sTube.Count - 2] == topColor);
                            }
                            
                            if (sWillBeEmpty || sNextMatches) possibleReverseMoves.Add((s, t));
                        }
                    }
                }
                if (possibleReverseMoves.Count > 0)
                {
                    var move = possibleReverseMoves[rng.Next(possibleReverseMoves.Count)];
                    board.ForceMove(move.s, move.t);
                }
            }
            var result = Solver.Solve(board, 1000000);
            Debug.Log($"Test C (30 Geçerli Geri Hamle): Sonuç = {result.Result}, Ziyaret Edilen Düğüm = {result.NodesExplored}");
        }

        private static void TestD()
        {
            var levelData = Resources.Load<LevelData>("Levels/Level_01");
            if (levelData == null) levelData = Resources.Load<LevelData>("Levels/Level_001");
            if (levelData == null) return;

            List<List<int>> tubes = new List<List<int>>();
            foreach (var t in levelData.Tubes) tubes.Add(new List<int>(t.balls));
            
            var resultNormal = Solver.Solve(new BoardState(levelData.TubeCapacity, tubes), 1000000, true);
            var resultNoPruning = Solver.Solve(new BoardState(levelData.TubeCapacity, tubes), 1000000, false);
            
            Debug.Log($"Test D (Level_001 Budamasız vs Normal):\n Normal = {resultNormal.Result} (Düğüm: {resultNormal.NodesExplored})\n Budamasız = {resultNoPruning.Result} (Düğüm: {resultNoPruning.NodesExplored})");
        }

        private static void TestE()
        {
            int colorCount = 5;
            int capacity = 4;
            int emptyCount = 2;

            List<List<int>> tubes = new List<List<int>>();
            for (int i = 0; i < colorCount; i++)
            {
                var tube = new List<int>();
                for (int c = 0; c < capacity; c++) tube.Add(i);
                tubes.Add(tube);
            }
            for (int i = 0; i < emptyCount; i++) tubes.Add(new List<int>());

            BoardState board = new BoardState(capacity, tubes);
            System.Random rng = new System.Random(99); 

            int shuffleMoves = 300; // 300 geri hamle
            for (int m = 0; m < shuffleMoves; m++)
            {
                List<(int s, int t)> possibleReverseMoves = new List<(int, int)>();
                for (int s = 0; s < board.TubeCount; s++)
                {
                    if (board.GetBallCount(s) == 0) continue;
                    for (int t = 0; t < board.TubeCount; t++)
                    {
                        if (s == t) continue;
                        if (board.GetBallCount(t) < board.TubeCapacity)
                        {
                            int topColor = board.PeekTop(s);
                            bool sWillBeEmpty = board.GetBallCount(s) == 1;
                            bool sNextMatches = false;
                            
                            if (!sWillBeEmpty)
                            {
                                var sTube = board.GetTube(s);
                                sNextMatches = (sTube[sTube.Count - 2] == topColor);
                            }
                            
                            if (sWillBeEmpty || sNextMatches) possibleReverseMoves.Add((s, t));
                        }
                    }
                }
                if (possibleReverseMoves.Count > 0)
                {
                    var move = possibleReverseMoves[rng.Next(possibleReverseMoves.Count)];
                    board.ForceMove(move.s, move.t);
                }
            }

            var result = Solver.Solve(board, 1000000);
            Debug.Log($"Test E (300 Geri Hamle - Ağır Karışım): Sonuç = {result.Result}, Ziyaret Edilen Düğüm = {result.NodesExplored}");
        }
    }
}
