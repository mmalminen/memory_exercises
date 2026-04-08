using Godot;
using System.Collections.Generic;

// handles memory game mechanics: card flipping, match checking and win condition.
public partial class Memory_game : Node2D
{
    // reference to the parent node containing all 24 card sprite nodes
    [Export] public NodePath cards_root_path;

    // z-index for flipped card face, must be above backside z-index 2
    const int CARD_FACE_FLIPPED_Z = 3;
    // z-index for unflipped card face, hidden behind backside
    const int CARD_FACE_DEFAULT_Z = 0;

    // currently flipped cards waiting for a match, max 2 at a time
    readonly List<Sprite2D> flipped_cards = new();
    // cards that have been matched and should stay face up
    readonly List<Sprite2D> matched_cards = new();

    // whether input is blocked while checking a mismatch
    bool input_blocked = false;

    // timer for hiding mismatched cards after a short delay
    Timer flip_back_timer;

    public override void _Ready()
    {
        // create a timer for flipping mismatched cards back over
        flip_back_timer = new Timer();
        flip_back_timer.OneShot = true;
        flip_back_timer.WaitTime = 2.0f;
        flip_back_timer.Timeout += On_flip_back_timeout;
        AddChild(flip_back_timer);
    }

    public override void _UnhandledInput(InputEvent input_event)
    {
        // listen for left mouse button clicks
        if (input_event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
            Handle_card_click();
    }

    // checks if mouse is over a card and flips it if valid
    void Handle_card_click()
    {
        // ignore clicks while waiting for mismatch timer
        if (input_blocked)
            return;

        Node cards_root = GetNodeOrNull(cards_root_path);
        if (cards_root == null)
            return;

        Vector2 mouse_pos = GetGlobalMousePosition();

        // check each card node for mouse overlap
        for (int row = 1; row <= 4; row++)
        {
            for (int col = 1; col <= 6; col++)
            {
                string node_name = $"{row}{col}_card";
                if (cards_root.GetNodeOrNull(node_name) is not Sprite2D card)
                    continue;

                // skip already matched or already flipped cards
                if (matched_cards.Contains(card) || flipped_cards.Contains(card))
                    continue;

                if (Is_mouse_over(card, mouse_pos))
                {
                    Flip_card(card);
                    return;
                }
            }
        }
    }

    // flips a card face up by raising its z-index above the backside
    void Flip_card(Sprite2D card)
    {
        card.ZIndex = CARD_FACE_FLIPPED_Z;
        flipped_cards.Add(card);

        // check for match once two cards are flipped
        if (flipped_cards.Count == 2)
            Check_match();
    }

    // checks if the two flipped cards share the same texture region
    void Check_match()
    {
        Sprite2D card_a = flipped_cards[0];
        Sprite2D card_b = flipped_cards[1];

        bool is_match = Is_same_texture(card_a, card_b);

        if (is_match)
        {
            // keep matched cards face up and clear the flipped list
            matched_cards.Add(card_a);
            matched_cards.Add(card_b);
            flipped_cards.Clear();

            GD.Print($"Memory_game: match found! {matched_cards.Count / 2} pairs matched.");

            // check if all pairs have been found
            if (matched_cards.Count == 24)
                On_game_won();
        }
        else
        {
            // block input and start timer to flip mismatched cards back
            input_blocked = true;
            flip_back_timer.Start();
        }
    }

    // compares two card sprites by their texture region to determine a match
    static bool Is_same_texture(Sprite2D card_a, Sprite2D card_b)
    {
        if (card_a.Texture is AtlasTexture atlas_a && card_b.Texture is AtlasTexture atlas_b)
            return atlas_a.Region == atlas_b.Region;

        return false;
    }

    // flips mismatched cards back face down after the timer expires
    void On_flip_back_timeout()
    {
        foreach (Sprite2D card in flipped_cards)
            card.ZIndex = CARD_FACE_DEFAULT_Z;

        flipped_cards.Clear();
        input_blocked = false;
    }

    // checks if the mouse position is within the card sprite bounds
    static bool Is_mouse_over(Sprite2D sprite, Vector2 mouse_pos)
    {
        if (sprite.Texture == null)
            return false;

        // get the card bounds in global space
        Rect2 rect = sprite.GetRect();
        Transform2D transform = sprite.GlobalTransform;
        Rect2 global_rect = transform * rect;

        return global_rect.HasPoint(mouse_pos);
    }

    // called when all 12 pairs have been matched
    void On_game_won()
    {
        GD.Print("Memory_game: all pairs matched, game won!");
    }
}