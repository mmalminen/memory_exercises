using Godot;
using System.Collections.Generic;

public partial class Prepare_deck : Node2D
{
    // reference to the Image_splitter child node
    [Export] public NodePath image_splitter_path;
    // reference to the parent node containing all 24 card sprite nodes
    [Export] public NodePath cards_root_path;

    // all tiles split from the atlas
    List<AtlasTexture> tiles = new();

    // 24 card faces arranged as 12 pairs, ready to be assigned to card nodes
    // each unique image appears exactly twice
    List<AtlasTexture> card_deck = new();

    // random instance for shuffling and selection
    readonly RandomNumberGenerator rng = new();

    public override void _Ready()
    {
        // get the Image_splitter node and retrieve its tiles
        if (GetNodeOrNull(image_splitter_path) is Image_splitter splitter)
            tiles = splitter.Tiles;
        else
        {
            GD.PushWarning("Prepare_deck: Image_splitter node not found.");
            return;
        }

        // atlas must contain exactly 24 tiles
        if (tiles.Count != 24)
        {
            GD.PushWarning($"Prepare_deck: expected 24 tiles, got {tiles.Count}.");
            return;
        }

        Remove_backside_tiles();
        Prepare_card_deck();
        Assign_textures_to_cards();
    }

    // removes the last 2 tiles from the 'tiles' list
    // those tiles are reserved for card backsides used by Hover_sprite_pairs
    void Remove_backside_tiles()
    {
        // remove last 2 tiles in the atlas
        for (int i = 0; i < 2; i++)
        {
            int last = tiles.Count - 1;
            tiles.RemoveAt(last);
        }

        GD.Print($"Prepare_deck: removed 2 backside tiles, {tiles.Count} card images remaining.");
    }

    // picks 12 random unique tiles from the remaining 22 and adds each twice to card_deck
    void Prepare_card_deck()
    {
        // shuffle a copy of the available tiles to pick randomly
        List<AtlasTexture> shuffled = new(tiles);
        Shuffle(shuffled);

        // take the first 12 tiles as the selected card images
        List<AtlasTexture> selected = shuffled.GetRange(0, 12);

        // add each selected tile twice as separate instances to form 12 pairs
        // separate instances are required so each sprite gets its own texture object
        foreach (AtlasTexture tile in selected)
        {
            card_deck.Add(new AtlasTexture { Atlas = tile.Atlas, Region = tile.Region });
            card_deck.Add(new AtlasTexture { Atlas = tile.Atlas, Region = tile.Region });
        }

        // shuffle the deck so pairs are not grouped together
        Shuffle(card_deck);

        GD.Print($"Prepare_deck: card deck prepared with {card_deck.Count} cards ({card_deck.Count / 2} pairs).");
    }

    // assigns textures from card_deck to card sprite nodes
    // nodes are named by row and column, e.g. 11_card, 12_card ... 46_card
    void Assign_textures_to_cards()
    {
        Node cards_root = GetNodeOrNull(cards_root_path);

        if (cards_root == null)
        {
            GD.PushWarning("Prepare_deck: cards root node not found.");
            return;
        }

        int deck_index = 0;

        // iterate rows 1-4 and columns 1-6 to match node names
        for (int row = 1; row <= 4; row++)
        {
            for (int col = 1; col <= 6; col++)
            {
                string node_name = $"{row}{col}_card";

                if (cards_root.GetNodeOrNull(node_name) is Sprite2D card_sprite)
                {
                    // assign the next texture from the shuffled deck
                    card_sprite.Texture = card_deck[deck_index];
                    deck_index++;
                }
                else
                    GD.PushWarning($"Prepare_deck: card node '{node_name}' not found.");
            }
        }

        GD.Print($"Prepare_deck: assigned textures to {deck_index} card nodes.");
    }

    // shuffle for any list ('Fisher-Yates' algorithm)
    void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.RandiRange(0, i);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}