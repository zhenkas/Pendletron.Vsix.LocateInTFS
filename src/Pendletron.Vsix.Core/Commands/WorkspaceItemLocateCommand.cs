namespace Pendletron.Vsix.Core.Commands
{
    public class WorkspaceItemLocateCommand : LocateCommand
	{
		public WorkspaceItemLocateCommand(ITfsLocater locater, ILocateInTfsVsPackage pkg) : base(locater, pkg) { }

		public override int CommandID
		{
			get { return (int)PackageIDs.cmdidLocateInTFS_WorkspaceItem; }
		}

		public override string GetSelectedLocalPath()
		{
			return Locater.GetSelectedPathFromSolutionExplorer();
		}
	}
}