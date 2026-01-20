using Core.Mapping;

namespace Core.Entities;

public class Hall
{
    public int Id { get; private set; }
    public string Name { get; private set; } = null!;
    public byte[] SeatLayout { get; private set; } = null!;
    public ICollection<Session> Sessions { get; private set; } = new List<Session>();

    private Hall() { }

    public Hall(string name, SeatLayoutMap layout)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Hall name cannot be empty", nameof(name));
        
        Name = name;
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

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Hall name cannot be empty", nameof(name));
        
        Name = name;
    }
}