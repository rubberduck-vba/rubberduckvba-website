namespace rubberduckvba.com.Server.ContentSynchronization;

/// <summary>
/// Encapsulates the inspection type and default severity setting override for an inspection.
/// </summary>
public record class InspectionDefaultConfig
{
    /// <summary>
    /// Get/sets the name (unique) of the inspection.
    /// </summary>
    public string InspectionName { get; init; } = default!;
    /// <summary>
    /// Gets/sets the type of inspection.
    /// </summary>
    public string InspectionType { get; init; } = default!;
    /// <summary>
    /// Gets/sets the default severity setting value.
    /// </summary>
    public string DefaultSeverity { get; init; } = default!;
}
