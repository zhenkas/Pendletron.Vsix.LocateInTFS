using System;
using System.CodeDom;
using System.ComponentModel.Design;
using System.Reflection;
using Pendletron.Vsix.Core.Commands;
using Pendletron.Vsix.Core.Wrappers;
using System.Collections.Generic;
using Pendletron.Vsix.Core;

namespace Pendletron.Vsix.LocateInTFS
{
	public abstract class ExplorerSccDispatchingLocater : TfsLocaterBase
	{
        public ExplorerSccDispatchingLocater(ILocateInTfsVsPackage package):base(package) 
        { 
            DispatchPollTime = TimeSpan.FromSeconds(0.5);
            MaxDispatchAttempts = 10;
        }

        public TimeSpan DispatchPollTime { get; set; }
        public int MaxDispatchAttempts { get; set; }

	    protected dynamic _explorerScc = null;
	    virtual public dynamic ExplorerScc
	    {
	        get
	        {
	            if (_explorerScc == null)
	            {
                    dynamic explorerSccDiag = new AccessPrivateWrapper(HatterasPackage._wrapped.ExplorerSccDiagProvider);
                    if (explorerSccDiag.m_explorerScc == null)
                    {
                        // if the tool window hasn't been opened yet "explorer" will be null, so we make sure it has opened at least once via ExecuteCommand
                        //HatterasPackage._wrapped.OpenSCE();
                    }
                    _explorerScc = new AccessPrivateWrapper(explorerSccDiag.m_explorerScc);
	            }
	            return _explorerScc;
	        }
	    }

	    protected class ServerItemAndWorkspace
	    {
	        public ServerItemAndWorkspace()
	        {
	            
	        }

	        public ServerItemAndWorkspace(dynamic workspace, string serverPath)
	        {
	            Workspace = workspace;
	            ServerPath = serverPath;
	        }

	        public dynamic Workspace { get; set; }
            public string ServerPath { get; set; }
	    }

        public override void Locate(string localPath)
        {
            var info = GetServerItemAndWorkspaceForLocalPath(localPath);
            //HatterasPackage._wrapped.OpenSceToPath("$/", workspace);
            //HatterasPackage._wrapped.OpenSceToPath(serverPath, workspace);
            DispatchOpenSceToPath(info.ServerPath, info.Workspace);
        }

	    protected virtual ServerItemAndWorkspace GetServerItemAndWorkspaceForLocalPath(string localPath)
	    {
            dynamic vcs;
            try
            {
                vcs = HatterasPackage.HatterasService.VersionControlServer;
            }
            catch (Exception ex)
            {
                throw new Exception($"TFS Source Control is not accessible. Exception: {ex}", ex);
            }
            if (vcs == null)
            {
                throw new Exception("TFS Source Control is not accessible");
            }

            dynamic workspace;
            try
            {
                workspace = vcs.GetWorkspace(localPath);
                if (workspace == null)
                {
                    throw new Exception("Workspace not found");
                }                
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to locate workspace for: {localPath}", ex);
            }        
            try
            {
                string serverPath = workspace.TryGetServerItemForLocalItem(localPath);
                return new ServerItemAndWorkspace(workspace, serverPath);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to locate item in tfs: {localPath}", ex);
            }
        }


	    virtual public void DispatchOpenSceToPath(string serverPath, object workspace)
        {
            /* In VS2013 (unlike VS2010) the Source Control Explorer opens asynchronously. This caused an issue that when
             * the SCE was not open and "Locate in TFS" was clicked we would not be able to open the SCE to the correct place
             * because it hadn't finished opening and connecting.
             * 
             * There aren't any obvious events to which to subscribe, thus we do some polling in a separate thread via 
             * the Dispatcher. It has to be a separate thread in order to not block the SCE from loading.
             * */

            // This will make sure the SCE window is open, or will open
            dynamic sccToolWindow = new AccessPrivateWrapper(HatterasPackage._wrapped.GetToolWindowSccExplorer(true));
            dynamic explorer = new AccessPrivateWrapper(sccToolWindow.SccExplorer);
            
            // We don't want to loop infinitely, if we've tried a few times and it's still not connected then just give up
            // By default this will be about five seconds (10 attempts, 0.5 seconds interval)
            var poller = new DispatchedPoller(MaxDispatchAttempts, DispatchPollTime, () =>
            {
                return !explorer.IsDisconnected;
            },
            () =>
            {
                // The explorer is connected, so we're ready to go
                OpenSceToPathWithPrecedingCall(serverPath, workspace, explorer);
            });
            poller.Go();
        }

        virtual public void OpenSceToSinglePath(string serverPath, object workspace)
        {
            HatterasPackage._wrapped.OpenSceToPath(serverPath, workspace);
        }

        virtual public void OpenSceToPathWithPrecedingCall(string serverPath, object workspace, dynamic explorer)
        {
            // If you call OpenSceToPath to the same directory, it will mess up and won't show any files
            // So we clear whatever was there before navigating to the desired path
            OpenSceToSinglePath("$/", workspace);
            OpenSceToSinglePath(serverPath, workspace);

            DispatchScrollToSccExplorerSelection(ScrollToDispatchLagTime, explorer);
        }

        public override void ShowInExplorer(string serverItem)
        {
            DispatchOpenSceToPath(serverItem, null);
        }

        public override string GetServerPathFromLocal(string localFilePath)
        {
            var info = GetServerItemAndWorkspaceForLocalPath(localFilePath);
            return info.ServerPath;
        }
    }
}