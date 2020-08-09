//
// ObjectInspectorTreeViewController.cs
//
// Author:
//       Matt Ward <matt.ward@microsoft.com>
//
// Copyright (c) 2020 Microsoft Corporation
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Threading;
using System.Threading.Tasks;
using Mono.Debugging.Client;
using MonoDevelop.Debugger;

namespace MonoDevelop.CSharpInteractive.Debugging
{
	class ObjectInspectorTreeViewController : ObjectValueTreeViewController
	{
		ObjectInspectorTreeView view;
		ObjectInspectorDebuggerService debuggerService;

		public ObjectInspectorTreeViewController ()
		{
			debuggerService = new ObjectInspectorDebuggerService (this);
		}

		public MacObjectValueTreeView GetMacTreeView (ObjectValueTreeViewFlags flags)
		{
			if (view != null)
				throw new InvalidOperationException ("You can only get the control once for each controller instance");

			var control = new ObjectInspectorTreeView (debuggerService, this, AllowEditing, flags);
			ConfigureView (control);
			view = control;

			return control;
		}

		class ObjectInspectorDebuggerService : IObjectValueDebuggerService
		{
			IObjectValueDebuggerService debuggerService;

			public ObjectInspectorDebuggerService (IObjectValueDebuggerService debuggerService)
			{
				this.debuggerService = debuggerService;
			}

			public bool CanQueryDebugger => true;
			public IStackFrame Frame => debuggerService.Frame;

			public Task<CompletionData> GetCompletionDataAsync (string expression, CancellationToken token)
			{
				return debuggerService.GetCompletionDataAsync (expression, token);
			}
		}
	}
}
