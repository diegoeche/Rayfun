using System;

using Raylib_cs;
using rlImGui_cs;
using ImGuiNET;

namespace rlImGui_cs
{
    class Program
    {
        static void Main(string[] args)
        {
            Raylib.SetConfigFlags(ConfigFlags.Msaa4xHint | ConfigFlags.VSyncHint | ConfigFlags.ResizableWindow);
            Raylib.InitWindow(1280, 800, "raylib-Extras-cs [ImGui] example - simple ImGui Demo");
            Raylib.SetTargetFPS(144);

            rlImGui.Setup(true);

	    // Texture2D myTexture = Raylib.LoadTexture("assets/assets.png");
	    var assetBrowser = new AssetBrowser();

	    while (!Raylib.WindowShouldClose())
            {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Beige);

		// Raylib.DrawTexture(myTexture, 100, 100, Color.White);
                rlImGui.Begin();

		assetBrowser.Render();

                if (ImGui.Begin("Simple Window"))
                {
                    ImGui.TextUnformatted("Icon text " + IconFonts.FontAwesome6.Book);
                }

                ImGui.End();
                rlImGui.End();

                Raylib.EndDrawing();
            }

            rlImGui.Shutdown();
            Raylib.CloseWindow();
        }
    }
}
