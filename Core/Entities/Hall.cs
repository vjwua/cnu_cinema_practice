
using Core.Mapping;

namespace Core.Entities;

public class Hall
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public byte[] SeatLayout { get; private set; } = null!;
    public ICollection<Session> Sessions { get; set; } = new List<Session>();


    private Hall() { }

    public Hall(SeatLayoutMap layout)
    {
        SeatLayout = layout.ToByteArray();
    }

    public SeatLayoutMap GetLayout()
        => SeatLayoutMap.FromByteArray(SeatLayout);
    
    public void UpdateLayout(Action<SeatLayoutMap> update)
    {
        var map = SeatLayoutMap.FromByteArray(SeatLayout);
        update(map);
        SeatLayout = map.ToByteArray();
    }

}
