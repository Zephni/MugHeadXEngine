using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using MonoXEngine;
using MonoXEngine.EntityComponents;
using MugHeadXEngine;
using MyGame.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using XEditor;
using StaticCoroutines;

namespace MyGame
{
    public class EntityInfoInterpreter
    {
        Level LevelScene;

        public EntityInfoInterpreter(List<EntityInfo> entities)
        {
            LevelScene = Global.SceneManager.CurrentScene as Level;

            foreach (var entity in entities)
                Interpret(entity);
        }

        public void Interpret(EntityInfo entityInfo)
        {
            if (entityInfo.Name == "StartPosition" && GameData.Get("Player/Position") == null)
            {
                GameGlobal.Player.Position = new Vector2(entityInfo.Position.X * 16, entityInfo.Position.Y * 16);
            }
            else if (entityInfo.Name == "MovableObject")
            {
                ZInterpreter data = new ZInterpreter(entityInfo.Data);
                new Entity(entity => {
                    entity.LayerName = "Main";
                    entity.SortingLayer = GameGlobal.PlayerGraphicEntity.SortingLayer -1;
                    entity.Position = (entityInfo.Position * 16) + (entityInfo.Size.ToVector2() / 2) * 16;
                    entity.AddComponent(new Sprite()).Run<Sprite>(sprite => {
                        sprite.LoadTexture("Graphics/WoodenBox");
                    });
                    entity.AddComponent(new MoveableObject());
                    if (data.GetString("id") != null)
                    {
                        entity.Data.Add("PO_ID", data.GetString("id"));
                        string poidposstr = GameData.Get("PO_ID:" + entity.Data["PO_ID"] + "/Position");
                        if (poidposstr != null)
                        {
                            string[] poidpos = poidposstr.Split(',');
                            entity.Position = new Vector2((float)Convert.ToDouble(poidpos[0]), (int)Convert.ToDouble(poidpos[1]));
                        }
                    }
                });
            }
            else if (entityInfo.Name == "MovablePlatform")
            {
                ZInterpreter data = new ZInterpreter(entityInfo.Data);
                new Entity(entity => {
                    entity.LayerName = "Main";
                    entity.SortingLayer = 10;
                    entity.Position = entityInfo.Position * 16;
                    entity.AddComponent(new Sprite()).Run<Sprite>(sprite => {
                        sprite.BuildRectangle(new Point(32, 16), Color.White);
                    });

                    entity.AddComponent(new MovablePlatform(entity.Position)).Run<MovablePlatform>(mp => {
                        mp.Speed = (data.HasKey("speed")) ? data.GetFloat("speed") : 1;
                        mp.XDistance = (data.HasKey("xdistance")) ? data.GetFloat("xdistance") : 0;
                        mp.YDistance = (data.HasKey("ydistance")) ? data.GetFloat("ydistance") : 0;
                    });
                });
            }
            else if (entityInfo.Name == "LightSource")
            {
                ZInterpreter data = new ZInterpreter(entityInfo.Data);

                // Pre set light sources
                if(data.HasKey("type"))
                {
                    if (data.GetString("type") == "OrangeTorch")
                    {
                        Level.RenderBlender.DrawableTextures.Add(new RenderBlender.DrawableTexture("alphamask2")
                        {
                            Position = (entityInfo.Position * 16),
                            Blend = RenderBlender.Subtract,
                            Update = item => { item.Scale = 1.6f; }
                        });
                        Level.RenderBlender.DrawableTextures.Add(new RenderBlender.DrawableTexture("alphamask2")
                        {
                            Position = (entityInfo.Position * 16),
                            Color = Color.Orange * 0.1f,
                            Update = item => { item.Scale = 0.6f + 0.04f * (float)Math.Sin(GameGlobal.TimeLoop * 50); }
                        });
                    }
                    else if (data.GetString("type") == "BlueMushroom")
                    {
                        Level.RenderBlender.DrawableTextures.Add(new RenderBlender.DrawableTexture("alphamask2") {
                            Position = (entityInfo.Position * 16),
                            Blend = RenderBlender.Subtract,
                            Color = Color.White * 0.8f,
                            Update = item => { item.Scale = 1.3f; }
                        });
                        Level.RenderBlender.DrawableTextures.Add(new RenderBlender.DrawableTexture("alphamask"){
                            Position = (entityInfo.Position * 16),
                            Color = Color.DeepSkyBlue * 0.2f,
                            Update = item => { item.Scale = 0.6f + 0.03f * (float)Math.Sin(GameGlobal.TimeLoop * 3); }
                        });
                    }
                }
                // Custom light sources
                else
                {
                    Level.RenderBlender.DrawableTextures.Add(new RenderBlender.DrawableTexture(data.GetString("texture") != "" ? data.GetString("texture"): "White"){
                        Position = (entityInfo.Position * 16) + data.GetPointArr("offset")[0].ToVector2(),
                        Blend = (data.GetString("blendstate") == "Subtract") ? RenderBlender.Subtract : RenderBlender.Lighting,
                        Color = GameMethods.GetProperty<Color>(data.GetString("color")),
                        Scale = data.GetFloat("scale"),
                        Layer = data.GetInt("layer")
                    });
                }
            }
            else if (entityInfo.Name == "Lighting")
            {
                ZInterpreter data = new ZInterpreter(entityInfo.Data);

                Level.RenderBlender.ClearColor = Color.Black * data.GetFloat("darkness");
            }
            else if (entityInfo.Name == "SavePoint")
            {
                new SavePoint(entityInfo);
            }
            else if (entityInfo.Name == "Enemy")
            {
                ZInterpreter data = new ZInterpreter(entityInfo.Data);

                Type type = Type.GetType("MyGame.Enemies." + data.GetString("type"));
                Activator.CreateInstance(type, new object[] { entityInfo });
            }
            else if (entityInfo.Name == "Audio2D")
            {
                ZInterpreter data = new ZInterpreter(entityInfo.Data);

                Vector2 position = (entityInfo.Position * 16) + (entityInfo.Size.ToVector2() / 2) * 16;
                float distance = ((data.HasKey("distance")) ? data.GetFloat("distance") : 5) * 16;
                float minvolume = ((data.HasKey("minvolume")) ? data.GetFloat("minvolume") : 0);
                float maxvolume = ((data.HasKey("maxvolume")) ? data.GetFloat("maxvolume") : 1);

                SoundEffectInstance sfi = Global.AudioController.Play("SFX/"+ data.GetString("file"));
                sfi.Volume = 0;
                CoroutineHelper.Always(() => {
                    if (Global.RunWhenEventLoops("ReplaySFX_"+data.GetString("file"), sfi == null || sfi.State != SoundState.Playing))
                        sfi = Global.AudioController.Play("SFX/" + data.GetString("file"));
                    
                    float difference = GameGlobal.Player.Position.GetDistance(position);
                    float percent = (((difference - distance) * 100) / ((maxvolume) - distance)).Between(minvolume, maxvolume);
                    sfi.Volume = (percent / 100);
                });

                Global.SceneManager.CurrentScene.OnExit += () => {
                    Global.AudioController.Stop("SFX/" + data.GetString("file"));
                };
            }
            else if (entityInfo.Name == "InteractScript")
            {
                ZInterpreter data = new ZInterpreter(entityInfo.Data);

                new Entity(entity => {
                    entity.Name = "InteractScript";
                    entity.LayerName = "Main";
                    entity.Collider = Entity.CollisionType.Pixel;
                    entity.Trigger = Entity.TriggerTypes.NonSolid;

                    entity.Data.Add("Script", data.GetString("script"));

                    entity.SortingLayer = GameGlobal.Player.SortingLayer;
                    entity.Position = (entityInfo.Position * 16) + (entityInfo.Size.ToVector2() / 2) * 16;
                    entity.AddComponent(new Drawable()).Run<Drawable>(d => {
                        d.BuildRectangle(new Point(entityInfo.Size.X * 16, entityInfo.Size.Y * 16), Color.Blue);
                        d.Visible = false;
                    });

                    if (data.HasKey("autoscript"))
                    {
                        CoroutineHelper.WaitRun(0.1f, () => {
                            string[] script = data.GetString("autoscript").Split('.');
                            Type type = Type.GetType("MyGame." + script[0]);
                            MethodInfo mi = type.GetMethod(script[1], BindingFlags.Static | BindingFlags.Public);
                            mi.Invoke(null, new object[] { entity });
                        });
                    }
                });
            }
            else if (entityInfo.Name == "TouchScript")
            {
                ZInterpreter data = new ZInterpreter(entityInfo.Data);

                new Entity(entity => {
                    entity.Name = "TouchScript";
                    entity.LayerName = "Main";
                    entity.Collider = Entity.CollisionType.Pixel;
                    entity.Trigger = Entity.TriggerTypes.NonSolid;

                    entity.Data.Add("Script", data.GetString("script"));

                    entity.SortingLayer = GameGlobal.Player.SortingLayer;
                    entity.Position = (entityInfo.Position * 16) + (entityInfo.Size.ToVector2() / 2) * 16;
                    entity.AddComponent(new Drawable()).Run<Drawable>(d => {
                        d.BuildRectangle(new Point(entityInfo.Size.X * 16, entityInfo.Size.Y * 16), Color.Blue);
                        d.Visible = false;
                    });
                });
            }
            else if (entityInfo.Name == "Ambience")
            {
                ZInterpreter data = new ZInterpreter(entityInfo.Data);
                
                SoundEffectInstance sfi = Global.AudioController.Play("SFX/"+data.GetString("file"));
                sfi.Volume = (data.HasKey("volume")) ? data.GetFloat("volume") : 1;
                CoroutineHelper.Always(() => {
                    if (sfi.State == SoundState.Stopped)
                    {
                        sfi = Global.AudioController.Play("SFX/"+ data.GetString("file"));
                        sfi.Volume = (data.HasKey("volume")) ? data.GetFloat("volume") : 1;
                    }
                });
                
                Global.SceneManager.CurrentScene.OnExit += () => {
                    Global.AudioController.Stop("SFX/" + data.GetString("file"));
                };
            }
            else if (entityInfo.Name == "Music")
            {
                ZInterpreter data = new ZInterpreter(entityInfo.Data);
                
                float fadeTo = (data.HasKey("fadeTo")) ? data.GetFloat("fadeTo") : 1;

                if(Global.AudioController.CurrentMusicFile == null || Global.AudioController.CurrentMusicFile == "")
                {
                    Global.AudioController.PlayMusic(data.GetString("file"));
                    Global.AudioController.CurrentMusicVolume = fadeTo;
                }
                else
                {
                    Global.AudioController.PlayMusic(data.GetString("file"), fadeTo);
                }
            }
            else if (entityInfo.Name == "NPCChest")
            {
                ZInterpreter data = new ZInterpreter(entityInfo.Data);

                NPCChest.Create(entityInfo, data);
            }
            else if (entityInfo.Name == "Chest")
            {
                ZInterpreter data = new ZInterpreter(entityInfo.Data);

                new Entity(entity => {
                    entity.Name = "Chest";
                    entity.LayerName = "Main";
                    entity.Trigger = Entity.TriggerTypes.NonSolid;
                    entity.Collider = Entity.CollisionType.Pixel;
                    entity.SortingLayer = GameGlobal.Player.SortingLayer - 1;
                    entity.Position = (entityInfo.Position * 16) + (entityInfo.Size.ToVector2() / 2) * 16;
                    entity.AddComponent(new Sprite()).Run<Sprite>(d => {
                        d.LoadTexture("Entities/Chest_" + data.GetString("type"));
                        d.AddAnimation(new Animation("Closed", 0, new Point(32, 32), new Point(0, 0)));
                        d.AddAnimation(new Animation("Opening", 0.1f, new Point(32, 32), new Point(1, 0), new Point(2, 0), new Point(3, 0), new Point(4, 0)));
                        d.RunAnimation("Closed");
                    });
                });
            }
            else if (entityInfo.Name == "NPC")
            {
                ZInterpreter data = new ZInterpreter(entityInfo.Data);

                new Entity(entity => {
                    entity.Name = "NPC";
                    entity.LayerName = "Main";
                    entity.Trigger = Entity.TriggerTypes.NonSolid;
                    entity.Collider = Entity.CollisionType.Pixel;
                    entity.SortingLayer = GameGlobal.Player.SortingLayer;
                    entity.Position = (entityInfo.Position * 16) + (entityInfo.Size.ToVector2() / 2) * 16;
                    entity.Origin = Vector2.Zero;
                    entity.AddComponent(new Drawable()).Run<Drawable>(d => {
                        d.BuildRectangle(new Point(entityInfo.Size.X * 16, entityInfo.Size.Y * 16), Color.CornflowerBlue);
                    });
                });
            }
            else if (entityInfo.Name == "Water")
            {
                ZInterpreter data = new ZInterpreter(entityInfo.Data);

                new Entity(entity => {
                    entity.Name = "Water";
                    entity.LayerName = "Main";
                    entity.Trigger = Entity.TriggerTypes.NonSolid;
                    entity.Collider = Entity.CollisionType.Pixel;
                    entity.SortingLayer = 4;
                    entity.Opacity = 1f;
                    entity.Position = (entityInfo.Position * 16) + (entityInfo.Size.ToVector2() / 2) * 16;
                    entity.AddComponent(new Drawable()).Run<Drawable>(d => {
                        Color waterColor = (data.HasKey("color")) ? GameMethods.GetProperty<Color>(data.GetString("color")) *0.2f : new Color(0, 0, 0, 0.2f);
                        d.BuildRectangle(new Point(entityInfo.Size.X * 16, entityInfo.Size.Y * 16), waterColor);

                        /* Animate water */
                        int offset = 0;
                        Color[] colors = new Color[6];
                        for (int i = 0; i < colors.Length; i++)
                            colors[i] = (i % 6 >= 3) ? Color.AliceBlue * 0.3f : Color.DarkSlateBlue * 0.1f;
                        colors[3] = Color.AliceBlue * 0.5f;

                        int offsetPrev = 0;
                        CoroutineHelper.Always(() => {
                            offset = 6 + (int)(Math.Sin(Global.GameTime.TotalGameTime.TotalMilliseconds / 500) * 3);

                            if(offset != offsetPrev)
                            {
                                offsetPrev = offset;
                                d.Texture2D.ManipulateColorsRect1D(new Rectangle(0, 0, d.Texture2D.Width, 2), colors1D => {
                                    int width = d.Texture2D.Width;
                                    for (int x = 0; x < width; x++)
                                    {
                                        colors1D[0 * width + x] = colors[(x + offset).Wrap(0, colors.Length - 1)];
                                        colors1D[1 * width + x] = colors[(2 - x + offset / 2).Wrap(0, colors.Length - 1)] * 0.5f;
                                    }

                                    return colors1D;
                                });
                            }
                        });
                        /* /Animate water */
                    });
                });
            }
            else if (entityInfo.Name == "Background")
            {
                ZInterpreter data = new ZInterpreter(entityInfo.Data);

                new Entity(entity => {
                    entity.LayerName = "Background";
                    entity.Position = new Vector2(0, 0);

                    if (data.HasKey("sortinglayer"))
                        entity.SortingLayer = data.GetInt("sortinglayer");

                    if (data.HasKey("opacity"))
                        entity.Opacity = data.GetFloat("opacity");

                    float[] coefficient = data.GetFloatArr("coefficient");
                    float[] offset = data.GetFloatArr("offset");

                    entity.AddComponent(new CameraOffsetTexture() { Texture2D = Global.Content.Load<Texture2D>("Backgrounds/" + data.GetString("image")), Coefficient = new Vector2(coefficient[0], coefficient[1]), Offset = new Vector2(offset[0], offset[1]) });

                    if (data.HasKey("animate"))
                    {
                        entity.GetComponent<CameraOffsetTexture>().Animate = true;
                        entity.GetComponent<CameraOffsetTexture>().AnimStepTime = data.GetFloat("animate");
                        entity.GetComponent<CameraOffsetTexture>().Size = data.GetPointArr("crop")[0];
                    }
                });
            }
            else if (entityInfo.Name == "Foreground")
            {
                ZInterpreter data = new ZInterpreter(entityInfo.Data);

                new Entity(entity => {
                    entity.LayerName = "Foreground";
                    entity.Position = Vector2.Zero;
                    entity.SortingLayer = 8;
                    float[] coefficient = data.GetFloatArr("coefficient");
                    float[] offset = data.GetFloatArr("offset");

                    entity.AddComponent(new CameraOffsetTexture() { Texture2D = Global.Content.Load<Texture2D>("Foregrounds/" + data.GetString("image")), Coefficient = new Vector2(coefficient[0], coefficient[1]), Offset = new Vector2(offset[0], offset[1]) });

                    if (data.HasKey("animate"))
                    {
                        entity.GetComponent<CameraOffsetTexture>().Animate = true;
                        entity.GetComponent<CameraOffsetTexture>().AnimStepTime = data.GetFloat("animate");
                        entity.GetComponent<CameraOffsetTexture>().Size = data.GetPointArr("crop")[0];
                    }
                });
            }
            else if (entityInfo.Name == "AutoDoor")
            {
                new Entity(entity => {
                    entity.Name = "AutoDoor";
                    entity.LayerName = "Main";
                    entity.Collider = Entity.CollisionType.Pixel;
                    entity.Trigger = Entity.TriggerTypes.NonSolid;

                    ZInterpreter data = new ZInterpreter(entityInfo.Data);
                    entity.Data.Add("Level", data.GetString("level"));
                    entity.Data.Add("Position", data.GetString("position"));

                    entity.SortingLayer = GameGlobal.Player.SortingLayer+5;
                    entity.Position = (entityInfo.Position * 16) + (entityInfo.Size.ToVector2() / 2) * 16;
                    entity.AddComponent(new Drawable()).Run<Drawable>(d => {
                        d.BuildRectangle(new Point(entityInfo.Size.X * 16, entityInfo.Size.Y * 16), Color.Blue);
                        d.Visible = false;
                    });
                });
            }
            else if (entityInfo.Name == "Door")
            {
                new Entity(entity => {
                    entity.Name = "Door";
                    entity.LayerName = "Main";
                    entity.Collider = Entity.CollisionType.Pixel;
                    entity.Trigger = Entity.TriggerTypes.NonSolid;

                    ZInterpreter data = new ZInterpreter(entityInfo.Data);
                    entity.Data.Add("Level", data.GetString("level"));
                    entity.Data.Add("Position", data.GetString("position"));

                    entity.SortingLayer = GameGlobal.Player.SortingLayer;
                    entity.Position = (entityInfo.Position * 16) + (entityInfo.Size.ToVector2() / 2) * 16;
                    entity.AddComponent(new Drawable()).Run<Drawable>(d => {
                        d.BuildRectangle(new Point(entityInfo.Size.X * 16, entityInfo.Size.Y * 16), Color.Blue);
                        d.Visible = false;
                    });
                });
            }
            else if (entityInfo.Name == "Graphic")
            {
                ZInterpreter data = new ZInterpreter(entityInfo.Data);

                new Entity(entity => {
                    entity.Name = "Graphic";
                    entity.LayerName = "Main";
                    entity.SortingLayer = (data.HasKey("sortinglayer")) ? data.GetInt("sortinglayer") : 2;
                    entity.Position = (entityInfo.Position * 16) + (entityInfo.Size.ToVector2() / 2) * 16;
                    
                    if(data.HasKey("id"))
                        entity.ID = data.GetString("id");

                    entity.AddComponent(new Sprite()).Run<Sprite>(d => {
                        d.LoadTexture("Graphics/" + data.GetString("image"));
                    });
                });
            }
            else if (entityInfo.Name == "Animation")
            {
                new Entity(entity => {
                    entity.Name = "Animation";
                    entity.LayerName = "Main";
                    entity.Origin = Vector2.Zero;
                    entity.SortingLayer = 2;
                    
                    ZInterpreter data = new ZInterpreter(entityInfo.Data);
                    entity.Opacity = (data.HasKey("opacity")) ? data.GetFloat("opacity") : 1;

                    entity.AddComponent(new Sprite()).Run<Sprite>(d => {
                        
                        d.LoadTexture("Graphics/" + data.GetString("image"));
                        d.AddAnimation(new Animation("Default", data.GetFloat("delay"), data.GetPointArr("size")[0], data.GetPointArr("frames")));
                        d.RunAnimation("Default");
                        entity.Position = (entityInfo.Position * 16) + (entityInfo.Size.ToVector2() / 2) * 16;
                    });
                });
            }
            else if(entityInfo.Name == "CameraLock")
            {
                new Entity(entity => {
                    entity.Trigger = Entity.TriggerTypes.NonSolid;
                    entity.Name = "CameraLock";
                    entity.LayerName = "Main";
                    entity.Collider = Entity.CollisionType.Pixel;
                    entity.Data.Add("Type", entityInfo.Data);
                    entity.SortingLayer = GameGlobal.Player.SortingLayer;
                    entity.Opacity = 0;
                    entity.AddComponent(new Drawable()).Run<Drawable>(d => {
                        d.BuildRectangle(new Point(entityInfo.Size.X * 16, entityInfo.Size.Y * 16), Color.Red);
                        entity.Position = (entityInfo.Position * 16) + (entityInfo.Size.ToVector2() / 2) * 16;
                    });
                });
            }
        }
    }
}
