using System.Linq;
using Godot;
using Kana.states;
using Kana.utils;

namespace Kana
{
	public partial class Card : StaticBody3D
	{
		[Export] public bool FaceUp = true;
		[Export] public bool Aura = false;
		[Export] public bool CloseUp = false;
		[Export] public bool Trails = false;

		public CardData Data;

		private const int BaseFontSize = 800;

		private bool isDragging = false;
		private SoundController soundController;
		private AnimationPlayer animationPlayer;
		private Label3D titleLabel;

		public override void _Ready()
		{
			InitializeNodes();
			SetupCardAppearance();
			SetupCardStats();
		}

		private void InitializeNodes()
		{
			soundController = GetNode<SoundController>("/root/SoundController");
			animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
			titleLabel = GetNode<Label3D>("Mesh/Title");
		}

		private void SetupCardAppearance()
		{
			// rotate card according to FaceUp status
			if (!FaceUp)
			{
				Rotation = new Vector3(Rotation.X, Rotation.Y + Mathf.DegToRad(180), Rotation.Z);
			}

			AdjustFontSize();
			titleLabel.Text = Data.NameJp;

			if (!string.IsNullOrEmpty(Data.ArtworkPath))
			{
				GetNode<Sprite3D>("Mesh/Artwork").Texture = GD.Load<Texture2D>(Data.ArtworkPath);
			}
		}

		private void AdjustFontSize()
		{
			// calculate font size according to the length of the Japanese title; up to two letters can be shown at max size
			int excessLength = Data.NameJp.Length - 2;
			// Example with 4-Length Title: 800 - (120 + 120 / 2) * 2 + 2² * 5 = 800 - 180 * 2 + 4 * 5 = 800 - 360 + 20 = 460
			titleLabel.FontSize = excessLength > 0 ? BaseFontSize - (120 + 120 / excessLength) * excessLength + (int)Mathf.Pow(2, excessLength) * 5 : BaseFontSize;
		}

		private void SetupRarityAppearance()
		{
			var artworkHolo = GetNodeOrNull<Sprite3D>("Mesh/Artwork/Holo");
			var titleHolo = GetNodeOrNull<MeshInstance3D>("Mesh/Title/Holo");


			if (Data.Rarity < 2)
			{
				artworkHolo.QueueFree();
				titleHolo.QueueFree();
				return;
			}

			ConfigureTitleHolo(titleHolo);

			if (Data.Rarity == 3)
			{
				GetNode<Sprite3D>("Mesh/Frame").Texture = GD.Load<Texture2D>("res://assets/images/cards/layout/black_frame.png");
				titleLabel.Modulate = KanaColors.MetallicGold;
			}
		}

		private void ConfigureTitleHolo(MeshInstance3D titleHolo)
		{
			TextMesh textMesh = titleHolo.Mesh as TextMesh;
			textMesh.Text = Data.NameJp;
			// TextMesh is limiting the font size to 127, so scale needs to be calculated
			// Usually, a font with a font size of 1/10th the original one would be the same size at 5x scale
			float scaleFactor = 5 / (127f / (titleLabel.FontSize / 10));
			titleHolo.Scale = new Vector3(scaleFactor, scaleFactor, 1);

			// Change color according to rarity
			(textMesh.Material as StandardMaterial3D).AlbedoColor = Data.Rarity == 2 ? KanaColors.Silver : KanaColors.DarkGold;
		}

		private void SetupCardStats()
		{
			Sprite3D artworkSprite = GetNode<Sprite3D>("Mesh/Artwork");
			if (Data.NameJp.Length <= 1)
			{
				artworkSprite.GetNode<Sprite3D>("AttackStatBox").QueueFree();
				artworkSprite.GetNode<Sprite3D>("DefenseStatBox").QueueFree();
				artworkSprite.GetNode<Label3D>("HealthLabel").QueueFree();
			}
			else
			{
				artworkSprite.GetNode<Label3D>("AttackStatBox/Label").Text = $"攻\n撃\n{Data.AttackStat}";
				artworkSprite.GetNode<Label3D>("DefenseStatBox/Label").Text = $"防\n御\n{Data.DefenseStat}";
				artworkSprite.GetNode<Label3D>("HealthLabel").Text = $"体\n力\n{Data.HealthStat}";
			}
		}

		public async void Flip()
		{
			soundController.PlaySoundEffect("res://assets/sounds/effects/card_flip.wav");
			ToggleAura();

			if (FaceUp)
			{
				animationPlayer.Play("flip_y");
			}
			else
			{
				animationPlayer.PlayBackwards("flip_y");
			}

			FaceUp = !FaceUp;
		}

		public void SetAuraIntensity(float intensity)
		{
			ShaderMaterial shader = GetNode<MeshInstance3D>("Mesh").MaterialOverlay as ShaderMaterial;
			shader.SetShaderParameter("AuraIntensity", intensity);
		}

		private void ToggleTrails()
		{
			GpuParticles3D trailsNode = GetNode<GpuParticles3D>("Trails");
			Trails = !Trails;
			trailsNode.Visible = Trails;
		}

		private void ToggleAura()
		{
			Aura = !Aura;
			if (!Aura)
			{
				animationPlayer.Play("aura");
				Position = new Vector3(Position.X, Position.Y, Position.Z + 0.5f);
				BattleState.selectedCards.Add(this);
			}
			else
			{
				animationPlayer.PlayBackwards("aura");
				Position = new Vector3(Position.X, Position.Y, Position.Z - 0.5f);
				BattleState.selectedCards.Remove(this);
			}
		}

		private void OnInputEvent(Node camera, InputEvent @event, Vector3 eventPosition, Vector3 normal, int shapeIndex)
		{
			if (CloseUp && @event is InputEventScreenDrag)
			{
				isDragging = true;
				Rotation = new Vector3(Mathf.RadToDeg(eventPosition.Y / 1000), Mathf.RadToDeg(eventPosition.X / -1000) + Mathf.DegToRad(FaceUp ? 0 : 180), 0);
			}
			else if (@event is InputEventScreenTouch screenTouchEvent)
			{
				// screenTouchEvent sends two events, one on initial press and one on lift
				if (!screenTouchEvent.Pressed)
				{
					if (CloseUp && isDragging)
					{
						// move card back to original position once dragging has stopped
						ResetRotation();
						isDragging = false;
					}
					else
					{
						soundController.PlaySoundEffect(Data.SoundPath);
						ToggleAura();
					}
				}
			}
		}

		private void ResetRotation()
		{
			Tween tween = CreateTween();
			tween.TweenProperty(this, "rotation", new Vector3(0, FaceUp ? 0 : Mathf.DegToRad(180), 0), 0.5);
		}

		private void OnMouseExited()
		{
			if (CloseUp)
			{
				// move card back to original position if the finger or mouse is not on the card anymore
				ResetRotation();
				isDragging = false;
			}
		}
	}
}
