using Godot;
using System;
using System.Collections.Generic;

// base class: collects _0/_1 Sprite2D pairs and switches visibility on mouse hover.
// inherit this in any scene that needs hover-based sprite switching.
public partial class Hover_sprite_pairs : Node2D
{
	// holds one sprite pair: _0 visible by default, _1 visible on hover
	sealed class Sprite_pair
	{
		public Sprite2D normal;   // _0
		public Sprite2D selected; // _1
	}

	[Export] public NodePath sprites_root_path;
	[Export] public bool include_nested_children = true;

	// key is base name before suffix, for example "memory_game" from "memory_game_0"
	readonly Dictionary<string, Sprite_pair> sprite_pairs = new();
	// currently hovered pair base name, updated every frame
	protected string hovered_pair_key { get; private set; }

	public override void _Ready()
	{
		// use exported root if set, otherwise start from this node
		Node start_node = !sprites_root_path.IsEmpty && GetNodeOrNull(sprites_root_path) is Node found
			? found : this;

		Build_sprite_pairs(start_node);
		Refresh_hover();

		GD.Print($"Built {sprite_pairs.Count} hoverable sprite pairs.");
	}

	public override void _Process(double delta)
	{
		Refresh_hover();
	}

	void Build_sprite_pairs(Node root)
	{
		// depth-first: collect Sprite2D nodes and group into _0/_1 pairs
		foreach (Node child in root.GetChildren())
		{
			if (child is Sprite2D sprite)
			{
				string name = sprite.Name.ToString();

				// _0 is the default (not hovered) state
				if (name.EndsWith("_0", StringComparison.Ordinal))
					Get_or_create_pair(name[..^2]).normal = sprite;
				// _1 is the selected (hovered) state
				else if (name.EndsWith("_1", StringComparison.Ordinal))
					Get_or_create_pair(name[..^2]).selected = sprite;
			}

			if (include_nested_children && child.GetChildCount() > 0)
				Build_sprite_pairs(child);
		}
	}

	Sprite_pair Get_or_create_pair(string key)
	{
		if (!sprite_pairs.TryGetValue(key, out Sprite_pair pair))
			sprite_pairs[key] = pair = new Sprite_pair();
		return pair;
	}

	void Refresh_hover()
	{
		hovered_pair_key = Find_hovered_key(GetGlobalMousePosition());
		Apply_hover_state();
	}

	string Find_hovered_key(Vector2 mouse_pos)
	{
		// return key of first pair whose sprite rect contains the mouse position
		foreach ((string key, Sprite_pair pair) in sprite_pairs)
		{
			if (Is_mouse_over(pair.normal, mouse_pos) || Is_mouse_over(pair.selected, mouse_pos))
				return key;
		}
		return null;
	}

	static bool Is_mouse_over(Sprite2D sprite, Vector2 mouse_pos)
	{
		if (sprite == null || sprite.Texture == null)
			return false;
		// convert mouse from global space into sprite local space before rect check
		return sprite.GetRect().HasPoint(sprite.ToLocal(mouse_pos));
	}

	void Apply_hover_state()
	{
		foreach ((string key, Sprite_pair pair) in sprite_pairs)
		{
			// show _1 for hovered pair, _0 for all others
			bool is_hovered = key == hovered_pair_key;
			if (pair.normal != null) pair.normal.Visible = !is_hovered;
			if (pair.selected != null) pair.selected.Visible = is_hovered;
		}
	}
}
