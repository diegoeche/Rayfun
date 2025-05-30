using System.Numerics;
using ImGuiNET;
using Raylib_cs;

public static class GlobalSettings
{
    public static bool Use3D = false;

    // Default camera settings
    private static readonly Vector3 DefaultPosition = new Vector3(30, 20, 30);
    private static readonly Vector3 DefaultTarget = new Vector3(10, 0, 10);
    private static readonly Vector3 DefaultUp = new Vector3(0, 1, 0);
    private const float DefaultScale = 3.0f;
    private const float DefaultFovY = 60.0f;
    private const CameraProjection DefaultProjection = CameraProjection.Perspective;

    // Current camera settings
    public static Vector3 CameraPosition = DefaultPosition;
    public static Vector3 CameraTarget = DefaultTarget;
    public static Vector3 CameraUp = DefaultUp;
    public static float Scale = DefaultScale;
    public static float CameraFovY = DefaultFovY;
    public static CameraProjection CameraProjection = DefaultProjection;

    public static Camera3D Camera => new Camera3D
    {
        Position = CameraPosition,
        Target = CameraTarget,
        Up = CameraUp,
        FovY = CameraFovY,
        Projection = CameraProjection
    };

    public static void Render()
    {
	ImGui.Begin("Global Settings");

	ImGui.Checkbox("3D Mode", ref Use3D);
	ImGui.Separator();

	ImGui.Text("Map Center / Camera Position");

        if (Use3D) {
	    ImGui.DragFloat3("Position (X, Y, Z)", ref CameraPosition, 0.1f);
	} else {
	    Vector2 pos2D = new Vector2(CameraPosition.X, CameraPosition.Y);
	    if (ImGui.DragFloat2("Position (X, Y)", ref pos2D, 0.1f))
	    {
		CameraPosition.X = pos2D.X;
		CameraPosition.Y = pos2D.Y;
	    }
	    ImGui.DragFloat("Zoom:", ref Scale, 0.1f, 1.0f, 8.0f, "%.1fx");
	}

        if (Use3D)
	{
	    ImGui.Separator();
	    ImGui.Text("3D Camera Settings");

	    ImGui.DragFloat3("Target", ref CameraTarget, 0.1f);
	    ImGui.DragFloat3("Up Vector", ref CameraUp, 0.01f, -1f, 1f);
	    ImGui.DragFloat("FOV Y", ref CameraFovY, 0.1f, 10f, 120f);

	    int projectionIndex = (int)CameraProjection;
	    if (ImGui.Combo("Projection", ref projectionIndex, "Perspective\0Orthographic\0"))
	    {
		CameraProjection = (CameraProjection)projectionIndex;
	    }
	}

	ImGui.Separator();
	if (ImGui.Button("Reset to Defaults"))
	{
	    ResetCamera();
	}

	ImGui.End();
    }

    private static void ResetCamera()
    {
        CameraPosition = DefaultPosition;
        CameraTarget = DefaultTarget;
        CameraUp = DefaultUp;
        CameraFovY = DefaultFovY;
        CameraProjection = DefaultProjection;
    }
}
