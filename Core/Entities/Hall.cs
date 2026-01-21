namespace Core.Entities;

public class Hall
{
    public int Id { get; private set; }
    public string Name { get; private set; } = null!;
    
    public byte Rows { get; private set; }
    public byte Columns { get; private set; }
    
    public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    public ICollection<Session> Sessions { get; set; } = new List<Session>();

    private Hall() { }

    public Hall(string name, byte rows, byte columns)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Hall name cannot be empty", nameof(name));
        
        if (rows == 0 || rows > 50)
            throw new ArgumentException("Rows must be between 1 and 50", nameof(rows));
        
        if (columns == 0 || columns > 50)
            throw new ArgumentException("Columns must be between 1 and 50", nameof(columns));
        
        Name = name;
        Rows = rows;
        Columns = columns;
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Hall name cannot be empty", nameof(name));
        
        Name = name;
    }

    public void UpdateDimensions(byte rows, byte columns)
    {
        if (rows == 0 || rows > 50)
            throw new ArgumentException("Rows must be between 1 and 50", nameof(rows));
        
        if (columns == 0 || columns > 50)
            throw new ArgumentException("Columns must be between 1 and 50", nameof(columns));
        
        Rows = rows;
        Columns = columns;
    }
}