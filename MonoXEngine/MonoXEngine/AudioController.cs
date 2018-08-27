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
                instance.Dispose();
            });

            return instance;
        }

        public SoundEffectInstance CurrentMusic;
        private SoundEffectInstance NextMusic;
        public string CurrentMusicFile;
        public float MusicFading = 0;
        public float FadeInStep = 0;
        public SoundEffectInstance PlayMusic(string filename, float fadeIn = 0.01f)
        {
            if(CurrentMusicFile != filename)
            {
                if (CurrentMusic != null && CurrentMusic.State == SoundState.Playing)
                {
                    MusicFading = -0.01f;
                }

                FadeInStep = fadeIn;

                NextMusic = Content.Load<SoundEffect>(Path + "Music/" + filename).CreateInstance();
            }
            
            return NextMusic;
        }

        public void Update()
        {
            if(CurrentMusic != null)
            {
                if (CurrentMusic.State == SoundState.Playing)
                {
                    if(MusicFading < 0)
                    {
                        float cv = CurrentMusic.Volume + MusicFading;
                        if (cv < 0) cv = 0;
                        CurrentMusic.Volume = cv;
                    }
                    else if (MusicFading > 0)
                    {
                        float cv = CurrentMusic.Volume + MusicFading;
                        if (cv > 1) cv = 1;
                        CurrentMusic.Volume = cv;
                    }

                    if (CurrentMusic.Volume == 0)
                    {
                        MusicFading = 0;
                        CurrentMusic.Stop();
                    }
                }
                else if (CurrentMusic.State == SoundState.Stopped)
                {
                    if(NextMusic != null)
                    {
                        CurrentMusic = NextMusic;
                        CurrentMusic.Volume = 0;
                        MusicFading = FadeInStep;
                        CurrentMusic.Play();
                    }
                }
            }
            else
            {
                if (NextMusic != null)
                {
                    CurrentMusic = NextMusic;
                    CurrentMusic.Volume = 0;
                    MusicFading = FadeInStep;
                    CurrentMusic.Play();
                }
            }
        }
    }
}
