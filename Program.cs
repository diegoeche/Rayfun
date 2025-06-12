using System;
using Raylib_cs;
using rlImGui_cs;
using ImGuiNET;
using Editor;
using Game;
using System.Numerics;

class Program
{
    static void Main(string[] args)
    {
        Raylib.SetConfigFlags(ConfigFlags.Msaa4xHint | ConfigFlags.VSyncHint | ConfigFlags.ResizableWindow);
        Raylib.InitWindow(1280, 800, "Map Editor");
        Raylib.SetTargetFPS(144);
        rlImGui.Setup(true);

        var fileExplorer = new FileExplorer();
        var uiManager = new UIManager();
        var statsOverlay = new StatsOverlay();
        var atlasExplorer = new AtlasExplorer();
        var mapExplorer = new MapExplorer();
	var map = MapSerializer.Load(mapExplorer.LastMap);

        var gameAtlas = new GameAtlas();
        gameAtlas.Load("./assets/assets.atlas.json");
        gameAtlas.Load("./assets/weapons.atlas.json");
        gameAtlas.Load("./assets/bodies.atlas.json");

        var voxelMapper = new VoxelSpriteMapper(gameAtlas);
        voxelMapper.LoadFromFile();
	var gameRenderer = new GameRenderer(gameAtlas, voxelMapper.GetMappings());

        while (!Raylib.WindowShouldClose())
        {
	    statsOverlay.RecordFrame(Raylib.GetFrameTime());
            uiManager.HandleShortcuts();

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Beige);

            RenderMap(map, gameRenderer);

            rlImGui.Begin();

            if (uiManager.ShowEditor)
            {
                GlobalSettings.Render();
                fileExplorer.Render();
                atlasExplorer.Render();
                mapExplorer.Render(map);
                voxelMapper.Render();
                Log.Render();

                if (mapExplorer.RequestedLoad && mapExplorer.SelectedMap != null)
                {
                    try
                    {
                        map = MapSerializer.Load(mapExplorer.SelectedMap);
                        Log.Write($"Loaded map: {mapExplorer.SelectedMap}");
                    }
                    catch (Exception e)
                    {
                        Log.Write($"Failed to load map: {e.Message}");
                    }

                    mapExplorer.ClearRequests(); // <- clear the flag
                }
            }

            if (uiManager.ShowStats)
            {
                statsOverlay.Render();
            }

            rlImGui.End();
            Raylib.EndDrawing();
        }

        rlImGui.Shutdown();
        Raylib.CloseWindow();
    }

    static void RenderMap(IMap map, GameRenderer renderer)
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
	    renderer.Render(map, centerX, centerY, 0, width, height, GlobalSettings.Scale);
        }
    }
}
