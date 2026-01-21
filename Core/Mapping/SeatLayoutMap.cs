// 🔄 ОНОВИТИ: Core/Mapping/SeatLayoutMap.cs
namespace Core.Mapping;

public sealed class SeatLayoutMap
{
    public const int Size = 16;
    public const int Cells = Size * Size;
    public const int Bytes = 64;

    private readonly byte[] _data;

    public SeatLayoutMap()
    {
        _data = new byte[Bytes];
    }

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

    // ✅ ЗМІНИТИ: повертає SeatTypeId (int) замість SeatType (enum)
    // 0 = Empty, 1 = Standard, 2 = Premium, 3 = VIP
    public int Get(int row, int col)
    {
        Validate(row, col);

        int index = Index(row, col);
        int bitOffset = index * 2;
        int byteIndex = bitOffset >> 3;
        int shift = bitOffset & 7;

        return (_data[byteIndex] >> shift) & 0b11;
    }

    // ✅ ЗМІНИТИ: приймає SeatTypeId (int) замість SeatType (enum)
    public void Set(int row, int col, int seatTypeId)
    {
        Validate(row, col);

        if (seatTypeId < 0 || seatTypeId > 3)
            throw new ArgumentException("SeatTypeId must be 0-3", nameof(seatTypeId));

        int index = Index(row, col);
        int bitOffset = index * 2;
        int byteIndex = bitOffset >> 3;
        int shift = bitOffset & 7;

        _data[byteIndex] &= (byte)~(0b11 << shift);
        _data[byteIndex] |= (byte)(seatTypeId << shift);
    }

    public byte[] ToByteArray() => _data;

    public static SeatLayoutMap FromByteArray(byte[] data)
        => new SeatLayoutMap(data);
}