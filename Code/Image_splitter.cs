using Godot;
using System.Collections.Generic;

// utility node that splits an attached texture atlas into 125x125 AtlasTexture tiles.
// attach this node to any scene that needs to split a texture atlas.
public partial class Image_splitter : Node2D
{
    // path to the texture atlas to split
    [Export] public string atlas_path = "";
    // size of each tile in pixels, defaults to 125x125
    [Export] public int tile_width = 125;
    [Export] public int tile_height = 125;
    // how many tiles to ignore from the beginning of the split result
    [Export] public int ignore_first = 0;
    // how many tiles to ignore from the end of the split result
    [Export] public int ignore_last = 0;

    // stores all tiles split from the atlas, indexed left-to-right, top-to-bottom
    public List<AtlasTexture> Tiles { get; private set; } = new();

    public override void _Ready()
    {
        // automatically split the atlas when the node is ready if a path is set
        if (!string.IsNullOrEmpty(atlas_path))
            Tiles = Split(atlas_path);
    }

    // splits the atlas at the given path using the exported tile size.
    // returns the resulting tile list and also stores it in Tiles.
    public List<AtlasTexture> Split(string path)
    {
        List<AtlasTexture> all = Split(path, tile_width, tile_height);
        int end = all.Count - ignore_last;
        Tiles = (ignore_first < end) ? all.GetRange(ignore_first, end - ignore_first) : new();
        return Tiles;
    }

    // splits the atlas at the given path using a custom tile size.
    // returns the resulting tile list and also stores it in Tiles.
    public List<AtlasTexture> Split(string path, int width, int height)
    {
        List<AtlasTexture> result = new();

        // verify the atlas exists before attempting to load it
        if (!ResourceLoader.Exists(path))
        {
            GD.PushWarning($"Image_splitter: atlas not found at '{path}'");
            return result;
        }

        Texture2D source = ResourceLoader.Load<Texture2D>(path);

        // calculate how many complete tiles fit in each dimension
        int cols = source.GetWidth() / width;
        int rows = source.GetHeight() / height;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                // AtlasTexture references a region of the source texture without copying pixels
                AtlasTexture tile = new()
                {
                    Atlas = source,
                    // calculate the pixel region for this tile
                    Region = new Rect2(col * width, row * height, width, height)
                };
                result.Add(tile);
            }
        }

        GD.Print($"Image_splitter: split '{path}' into {result.Count} tiles ({cols} cols x {rows} rows).");
        Tiles = result;
        return result;
    }
}