using HarmonyLib;
using InnerNet;

namespace HydraMenu.modules.protections
{
	internal class ForceDTLS : Module
	{
		public ForceDTLS() : base("ForceDTLS") { }

		private static ForceDTLS Instance
		{
			get { return ModuleManager.forceDtls; }
		}

		[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.SetEndpoint))]
		class EnableDTLS
		{
			static void Prefix(ref bool dtls)
			{
				if(Instance.Enabled) dtls = true;
			}
		}
	}
}