using Raylib_cs;
using System;

public class PixelSorter
{
    private Color[]? _originalPixels;
    private Color[]? _sortedPixels;
    private int _pixelsSorted = 0;
    private int _totalPixels = 0;
    private Texture2D _texture;
    private bool _done = true;

    private static readonly Comparer<Color> _comparer = Comparer<Color>.Create(
        (a, b) => Brightness(a).CompareTo(Brightness(b))
    );

    public void Start(RenderTexture2D source)
    {
        unsafe
        {
            Image image = Raylib.LoadImageFromTexture(source.Texture);
            Color* pixelPtr = Raylib.LoadImageColors(image);
            int count = image.Width * image.Height;

            // Copy unmanaged pointer to managed array
            Color[] managedPixels = new Color[count];
            for (int i = 0; i < count; i++)
            {
                managedPixels[i] = pixelPtr[i];
            }

            Raylib.UnloadImageColors(pixelPtr);
            Raylib.UnloadImage(image);

            // Store sorted state
            _originalPixels = managedPixels;
            _sortedPixels = (Color[])managedPixels.Clone();
            _totalPixels = count;
            _pixelsSorted = 0;
            _done = false;

            fixed (Color* ptr = _sortedPixels)
            {
                Image sortedImage = new Image
                {
                    Data = ptr,
                    Width = source.Texture.Width,
                    Height = source.Texture.Height,
                    Format = PixelFormat.UncompressedR8G8B8A8,
                    Mipmaps = 1
                };

                _texture = Raylib.LoadTextureFromImage(sortedImage);
            }
        }
    }

    public void Update(int pixelsPerFrame = 2000)
    {
        if (_done || _sortedPixels == null || _originalPixels == null) return;

        int remaining = _totalPixels - _pixelsSorted;
        int chunkSize = Math.Min(pixelsPerFrame, remaining);

        Array.Sort(_sortedPixels, _pixelsSorted, chunkSize, _comparer);

        Raylib.UpdateTexture(_texture, _sortedPixels);

        _pixelsSorted += chunkSize;
        if (_pixelsSorted >= _totalPixels) _done = true;
    }

    public void RenderFullScreen()
    {
        if (_sortedPixels == null) return;
        Raylib.DrawTexture(_texture, 0, 0, Color.White);
    }

    public bool IsDone => _done;

    private static float Brightness(Color c) => (c.R + c.G + c.B) / 3f;
}
