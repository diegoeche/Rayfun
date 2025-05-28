using Raylib_cs;
using ImGuiNET;
using System.Numerics;

public class StatsOverlay
{
    private const int MaxSamples = 120;
    private readonly float[] _frameTimes = new float[MaxSamples];
    private int _index = 0;

    public void RecordFrame(float deltaTime)
    {
        _frameTimes[_index] = deltaTime * 1000f; // convert to milliseconds
        _index = (_index + 1) % MaxSamples;
    }

    public void Render()
    {
        ImGui.SetNextWindowBgAlpha(0.35f); // Transparent background
        ImGui.Begin(
	    "Stats",
	    ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoDecoration |
	    ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoNav
	);

        ImGui.Text($"FPS: {Raylib.GetFPS()}");
        ImGui.PlotLines("Frame Time (ms)", ref _frameTimes[0], MaxSamples, _index,
            null, 0.0f, 40.0f, new Vector2(200, 60));

        ImGui.End();
    }
}
