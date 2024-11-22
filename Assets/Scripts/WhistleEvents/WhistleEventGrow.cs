using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;
using UnityEngine;

namespace Platformer.Gameplay
{
    public class WhistleEventGrow : Simulation.Event<WhistleEventGrow>
    {
        public override void Execute()
        {
            // TODO: Every nearby plant starts to GROW
                // sub-TODO: make plants that the player can stand on and can grow.
        }
    }
}