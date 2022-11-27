using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Pendletron.Vsix.Core.Commands
{
    public class BaseCommand : MenuCommand, IMenuCommandInvokeEx, IOleMenuCommand
    {
        public BaseCommand(EventHandler handler, CommandID command, string text) : base(handler, command)
        {
            m_text = text;
        }
        private string m_text;
        public string ParametersDescription { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Text { get => m_text; set => m_text = value; }

        public bool DynamicItemMatch(int cmdId)
        {
            return cmdId == base.CommandID.ID;
        }

        public void Invoke(object inArg, IntPtr outArg, OLECMDEXECOPT options)
        {
            base.Invoke(inArg);
        }

        public void Invoke(object inArg, IntPtr outArg)
        {
            base.Invoke(inArg);
        }
    }
}
