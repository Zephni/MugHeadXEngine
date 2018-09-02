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

            Global.Entities.FindAll(entity => entity.HasComponent<PlayerController>()).ForEach(entity => {
                entity.GetComponent<PlayerController>().Disabled = !active;
            });
        }

        public static void ShowOptionSelector(Vector2 position, List<Option> optionList, Action<string> action = null, Entity player = null, string texture9Patch = "Defaults/9Patch_8")
        {
            if(player != null)
                player.GetComponent<PlayerController>().MovementEnabled = false;

            OptionSelector os = new OptionSelector();
            os.Build(position, optionList, result => {
                action?.Invoke(result);

                if (player != null)
                    player.GetComponent<PlayerController>().MovementEnabled = true;
            }, texture9Patch);
        }

        public static void ShowMessages(List<MessageBox> Messages, bool? DisablePlayerMovement = null, Action action = null, bool overrideInteraction = false)
        {
            if (!overrideInteraction && GameGlobal.DisableInteraction)
                return;

            GameGlobal.DisableInteraction = true;
            if (Messages.Count > 0)
            {
                MessageBox CurrentMessage = Messages.First();
                Messages.Remove(CurrentMessage);

                if(DisablePlayerMovement != null)
                {
                    GameGlobal.Player.GetComponent<PlayerController>().MovementMode = (DisablePlayerMovement == true) ? PlayerController.MovementModes.None : PlayerController.MovementModes.Normal;
                }

                CurrentMessage.Build(() => {
                    ShowMessages(Messages, DisablePlayerMovement, action, true);
                });
            }
            else
            {
                GameGlobal.DisableInteraction = false;

                action?.Invoke();

                if (DisablePlayerMovement == true)
                    GameGlobal.Player.GetComponent<PlayerController>().MovementMode = PlayerController.MovementModes.Normal;
            }
        }
    }
}
