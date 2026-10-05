using System.Collections.Generic;

namespace ColorSortPuzzle
{
    public enum SolverResult
    {
        Solvable,
        Unsolvable,
        Undetermined
    }

    /// <summary>
    /// Saf C# Çözücü. Seviyelerin çözülebilir olup olmadığını BFS (Genişlik Öncelikli Arama) ile doğrular.
    /// Ziyaret edilen durumları (visited-set) kanonik formda tutarak döngüleri ve simetrik kopyaları engeller.
    /// </summary>
    public struct SolverResultData
    {
        public SolverResult Result;
        public int NodesExplored;
    }

    public class CanonicalState : System.IEquatable<CanonicalState>
    {
        private readonly ulong[] _hashes;
        private readonly int _hashCode;

        public CanonicalState(BoardState board)
        {
            _hashes = board.GetTubeHashes();
            System.Array.Sort(_hashes);

            unchecked
            {
                int hash = 17;
                for (int i = 0; i < _hashes.Length; i++)
                    hash = hash * 31 + _hashes[i].GetHashCode();
                _hashCode = hash;
            }
        }

        public bool Equals(CanonicalState other)
        {
            if (other == null || _hashes.Length != other._hashes.Length) return false;
            for (int i = 0; i < _hashes.Length; i++)
            {
                if (_hashes[i] != other._hashes[i]) return false;
            }
            return true;
        }

        public override int GetHashCode() => _hashCode;
        public override bool Equals(object obj) => obj is CanonicalState other && Equals(other);
    }

    public static class Solver
    {
        public static SolverResultData Solve(BoardState initialState, int maxNodes = 200000, bool enablePruning = true)
        {
            if (initialState.IsSolved()) return new SolverResultData { Result = SolverResult.Solvable, NodesExplored = 0 };

            var stack = new Stack<BoardState>();
            var visited = new HashSet<CanonicalState>();

            stack.Push(initialState);
            visited.Add(new CanonicalState(initialState));

            int nodesExplored = 0;

            while (stack.Count > 0)
            {
                if (nodesExplored >= maxNodes)
                {
                    return new SolverResultData { Result = SolverResult.Undetermined, NodesExplored = nodesExplored };
                }

                var current = stack.Pop();
                nodesExplored++;

                if (current.IsSolved())
                {
                    return new SolverResultData { Result = SolverResult.Solvable, NodesExplored = nodesExplored };
                }

                int tubeCount = current.TubeCount;
                var moves = new List<(int s, int t, int priority)>();

                for (int s = 0; s < tubeCount; s++)
                {
                    if (current.GetBallCount(s) == 0) continue;
                    if (enablePruning && current.IsTubeCompleted(s)) continue;

                    for (int t = 0; t < tubeCount; t++)
                    {
                        if (s == t) continue;
                        if (enablePruning && current.GetBallCount(t) == 0 && current.IsTubeHomogeneous(s)) continue;

                        if (current.CanMove(s, t))
                        {
                            int priority = current.GetBallCount(t) > 0 ? 2 : 1;
                            moves.Add((s, t, priority));
                        }
                    }
                }

                // Stack'e koyarken düşük önceliklileri önce, yüksek önceliklileri sonra koyuyoruz (LIFO mantığıyla yüksek olan ilk çıkar)
                moves.Sort((a, b) => a.priority.CompareTo(b.priority));

                foreach (var move in moves)
                {
                    var nextState = new BoardState(current);
                    nextState.Move(move.s, move.t);
                    
                    var hash = new CanonicalState(nextState);
                    if (!visited.Contains(hash))
                    {
                        visited.Add(hash);
                        stack.Push(nextState);
                    }
                }
            }

            return new SolverResultData { Result = SolverResult.Unsolvable, NodesExplored = nodesExplored };
        }
    }
}
