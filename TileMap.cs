using Godot;
using System;

public class TileMap : Godot.TileMap
{
	bool isBuilding = false;
	int selected = 0;
	
	Selector selector = new Selector();
	Vector2 curTilePos = new Vector2();
	Vector2 startTilePos = new Vector2();
	
	Box box = new Box();
	Godot.Collections.Array<Mover> movers = new Godot.Collections.Array<Mover>();
	
	Viewport view;
	
	AudioStreamPlayer2D sfxPlayer = new AudioStreamPlayer2D();
	
	public override void _Ready()
	{
		view = GetViewport();
		
		AddChild(selector);
		
		AddChild(box);
		box.Position = new Vector2(-1000,-1000);
		AddChild(sfxPlayer);
	}
	public override void _Process(float delta)
	{
		curTilePos = WorldToMap(view.GetMousePosition());
		
		if(isBuilding == false || selected == 1)
			selector.Position = MapToWorld(curTilePos);

		else
		{
			if(Math.Abs(curTilePos.x - startTilePos.x) > Math.Abs(curTilePos.y - startTilePos.y))
			{
				if(selector.Scale != new Vector2(curTilePos.x - startTilePos.x + Math.Sign(curTilePos.x - startTilePos.x + 0.1f),1))
					playSfx("res://Click Sound Effect.mp3",5,0.36f);
						
				selector.Scale = new Vector2(curTilePos.x - startTilePos.x + Math.Sign(curTilePos.x - startTilePos.x + 0.1f),1);
				selector.Position = MapToWorld(new Vector2(startTilePos.x-Math.Sign(selector.Scale.x-Math.Abs(selector.Scale.x)),startTilePos.y));
			}
			else
			{
				if(selector.Scale != new Vector2(1,curTilePos.y - startTilePos.y + Math.Sign(curTilePos.y - startTilePos.y + 0.1f)))
					playSfx("res://Click Sound Effect.mp3",5,0.36f);
				
				selector.Scale = new Vector2(1,curTilePos.y - startTilePos.y + Math.Sign(curTilePos.y - startTilePos.y + 0.1f));
				selector.Position = MapToWorld(new Vector2(startTilePos.x,startTilePos.y-Math.Sign(selector.Scale.y-Math.Abs(selector.Scale.y))));
			}
		}
	}
	public override void _Input(InputEvent inputEvent)
	{
		if (inputEvent.IsActionPressed("ui_click"))
		{
			playSfx("res://Mouse Click - Sound Effect (HD).mp3",0,0.23f);
			
			startTilePos=WorldToMap(view.GetMousePosition());
			if(GetCell((int)curTilePos.x,(int)curTilePos.y) == -1 || selected == 1)
			{
				selector.Modulate = new Color(0,1,0,0.5f);
				isBuilding = true;
			}
			else
			{
				selector.Modulate = new Color(1,1,0,0.5f);
			}
		}
		else if(inputEvent.IsActionReleased("ui_click"))
		{
			playSfx("res://Mouse Click - Sound Effect (HD).mp3",0,0.23f);
				
			if(selector.Modulate != new Color(1,1,0,0.5f) || selected == 1 || isBuilding == true)
			{
				if(selected == 0)
					spawnConveyor(0);
				else
					box.Position = MapToWorld(curTilePos);
				selector.Scale = new Vector2(1,1);
				selector.Modulate = new Color(1,1,1,0.25f);
				
				isBuilding = false;
			}
			else if(selector.Modulate == new Color(1,1,0,0.5f))
			{
				flipConveyor(startTilePos);
				selector.Scale = new Vector2(1,1);
				selector.Modulate = new Color(1,1,1,0.25f);
			}
		}
		else if (inputEvent.IsActionPressed("ui_rightClick"))
		{
			playSfx("res://Mouse Click - Sound Effect (HD).mp3",0,0.23f);
			
			startTilePos=WorldToMap(view.GetMousePosition());
			selector.Modulate = new Color(1,0,0,0.5f);
			isBuilding = true;
		}
		else if(inputEvent.IsActionReleased("ui_rightClick"))
		{
			playSfx("res://Mouse Click - Sound Effect (HD).mp3",0,0.23f);
				
			spawnConveyor(-1);
			selector.Scale = new Vector2(1,1);
			selector.Modulate = new Color(1,1,1,0.25f);
			
			isBuilding = false;
		}
		else if(inputEvent.IsActionReleased("ui_one"))
		{
			playSfx("res://Click Sound Effect.mp3",5,0.36f);
				
			selected=0;
			selector.Texture = (Texture)GD.Load("res://selector.png");
		}
		else if(inputEvent.IsActionReleased("ui_two"))
		{
			playSfx("res://Click Sound Effect.mp3",5,0.36f);
				
			selected=1;
			selector.Texture = (Texture)GD.Load("res://box.png");
		}
	}
	void spawnConveyor(int tile=0)
	{
		if(Math.Abs(curTilePos.x - startTilePos.x) > Math.Abs(curTilePos.y - startTilePos.y))
		{
			int i = (int)selector.Scale.x;
			while(i != 0)
			{
				if(tile!=-1)
					SetCell((int)startTilePos.x+i-Math.Sign(i),(int)startTilePos.y,tile-Math.Sign(Math.Sign(-i)-1));
				else
					SetCell((int)startTilePos.x+i-Math.Sign(i),(int)startTilePos.y,-1);
				UpdateBitmaskArea(new Vector2((int)startTilePos.x+i-Math.Sign(i),(int)startTilePos.y));
				addMovers(tile,i);
				
				i -= Math.Sign(i);
			}
		}
		else
		{
			int i = (int)selector.Scale.y;
			while(i != 0)
			{
				Mover mov = new Mover();
				
				if(tile!=-1)
					SetCell((int)startTilePos.x,(int)startTilePos.y+i-Math.Sign(i),tile-Math.Sign(Math.Sign(-i)-1)+2);
				else
					SetCell((int)startTilePos.x,(int)startTilePos.y+i-Math.Sign(i),-1);
				UpdateBitmaskArea(new Vector2((int)startTilePos.x,(int)startTilePos.y+i-Math.Sign(i)));
				addMovers(tile,i);
				
				i -= Math.Sign(i);
			}
		}
	}
	void flipConveyor(Vector2 pos)
	{
		if(GetCell((int)pos.x,(int)pos.y) != 3)
		{
			SetCell((int)pos.x,(int)pos.y,GetCell((int)pos.x,(int)pos.y)+1);
			for(int j=0;j<movers.Count;j++)
				if(WorldToMap(movers[j].Position) == pos)
					movers[j].dir = movers[j].directions[GetCell((int)pos.x,(int)pos.y)];
		}
		else
		{
			SetCell((int)pos.x,(int)pos.y,0);
			for(int j=0;j<movers.Count;j++)
				if(WorldToMap(movers[j].Position) == pos)
					movers[j].dir = movers[j].directions[0];
		}
	}
	void addMovers(int tile, int i)
	{
		Mover mov = new Mover();
		if(Math.Abs(curTilePos.x - startTilePos.x) > Math.Abs(curTilePos.y - startTilePos.y))
		{
			mov.Position = MapToWorld(new Vector2(startTilePos.x+i-Math.Sign(i),startTilePos.y));
			
			if(selector.Scale.x>0)
				mov.dir = mov.directions[1];
			else
				mov.dir = mov.directions[0];
				
			if(GetCell((int)startTilePos.x+i-Math.Sign(i),(int)startTilePos.y) != -1 || tile == -1)
			{
				for(int j=0;j<movers.Count;j++)
				{
					if(movers[j].Position == mov.Position)
					{
						movers[j].QueueFree();
						movers.Remove(movers[j]);
					}
				}
			}
			if(tile != -1)
			{
				AddChild(mov);
				movers.Add(mov);
			}
		}
		else
		{
			mov.Position = MapToWorld(new Vector2(startTilePos.x,startTilePos.y+i-Math.Sign(i)));
			
			if(selector.Scale.y>0)
				mov.dir = mov.directions[3];
			else
				mov.dir = mov.directions[2];
				
			if(GetCell((int)startTilePos.x,(int)startTilePos.y+i-Math.Sign(i)) != -1 || tile == -1)
			{
				for(int j=0;j<movers.Count;j++)
				{
					if(movers[j].Position == mov.Position)
					{
						movers[j].QueueFree();
						movers.Remove(movers[j]);
					}
				}
			}
			if(tile != -1)
			{
				AddChild(mov);
				movers.Add(mov);
			}
		}
	}
	void playSfx(string path, float volume, float offset)
	{
		sfxPlayer.Stream = (AudioStream)GD.Load(path);
		sfxPlayer.VolumeDb = volume;
		sfxPlayer.Play(offset);
	}
}

class Box : KinematicBody2D
{
	CollisionShape2D collider = new CollisionShape2D();
	Sprite sprite = new Sprite();
	
	public override void _Ready()
	{
		sprite.Texture = (Texture)GD.Load("res://box.png");
		sprite.Offset = sprite.Texture.GetSize() / 2;
		collider.Shape = new RectangleShape2D();
		collider.Scale = sprite.Texture.GetSize() / 40;
		collider.Position = sprite.Offset;
		
		AddChild(sprite);
		AddChild(collider);
	}
}
class Selector : Sprite
{
	public override void _Ready()
	{
		Modulate = new Color(1,1,1,0.25f);
		Texture = (Texture)GD.Load("res://selector.png");
		Offset = Texture.GetSize() / 2;
	}
}
class Mover : Area2D
{
	CollisionShape2D collider = new CollisionShape2D();
	public Vector2[] directions = new Vector2[]{Vector2.Left,Vector2.Right,Vector2.Up,Vector2.Down};
	public Vector2 dir = Vector2.Zero;
	
	public override void _Ready()
	{
		collider.Shape = new RectangleShape2D();
		collider.Scale = new Vector2(64,64) / 20;
		collider.Position = new Vector2(32,32);
		
		AddChild(collider);
	}
	public override void _PhysicsProcess(float delta)
	{
		if(GetOverlappingBodies().Count > 0)
		{
			foreach(KinematicBody2D i in GetOverlappingBodies())
			{
				i.MoveAndSlide(500 * dir.Normalized(),new Vector2(0,0),false,1);
				if(i.Position.y != Position.y && (dir ==Vector2.Right || dir == Vector2.Left))
					i.Position = new Vector2(i.Position.x,Position.y);
				else if(i.Position.x != Position.x && (dir == Vector2.Up || dir == Vector2.Down))
					i.Position = new Vector2(Position.x,i.Position.y);
			}
		}
	}
}
