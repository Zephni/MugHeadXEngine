using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoXEngine;
using MonoXEngine.EntityComponents;
using MugHeadXEngine;
using MugHeadXEngine.EntityComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGame
{
    public static class GameMethods
    {
        public static void PhysicsActive(bool active)
        {
            Global.Entities.FindAll(entity => entity.HasComponent<Physics>()).ForEach(entity => {
                entity.GetComponent<Physics>().Disabled = !active;
            });

            Global.Entities.FindAll(entity => entity.HasComponent<PlatformerController>()).ForEach(entity => {
                entity.GetComponent<PlatformerController>().Disabled = !active;
            });
        }

        public static void ShowOptionSelector(Vector2 position, List<Option> optionList, Action<string> action = null, Entity player = null, string texture9Patch = "Defaults/9Patch_8")
        {
            if(player != null)
                player.GetComponent<PlatformerController>().MovementEnabled = false;

            OptionSelector.Build(position, optionList, result => {
                action?.Invoke(result);

                if (player != null)
                    player.GetComponent<PlatformerController>().MovementEnabled = true;
            }, texture9Patch);
        }

        public static void ShowMessages(List<MessageBox> Messages, Entity player, Action action = null)
        {
            if (Messages.Count > 0)
            {
                MessageBox CurrentMessage = Messages.First();
                Messages.Remove(CurrentMessage);

                CurrentMessage.Build(() => {
                    ShowMessages(Messages, player, action);
                });
            }
            else
            {
                player.GetComponent<PlatformerController>().MovementEnabled = true;
            }
        }
    }
}
