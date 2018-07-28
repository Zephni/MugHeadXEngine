using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MonoXEngine
{
    public class ViewportTexture
    {
        public RenderTarget2D RenderTarget;
        public Point WindowSize;
        public Point Resolution;
        public Rectangle TextureRect;

        private SpriteBatch spriteBatch;
        private string viewportArea;

        public ViewportTexture(Point resolution, string viewportArea = "FIT_Y")
        {
            this.viewportArea = viewportArea;
            this.Resolution = resolution;

            this.WindowSizeUpdate();

            this.RenderTarget = new RenderTarget2D(
               Global.GraphicsDevice,
               this.Resolution.X,
               this.Resolution.Y,
               false,
               Global.GraphicsDevice.PresentationParameters.BackBufferFormat,
               DepthFormat.Depth24
            );

            this.spriteBatch = new SpriteBatch(Global.GraphicsDevice);
        }

        public void WindowSizeUpdate()
        {
            this.WindowSize = new Point(Global.GraphicsDevice.PresentationParameters.BackBufferWidth, Global.GraphicsDevice.PresentationParameters.BackBufferHeight);
            this.TextureRect = this.GetRect();
        }

        private Rectangle GetRect(string newViewportArea = null)
        {
            if (newViewportArea != null)
                this.viewportArea = newViewportArea;

            if (this.viewportArea == "FIT_Y")
            {
                float otherRatio = (this.WindowSize.Y - this.Resolution.Y) / (float)this.Resolution.Y;
                Point size = new Point(this.Resolution.X + (int)(this.Resolution.X * otherRatio), this.Resolution.Y + (int)(this.Resolution.Y * otherRatio));

                return new Rectangle(
                    this.WindowSize.X / 2 - size.X / 2,
                    this.WindowSize.Y / 2 - size.Y / 2,
                    size.X,
                    size.Y
                );
            }
            else if (this.viewportArea == "FIT_X")
            {
                float otherRatio = (this.WindowSize.X - this.Resolution.X) / (float)this.Resolution.X;
                Point size = new Point(this.Resolution.X + (int)(this.Resolution.X * otherRatio), this.Resolution.Y + (int)(this.Resolution.Y * otherRatio));

                return new Rectangle(
                    this.WindowSize.X / 2 - size.X / 2,
                    this.WindowSize.Y / 2 - size.Y / 2,
                    size.X,
                    size.Y
                );
            }
            else if (this.viewportArea == "STRETCH")
            {
                return new Rectangle(0, 0, this.WindowSize.X, this.WindowSize.Y);
            }
            else if (this.viewportArea == "FIT_BEST")
            {
                float scale = Math.Min((float)this.WindowSize.X / (float)this.Resolution.X, (float)this.WindowSize.Y / (float)this.Resolution.Y);

                Point size = new Point((int)((float)this.Resolution.X * scale), (int)((float)this.Resolution.Y * scale));

                return new Rectangle(
                    this.WindowSize.X / 2 - size.X / 2,
                    this.WindowSize.Y / 2 - size.Y / 2,
                    size.X,
                    size.Y
                );

            }
            else if (this.viewportArea.Substring(0, 7) == "CUSTOM ")
            {
                string[] parts = new string[4];
                parts = this.viewportArea.Substring(7).Split(' ');

                int[] rectParts = new int[4];

                for(int I = 0; I < parts.Length; I++)
                    rectParts[I] = Convert.ToInt16(parts[I]);

                return new Rectangle(rectParts[0], rectParts[1], rectParts[2], rectParts[3]);
            }

            return this.GetRect();
        }

        private void BeginCapture()
        {
            Global.GraphicsDevice.SetRenderTarget(this.RenderTarget);
            Global.GraphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };
        }

        private void EndCapture()
        {
            Global.GraphicsDevice.SetRenderTarget(null);
        }

        private void RenderToViewport()
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied,
               SamplerState.PointClamp, DepthStencilState.Default,
               RasterizerState.CullNone);

            spriteBatch.Draw(this.RenderTarget, this.TextureRect, Color.White);

            spriteBatch.End();
        }

        public void CaptureAndRender(MonoXEngineGame gameInstance, Action drawCalls)
        {
            this.BeginCapture();
            drawCalls();
            this.EndCapture();
            this.RenderToViewport();
        }
    }
}