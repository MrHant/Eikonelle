namespace Eikonelle;

/// <summary>How a screenshot is taken when the screenshot hotkey is pressed.</summary>
public enum CaptureMode
{
    /// <summary>The whole primary monitor.</summary>
    FullScreen,

    /// <summary>A region of the primary monitor dragged out by the user.</summary>
    Region,
}
