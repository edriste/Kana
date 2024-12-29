using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kana
{
	public partial class DrawPile : Node3D
	{
		private const float EmptyCardYOffset = 0.05f;
		private static readonly Vector3 EmptyCardRotation = new Vector3(Mathf.DegToRad(90), Mathf.DegToRad(180), 0);

		private Stack<CardData> stack;
		private Card topCard;
		private CardRepository cardRepository;
		private PackedScene cardScene;
		private PackedScene emptyCardScene;

		public override void _Ready()
		{
			emptyCardScene = GD.Load<PackedScene>("res://scenes/components/emptycard/EmptyCard.tscn");
			cardScene = GD.Load<PackedScene>("res://scenes/components/card/Card.tscn");
			cardRepository = GetNode<CardRepository>("/root/CardRepository");

			stack = new();

			// fill up draw pile with empty cards
			for (int i = 0; i < 100; i++)
			{
				EmptyCard emptyCard = emptyCardScene.Instantiate() as EmptyCard;
				emptyCard.Position = new Vector3(0, EmptyCardYOffset * i, 0);
				emptyCard.Rotation = EmptyCardRotation;
				stack.Push(cardRepository.GetRandomKanaCardData());
				AddChild(emptyCard);
			}

			// make the top card real for drawing
			SetupTopCard();
		}

		public async Task DrawCard(CardFan newParent)
		{
			if (stack.Count == 0)
			{
				return;
			}

			Vector3 nextRelativePosition = newParent.GetNextCardRelativePosition();
			Vector3 nextRelativeRotation = newParent.GetNextCardRelativeRotation();

			Transform3D newTransform = newParent.GlobalTransform;
			newTransform = newTransform.Translated(newTransform.Basis * nextRelativePosition);
			newTransform.Basis *= Basis.FromEuler(nextRelativeRotation);

			Tween drawTween = GetTree().CreateTween();
			drawTween.TweenProperty(topCard, "global_transform", newTransform, 0.25f);

			newParent.RearrangeCards(true);
			topCard.FaceUp = true;
			await Task.Delay(250);

			topCard.Reparent(newParent);
			stack.Pop();
			SetupTopCard();
			drawTween.Dispose();
		}

		public void SetupTopCard()
		{
			if (stack.Count == 0)
			{
				return;
			}

			EmptyCard cardToReplace = GetChildren().OfType<EmptyCard>().Last();
			if (cardToReplace == null)
			{
				return;
			}

			Card card = cardScene.Instantiate() as Card;
			card.Position = cardToReplace.Position;
			card.RotateX(Mathf.DegToRad(90));
			card.Data = stack.Peek();
			card.FaceUp = false;
			cardToReplace.ReplaceBy(card);
			topCard = card;

		}
	}
}
