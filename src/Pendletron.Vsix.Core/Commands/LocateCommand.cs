﻿using System;
using System.ComponentModel.Design;
using System.Threading.Tasks;

namespace Pendletron.Vsix.Core.Commands
{

	public abstract class LocateCommand
	{
		public LocateCommand(ITfsLocater locater, ILocateInTfsVsPackage pkg)
		{
			Locater = locater;
			Package = pkg;
		}

		public ITfsLocater Locater { get; set; }
		public ILocateInTfsVsPackage Package { get; set; }
	    public abstract int CommandID { get; }

		public IVsPackageIdentifiers PackageIDs { get { return Package.PackageIDs; } }

        protected virtual async Task AddToMenuCommandServiceAsync(BaseCommand cmd)
		{
			IMenuCommandService mcs = await Package.GetServiceAsync<IMenuCommandService>(typeof(IMenuCommandService));
			var found = mcs.FindCommand(cmd.CommandID);

			if (found == null)
			{
				mcs.AddCommand(cmd);
			}
		}

		public virtual async Task<MenuCommand> RegisterCommandAsync()
		{
			// Create the command for the menu item.
			CommandID menuCommandID = new CommandID(PackageIDs.guidVisualStudio_LocateInTFS_VSIPCmdSet, CommandID);
			var menuItem = new BaseCommand(Execute, menuCommandID, "Locate in TFS");
			//menuItem.BeforeQueryStatus += new EventHandler(BeforeQueryStatus);
			await AddToMenuCommandServiceAsync(menuItem);
		    return menuItem;
		}

		public abstract string GetSelectedLocalPath();

		public virtual bool BeforeQueryStatus(object sender, EventArgs e)
		{
			var menuCommand = sender as MenuCommand;
			string selectedPath = GetSelectedLocalPath();
			bool isVersionControlled = Locater.IsVersionControlled(selectedPath);
			return isVersionControlled;
		}

		public virtual void Execute(object sender, EventArgs e)
		{
			string selectedPath = GetSelectedLocalPath();
			Locater.Locate(selectedPath);
		}
	}
}