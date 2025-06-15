using Raylib_cs;
using ImGuiNET;
using System.Numerics;
using Game;

namespace Editor
{
    public interface ITileClickAction
    {
        void OnTileHovered(int voxelX, int voxelY, Vector2 screenPos);
        void OnTileClicked(int voxelX, int voxelY, Vector2 screenPos);
    }


    public static class TileSelector
    {
        public static void HandleInteraction(GameRenderer renderer, Ref<IMap> refMap, ITileClickAction? action)
        {
            if (action == null || ImGui.GetIO().WantCaptureMouse)
                return;

            int screenWidth = Raylib.GetScreenWidth();
            int screenHeight = Raylib.GetScreenHeight();
            float scale = GlobalSettings.Scale;
            int centerX = (int)GlobalSettings.CameraPosition.X;
            int centerY = (int)GlobalSettings.CameraPosition.Y;

            float tileSize = 16 * scale;
            int tilesX = (int)Math.Ceiling(screenWidth / tileSize);
            int tilesY = (int)Math.Ceiling(screenHeight / tileSize);
            int radiusX = tilesX / 2;
            int radiusY = tilesY / 2;

            Vector2 mousePos = Raylib.GetMousePosition();
            int dx = (int)Math.Floor(mousePos.X / tileSize) - radiusX;
            int dy = (int)Math.Floor(mousePos.Y / tileSize) - radiusY;

            int voxelX = centerX + dx;
            int voxelY = centerY + dy;

            Vector2 screenPos = renderer.ScreenPositionFor(
		voxelX, voxelY,
		centerX, centerY,
		screenWidth, screenHeight,
		scale
            );

            action.OnTileHovered(voxelX, voxelY, screenPos);

            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                action.OnTileClicked(voxelX, voxelY, screenPos);
            }
        }
    }

    public class TileInfoOverlay : ITileClickAction
    {
	private readonly GameRenderer _renderer;
	private readonly Ref<IMap> _refMap;

	public TileInfoOverlay(GameRenderer renderer, Ref<IMap> refMap)
	{
	    _renderer = renderer;
	    _refMap = refMap;
	}

	public void OnTileHovered(int voxelX, int voxelY, Vector2 screenPos)
	{
	    float tileSize = 16 * GlobalSettings.Scale;

	    Color selectionColor = new Color(0, 255, 0, 128);
	    Raylib.DrawRectangle((int)screenPos.X, (int)screenPos.Y, (int)tileSize, (int)tileSize, selectionColor);
	}

	public void OnTileClicked(int voxelX, int voxelY, Vector2 screenPos)
	{
	    var voxel = _refMap.Value.Get(voxelX, voxelY, 0);
	    if (voxel != null)
	    {
		Log.Write($"({voxelX}, {voxelY}, 0) '{voxel.Type}' Voxel");
	    }
	    else
	    {
		Log.Write($"Empty space at ({voxelX}, {voxelY}, 0)");
	    }
	}
    }

    public class CopyDragAction : ITileClickAction
    {
	private readonly Ref<IMap> _refMap;
	private string? _copiedType = null;
	private bool _active = false;

	public CopyDragAction(Ref<IMap> refMap)
	{
	    _refMap = refMap;
	}

	public void OnTileClicked(int voxelX, int voxelY, Vector2 screenPos)
	{
	    var voxel = _refMap.Value.Get(voxelX, voxelY, 0);
	    if (voxel != null)
	    {
		_copiedType = voxel.Type;
		_active = true;
		Log.Write($"Started CopyDrag with type: {_copiedType}");
	    }
	    else
	    {
		_active = false;
		_copiedType = null;
		Log.Write($"CopyDrag cancelled — clicked empty voxel");
	    }
	}

	public void OnTileHovered(int voxelX, int voxelY, Vector2 screenPos)
	{
	    if (_active && _copiedType != null && Raylib.IsMouseButtonDown(MouseButton.Left))
	    {
		_refMap.Value.Set(voxelX, voxelY, 0, new Voxel(_copiedType));
	    }
	    else if (!Raylib.IsMouseButtonDown(MouseButton.Left))
	    {
		_active = false; // Reset when button released
	    }
	}
    }
}
