using System.Collections.Generic;
using Raylib_cs;
using System.Numerics;
using Editor;

namespace Game
{
    public class Voxel
    {
        public string Type { get; set; }

        public Voxel(string type)
        {
            Type = type;
        }

        public Voxel()
        {
            Type = "grass";
        }
    }

    public interface IMap
    {
        Voxel? Get(int x, int y, int z);
        void Set(int x, int y, int z, Voxel voxel);
        IEnumerable<((int x, int y, int z) pos, Voxel voxel)> Enumerate();
    }

    public class SparseMap : IMap
    {
        private readonly Dictionary<(int x, int y, int z), Voxel> _voxels = new();

        public IEnumerable<((int x, int y, int z) pos, Voxel voxel)> Enumerate()
        {
            foreach (var kv in _voxels)
                yield return (kv.Key, kv.Value);
        }

        public Voxel? Get(int x, int y, int z)
        {
            return _voxels.TryGetValue((x, y, z), out var voxel) ? voxel : null;
        }

        public void Set(int x, int y, int z, Voxel voxel)
        {
            _voxels[(x, y, z)] = voxel;
        }
    }



    public class GameRenderer
    {
	private readonly GameAtlas _atlas;
	private readonly Dictionary<string, VoxelMapping> _voxelMappings;

	public GameRenderer(GameAtlas atlas, Dictionary<string, VoxelMapping> voxelMappings)
	{
	    _atlas = atlas;
	    _voxelMappings = voxelMappings;
	}

	public Vector2 ScreenPositionFor(int voxelX, int voxelY, int centerX, int centerY, int screenWidth, int screenHeight, float scale)
	{
	    int baseTileSize = 16;
	    int tileSize = (int)(baseTileSize * scale);

	    int tilesX = (int)Math.Ceiling(screenWidth / (float)tileSize);
	    int tilesY = (int)Math.Ceiling(screenHeight / (float)tileSize);
	    int radiusX = tilesX / 2;
	    int radiusY = tilesY / 2;

	    int dx = voxelX - centerX;
	    int dy = voxelY - centerY;

	    float screenX = (dx + radiusX) * tileSize;
	    float screenY = (dy + radiusY) * tileSize;

	    return new Vector2(screenX, screenY);
	}

	public void Render(IMap map, int centerX, int centerY, int centerZ, int width, int height, float scale, int depth = 4)
	{
	    int baseTileSize = 16;
	    int tileSize = (int)(baseTileSize * scale);
	    int tilesX = (int)Math.Ceiling(width / (float)tileSize);
	    int tilesY = (int)Math.Ceiling(height / (float)tileSize);
	    int radiusX = tilesX / 2;
	    int radiusY = tilesY / 2;

	    for (int dz = 0; dz < depth; dz++)
	    {
		int z = centerZ + dz;

		for (int dy = -radiusY; dy <= radiusY; dy++)
		{
		    for (int dx = -radiusX; dx <= radiusX; dx++)
		    {
			int x = centerX + dx;
			int y = centerY + dy;
			var voxel = map.Get(x, y, z);
                        if (voxel == null) continue;

			var screenX = (dx + radiusX) * tileSize;
			var screenY = (dy + radiusY) * tileSize;

			if (_voxelMappings.TryGetValue(voxel.Type, out var mapping) &&
			    !string.IsNullOrEmpty(mapping.SpriteKey) &&
			    _atlas.Get(mapping.SpriteKey) is { } sprite)
			{
			    var dest = new Rectangle(screenX, screenY, tileSize, tileSize);
			    Raylib.DrawTexturePro(sprite.Texture, sprite.SourceRect, dest, Vector2.Zero, 0f, Color.White);
                        }
			else
			{
			    Raylib.DrawRectangle(screenX, screenY, tileSize, tileSize, Color.Magenta);
			}
		    }
		}
	    }
	}
    }

    public static class MapRenderer
    {
	public static void Render(IMap map, int centerX, int centerY, int centerZ, int width, int height, float scale)
	{
	    int baseTileSize = 16;
	    int tileSize = (int)(baseTileSize * scale);

	    int tilesX = (int)Math.Ceiling(width / (float)tileSize);
	    int tilesY = (int)Math.Ceiling(height / (float)tileSize);

	    int radiusX = tilesX / 2;
	    int radiusY = tilesY / 2;

	    for (int dy = -radiusY; dy <= radiusY; dy++)
	    {
		for (int dx = -radiusX; dx <= radiusX; dx++)
		{
		    int x = centerX + dx;
		    int y = centerY + dy;
		    var voxel = map.Get(x, y, centerZ);
		    if (voxel == null) continue;

		    Color color;
		    if (voxel.Type == "grass")
			color = Color.Green;
		    else if (voxel.Type == "dirt")
			color = new Color(139, 69, 19, 255);
		    else if (voxel.Type == "water")
			color = Color.Blue;
		    else
			color = Color.Gray;

		    int screenX = (dx + radiusX) * tileSize;
		    int screenY = (dy + radiusY) * tileSize;

		    Raylib.DrawRectangle(screenX, screenY, tileSize, tileSize, color);
		}
	    }
	}

        public static void Render3D(IMap map, int centerX, int centerY, int centerZ, int radius, float cubeSize)
        {
            for (int dz = 0; dz <= centerZ + 5; dz++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    for (int dx = -radius; dx <= radius; dx++)
                    {
                        int x = centerX + dx;
                        int y = centerY + dy;
                        int z = dz;

                        var voxel = map.Get(x, y, z);
                        if (voxel == null) continue;

                        Color color;
                        if (voxel.Type == "grass")
                            color = Color.Green;
                        else if (voxel.Type == "dirt")
                            color = new Color(139, 69, 19, 255);
                        else if (voxel.Type == "water")
                            color = Color.Blue;
                        else
                            color = Color.Gray;

                        var cubePos = new Vector3(x * cubeSize, z * cubeSize, y * cubeSize);
                        Raylib.DrawCube(cubePos, cubeSize, cubeSize, cubeSize, color);
                        Raylib.DrawCubeWires(cubePos, cubeSize, cubeSize, cubeSize, Color.DarkGray);
                    }
                }
            }
        }
    }
}
