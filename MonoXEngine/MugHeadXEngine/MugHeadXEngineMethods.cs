﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoXEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MugHeadXEngine
{
    public static class Methods
    {
        public static Texture2D RoundedRect(Texture2D texture9Patch, Point size)
        {
            int border = texture9Patch.Bounds.Width / 3;
            Color[,] colors9Patch = texture9Patch.To2DArray();
            Color[,] colors2D = new Color[size.X, size.Y];

            // Top left
            for(int x = 0; x < border; x++)
                for (int y = 0; y < border; y++)
                    colors2D[x, y] = colors9Patch[x, y];

            // Top
            for(int x = border; x < size.X - border; x++)
                for (int y = 0; y < border; y++)
                    colors2D[x, y] = colors9Patch[x.Wrap(border, border * 2), y];

            // Top right
            for (int x = size.X - border; x < size.X; x++)
                for (int y = 0; y < border; y++)
                    colors2D[x, y] = colors9Patch[(x - (size.X - border)) + (border * 2), y];

            // Middle left
            for (int x = 0; x < border; x++)
                for (int y = border; y < size.Y - border; y++)
                    colors2D[x, y] = colors9Patch[x, y.Wrap(border, border * 2)];

            // Middle
            for (int x = border; x < size.X - border; x++)
                for (int y = border; y < size.Y - border; y++)
                    colors2D[x, y] = colors9Patch[x.Wrap(border, border * 2), y.Wrap(border, border * 2)];

            // Middle right
            for (int x = size.X - border; x < size.X; x++)
                for (int y = border; y < size.Y - border; y++)
                    colors2D[x, y] = colors9Patch[(x - (size.X - border)) + (border * 2), y.Wrap(border, border * 2)];

            // Bottom left
            for (int x = 0; x < border; x++)
                for (int y = size.Y - border; y < size.Y; y++)
                    colors2D[x, y] = colors9Patch[x, (y - (size.Y - border)) + (border * 2)];

            // Bottom
            for (int x = border; x < size.X - border; x++)
                for (int y = size.Y - border; y < size.Y; y++)
                    colors2D[x, y] = colors9Patch[x.Wrap(border, border * 2), (y - (size.Y - border)) + (border * 2)];

            // Bottom right
            for (int x = size.X - border; x < size.X; x++)
                for (int y = size.Y - border; y < size.Y; y++)
                    colors2D[x, y] = colors9Patch[(x - (size.X - border)) + (border * 2), (y - (size.Y - border)) + (border * 2)];

            Texture2D texture2D = new Texture2D(Global.GraphicsDevice, size.X, size.Y);
            texture2D.From2DArray(colors2D);

            return texture2D;
        }
    }
}
