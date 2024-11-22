using System.Collections;
using System.Collections.Generic;
using Platformer.Core;
using Platformer.Model;
using UnityEngine;

namespace Platformer.Gameplay
{
    /// Fired when the player has died.
    /// <typeparam name="PlayerDeath"></typeparam>
    public class PlayerDeath : Simulation.Event<PlayerDeath>
    {
        PlatformerModel model = Simulation.GetModel<PlatformerModel>();
        FadePanel panel = Simulation.GetModel<FadePanel>();


        public override void Execute()
        {
            var player = model.player;
            if (player.health.IsAlive)
            {
                player.health.Die();
                model.virtualCamera.m_Follow = null;
                model.virtualCamera.m_LookAt = null;
                // player.collider.enabled = false;
                player.controlEnabled = false;
                
                model.virtualCamera.GetComponent<CameraController>().CompleteResetZoom();

                if (player.audioSource && player.ouchAudio)
                    player.audioSource.PlayOneShot(player.ouchAudio);
                player.spriteAnimator.SetTrigger("hurt");
                player.spriteAnimator.SetBool("dead", true);
                
                panel.Fade();
                
                Simulation.Schedule<PlayerSpawn>(2);
            }
        }
    }
}