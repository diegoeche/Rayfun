using System;

using Raylib_cs;
using rlImGui_cs;
using ImGuiNET;
using Editor;

namespace rlImGui_cs
{
    class Program
    {
        static void Main(string[] args)
        {
            Raylib.SetConfigFlags(ConfigFlags.Msaa4xHint | ConfigFlags.VSyncHint | ConfigFlags.ResizableWindow);
            Raylib.InitWindow(1280, 800, "Map Editor");
            Raylib.SetTargetFPS(144);

            rlImGui.Setup(true);

	    // Texture2D myTexture = Raylib.LoadTexture("assets/assets.png");
	    var textureExplorer = new TextureExplorer();
	    var uiManager = new UIManager();
	    var statsOverlay = new StatsOverlay();

	    while (!Raylib.WindowShouldClose())
            {
		statsOverlay.RecordFrame(Raylib.GetFrameTime());
		uiManager.HandleShortcuts();
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Beige);
		// Raylib.DrawTexture(myTexture, 100, 100, Color.White);
		rlImGui.Begin();

		if (uiManager.ShowEditor) {
		    textureExplorer.Render();
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
    }
}
