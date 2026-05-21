namespace HydraMenu.routines
{
	public abstract class IRoutine
	{
		public string name = "";
		public virtual bool Enabled { get; set; } = false;

		public abstract void Run();
	}
}