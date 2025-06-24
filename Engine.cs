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
    private readonly MapExplorer _mapExplorer = new(true);
    private readonly GameAtlas _gameAtlas = new();
    private readonly EntityEditor _entityEditor = new();

    private Ref<IMap> _mapRef;
    private readonly VoxelSpriteMapper _voxelMapper;
    private readonly GameRenderer _gameRenderer;

    private readonly TileInfoOverlay _tileInfoTool;
    private readonly CopyDragAction _copyTileAction;
    private readonly PlaceAboveAction _placeAboveAction;

    private readonly PixelSorter _pixelSorter = new();
    private bool _activateMagic = false;
    private bool _magicActivated = true;
    private Simulation _simulation;
    private float simulationRate = 1.0f / 60.0f; // Define the simulation update rate (60 updates per second)
    private float accumulatedTime = 0f;

    static string DEFAULT_MAP = "./maps/default.json";

    public Engine()
    {
        _gameAtlas.Load("./assets/assets.atlas.json");
        _gameAtlas.Load("./assets/weapons.atlas.json");
        _gameAtlas.Load("./assets/bodies.atlas.json");

	string mapToLoad = _mapExplorer.LastMap ?? DEFAULT_MAP;
	_mapRef = new Ref<IMap>(MapSerializer.Load(mapToLoad));

        _voxelMapper = new VoxelSpriteMapper(_gameAtlas);
        _voxelMapper.LoadFromFile();
        _simulation = new Simulation(_mapRef);
        _gameRenderer = new GameRenderer(_gameAtlas, _voxelMapper.GetMappings());

        _tileInfoTool = new TileInfoOverlay(_gameRenderer, _mapRef);
        _copyTileAction = new CopyDragAction(_mapRef);
        _placeAboveAction = new PlaceAboveAction(_mapRef, GlobalSettings.SelectedVoxelName());
    }

    private void activateMagic() {
	if (_magicActivated && !_activateMagic && Raylib.IsKeyPressed(KeyboardKey.P))
	{
	    int w = Raylib.GetScreenWidth();
	    int h = Raylib.GetScreenHeight();
	    var rt = Raylib.LoadRenderTexture(w, h);

	    Raylib.BeginTextureMode(rt);
	    Raylib.ClearBackground(Color.Beige);

	    RenderMap();

	    Raylib.EndTextureMode();

	    _pixelSorter.Start(rt);
	    _activateMagic = true;
	}
    }

    private void Simulate(float deltaTime) {
	accumulatedTime += deltaTime;

	// Update simulation based on fixed time step
	while (accumulatedTime >= simulationRate)
	{
	    _simulation.Tick(); // Update simulation logic
	    accumulatedTime -= simulationRate; // Reduce accumulated time
	}
    }

    private void Render() {
	activateMagic();

	Raylib.BeginDrawing();
	Raylib.ClearBackground(Color.Beige);

	if (!_activateMagic)
	{
	    RenderMap();
	}
	else
	{
	    _pixelSorter.Update(50000);
	    _pixelSorter.RenderFullScreen();

	    if (_pixelSorter.IsDone)
		_activateMagic = false;
	}

	rlImGui.Begin();

	// Extracted Editor UI logic to a separate method
	RenderEditorUI();

	rlImGui.End();

	Raylib.EndDrawing();
    }

    public void Run()
    {
	while (!Raylib.WindowShouldClose())
	{
            float deltaTime = Raylib.GetFrameTime();

            Simulate(deltaTime);

            _statsOverlay.RecordFrame(deltaTime);
	    _uiManager.HandleShortcuts();

            Render();
        }
    }

    private void RenderEditorUI()
    {
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

	    if (GlobalSettings.ShowEntityEditor)
		_entityEditor.Render();

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
		    ClickToolType.AddVoxel => _placeAboveAction,
		    _ => null
		};

            _placeAboveAction.TileType = GlobalSettings.SelectedVoxelName();
            TileSelector.HandleInteraction(_gameRenderer, _mapRef, activeTool);
        }
    }
}
