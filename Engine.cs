using Raylib_cs;
using rlImGui_cs;
using Game;
using Editor;

public class Engine
{
    private readonly UIManager _uiManager = new();
    private readonly FileExplorer _fileExplorer = new();
    private readonly StatsOverlay _statsOverlay = new();
    private readonly AtlasExplorer _atlasExplorer = new();
    private readonly MapExplorer _mapExplorer = new();
    private readonly GameAtlas _gameAtlas = new();
    private Ref<IMap> _mapRef;
    private readonly VoxelSpriteMapper _voxelMapper;
    private readonly GameRenderer _gameRenderer;

    private readonly TileInfoOverlay _tileInfoTool;
    private readonly CopyDragAction _copyTileAction;

    private readonly PixelSorter _pixelSorter = new();
    private bool _activateMagic = false;
    private RenderTexture2D? _capture; // optional: store last render
    private bool _magicActivated = false;

    public Engine()
    {
        _gameAtlas.Load("./assets/assets.atlas.json");
        _gameAtlas.Load("./assets/weapons.atlas.json");
        _gameAtlas.Load("./assets/bodies.atlas.json");

	_mapRef = new Ref<IMap>(MapSerializer.Load(_mapExplorer.LastMap));

        _voxelMapper = new VoxelSpriteMapper(_gameAtlas);
        _voxelMapper.LoadFromFile();

        _gameRenderer = new GameRenderer(_gameAtlas, _voxelMapper.GetMappings());

        _tileInfoTool = new TileInfoOverlay(_gameRenderer, _mapRef);
        _copyTileAction = new CopyDragAction(_mapRef);
    }

    private void activateMagic() {
	if (_magicActivated && !_activateMagic && Raylib.IsKeyPressed(KeyboardKey.P))
	{
	    int w = Raylib.GetScreenWidth();
	    int h = Raylib.GetScreenHeight();
	    var rt = Raylib.LoadRenderTexture(w, h);

	    Raylib.BeginTextureMode(rt);
	    Raylib.ClearBackground(Color.Beige);
	    RenderMap(); // Just once
	    Raylib.EndTextureMode();

	    _pixelSorter.Start(rt);
	    _activateMagic = true;
	}
    }

    public void Run()
    {
        while (!Raylib.WindowShouldClose())
        {
            _statsOverlay.RecordFrame(Raylib.GetFrameTime());
            _uiManager.HandleShortcuts();

            activateMagic();

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Beige);

	    if (_activateMagic)
	    {
		_pixelSorter.Update(50000);
		_pixelSorter.RenderFullScreen();

		if (_pixelSorter.IsDone)
		    _activateMagic = false;
	    }
	    else
	    {
		RenderMap();
	    }

            rlImGui.Begin();

            if (_uiManager.ShowEditor)
            {
                GlobalSettings.Render();
                _atlasExplorer.Render();

                if (GlobalSettings.ShowFileExplorer)
                    _fileExplorer.Render();

                if (GlobalSettings.ShowMapExplorer)
                    _mapExplorer.Render(_mapRef);

                if (GlobalSettings.ShowVoxelMapper)
                    _voxelMapper.Render();

                if (GlobalSettings.ShowLog)
                    Log.Render();

                if (_mapExplorer.RequestedLoad && _mapExplorer.SelectedMap != null)
                {
                    try
                    {
                        _mapRef.Value = MapSerializer.Load(_mapExplorer.SelectedMap);
                        Log.Write($"Loaded map: {_mapExplorer.SelectedMap}");
                        _mapExplorer.ClearRequests();
                    }
                    catch (Exception e)
                    {
                        Log.Write($"Failed to load map: {e.Message}");
                    }
                }
            }

            if (_uiManager.ShowStats)
            {
                _statsOverlay.Render();
            }

            rlImGui.End();
            Raylib.EndDrawing();
        }
    }

    private void RenderMap()
    {
        int centerX = (int)GlobalSettings.CameraPosition.X;
        int centerY = (int)GlobalSettings.CameraPosition.Y;
        int centerZ = 0;

        int screenWidth = Raylib.GetScreenWidth();
        int screenHeight = Raylib.GetScreenHeight();
        float scale = GlobalSettings.Scale;

        if (GlobalSettings.Use3D)
        {
            Raylib.BeginMode3D(GlobalSettings.Camera);
            MapRenderer.Render3D(_mapRef.Value, centerX - 10, centerY - 10, centerZ, 10, 1f);
            Raylib.EndMode3D();
        }
        else
        {
            _gameRenderer.Render(_mapRef.Value, centerX, centerY, centerZ, screenWidth, screenHeight, scale, 4);

            ITileClickAction? activeTool = GlobalSettings.ClickToolType switch
            {
                ClickToolType.TileInfo => _tileInfoTool,
                ClickToolType.CopyDrag => _copyTileAction,
                _ => null
            };

            TileSelector.HandleInteraction(_gameRenderer, _mapRef, activeTool);
        }
    }
}
