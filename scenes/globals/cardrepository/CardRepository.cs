using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace Kana
{
	public partial class CardData
	{
		[JsonPropertyName("NameEn")]
		public string NameEn { get; set; }
		[JsonPropertyName("NameJp")]
		public string NameJp { get; set; }
		[JsonPropertyName("Rarity")]
		public int Rarity { get; set; }
		[JsonPropertyName("ArtworkPath")]
		public string ArtworkPath { get; set; }
		[JsonPropertyName("SoundPath")]
		public string SoundPath { get; set; }
		[JsonPropertyName("AttackStat")]
		public int AttackStat { get; set; }
		[JsonPropertyName("DefenseStat")]
		public int DefenseStat { get; set; }
		[JsonPropertyName("HealthStat")]
		public int HealthStat { get; set; }
	}

	public partial class CardRepository : Node
	{
		Random random;

		public List<CardData> CardList;
		public List<CardData> KanaList;
		public List<CardData> SummonList;

		public override void _Ready()
		{
			random = new();

			string jsonString = FileAccess.Open("res://assets/data/cards-test.json", FileAccess.ModeFlags.Read).GetAsText();
			CardList = JsonSerializer.Deserialize<List<CardData>>(jsonString);

			CardList = CardList.OrderBy(card => card.NameEn).OrderBy(card => card.NameJp.Length).ToList();
			KanaList = CardList.Where(card => card.NameJp.Length == 1).ToList();
			SummonList = CardList.Where(card => card.NameJp.Length >= 2).ToList();
		}

		public CardData GetRandomKanaCardData()
		{
			int randomIndex = random.Next(0, KanaList.Count);
			return KanaList[randomIndex];
		}

		public CardData GetRandomSummonCardData()
		{
			int randomIndex = random.Next(0, SummonList.Count);
			return SummonList[randomIndex];
		}

		public CardData FindCardByNameEn(string nameEn)
		{
			return CardList.Find(cardData => cardData.NameEn == nameEn);
		}

		public CardData FindCardByNameJp(string nameJp)
		{
			return CardList.Find(cardData => cardData.NameJp == nameJp);
		}
	}
}
