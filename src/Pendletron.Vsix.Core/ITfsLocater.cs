using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pendletron.Vsix.Core
{
	public interface ITfsLocater
	{
		bool IsVersionControlled(string fileSystemPath);
		void Locate(string localPath);
		Task InitializeAsync();
		string GetSelectedPathFromSolutionExplorer();
        string GetSelectedPathFromFolderView();
        string GetSelectedPathFromActiveDocument();
        int CommandExecute(ICommandExecParams e);
        IQueryStatusResult CommandBeforeQueryStatus(ICommandQueryStatusParams e);
    }
}
