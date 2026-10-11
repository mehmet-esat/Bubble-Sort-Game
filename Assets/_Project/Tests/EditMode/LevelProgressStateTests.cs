using NUnit.Framework;

namespace ColorSortPuzzle.Tests
{
    public class LevelProgressStateTests
    {
        [Test]
        public void InitialState_HighestUnlockedAndPlayingMatch_Clamped()
        {
            var state = new LevelProgressState(0, 5);
            Assert.AreEqual(0, state.HighestUnlockedIndex);
            Assert.AreEqual(0, state.PlayingIndex);
            Assert.AreEqual(5, state.TotalLevels);
        }

        [Test]
        public void ClampSavedIndex_WhenSavedIndexGreaterThanTotalLevels_ClampsToLastLevel()
        {
            var state = new LevelProgressState(10, 5);
            Assert.AreEqual(4, state.HighestUnlockedIndex);
            Assert.AreEqual(4, state.PlayingIndex);
        }

        [Test]
        public void ClampSavedIndex_WhenSavedIndexNegative_ClampsToZero()
        {
            var state = new LevelProgressState(-5, 5);
            Assert.AreEqual(0, state.HighestUnlockedIndex);
            Assert.AreEqual(0, state.PlayingIndex);
        }

        [Test]
        public void IsLevelUnlocked_ReturnsTrueForUnlocked_FalseForLocked()
        {
            var state = new LevelProgressState(2, 5);
            Assert.IsTrue(state.IsLevelUnlocked(0));
            Assert.IsTrue(state.IsLevelUnlocked(1));
            Assert.IsTrue(state.IsLevelUnlocked(2));
            Assert.IsFalse(state.IsLevelUnlocked(3));
            Assert.IsFalse(state.IsLevelUnlocked(4));
            Assert.IsFalse(state.IsLevelUnlocked(-1));
            Assert.IsFalse(state.IsLevelUnlocked(5));
        }

        [Test]
        public void SetPlayingIndex_UnlockedLevel_Succeeds()
        {
            var state = new LevelProgressState(3, 5);
            Assert.IsTrue(state.SetPlayingIndex(1));
            Assert.AreEqual(1, state.PlayingIndex);
        }

        [Test]
        public void SetPlayingIndex_LockedLevel_FailsAndKeepsPrevious()
        {
            var state = new LevelProgressState(1, 5);
            Assert.IsFalse(state.SetPlayingIndex(3));
            Assert.AreEqual(1, state.PlayingIndex);
        }

        [Test]
        public void CompleteCurrentLevel_WhenCompletingHighestUnlocked_UnlocksNextLevel()
        {
            var state = new LevelProgressState(1, 5);
            bool unlockedNext = state.CompleteCurrentLevel();

            Assert.IsTrue(unlockedNext);
            Assert.AreEqual(2, state.HighestUnlockedIndex);
            Assert.AreEqual(2, state.PlayingIndex);
        }

        [Test]
        public void CompleteCurrentLevel_WhenCompletingEarlierLevel_DoesNotChangeHighestUnlocked()
        {
            var state = new LevelProgressState(3, 5);
            state.SetPlayingIndex(1);

            bool unlockedNext = state.CompleteCurrentLevel();

            Assert.IsFalse(unlockedNext);
            Assert.AreEqual(3, state.HighestUnlockedIndex); // İlerleme bozulmaz / artmaz
            Assert.AreEqual(2, state.PlayingIndex); // Oynanan seviye 1'den 2'ye geçer
        }

        [Test]
        public void CompleteCurrentLevel_WhenOnLastLevel_WrapsPlayingIndexToZero_DoesNotExceedMax()
        {
            var state = new LevelProgressState(4, 5);
            Assert.AreEqual(4, state.PlayingIndex);

            bool unlockedNext = state.CompleteCurrentLevel();

            Assert.IsFalse(unlockedNext);
            Assert.AreEqual(4, state.HighestUnlockedIndex); // Son indexi aşamaz
            Assert.AreEqual(0, state.PlayingIndex); // Başa döner (Level_001)
        }

        [Test]
        public void EdgeCase_ZeroLevels_HandlesGracefullyWithoutThrowing()
        {
            var state = new LevelProgressState(0, 0);
            Assert.AreEqual(0, state.TotalLevels);
            Assert.AreEqual(0, state.HighestUnlockedIndex);
            Assert.AreEqual(0, state.PlayingIndex);
            Assert.IsFalse(state.IsLevelUnlocked(0));
            Assert.IsFalse(state.SetPlayingIndex(0));
            Assert.IsFalse(state.CompleteCurrentLevel());
            Assert.AreEqual(0, state.GetDisplayLevelNumber());
        }

        [Test]
        public void EdgeCase_SingleLevel_HandlesUnlockingAndWrapping()
        {
            var state = new LevelProgressState(0, 1);
            Assert.AreEqual(1, state.TotalLevels);
            Assert.AreEqual(0, state.HighestUnlockedIndex);
            Assert.AreEqual(0, state.PlayingIndex);
            Assert.IsTrue(state.IsLevelUnlocked(0));
            Assert.IsFalse(state.IsLevelUnlocked(1));

            bool unlockedNext = state.CompleteCurrentLevel();
            Assert.IsFalse(unlockedNext);
            Assert.AreEqual(0, state.HighestUnlockedIndex);
            Assert.AreEqual(0, state.PlayingIndex);
            Assert.AreEqual(1, state.GetDisplayLevelNumber());
        }

        [Test]
        public void IsOnLastLevel_SingleLevel_ReturnsTrue()
        {
            var state = new LevelProgressState(0, 1);
            Assert.IsTrue(state.IsOnLastLevel);
        }

        [Test]
        public void IsOnLastLevel_MultipleLevels_WhenOnLastLevel_ReturnsTrue()
        {
            var state = new LevelProgressState(4, 5);
            Assert.AreEqual(4, state.PlayingIndex);
            Assert.IsTrue(state.IsOnLastLevel);
        }

        [Test]
        public void IsOnLastLevel_MultipleLevels_WhenOnEarlierLevel_ReturnsFalse()
        {
            var state = new LevelProgressState(4, 5);
            state.SetPlayingIndex(2);
            Assert.AreEqual(2, state.PlayingIndex);
            Assert.IsFalse(state.IsOnLastLevel);
        }

        [Test]
        public void IsOnLastLevel_MultipleLevels_WhenOnFirstLevel_ReturnsFalse()
        {
            var state = new LevelProgressState(0, 5);
            Assert.AreEqual(0, state.PlayingIndex);
            Assert.IsFalse(state.IsOnLastLevel);
        }

        [Test]
        public void IsOnLastLevel_ZeroLevels_ReturnsFalse()
        {
            var state = new LevelProgressState(0, 0);
            Assert.IsFalse(state.IsOnLastLevel);
        }
    }
}
