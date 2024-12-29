using Godot;

namespace Kana
{
	public partial class SoundController : Node
	{
		private AudioStreamPlayer musicPlayer;
		private AudioStreamPlayer uiSoundEffectsPlayer;
		private AudioStreamPlayer soundEffectsPlayer;

		private AudioStreamPlayer[] soundEffectPlayers;
		private int nextSoundEffectPlayerToUse = 0;

		private ConfigFile config = new();

        private float mainVolume = 0.5f;
		private float musicVolume = 1;
		private float uiSoundEffectsVolume = 1;
		private float soundEffectsVolume = 1;

		public float MainVolume
		{
			get { return mainVolume; }
			set
			{
				mainVolume = value;
				musicPlayer.VolumeDb = Mathf.LinearToDb(musicVolume * value);
				uiSoundEffectsPlayer.VolumeDb = Mathf.LinearToDb(uiSoundEffectsVolume * value);
				soundEffectsPlayer.VolumeDb = Mathf.LinearToDb(soundEffectsVolume * value);
				foreach (var soundEffectPlayer in soundEffectPlayers)
				{
					soundEffectPlayer.VolumeDb = Mathf.LinearToDb(soundEffectsVolume * value);
				}
				config.SetValue("Audio", "MainVolume", value);
				config.Save("user://settings.cfg");
			}
		}
		public float MusicVolume
		{
			get { return musicVolume; }
			set
			{
				musicVolume = value;
				musicPlayer.VolumeDb = Mathf.LinearToDb(value * mainVolume);
				config.SetValue("Audio", "MusicVolume", value);
				config.Save("user://settings.cfg");
			}
		}
		public float UiSoundEffectsVolume
		{
			get { return uiSoundEffectsVolume; }
			set
			{
				uiSoundEffectsVolume = value;
				uiSoundEffectsPlayer.VolumeDb = Mathf.LinearToDb(value * mainVolume);
				config.SetValue("Audio", "UiSoundEffectsVolume", value);
				config.Save("user://settings.cfg");
			}
		}
		public float SoundEffectsVolume
		{
			get { return soundEffectsVolume; }
			set
			{
				soundEffectsVolume = value;
				soundEffectsPlayer.VolumeDb = Mathf.LinearToDb(value * mainVolume);
				foreach (var soundEffectPlayer in soundEffectPlayers)
				{
					soundEffectPlayer.VolumeDb = Mathf.LinearToDb(value * mainVolume);
				}
				config.SetValue("Audio", "SoundEffectsVolume", value);
				config.Save("user://settings.cfg");
			}
		}

		private string currentlyPlayingMusicPath;

		public string CurrentlyPlayingMusicPath
		{
			get { return currentlyPlayingMusicPath; }
			set { currentlyPlayingMusicPath = value; }
		}

		private bool isMusicPlaying = false;
		public bool IsMusicPlaying { get { return isMusicPlaying; } set { isMusicPlaying = value; } }

		public override void _Ready()
		{
			musicPlayer = GetNode<AudioStreamPlayer>("MusicPlayer");
			uiSoundEffectsPlayer = GetNode<AudioStreamPlayer>("UISoundEffectsPlayer");
			soundEffectsPlayer = GetNode<AudioStreamPlayer>("SoundEffectsPlayer");
			soundEffectPlayers = new AudioStreamPlayer[10];
			for (int i = 0; i < 10; i++)
			{
				var currentPlayer = soundEffectsPlayer.Duplicate() as AudioStreamPlayer;
				soundEffectPlayers[i] = currentPlayer;
				soundEffectsPlayer.GetParent().AddChild(currentPlayer);
				currentPlayer.Finished += () =>
				{
					SoundEffectFinished(currentPlayer);
				};
			}

			Error err = config.Load("user://settings.cfg");

			// If the file didn't load, ignore it.
			if (err == Error.Ok)
			{
				MusicVolume = (float)config.GetValue("Audio", "MusicVolume", 1);
				UiSoundEffectsVolume = (float)config.GetValue("Audio", "UiSoundEffectsVolume", 1);
				SoundEffectsVolume = (float)config.GetValue("Audio", "SoundEffectsVolume", 1);
				MainVolume = (float)config.GetValue("Audio", "MainVolume", 0.5);
			}
		}

		public void PlayMusic(string musicPath)
		{
			musicPlayer.Stream = GD.Load<AudioStream>(musicPath);
			musicPlayer.Play();
			isMusicPlaying = true;
			currentlyPlayingMusicPath = musicPlayer.Stream.ResourcePath;
		}

		public void StopMusic()
		{
			musicPlayer.Stop();
			isMusicPlaying = false;
		}

		public void PlayUiSoundEffect(string soundEffectPath)
		{
			uiSoundEffectsPlayer.Stream = GD.Load<AudioStream>(soundEffectPath);
			uiSoundEffectsPlayer.Play();
		}

		public void PlaySoundEffect(string soundEffectPath, float soundEffectVolumeMultiplicator = 1)
		{
			var currentSoundEffectPlayer = GetNextSoundEffectPlayer();
			if (soundEffectVolumeMultiplicator != 1)
			{
				currentSoundEffectPlayer.VolumeDb = Mathf.LinearToDb(
                    soundEffectsVolume * soundEffectVolumeMultiplicator * mainVolume
                );
			}
			currentSoundEffectPlayer.Stream = GD.Load<AudioStream>(soundEffectPath);
			currentSoundEffectPlayer.Play();
		}

		private AudioStreamPlayer GetNextSoundEffectPlayer()
		{
			var currentSoundEffectPlayer = soundEffectPlayers[nextSoundEffectPlayerToUse];
			if (nextSoundEffectPlayerToUse + 1 > 9) nextSoundEffectPlayerToUse = 0;
			else nextSoundEffectPlayerToUse++;
			return currentSoundEffectPlayer;
		}

		public void SoundEffectFinished(AudioStreamPlayer soundEffectPlayer)
		{
			// reset volume back to old value because it might have been affected by a multiplier for certain sounds
			soundEffectPlayer.VolumeDb = Mathf.LinearToDb(soundEffectsVolume * mainVolume);
		}

		public void OnMusicPlayerFinished()
		{
			musicPlayer.Play();
		}

		public void PlayButtonSoundEffect()
		{
			PlayUiSoundEffect("res://assets/sounds/ui/UIButtonClick.ogg");
		}
	}
}