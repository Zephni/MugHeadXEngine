using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoXEngine.EntityComponents;
using System.Diagnostics;
using MonoXEngine.Structs;

namespace MonoXEngine
{
    public class TileMap
    {
        private Texture2D[,] tileset;
        private Point tileSize;
        private List<Tile> tiles;
        private Texture2D[,,] chunks;
        private Point totalMapSize;
        private Point3D totalChunks;
        private Point chunkSize;
        private List<int> chunkLayers;

        public List<Entity> TileGroupEntities;

        /// <summary>
        /// TileMap construct
        /// </summary>
        /// <param name="_tileSize"></param>
        /// <param name="tilesetName"></param>
        /// <param name="tiles"></param>
        public TileMap(Point _tileSize, string tilesetName = null, List<Tile> tiles = null)
        {
            this.tileSize = _tileSize;

            if(tilesetName != null)
                this.LoadTileset(tilesetName);

            if(tiles != null)
                this.SetTiles(tiles);

            TileGroupEntities = new List<Entity>();
        }

        /// <summary>
        /// Loads a tileset and splits it into individual tiles, stores in this.tileset
        /// </summary>
        /// <param name="fileName">File name of texture to load</param>
        protected void LoadTileset(string fileName)
        {
            Texture2D texture2D = Global.Content.Load<Texture2D>(fileName);
            this.tileset = this.Split(texture2D, this.tileSize.X, this.tileSize.Y);
        }

        /// <summary>
        /// Set list of tiles
        /// </summary>
        /// <param name="_tiles"></param>
        protected void SetTiles(List<Tile> _tiles)
        {
            this.tiles = _tiles;
        }

        protected Point TotalMapSize()
        {
            int minX = this.tiles.Min(tile => tile.Position.X);
            int minY = this.tiles.Min(tile => tile.Position.Y);
            int maxX = this.tiles.Max(tile => tile.Position.X);
            int maxY = this.tiles.Max(tile => tile.Position.Y);

            return new Point(
                (1+maxX - minX) * this.tileSize.X,
                (1+maxY - minY) * this.tileSize.Y
            );
        }

        /// <summary>
        /// Splits a texture into an array of smaller textures of the specified size.
        /// </summary>
        /// <param name="original">The texture to be split into smaller textures</param>
        /// <param name="partWidth">The width of each of the smaller textures that will be contained in the returned array.</param>
        /// <param name="partHeight">The height of each of the smaller textures that will be contained in the returned array.</param>
        protected Texture2D[,] Split(Texture2D original, int partWidth, int partHeight)
        {
            int yCount = original.Height / partHeight + (partHeight % original.Height == 0 ? 0 : 1);//The number of textures in each horizontal row
            int xCount = original.Width / partWidth + (partWidth % original.Width == 0 ? 0 : 1);//The number of textures in each vertical column
            Texture2D[,] r = new Texture2D[xCount, yCount];//Number of parts = (area of original) / (area of each part).
            int dataPerPart = partWidth * partHeight;//Number of pixels in each of the split parts

            //Get the pixel data from the original texture:
            Color[] originalData = new Color[original.Width * original.Height];
            original.GetData<Color>(originalData);

            for (int y = 0; y < yCount * partHeight; y += partHeight)
                for (int x = 0; x < xCount * partWidth; x += partWidth)
                {
                    //The texture at coordinate {x, y} from the top-left of the original texture
                    Texture2D part = new Texture2D(original.GraphicsDevice, partWidth, partHeight);
                    //The data for part
                    Color[] partData = new Color[dataPerPart];

                    //Fill the part data with colors from the original texture
                    for (int py = 0; py < partHeight; py++)
                        for (int px = 0; px < partWidth; px++)
                        {
                            int partIndex = px + py * partWidth;
                            //If a part goes outside of the source texture, then fill the overlapping part with Color.Transparent
                            if (y + py >= original.Height || x + px >= original.Width)
                                partData[partIndex] = Color.Transparent;
                            else
                                partData[partIndex] = originalData[(x + px) + (y + py) * original.Width];
                        }

                    //Fill the part with the extracted data
                    part.SetData<Color>(partData);
                    //Stick the part in the return array:                    
                    r[x / partWidth, y / partHeight] = part;
                }
            //Return the array of parts.
            return r;
        }

        /// <summary>
        /// Builds entities based on tilemap, and tiles provided and splits them up into the chunks passed
        /// </summary>
        /// <param name="perChunkTileAmount">Number of tiles per chunk</param>
        public void Build(Point perChunkTileAmount)
        {
            bool AddedTileForChunkFix = false;
            if(this.tiles.FindAll(e => e.Position3D.X == 0 && e.Position3D.Y == 0 && e.Position3D.Z == 0).Count == 0)
            {
                this.tiles.Add(new Tile(Point.Zero, new Point3D(0, 0, 0)));
                AddedTileForChunkFix = true;
            }

            chunkLayers = new List<int>();
            foreach (Tile tile in this.tiles)
                if (!chunkLayers.Contains(tile.Position3D.Z))
                    chunkLayers.Add(tile.Position3D.Z);
            chunkLayers.Sort((t1, t2) => { return t1 - t2; });

            totalMapSize = this.TotalMapSize();
            totalChunks = new Point3D(
                Math.Max((int)Math.Ceiling((double)totalMapSize.X / (perChunkTileAmount.X * this.tileSize.X)), 1),
                Math.Max((int)Math.Ceiling((double)totalMapSize.Y / (perChunkTileAmount.Y * this.tileSize.Y)), 1),
                chunkLayers.Count
            );
            chunkSize = perChunkTileAmount * this.tileSize;
            chunks = new Texture2D[totalChunks.X, totalChunks.Y, totalChunks.Z];

            foreach (Tile tile in this.tiles)
            {
                if (AddedTileForChunkFix && tile.Position3D.X == 0 && tile.Position3D.Y == 0 && tile.Position3D.Z == 0)
                {
                    AddedTileForChunkFix = false;
                    continue;
                }
                    
                Point3D chunkIndex = Point3D.FromPoint((tile.Position * tileSize) / chunkSize);
                chunkIndex.Z = chunkLayers.FindIndex(x => x == tile.Position3D.Z);
                Rectangle chunkRect = new Rectangle((chunkIndex * chunkSize).ToPoint(), chunkSize);
                Color[] tileColors = new Color[this.tileSize.X * this.tileSize.Y];
                this.tileset[tile.SourcePosition.X, tile.SourcePosition.Y].GetData<Color>(tileColors, 0, tileColors.Length);

                if (chunks[chunkIndex.X, chunkIndex.Y, chunkIndex.Z] == null)
                    chunks[chunkIndex.X, chunkIndex.Y, chunkIndex.Z] = new Texture2D(Global.GraphicsDevice, chunkSize.X, chunkSize.Y);

                Rectangle relativeTileRect = new Rectangle((tile.Position * this.tileSize) - chunkRect.Location, this.tileSize);
                chunks[chunkIndex.X, chunkIndex.Y, chunkIndex.Z].SetData<Color>(0, relativeTileRect, tileColors, 0, this.tileSize.X * this.tileSize.Y);
            }


            for (int x = 0; x < totalChunks.X; x++)
            {
                for (int y = 0; y < totalChunks.Y; y++)
                {
                    for (int z = 0; z < totalChunks.Z; z++)
                    {
                        if (chunks[x, y, z] == null)
                            continue;

                        TileGroupEntities.Add(new Entity(entity => {
                            entity.Origin = Vector2.Zero;
                            entity.Position = new Vector2(x * chunkSize.X, y * chunkSize.Y);
                            entity.SortingLayer = chunkLayers[z];
                            entity.AddComponent(new Drawable()).Run<Drawable>(component => {
                                component.Texture2D = chunks[x, y, z];
                            });
                        }));
                    }
                }
            }
        }
    }
}
