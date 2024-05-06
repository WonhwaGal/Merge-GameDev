using System;
using UnityEngine;
using GamePush;

namespace Code.Sounds
{
    public class SoundManager : IDisposable
    {
        private readonly SoundSO _soundSO;
        private AudioSource _musicSource;
        private AudioSource _soundSource;
        private bool _totalMusicOn;
        private bool _totalSoundOn;

        public SoundManager(SoundSO soundSO)
        {
            _soundSO = soundSO;
            GameEventSystem.Subscribe<SoundEvent>(PlaySound);
            SetUPSources();
        }

        private void SetUPSources()
        {
            var soundView = GameObject.Instantiate(_soundSO.SoundObject);
            _musicSource = soundView.MusicSource;
            _soundSource = soundView.SoundSource;
            _musicSource.clip = _soundSO.FindClip(SoundType.BackGroundMusic);
            _totalMusicOn = GP_Player.GetBool("music_on");
            _totalSoundOn = GP_Player.GetBool("sound_on");
            HandleMusic(_totalMusicOn);
        }

        private void PlaySound(SoundEvent @event)
        {
            if (@event.SoundType == SoundType.VolumeMusic || @event.SoundType == SoundType.VolumeSound)
                HandleVolume(@event.SoundType, @event.TurnOn);
            else if (@event.SoundType == SoundType.BackGroundMusic)
                HandleMusic(@event.TurnOn);
            else
                HandleSounds(@event);
        }

        private void HandleVolume(SoundType type, bool on)
        {
            if (type == SoundType.VolumeMusic)
            {
                _totalMusicOn = on;
                GP_Player.Set("music_on", on);
                HandleMusic(on);
            }
            else
            {
                _totalSoundOn = on;
                GP_Player.Set("sound_on", on);
            }
            GP_Player.Sync();
        }

        private void HandleMusic(bool turnOn)
        {
            if (turnOn && _totalMusicOn)
                _musicSource.Play();
            else
                _musicSource.Stop();
        }

        private void HandleSounds(SoundEvent @event)
        {
            if (@event.TurnOn && _totalSoundOn)
                _soundSource.PlayOneShot(_soundSO.FindClip(@event.SoundType));
        }

        public void Dispose() => GameEventSystem.UnSubscribe<SoundEvent>(PlaySound);
    }
}