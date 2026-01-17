using Core.Enums;

namespace Core.Mapping;
public sealed class SeatLayoutMap
{
    public const int Size = 16;
    public const int Cells = Size * Size;
    public const int Bytes = 64; // 256 * 2 bits

    private readonly byte[] _data;

    /// <summary>
    /// Порожній layout (усе Empty)
    /// </summary>
    public SeatLayoutMap()
    {
        _data = new byte[Bytes];
    }

    /// <summary>
    /// Відновлення з БД
    /// </summary>
    private SeatLayoutMap(byte[] data)
    {
        if (data.Length != Bytes)
            throw new ArgumentException("Invalid layout size");

        _data = data;
    }

    private static int Index(int row, int col)
        => row * Size + col;

    private static void Validate(int row, int col)
    {
        if ((uint)row >= Size)
            throw new ArgumentOutOfRangeException(nameof(row));
        if ((uint)col >= Size)
            throw new ArgumentOutOfRangeException(nameof(col));
    }

    public SeatType Get(int row, int col)
    {
        Validate(row, col);

        int index = Index(row, col);
        int bitOffset = index * 2;
        int byteIndex = bitOffset >> 3;
        int shift = bitOffset & 7;

        return (SeatType)((_data[byteIndex] >> shift) & 0b11);
    }

    public void Set(int row, int col, SeatType type)
    {
        Validate(row, col);

        int index = Index(row, col);
        int bitOffset = index * 2;
        int byteIndex = bitOffset >> 3;
        int shift = bitOffset & 7;

        _data[byteIndex] &= (byte)~(0b11 << shift);     // очистити
        _data[byteIndex] |= (byte)((byte)type << shift);
    }

    // ===== DB boundary =====

    public byte[] ToByteArray() => _data;

    public static SeatLayoutMap FromByteArray(byte[] data)
        => new SeatLayoutMap(data);
}