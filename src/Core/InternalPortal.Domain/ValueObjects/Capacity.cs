namespace InternalPortal.Domain.ValueObjects;

public class Capacity : IEquatable<Capacity>
{
    public int MinAttendees { get; }
    public int MaxAttendees { get; }

    private Capacity() { }

    public Capacity(int minAttendees, int maxAttendees)
    {
        if (minAttendees < 0)
            throw new ArgumentException("Minimum attendees cannot be negative.", nameof(minAttendees));
        if (maxAttendees < minAttendees)
            throw new ArgumentException("Maximum attendees must be greater than or equal to minimum.", nameof(maxAttendees));

        MinAttendees = minAttendees;
        MaxAttendees = maxAttendees;
    }

    public bool IsFull(int currentCount) => currentCount >= MaxAttendees;
    public int RemainingSpots(int currentCount) => Math.Max(0, MaxAttendees - currentCount);

    public bool Equals(Capacity? other)
    {
        if (other is null) return false;
        return MinAttendees == other.MinAttendees && MaxAttendees == other.MaxAttendees;
    }

    public override bool Equals(object? obj) => Equals(obj as Capacity);
    public override int GetHashCode() => HashCode.Combine(MinAttendees, MaxAttendees);
}
