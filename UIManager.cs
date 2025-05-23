using System;
using Raylib_cs;

public class UIManager
{
    public bool ShowEditor { get; private set; } = true;
    public bool ShowStats { get; private set; } = true;

    public void ToggleEditor()
    {
	ShowEditor = !ShowEditor;
    }

    public void ToggleStats()
    {
	ShowStats = !ShowStats;
    }

    public void HandleShortcuts()
    {
	if (Raylib.IsKeyPressed(KeyboardKey.Tab))        // Toggle main UI
	    ToggleEditor();

	if (Raylib.IsKeyPressed(KeyboardKey.F2))         // Toggle stats overlay
	    ToggleStats();
    }
}
