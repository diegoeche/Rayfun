using Raylib_cs;
using rlImGui_cs;


class Program
{
    static void Main(string[] args)
    {
        Raylib.SetConfigFlags(ConfigFlags.Msaa4xHint | ConfigFlags.VSyncHint | ConfigFlags.ResizableWindow);
        Raylib.InitWindow(1280, 800, "Map Editor");
        Raylib.SetTargetFPS(144);
        rlImGui.Setup(true);

        var engine = new Engine();
        engine.Run();

        rlImGui.Shutdown();
        Raylib.CloseWindow();
    }
}
