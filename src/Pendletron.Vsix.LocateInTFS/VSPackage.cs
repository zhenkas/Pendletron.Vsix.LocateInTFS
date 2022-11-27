using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Pendletron.Vsix.Core;
using Pendletron.Vsix.Core.Locaters;
using System.Threading.Tasks;
using System.Threading;

namespace Pendletron.Vsix.LocateInTFS
{
	/// <summary>
	/// This is the class that implements the package exposed by this assembly.
	///
	/// The minimum requirement for a class to be considered a valid package for Visual Studio
	/// is to implement the IVsPackage interface and register itself with the shell.
	/// This package uses the helper classes defined inside the Managed Package Framework (MPF)
	/// to do it: it derives from the Package class that provides the implementation of the 
	/// IVsPackage interface and uses the registration attributes defined in the framework to 
	/// register itself and its components with the shell.
	/// </summary>
	// This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
	// a package.
	// This attribute is used to register the informations needed to show the this package
	// in the Help/About dialog of Visual Studio.
	[InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
	// This attribute is needed to let the shell know that this package exposes some menus.
	[ProvideMenuResource("Menus.ctmenu", 1)]
	[Guid(GuidList.guidVisualStudio_LocateInTFS_VSIPPkgString)]
	public sealed class VisualStudio_LocateInTFS_VSIPPackage : AsyncPackage, IOleCommandTarget, ILocateInTfsVsPackage
	{
		public static bool _initialized = false;
		/// <summary>
		/// Default constructor of the package.
		/// Inside this method you can place any initialization code that does not require 
		/// any Visual Studio service because at this point the package object is created but 
		/// not sited yet inside Visual Studio environment. The place to do all the other 
		/// initialization is the Initialize method.
		/// </summary>
		public VisualStudio_LocateInTFS_VSIPPackage()
		{
			Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
		}

        private ITfsLocater LocaterPackage { get; set; }

        /////////////////////////////////////////////////////////////////////////////
        // Overriden Package Implementation

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
			_initialized = true;
			progress.Report(new ServiceProgressData("Initializing \"Located in TFS\"", "Calling base...", 0, 3));
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
			await base.InitializeAsync(cancellationToken, progress);

            progress.Report(new ServiceProgressData("Initializing \"Located in TFS\"", "Creating package...", 1, 3));

            LocaterPackage = DerivePackageByVisualStudioVersion();
            if (LocaterPackage != null)
			{
                progress.Report(new ServiceProgressData("Initializing \"Located in TFS\"", "Initializing package...", 2, 3));
                LocaterPackage.Initialize();
			}
            progress.Report(new ServiceProgressData("Initializing \"Located in TFS\"", "DONE", 3, 3));

        }

        private ITfsLocater DerivePackageByVisualStudioVersion()
        {
            ITfsLocater results = null;
            int version = DetermineVisualStudioVersionNumber();
            switch (version)
            {
                default:
                    results = new Vs2013DispatchingLocater(this);
                    break;
            }
            return results;
        }

	    public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
	    {
	        var p = new CommandQueryStatusParams();
	        p.CmdGroup = pguidCmdGroup;
	        p.Cmds = cCmds;
	        p.PrgCmds = prgCmds;
	        p.CmdText = pCmdText;

	        var result = LocaterPackage.CommandBeforeQueryStatus(p);
	        prgCmds[0].cmdf |= result.PrgCmdsValue;
	        return (int) result.ReturnValue;
	    }

	    public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	    {
            var p = new CommandExecParams();
            p.CmdGroup = pguidCmdGroup;
            p.CommandExecOpt = nCmdexecopt;
            p.CommandID = nCmdID;
            p.In = pvaIn;
            p.Out = pvaOut;

            return LocaterPackage.CommandExecute(p);
	    }

	    public int DetermineVisualStudioVersionNumber()
		{
			var d = GetDteAsDynamic();
			string version = d.Version;
			if (version.Contains("."))
			{
				version = version.Remove(version.IndexOf('.'));
			}
			int result = 0;
			Int32.TryParse(version, out result);
			return result;
		}

		public dynamic GetServiceAsDynamic(Type serviceInterfaceType)
		{
			return GetService(serviceInterfaceType);
		}

		public dynamic GetDteAsDynamic()
		{
			return GetService(typeof (EnvDTE.DTE));
		}

		private VsPackageIdentifiers packageIDs = null;
		public IVsPackageIdentifiers PackageIDs
		{
			get
			{
				if (packageIDs == null)
				{
					packageIDs = new VsPackageIdentifiers();
					packageIDs.guidSolutionExplorer = GuidList.guidSolutionExplorer;
					packageIDs.guidSolutionExplorerGuid_String = GuidList.guidSolutionExplorerGuid_String;
					packageIDs.guidVisualStudio_LocateInTFS_VSIPCmdSet = GuidList.guidVisualStudio_LocateInTFS_VSIPCmdSet;
					packageIDs.guidVisualStudio_LocateInTFS_VSIPCmdSetString = GuidList.guidVisualStudio_LocateInTFS_VSIPCmdSetString;
					packageIDs.guidVisualStudio_LocateInTFS_VSIPPkgString = GuidList.guidVisualStudio_LocateInTFS_VSIPPkgString;
                    packageIDs.cmdidLocateInTFS_SolutionExplorer = PkgCmdIDList.cmdidLocateInTFS_SolutionExplorer;
                    packageIDs.cmdidLocateInTFS_CodeWindow = PkgCmdIDList.cmdidLocateInTFS_CodeWindow;
					packageIDs.cmdidLocateInTFS_WorkspaceItem = PkgCmdIDList.cmdidLocateInTFS_WorkspaceItem;
				}
				return packageIDs;
			}
		}
    }
}
