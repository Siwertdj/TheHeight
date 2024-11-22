using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;
using UnityEngine;

namespace Platformer.Gameplay
{
    public class WhistleEventGuide : Simulation.Event<WhistleEventGuide>
    {
        private WaypointMover guide;
        
        public override void Execute()
        {
            // TODO: Show path OR arrow towards the goal.
        }
    }
}