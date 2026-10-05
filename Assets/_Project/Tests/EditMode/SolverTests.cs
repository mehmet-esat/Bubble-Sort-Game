using NUnit.Framework;
using System.Collections.Generic;

namespace ColorSortPuzzle.Tests
{
    public class SolverTests
    {
        private BoardState CreateBoard(int capacity, params int[][] tubes)
        {
            var tubeList = new List<List<int>>();
            foreach (var tube in tubes)
            {
                tubeList.Add(new List<int>(tube));
            }
            return new BoardState(capacity, tubeList);
        }

        [Test]
        public void Solve_AlreadySolved_ReturnsSolvable()
        {
            var board = CreateBoard(4, new[] { 0, 0, 0, 0 }, new[] { 1, 1, 1, 1 }, new int[0]);
            var result = Solver.Solve(board);
            Assert.AreEqual(SolverResult.Solvable, result.Result);
        }

        [Test]
        public void Solve_SimpleSolvable_ReturnsSolvable()
        {
            // 0: [0, 1] 
            // 1: [1, 0]
            // 2: []
            var board = CreateBoard(2, new[] { 0, 1 }, new[] { 1, 0 }, new int[0]);
            var result = Solver.Solve(board);
            Assert.AreEqual(SolverResult.Solvable, result.Result);
        }

        [Test]
        public void Solve_Unsolvable_ReturnsUnsolvable()
        {
            // Çözümsüz: Boş tüp yok ve hepsi karışık
            var board = CreateBoard(2, new[] { 0, 1 }, new[] { 1, 0 });
            var result = Solver.Solve(board);
            Assert.AreEqual(SolverResult.Unsolvable, result.Result);
        }
    }
}
