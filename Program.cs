using System;

using Raylib_cs;
using rlImGui_cs;
using ImGuiNET;
using Editor;
using Game;
using System.Numerics;
using System.Runtime.InteropServices;

class Program
{
    static void Main(string[] args)
    {
	Raylib.SetConfigFlags(ConfigFlags.Msaa4xHint | ConfigFlags.VSyncHint | ConfigFlags.ResizableWindow);
	Raylib.InitWindow(1280, 800, "Map Editor");
	Raylib.SetTargetFPS(144);
	rlImGui.Setup(true);
        // var font = SetupIcons();
        // rlImGui.ReloadFonts();

        // Texture2D myTexture = Raylib.LoadTexture("assets/assets.png");
        var fileExplorer = new FileExplorer();
        var uiManager = new UIManager();
	var statsOverlay = new StatsOverlay();
	var atlasExplorer = new AtlasExplorer();
	var map = InitializeDummyMap();

	while (!Raylib.WindowShouldClose())
	{
	    statsOverlay.RecordFrame(Raylib.GetFrameTime());
	    uiManager.HandleShortcuts();

	    Raylib.BeginDrawing();
	    Raylib.ClearBackground(Color.Beige);

	    RenderMap(map);

	    rlImGui.Begin();
	    // ImGui.ShowMetricsWindow();

            if (uiManager.ShowEditor) {
		GlobalSettings.Render();
		fileExplorer.Render();
		atlasExplorer.Render();
		Log.Render();
	    }
	    if (uiManager.ShowStats) {
		statsOverlay.Render();
	    }

            rlImGui.End();

	    Raylib.EndDrawing();
	}

	rlImGui.Shutdown();
	Raylib.CloseWindow();
    }

    static void RenderMap(IMap map)
    {
	int centerX = (int)GlobalSettings.CameraPosition.X;
	int centerY = (int)GlobalSettings.CameraPosition.Y;
	int centerZ = (int)GlobalSettings.CameraPosition.Z;

	if (GlobalSettings.Use3D)
	{
	    Raylib.BeginMode3D(GlobalSettings.Camera);
	    MapRenderer.Render3D(map, centerX - 10, centerY - 10, centerZ, 10, 1f);
	    Raylib.EndMode3D();
	}
	else
	{
	    int width = Raylib.GetScreenWidth();
	    int height = Raylib.GetScreenHeight();

	    MapRenderer.Render(map, centerX, centerY, 0, width, height, GlobalSettings.Scale);
	}
    }

    static IMap InitializeDummyMap() {
	var map = new SparseMap();

	// Fill a small area with test voxels
	var radius = 100;
	for (int x = -radius; x <= radius; x++)
	{
	    for (int y = -radius; y <= radius; y++)
	    {
		string type;
		int mod = (x + y) % 3;
		if (mod == 0)
		    type = "grass";
		else if (mod == 1)
		    type = "dirt";
		else
		    type = "water";

		map.Set(x, y, 0, new Voxel(type));
	    }
	}
	return map;
    }
}
