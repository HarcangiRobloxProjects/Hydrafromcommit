using HarmonyLib;

namespace HydraMenu.features
{
    public static class BypassImmortalityPatch
    {
        
        public static bool Enabled { get; set; } = false; 
 // Let me explain the theory here, as the immortality exploit is based on the fact that the CheckMurder RPC
        // checks if the target player is inside a vent, and if they are, it will not allow the murder to go through.
        //and this exploit tricks the anti-cheat into thinking that the targeted player is inside the vent ( actually customvent)
        //so it basically makes us immortal, but with my bypass immortality feature, we can still kill that player 
        // this bypass immortality uses VentilationSystem.Update(Exit, 50), which basically means force exit fake vent 
        // with this you can forcefully kick them out from that fake vent.

      // all credits go to morningstar even tho im making the pull request
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckMurder))]
        public static class CheckMurderPatch
        {
            public static bool Prefix(PlayerControl __instance, PlayerControl target)
            {
               
                if (!Enabled) return true;
  // Make the target that you sent CheckMurder to Exit out of the vent so you can kill them
                VentilationSystem.Update(VentilationSystem.Operation.Exit, 50);

                return true;
            }
        }
    }
}
