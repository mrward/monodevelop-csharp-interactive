//
// ObjectInspectorPad.cs
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
using System.Reflection;
using Foundation;
using Gtk;
using Mono.Debugging.Client;
using MonoDevelop.Components;
using MonoDevelop.Debugger;
using MonoDevelop.Ide.Gui;
using Pango;

namespace MonoDevelop.CSharpInteractive.Debugging
{
	class ObjectInspectorPad : PadContent
	{
		readonly ObjectValueTreeViewController controller;
		readonly MacObjectValueTreeView treeView;
		readonly Control control;
		readonly PadFontChanger fontChanger;

		bool disposed;

		public ObjectInspectorPad ()
		{
			controller = new ObjectValueTreeViewController ();
			controller.AllowEditing = false;

			treeView = controller.GetMacControl (ObjectValueTreeViewFlags.ObjectValuePadFlags);

			fontChanger = new PadFontChanger (treeView, SetCustomFont);

			var scrolled = new AppKit.NSScrollView {
				DocumentView = treeView,
				AutohidesScrollers = false,
				HasVerticalScroller = true,
				HasHorizontalScroller = true,
			};

			scrolled.WantsLayer = true;
			scrolled.Layer.Actions = new NSDictionary (
				"actions", NSNull.Null,
				"contents", NSNull.Null,
				"hidden", NSNull.Null,
				"onLayout", NSNull.Null,
				"onOrderIn", NSNull.Null,
				"onOrderOut", NSNull.Null,
				"position", NSNull.Null,
				"sublayers", NSNull.Null,
				"transform", NSNull.Null,
				"bounds", NSNull.Null);

			var host = new GtkNSViewHost (scrolled);
			host.ShowAll ();

			control = host;
		}

		void SetCustomFont (FontDescription font)
		{
			Type type = treeView.GetType ();
			MethodInfo method = type.GetMethod ("SetCustomFont", BindingFlags.NonPublic | BindingFlags.Instance);
			if (method != null) {
				method.Invoke (treeView, new object[] { font });
			}
		}

		public override Control Control {
			get { return control; }
		}

		public override void Dispose ()
		{
			if (disposed)
				return;

			fontChanger.Dispose ();
			disposed = true;

			base.Dispose ();
		}

		public void Inspect (ObjectValue value)
		{
			controller.AddValue (value);
		}
	}
}
