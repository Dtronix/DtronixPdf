namespace DtronixPdf.Annotations;

/// <summary>
/// Specifies the appearance mode for an annotation in a PDF document.
/// </summary>
public enum AnnotationAppearanceMode
{
    /// <summary>
    /// The annotation appears in its normal state.
    /// </summary>
    Normal = 0,
    /// <summary>
    /// The annotation appears in a rollover state.
    /// </summary>
    Rollover = 1,
    /// <summary>
    /// The annotation appears in a pressed down state.
    /// </summary>
    Down = 2,
    /// <summary>
    /// Represents the count of appearance modes.
    /// </summary>
    Count = 3
}
