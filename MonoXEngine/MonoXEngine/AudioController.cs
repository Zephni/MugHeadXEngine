using Microsoft.Xna.Framework.Audio;
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
            if(!SoundEffectInstances.ContainsKey(filename))
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
    }
}
