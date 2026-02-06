namespace InternalPortal.Domain.ValueObjects;

public class DateTimeRange : IEquatable<DateTimeRange>
{
    public DateTime StartUtc { get; }
    public DateTime EndUtc { get; }

    private DateTimeRange() { }

    public DateTimeRange(DateTime startUtc, DateTime endUtc)
    {
        if (endUtc <= startUtc)
            throw new ArgumentException("End date must be after start date.");

        StartUtc = startUtc;
        EndUtc = endUtc;
    }

    public bool Overlaps(DateTimeRange other)
    {
        return StartUtc < other.EndUtc && other.StartUtc < EndUtc;
    }

    public TimeSpan Duration => EndUtc - StartUtc;

    public bool Equals(DateTimeRange? other)
    {
        if (other is null) return false;
        return StartUtc == other.StartUtc && EndUtc == other.EndUtc;
    }

    public override bool Equals(object? obj) => Equals(obj as DateTimeRange);
    public override int GetHashCode() => HashCode.Combine(StartUtc, EndUtc);
}
