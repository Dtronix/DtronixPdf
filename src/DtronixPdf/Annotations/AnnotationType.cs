namespace DtronixPdf.Annotations;

/// <summary>
/// Represents the types of annotations that can be used in a PDF document.
/// </summary>
/// <remarks>
/// Each member of this enumeration corresponds to a specific type of annotation, such as text, link, freetext, etc.
/// </remarks>
public enum AnnotationType
{

    /// <summary>
    /// Represents an unknown annotation type.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Represents a text annotation.
    /// </summary>
    Text = 1,

    /// <summary>
    /// Represents a link annotation.
    /// </summary>
    Link = 2,

    /// <summary>
    /// Represents a free text annotation.
    /// </summary>
    FreeText = 3,

    /// <summary>
    /// Represents a line annotation.
    /// </summary>
    Line = 4,

    /// <summary>
    /// Represents a square annotation.
    /// </summary>
    Square = 5,

    /// <summary>
    /// Represents a circle annotation.
    /// </summary>
    Circle = 6,

    /// <summary>
    /// Represents a polygon annotation.
    /// </summary>
    Polygon = 7,

    /// <summary>
    /// Represents a polyline annotation.
    /// </summary>
    Polyline = 8,

    /// <summary>
    /// Represents a highlight annotation.
    /// </summary>
    Highlight = 9,

    /// <summary>
    /// Represents an underline annotation.
    /// </summary>
    Underline = 10,

    /// <summary>
    /// Represents a squiggly underline annotation.
    /// </summary>
    Squiggly = 11,

    /// <summary>
    /// Represents a strikeout annotation.
    /// </summary>
    Strikeout = 12,

    /// <summary>
    /// Represents a stamp annotation.
    /// </summary>
    Stamp = 13,

    /// <summary>
    /// Represents a caret annotation.
    /// </summary>
    Caret = 14,

    /// <summary>
    /// Represents an ink annotation.
    /// </summary>
    Ink = 15,

    /// <summary>
    /// Represents a popup annotation.
    /// </summary>
    Popup = 16,

    /// <summary>
    /// Represents a file attachment annotation.
    /// </summary>
    FileAttachment = 17,

    /// <summary>
    /// Represents a sound annotation.
    /// </summary>
    Sound = 18,

    /// <summary>
    /// Represents a movie annotation.
    /// </summary>
    Movie = 19,

    /// <summary>
    /// Represents a widget annotation.
    /// </summary>
    Widget = 20,

    /// <summary>
    /// Represents a screen annotation.
    /// </summary>
    Screen = 21,

    /// <summary>
    /// Represents a printer mark annotation.
    /// </summary>
    PrinterMark = 22,

    /// <summary>
    /// Represents a trapnet annotation.
    /// </summary>
    TrapNet = 23,

    /// <summary>
    /// Represents a watermark annotation.
    /// </summary>
    Watermark = 24,

    /// <summary>
    /// Represents a 3D annotation.
    /// </summary>
    ThreeD = 25,

    /// <summary>
    /// Represents a rich media annotation.
    /// </summary>
    RichMedia = 26,

    /// <summary>
    /// Represents an XFA widget annotation.
    /// </summary>
    XfaWidget = 27,

    /// <summary>
    /// Represents a redact annotation.
    /// </summary>
    Redact = 28,

}
