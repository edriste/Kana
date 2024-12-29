using System.Linq;
using Godot;

namespace Kana
{
	public partial class CardFan : Node3D
	{
		[Export] public Curve curve;

		private const float xBounds = 15;
		private const float yMulti = 3;
		private const float zDivide = 20;
		private const float maxRotation = 20;
		private const float animDuration = 0.2f;

		private PackedScene CardScene;
		private SoundController soundController;
		private bool prearranged = false;

		public override void _Ready()
		{
			CardScene = GD.Load<PackedScene>("res://scenes/components/card/Card.tscn");
			soundController = GetNode<SoundController>("/root/SoundController");
		}

		public void RearrangeCards(bool prearrange = false)
		{
			prearranged = prearrange;
			int prearrangeBonus = prearrange ? 1 : 0;
			int childCount = GetChildCount();

			foreach (Card card in GetChildren().Cast<Card>())
			{
				// if prearrange is true, leave last slot empty for new card
				float cardIndex = childCount + prearrangeBonus == 1
	? 0.5f
	: (float)card.GetIndex() / (childCount - 1 + prearrangeBonus);

				float curvePosition = curve.Sample(cardIndex);
				Vector3 newPosition = new(Mathf.Lerp(-xBounds, xBounds, cardIndex), curvePosition * yMulti, card.GetIndex() / zDivide);
				Vector3 newRotation = new(0, 0, Mathf.DegToRad(Mathf.Lerp(maxRotation, -maxRotation, cardIndex)));

				CreateTween().TweenProperty(card, "position", newPosition, animDuration);
				CreateTween().TweenProperty(card, "rotation", newRotation, animDuration);
			}

			soundController.PlaySoundEffect("res://assets/sounds/effects/card_fan.wav");
		}

		public Vector3 GetNextCardRelativePosition()
		{
			return GetChildCount() == 0 ? Vector3.Zero : new Vector3(xBounds, curve.Sample(1) * yMulti, GetChildCount() / zDivide);
		}

		public Vector3 GetNextCardRelativeRotation()
		{
			return GetChildCount() == 0 ? Vector3.Zero : new Vector3(0, 0, Mathf.DegToRad(-maxRotation));
		}

		private void OnChildOrderChanged()
		{
			if (!prearranged)
			{
				RearrangeCards();
			}
			prearranged = false;
		}
	}
}
