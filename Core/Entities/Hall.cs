namespace Core.Entities;

using Core.Mapping;

public class Hall
{
    public int Id { get; private set; }
    public string Name { get; private set; } = null!;
    public byte[] SeatLayout { get; private set; } = null!;
    
    // ✅ ДОДАТИ:
    public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    
    public ICollection<Session> Sessions { get; set; } = new List<Session>();

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

    public void UpdateLayout(SeatLayoutMap newLayout)
    {
        ArgumentNullException.ThrowIfNull(newLayout);
        SeatLayout = newLayout.ToByteArray();
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Hall name cannot be empty", nameof(name));
        
        Name = name;
    }
}