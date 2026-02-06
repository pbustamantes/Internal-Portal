namespace InternalPortal.Domain.ValueObjects;

public class Address : IEquatable<Address>
{
    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public string ZipCode { get; }
    public string? Building { get; }
    public string? Room { get; }

    private Address() 
    {
        Street = string.Empty;
        City = string.Empty;
        State = string.Empty;
        ZipCode = string.Empty;
    }

    public Address(string street, string city, string state, string zipCode, string? building = null, string? room = null)
    {
        Street = street ?? throw new ArgumentNullException(nameof(street));
        City = city ?? throw new ArgumentNullException(nameof(city));
        State = state ?? throw new ArgumentNullException(nameof(state));
        ZipCode = zipCode ?? throw new ArgumentNullException(nameof(zipCode));
        Building = building;
        Room = room;
    }

    public bool Equals(Address? other)
    {
        if (other is null) return false;
        return Street == other.Street && City == other.City && State == other.State
            && ZipCode == other.ZipCode && Building == other.Building && Room == other.Room;
    }

    public override bool Equals(object? obj) => Equals(obj as Address);
    public override int GetHashCode() => HashCode.Combine(Street, City, State, ZipCode, Building, Room);
    public override string ToString() => $"{Street}, {City}, {State} {ZipCode}";
}
