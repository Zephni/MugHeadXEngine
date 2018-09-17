using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoXEngine
{
    public class AudioController
    {
        public bool MusicSetThisFrame = false;
        private string Path;
        private ContentManager Content;

        public float MasterVolume
        {
            get {return SoundEffect.MasterVolume; }
            set {
                SoundEffect.MasterVolume = (value < 0) ? 0 : (value > 1) ? 1 : value;
                MediaPlayer.Volume = SoundEffect.MasterVolume;
            }
        }

        public Dictionary<string, List<SoundEffectInstance>> SoundEffectInstances {
            get;
            private set;
        }

        public AudioController(string path, ContentManager content)
        {
            Path = path;
            Content = content;
            SoundEffectInstances = new Dictionary<string, List<SoundEffectInstance>>();
        }

        public SoundEffectInstance Play(string filename)
        {
            if (!SoundEffectInstances.ContainsKey(filename))
                SoundEffectInstances.Add(filename, new List<SoundEffectInstance>());

            SoundEffectInstance instance = Content.Load<SoundEffect>(Path + filename).CreateInstance();
            SoundEffectInstances[filename].Add(instance);
            instance.Play();

            StaticCoroutines.CoroutineHelper.RunWhen(() => instance.State == SoundState.Stopped, () => {
                SoundEffectInstances[filename].Remove(instance);
                if (SoundEffectInstances[filename].Count == 0)
                    SoundEffectInstances.Remove(filename);
                instance.Dispose();
            });

            return instance;
        }

        public void Stop(string Name)
        {
            for(int I = 0; I < SoundEffectInstances[Name].Count; I++)
                SoundEffectInstances[Name][I].Stop();

            SoundEffectInstances.Remove(Name);
        }

        public SoundEffectInstance CurrentMusic;
        private SoundEffectInstance NextMusic;
        public string CurrentMusicFile;
        public string NextMusicFile;
        public bool Loop;
        public float CurrentMusicVolume = 0;
        public float FadeTo;
        public float FadeTime;
        private float NextFadeTo;
        private float NextFadeTime;

        public SoundEffectInstance PlayMusic(string filename, float fadeTo = 1, float fadeTime = 0.5f, bool loop = true)
        {
            MusicSetThisFrame = true;
            Loop = loop;
            FadeTo = fadeTo;
            FadeTime = fadeTime;

            if (CurrentMusicFile != filename)
            {
                NextMusicFile = filename;

                if (CurrentMusic != null && CurrentMusic.State == SoundState.Playing)
                {
                    NextFadeTo = fadeTo;
                    NextFadeTime = fadeTime;
                    FadeTo = 0;
                    FadeTime = 0.5f;
                }

                NextMusic = Content.Load<SoundEffect>(Path + "Music/" + filename).CreateInstance();
            }
            
            return NextMusic;
        }

        public void MusicFadeOut(float fadeTime = 1f)
        {
            FadeTo = 0;
            FadeTime = fadeTime;
            NextMusicFile = null;
        }

        public void Update()
        {
            if(NextMusic != null)
            {
                if(CurrentMusic == null)
                {
                    CurrentMusicFile = NextMusicFile;
                    CurrentMusic = NextMusic;
                    CurrentMusic.Play();
                    FadeTo = 1;
                    FadeTime = 0;
                }
                else
                {
                    if(CurrentMusicVolume == 0 && CurrentMusicFile != NextMusicFile && NextMusicFile != null)
                    {
                        CurrentMusic.Stop();
                        CurrentMusicFile = NextMusicFile;
                        CurrentMusic = NextMusic;
                        CurrentMusic.Play();
                        FadeTo = NextFadeTo;
                        FadeTime = NextFadeTime;
                    }
                }
            }

            if(CurrentMusic != null && CurrentMusic.State == SoundState.Playing)
            {
                if (CurrentMusicVolume != FadeTo)
                {
                    CurrentMusicVolume += (FadeTo - CurrentMusicVolume) * Global.DeltaTime / FadeTime;

                    if (Math.Abs(CurrentMusicVolume - FadeTo) < 0.05f)
                        CurrentMusicVolume = FadeTo;
                }

                if (CurrentMusicVolume < 0) CurrentMusicVolume = 0;
                else if (CurrentMusicVolume > 1) CurrentMusicVolume = 1;
                CurrentMusic.Volume = CurrentMusicVolume;
            }
            else if (CurrentMusic != null && CurrentMusic.State == SoundState.Stopped)
            {
                CurrentMusic.Play();
            }
        }
    }
}
