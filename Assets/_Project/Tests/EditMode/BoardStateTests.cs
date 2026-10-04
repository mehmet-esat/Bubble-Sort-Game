using NUnit.Framework;
using System.Collections.Generic;

namespace ColorSortPuzzle.Tests
{
    public class BoardStateTests
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
        public void MoveToEmptyTube_Succeeds()
        {
            var board = CreateBoard(4, new[] { 1, 1 }, new int[0]);
            Assert.IsTrue(board.Move(0, 1));
            Assert.AreEqual(1, board.GetBallCount(0));
            Assert.AreEqual(1, board.GetBallCount(1));
            Assert.AreEqual(1, board.PeekTop(1));
        }

        [Test]
        public void MoveToSameColor_Succeeds()
        {
            var board = CreateBoard(4, new[] { 2, 1 }, new[] { 1 });
            Assert.IsTrue(board.Move(0, 1));
            Assert.AreEqual(1, board.GetBallCount(0));
            Assert.AreEqual(2, board.GetBallCount(1));
            Assert.AreEqual(1, board.PeekTop(1));
        }

        [Test]
        public void MoveToDifferentColor_Fails()
        {
            var board = CreateBoard(4, new[] { 2, 1 }, new[] { 2 });
            Assert.IsFalse(board.Move(0, 1));
            Assert.AreEqual(2, board.GetBallCount(0)); // Değişmemeli
            Assert.AreEqual(1, board.GetBallCount(1)); // Değişmemeli
        }

        [Test]
        public void MoveToFullTube_Fails()
        {
            var board = CreateBoard(2, new[] { 1, 1 }, new[] { 1, 1 }); // Kapasite 2, hedef dolu
            Assert.IsFalse(board.Move(0, 1));
        }

        [Test]
        public void MoveToSameTube_Fails()
        {
            var board = CreateBoard(4, new[] { 1, 1 }, new int[0]);
            Assert.IsFalse(board.Move(0, 0));
        }

        [Test]
        public void MoveFromEmptyTube_Fails()
        {
            var board = CreateBoard(4, new int[0], new[] { 1, 1 });
            Assert.IsFalse(board.Move(0, 1));
        }

        [Test]
        public void Undo_RestoresExactPreviousState()
        {
            var board = CreateBoard(4, new[] { 2, 1 }, new[] { 1 });
            board.Move(0, 1);
            
            Assert.IsTrue(board.Undo());
            Assert.AreEqual(2, board.GetBallCount(0));
            Assert.AreEqual(1, board.PeekTop(0));
            Assert.AreEqual(1, board.GetBallCount(1));
            Assert.AreEqual(1, board.PeekTop(1));
            Assert.AreEqual(0, board.MoveCount);
        }

        [Test]
        public void IsSolved_AllTubesFullAndSingleColor_ReturnsTrue()
        {
            var board = CreateBoard(2, new[] { 1, 1 }, new[] { 2, 2 }, new int[0]);
            Assert.IsTrue(board.IsSolved());
        }

        [Test]
        public void IsSolved_ColorSplitAcrossTubes_ReturnsFalse()
        {
            // İki farklı tüpte aynı renkten var (bölünmüş), kapasite dolmamış
            var board = CreateBoard(4, new[] { 1, 1 }, new[] { 1, 1 }, new int[0]);
            Assert.IsFalse(board.IsSolved());
        }
    }
}
