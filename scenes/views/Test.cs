using Godot;
using Kana.states;

namespace Kana
{
	public partial class Test : Node3D
	{
		private CardRepository cardRepository;
		private PackedScene CardScene;
		private DrawPile DrawPile;
		private CardFan Hand;
		private CardFan EnemyHand;

		public async override void _Ready()
		{
			BattleState.battleStarted = true;

			CardScene = GD.Load<PackedScene>("res://scenes/components/card/Card.tscn");
			cardRepository = GetNode<CardRepository>("/root/CardRepository");
			DrawPile = GetNode<DrawPile>("DrawPile");
			Hand = GetNode<CardFan>("Hand");
			EnemyHand = GetNode<CardFan>("EnemyHand");

			for (int i = 0; i < 5; i++)
			{
				await DrawPile.DrawCard(Hand);
			}
		}

		private void OnButtonPressed()
		{
			DrawPile.DrawCard(Hand);
		}

		private void OnFusionButtonPressed()
		{
			string selectedCombo = "";
			foreach (Card c in BattleState.selectedCards)
			{
				selectedCombo += c.Data.NameJp;
				c.QueueFree();
			}
			BattleState.selectedCards.Clear();

			CardData summonData = cardRepository.FindCardByNameJp(selectedCombo);

			PackedScene CardScene = GD.Load<PackedScene>("res://scenes/components/card/Card.tscn");
			Card card = CardScene.Instantiate() as Card;
			card.Data = summonData;
			Hand.AddChild(card);
		}
	}
}
