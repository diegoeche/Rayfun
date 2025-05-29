using System;

using Raylib_cs;
using rlImGui_cs;
using ImGuiNET;
using Editor;
using Game;
using System.Numerics;

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
            var atlasExplorer = new AtlasExplorer();
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

            while (!Raylib.WindowShouldClose())
            {
                var total = 0;
                while (total < 1000000) {
                    total++;
                }

                statsOverlay.RecordFrame(Raylib.GetFrameTime());
		uiManager.HandleShortcuts();
		textureExplorer.HandleInput();

                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Beige);

		var centerX = 10;
		var centerY = 10;
		var centerZ = 0;

		Camera3D camera = new Camera3D
		{
		    Position = new Vector3(centerX + 20, centerZ + 20, centerY + 20),
		    Target = new Vector3(centerX, centerZ, centerY),
		    Up = new Vector3(0, 1, 0),
		    FovY = 45.0f,
		    Projection = CameraProjection.Perspective
		};

		Raylib.BeginMode3D(camera);
		MapRenderer.Render3D(map, centerX, centerY, centerZ, 10, 1f);
		Raylib.EndMode3D();

		Raylib.BeginMode3D(camera);

		MapRenderer.Render3D(map, 10, 10, 10, 40, 1f);

		Raylib.EndMode3D();

                rlImGui.Begin();

		if (uiManager.ShowEditor) {
		    textureExplorer.Render();
                    atlasExplorer.Render();
                }
                Log.Render();
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
