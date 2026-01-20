namespace Core.Constants;

public static class HallLayout
{
    public const int Size = 16;
    public const int Cells = Size * Size; // 256
    public const int BitmapSize = Cells * 2 / 8; // 64 bytes (2 біти на комірку)
}