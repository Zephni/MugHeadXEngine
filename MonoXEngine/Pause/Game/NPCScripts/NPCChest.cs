using Microsoft.Xna.Framework;
using MonoXEngine;
using MonoXEngine.EntityComponents;
using MugHeadXEngine;
using MugHeadXEngine.EntityComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XEditor;

namespace MyGame
{
    public static class NPCChest
    {
        public static void Create(EntityInfo entityInfo,  ZInterpreter data)
        {
            new Entity(entity => {
                entity.Name = "NPCChest";
                entity.LayerName = "Main";

                entity.SortingLayer = GameGlobal.PlayerGraphicEntity.SortingLayer - 1;
                entity.Position = (entityInfo.Position * 16) + (entityInfo.Size.ToVector2() / 2) * 16;
                entity.Data.Add("id", data.GetString("id"));
                entity.AddComponent(new Sprite()).Run<Sprite>(d => {
                    d.LoadTexture("Entities/Chest_" + data.GetString("type"));
                    d.AddAnimation(new Animation("Closed", 0, new Point(32, 32), new Point(0, 0)));
                    d.AddAnimation(new Animation("Closing", 0.1f, new Point(32, 32), new Point(4, 0), new Point(3, 0), new Point(2, 0), new Point(1, 0), new Point(0, 0)));
                    d.AddAnimation(new Animation("Opening", 0.1f, new Point(32, 32), new Point(1, 0), new Point(2, 0), new Point(3, 0), new Point(4, 0)));
                    d.AddAnimation(new Animation("Open", 0, new Point(32, 32), new Point(4, 0)));

                    if (GameData.Get("NPCChest/" + entity.Data["id"] + "/" + "open") == "true")
                        d.RunAnimation("Open");
                    else
                        d.RunAnimation("Closed");
                });

                entity.AddComponent(new Collider()).Run<Collider>(c => { c.TriggerType = Collider.TriggerTypes.NonSolid; c.ColliderType = Collider.ColliderTypes.Box; });
            });
        }

        public static void ID_first(Entity obj)
        {
            if(GameData.Get("NPCChest/" + obj.Data["id"] + "/" + "script") == null)
            {
                Global.AudioController.Play("SFX/OpenChest");
                obj.GetComponent<Sprite>().RunAnimation("Opening", false);
                GameData.Set("NPCChest/" + obj.Data["id"] + "/" + "open", "true");

                GameGlobal.PlayerController.MovementMode = PlayerController.MovementModes.None;

                StaticCoroutines.CoroutineHelper.WaitRun(1, () => {
                    GameMethods.ShowMessages(new List<MessageBox>() {
                    new MessageBox(null, "There's nothing inside.|.|.", new Vector2(obj.Position.X, obj.Position.Y - 30))
                }, null, () => {
                    StaticCoroutines.CoroutineHelper.WaitRun(2, () => {
                        GameMethods.ShowMessages(new List<MessageBox>() {
                            new MessageBox("Majestic Chest", "Hi!|| Did you want something?", new Vector2(obj.Position.X, obj.Position.Y - 30)),
                            new MessageBox("Pause", "|.|.|.| I wasn't expecting that.", new Vector2(GameGlobal.Player.Position.X, GameGlobal.Player.Position.Y - 20))
                        }, null, () => {
                            StaticCoroutines.CoroutineHelper.WaitRun(1, () => {
                                GameGlobal.PlayerController.MovementMode = PlayerController.MovementModes.Normal;
                                GameData.Set("NPCChest/" + obj.Data["id"] + "/" + "open", "false");
                                GameData.Set("NPCChest/" + obj.Data["id"] + "/" + "script", "2");
                                obj.GetComponent<Sprite>().RunAnimation("Closing", false);
                                Global.AudioController.Play("SFX/OpenChest");
                            });
                        });
                        });
                    });
                });
            }
            else if(GameData.Get("NPCChest/" + obj.Data["id"] + "/" + "script") == "2")
            {
                    GameMethods.ShowMessages(new List<MessageBox>() {
                        new MessageBox(null, ".|.|. nothing.", new Vector2(obj.Position.X, obj.Position.Y - 30))
                    }, true, () =>
                    {

                    });
            }
        }
    }
}
