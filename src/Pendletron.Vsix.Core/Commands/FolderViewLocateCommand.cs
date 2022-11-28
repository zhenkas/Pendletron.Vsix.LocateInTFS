namespace Pendletron.Vsix.Core.Commands
{
    public class FolderViewLocateCommand : LocateCommand
	{
		public FolderViewLocateCommand(ITfsLocater locater, ILocateInTfsVsPackage pkg) : base(locater, pkg) { }

		public override int CommandID
		{
			get { return (int)PackageIDs.cmdidLocateInTFS_FolderView; }
		}

		public override string GetSelectedLocalPath()
		{
			return Locater.GetSelectedPathFromFolderView();
		}
	}
}