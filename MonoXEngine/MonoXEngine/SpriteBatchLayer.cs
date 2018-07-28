using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MonoXEngine
{
    public class SpriteBatchLayer
    {
        private SpriteBatch SpriteBatch;

        public SamplerState SamplerState;

        public Func<Matrix> MatrixUpdater;
        private Matrix TransformMatrix;

        public static SpriteBatchLayer Get(string Key)
        {
            return Global.SpriteBatchLayers[Key];
        }

        public SpriteBatchLayer(Dictionary<string, object> layerOptions)
        {
            this.SpriteBatch = new SpriteBatch(Global.GraphicsDevice);
            this.ApplyOptions(layerOptions);
        }

        public void ApplyOptions(Dictionary<string, object> layerOptions)
        {
            // Defaults
            this.TransformMatrix = Global.Camera.GetTransformation();
            this.SamplerState = SamplerState.PointClamp;

            foreach (KeyValuePair<string, object> option in layerOptions)
            {
                string key = option.Key.ToString().Trim().ToLower();
                string value = option.Value.ToString().Trim();

                if (key == "matrix")
                {
                    if (value.ToLower() == "camera")
                    {
                        this.MatrixUpdater = delegate()
                        {
                            return Global.Camera.GetTransformation();
                        };
                    }
                }
                else if (key == "samplerstate")
                {
                    var field = typeof(SamplerState).GetField(value, BindingFlags.Public | BindingFlags.Static);
                    var samplerstate = (SamplerState)field.GetValue(null);
                    this.SamplerState = samplerstate;
                }
            }
        }

        public void Draw(List<Entity> Entities)
        {
            if(this.MatrixUpdater != null)
                this.TransformMatrix = this.MatrixUpdater();

            this.SpriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.NonPremultiplied,
                this.SamplerState,
                null,
                null,
                null,
                this.TransformMatrix
            );
            
            foreach (Entity entity in Entities)
                entity.Draw(this.SpriteBatch);

            this.SpriteBatch.End();
        }
    }
}
