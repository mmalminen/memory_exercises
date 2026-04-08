using Godot;

public partial class Main_menu : Hover_sprite_pairs
{
    // folder containing game scene files named like "memory_game.tscn"
    [Export] public string scenes_folder_path = "res://Scenes";

    public override void _UnhandledInput(InputEvent input_event)
    {
        // open hovered scene or handle menu action on left mouse button click
        if (input_event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
            Handle_selection();
    }

    void Handle_selection()
    {
        if (string.IsNullOrEmpty(hovered_pair_key))
            return;

        switch (hovered_pair_key.ToLower())
        {
            case "close_application":
                // close the application
                GetTree().Quit();
                break;

            default:
                // treat any other hovered key as a scene name and try to open it
                Open_selected_scene(hovered_pair_key);
                break;
        }
    }

    void Open_selected_scene(string scene_name)
    {
        // build scene path, for example "res://Scenes/memory_game.tscn"
        string scene_path = $"{scenes_folder_path.TrimEnd('/', '\\')}/{scene_name}.tscn";

        if (!ResourceLoader.Exists(scene_path))
        {
            GD.PushWarning($"Scene not found: {scene_path}");
            return;
        }

        GetTree().ChangeSceneToFile(scene_path);
    }
}