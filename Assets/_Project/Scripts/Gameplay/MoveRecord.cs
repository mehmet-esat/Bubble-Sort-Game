namespace ColorSortPuzzle
{
    /// <summary>
    /// Tek bir hamlenin kaydı. Saf C#, UnityEngine bağımlılığı yok.
    /// Undo işleminde tersine çevirmek için yeterli bilgiyi tutar.
    /// </summary>
    public readonly struct MoveRecord
    {
        public readonly int SourceIndex;
        public readonly int TargetIndex;
        public readonly int ColorId;

        public MoveRecord(int sourceIndex, int targetIndex, int colorId)
        {
            SourceIndex = sourceIndex;
            TargetIndex = targetIndex;
            ColorId = colorId;
        }
    }
}
