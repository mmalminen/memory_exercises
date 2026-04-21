using Godot;
using System.Collections.Generic;

public partial class Number_sequences : Hover_sprite_pairs
{
    Image_splitter image_splitter;

    Sprite2D[] number_slots = new Sprite2D[9];
    Sprite2D[] input_field_slots = new Sprite2D[9];
    Sprite2D[] card_backsides = new Sprite2D[9];

    List<int> current_sequence = new();
    List<int> user_input = new();
    int current_length = 3;
    int attempt_count = 0;
    const int max_attempts = 3;
    bool input_enabled = false;

    public override void _Ready()
    {
        base._Ready();

        image_splitter = GetNode<Image_splitter>("sequence_table/image_splitter");

        for (int i = 0; i < 9; i++)
        {
            number_slots[i]      = GetNode<Sprite2D>($"sequence_table/numbers/number_slot_{i}");
            input_field_slots[i] = GetNode<Sprite2D>($"sequence_table/input_field/input_field_slot_{i}");
            card_backsides[i]    = GetNode<Sprite2D>($"sequence_table/card_backsides/card_{i}");
        }

        Start_new_round();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is not InputEventMouseButton mouse) return;
        if (mouse.ButtonIndex != MouseButton.Left || !mouse.Pressed) return;

        string key = hovered_pair_key;
        if (key == null) return;

        if (key == "confirm")  { On_confirm_pressed(); return; }
        if (key == "clear")    { On_clear_pressed();   return; }
        if (key == "erase")    { On_erase_pressed();   return; }

        // number buttons: zero, one, two ... nine
        string[] names = { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };
        for (int i = 0; i < names.Length; i++)
        {
            if (key == names[i])
            {
                On_number_pressed(i);
                return;
            }
        }
    }

    // -------------------------------------------------------------------------
    // Game flow
    // -------------------------------------------------------------------------

    void Start_new_round()
    {
        Generate_sequence();
        attempt_count = 0;
        Show_sequence();
    }

    void Generate_sequence()
    {
        current_sequence.Clear();
        var rng = new RandomNumberGenerator();
        rng.Randomize();
        for (int i = 0; i < current_length; i++)
            current_sequence.Add(rng.RandiRange(0, 9));
    }

    void Show_sequence()
    {
        user_input.Clear();
        Update_input_field();
        Set_input_enabled(false);

        // show sequence digits in number slots
        for (int i = 0; i < 9; i++)
        {
            bool in_range = i < current_length;
            number_slots[i].Visible = in_range;
            if (in_range)
                number_slots[i].Texture = image_splitter.Tiles[current_sequence[i]];
        }

        // hide cards so sequence is visible
        Set_cards_visible(false);

        // after current_length seconds, cover with cards
        var timer = GetTree().CreateTimer(current_length);
        timer.Timeout += On_timer_timeout;
    }

    void On_timer_timeout()
    {
        Set_cards_visible(true);
        Set_input_enabled(true);
    }

    // -------------------------------------------------------------------------
    // Input handlers
    // -------------------------------------------------------------------------

    void On_number_pressed(int number)
    {
        if (!input_enabled) return;
        if (user_input.Count >= current_length) return;

        user_input.Add(number);
        Update_input_field();
    }

    void On_clear_pressed()
    {
        if (!input_enabled) return;
        user_input.Clear();
        Update_input_field();
    }

    void On_erase_pressed()
    {
        if (!input_enabled) return;
        if (user_input.Count > 0)
        {
            user_input.RemoveAt(user_input.Count - 1);
            Update_input_field();
        }
    }

    void On_confirm_pressed()
    {
        if (!input_enabled) return;
        if (user_input.Count < current_length) return;

        Set_input_enabled(false);

        bool correct = true;
        for (int i = 0; i < current_length; i++)
        {
            if (user_input[i] != current_sequence[i])
            {
                correct = false;
                break;
            }
        }

        if (correct) On_correct_answer();
        else         On_wrong_answer();
    }

    // -------------------------------------------------------------------------
    // Outcome handlers
    // -------------------------------------------------------------------------

    void On_correct_answer()
    {
        Set_cards_visible(false);

        if (current_length >= 9)
        {
            // completed all sequences — restart from beginning after delay
            current_length = 3;
            var timer = GetTree().CreateTimer(2.5);
            timer.Timeout += Start_new_round;
        }
        else
        {
            current_length++;
            var timer = GetTree().CreateTimer(2.0);
            timer.Timeout += Start_new_round;
        }
    }

    void On_wrong_answer()
    {
        attempt_count++;

        if (attempt_count >= max_attempts)
        {
            // reveal correct sequence briefly before restarting
            Set_cards_visible(false);
            current_length = 3;
            var timer = GetTree().CreateTimer(2.5);
            timer.Timeout += Start_new_round;
        }
        else
        {
            // show sequence again for another attempt
            var timer = GetTree().CreateTimer(1.5);
            timer.Timeout += Show_sequence;
        }
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    void Update_input_field()
    {
        for (int i = 0; i < 9; i++)
        {
            bool in_range = i < current_length;
            input_field_slots[i].Visible = in_range && i < user_input.Count;
            if (in_range && i < user_input.Count)
                input_field_slots[i].Texture = image_splitter.Tiles[user_input[i]];
        }
    }

    void Set_cards_visible(bool visible)
    {
        for (int i = 0; i < current_length; i++)
            card_backsides[i].Visible = visible;

        // always hide cards beyond current sequence length
        for (int i = current_length; i < 9; i++)
            card_backsides[i].Visible = false;
    }

    void Set_input_enabled(bool enabled)
    {
        input_enabled = enabled;
    }
}