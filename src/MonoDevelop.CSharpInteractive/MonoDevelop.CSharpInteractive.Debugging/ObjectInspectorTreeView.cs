//
// ObjectInspectorTreeView.cs
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

using MonoDevelop.Components.Commands;
using MonoDevelop.Debugger;
using MonoDevelop.Ide.Commands;

namespace MonoDevelop.CSharpInteractive.Debugging
{
	/// <summary>
	/// Inherited from MacObjectValueTreeView which allows the command handlers defined here to
	/// be used instead of those on the MacObjectValueTreeView. The CommandManager uses
	/// reflection to find the handlers and reflection returns the ObjectInspectorTreeView methods
	/// first.
	/// </summary>
	class ObjectInspectorTreeView : MacObjectValueTreeView
	{
		public ObjectInspectorTreeView (
			IObjectValueDebuggerService debuggerService,
			ObjectValueTreeViewController controller,
			bool allowEditing,
			ObjectValueTreeViewFlags flags)
			: base (debuggerService, controller, allowEditing, flags)
		{
		}

		[CommandUpdateHandler (EditCommands.Delete)]
		[CommandUpdateHandler (EditCommands.DeleteKey)]
		protected void OnUpdateDelete (CommandInfo info)
		{
			// Always enabled. This is incorrect but the base class CanDelete
			// uses internal classes to determine the result. However the base
			// class delete method will not allow child nodes to be deleted.
			info.Visible = true;
			info.Enabled = true;
		}

		[CommandUpdateHandler (DebugCommands.AddWatch)]
		protected void OnUpdateAddWatch (CommandInfo info)
		{
			info.Visible = false;
			info.Enabled = false;
		}
	}
}
