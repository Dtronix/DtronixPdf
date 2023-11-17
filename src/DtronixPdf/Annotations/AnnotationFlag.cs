using System;

namespace DtronixPdf.Annotations;

/// <summary>
/// Defines the flags for PDF annotations.
/// </summary>
/// <remarks>
/// The flags are used to control the visibility and behavior of annotations.
/// </remarks>
[Flags]
public enum AnnotationFlag
{
    /// <summary>
    /// Represents the default state.
    /// </summary>
    None = 0,

    /// <summary>
    /// Represents a state where the object is invisible.
    /// </summary>
    Invisible = 1 << 0,

    /// <summary>
    /// Represents a state where the object is hidden.
    /// </summary>
    Hidden = 1 << 1,

    /// <summary>
    /// Represents a state where the object is printable.
    /// </summary>
    Print = 1 << 2,

    /// <summary>
    /// Represents a state where the object cannot be zoomed.
    /// </summary>
    NoZoom = 1 << 3,

    /// <summary>
    /// Represents a state where the object cannot be rotated.
    /// </summary>
    NoRotate = 1 << 4,

    /// <summary>
    /// Represents a state where the object cannot be viewed.
    /// </summary>
    NoView = 1 << 5,

    /// <summary>
    /// Represents a state where the object is read-only.
    /// </summary>
    Readonly = 1 << 6,

    /// <summary>
    /// Represents a state where the object is locked.
    /// </summary>
    Locked = 1 << 7,

    /// <summary>
    /// Represents a state where the object's viewability can be toggled.
    /// </summary>
    ToggleNoView = 1 << 8
}
