﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Pendletron.Vsix.Core.Wrappers
{
	public class HatPackage 
	{
		public dynamic _wrapped = null;
		public Assembly _vcAssembly = null;

        public HatPackage() {
			_vcAssembly = Assembly.Load("Microsoft.VisualStudio.TeamFoundation.VersionControl");
		}

		private dynamic _hatterasService = null;
		public dynamic HatterasService
		{
			get {
                if (_wrapped == null)
                {
                    Type t = _vcAssembly.GetType("Microsoft.VisualStudio.TeamFoundation.VersionControl.HatPackage");
                    var prop = t.GetProperty("Instance", BindingFlags.NonPublic | BindingFlags.Static);
                    object instance = prop.GetValue(null, null);
					if (instance == null)
					{
						throw new Exception("Make sure VS is configured to work with TFS");
					}
                    _wrapped = new AccessPrivateWrapper(instance);
                }
                if (_hatterasService == null)
				{
					_hatterasService = new AccessPrivateWrapper(_wrapped.HatterasService);
				}
				return _hatterasService; }
			set { _hatterasService = value; }
		}

		public dynamic GetVersionControlServer()
		{
			return HatterasService.VersionControlServer;
		}
	}
}
