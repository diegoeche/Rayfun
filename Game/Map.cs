using System.Collections.Generic;
using Raylib_cs;
using System.Numerics;

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

    public static class MapRenderer
    {
        public static void Render(IMap map, int centerX, int centerY, int centerZ, int radius)
        {
            int tileSize = 16;

            for (int dy = -radius; dy <= radius; dy++)
            {
                for (int dx = -radius; dx <= radius; dx++)
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

                    Raylib.DrawRectangle((dx + radius) * tileSize, (dy + radius) * tileSize, tileSize, tileSize, color);
                }
            }
        }

        public static void Render3D(IMap map)
        {
            foreach (var (pos, voxel) in map.Enumerate())
            {
                Color color;
                if (voxel.Type == "grass")
                    color = Color.Green;
                else if (voxel.Type == "dirt")
                    color = new Color(139, 69, 19, 255);
                else if (voxel.Type == "water")
                    color = Color.Blue;
                else
                    color = Color.Gray;

                var cubePos = new Vector3(pos.x, pos.z, pos.y); // flip y/z for top-down
                Raylib.DrawCube(cubePos, 1f, 1f, 1f, color);
                Raylib.DrawCubeWires(cubePos, 1f, 1f, 1f, Color.DarkGray);
            }
        }
    }
}
