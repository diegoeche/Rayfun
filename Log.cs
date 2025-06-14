using System;
using System.Collections.Generic;
using ImGuiNET;

public static class Log
{
    private struct LogEntry
    {
        public string Message;
        public string Timestamp;
        public int Count;

        public LogEntry(string message)
        {
            Message = message;
            Timestamp = DateTime.Now.ToString("HH:mm:ss");
            Count = 1;
        }
    }

    private static readonly List<LogEntry> _entries = new();
    private static bool _scrollToBottom = true;

    public static void Write(string message)
    {
        if (_entries.Count > 0 && _entries[^1].Message == message)
        {
            var last = _entries[^1];
            last.Count++;
            _entries[^1] = last;
        }
        else
        {
            _entries.Add(new LogEntry(message));
        }

        _scrollToBottom = true;
    }

    public static void Render()
    {
	ImGui.Begin("Log");

	// Start a horizontal bar
	if (ImGui.BeginTable("LogTopBar", 2, ImGuiTableFlags.SizingStretchProp))
	{
	    ImGui.TableSetupColumn("Label", ImGuiTableColumnFlags.WidthStretch);
	    ImGui.TableSetupColumn("Buttons", ImGuiTableColumnFlags.WidthFixed);

	    ImGui.TableNextRow();
	    ImGui.TableSetColumnIndex(0);
	    ImGui.Text("Log Output");

	    ImGui.TableSetColumnIndex(1);
	    if (ImGui.Button("Clear"))
	    {
		_entries.Clear();
	    }

	    ImGui.EndTable();
	}

	// Scrollable log area
	ImGui.BeginChild("LogScroll", new System.Numerics.Vector2(0, 0), ImGuiChildFlags.None, ImGuiWindowFlags.HorizontalScrollbar);

	foreach (var entry in _entries)
	{
	    ImGui.TextColored(new System.Numerics.Vector4(0.8f, 0.8f, 0.8f, 1.0f), $"[{entry.Timestamp}] {entry.Message}");

	    if (entry.Count > 1)
	    {
		float rightAlign = ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - 30;
		ImGui.SameLine(rightAlign);
		ImGui.TextColored(new System.Numerics.Vector4(1.0f, 0.7f, 0.2f, 1.0f), $"×{entry.Count}");
	    }
	}

	if (_scrollToBottom && ImGui.GetScrollY() >= ImGui.GetScrollMaxY() - 10)
	    ImGui.SetScrollHereY(1.0f);

	_scrollToBottom = false;

	ImGui.EndChild();
	ImGui.End();
    }
}
