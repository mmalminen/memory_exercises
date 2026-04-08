using Godot;

// in-game menu that handles restarting the current scene or returning to the main menu.
// inherits hover detection logic from Hover_sprite_pairs.
public partial class Game_menu : Hover_sprite_pairs
{
    // path to the main menu scene, can be overridden in the Godot editor
    [Export] public string main_menu_scene_path = "res://Scenes/main_menu.tscn";

    public override void _UnhandledInput(InputEvent input_event)
    {
        // listen for left mouse button clicks
        if (input_event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
            Handle_selection();
    }

    void Handle_selection()
    {
        // do nothing if no menu item is currently hovered
        if (string.IsNullOrEmpty(hovered_pair_key))
            return;

        // hovered_pair_key contains the prefix of the active sprite pair, e.g. "new_game" or "return_main_menu"
        switch (hovered_pair_key.ToLower())
        {
            case "new_game":
                // restart the current scene from the beginning
                GetTree().ReloadCurrentScene();
                break;

            case "return_main_menu":
                // verify the main menu scene exists before trying to load it
                if (!ResourceLoader.Exists(main_menu_scene_path))
                {
                    GD.PushWarning($"Main menu scene not found: {main_menu_scene_path}");
                    return;
                }
                GetTree().ChangeSceneToFile(main_menu_scene_path);
                break;

            default:
                // log a warning if an unrecognized sprite pair key is hovered
                GD.PushWarning($"Unknown menu option: {hovered_pair_key}");
                break;
        }
    }
}