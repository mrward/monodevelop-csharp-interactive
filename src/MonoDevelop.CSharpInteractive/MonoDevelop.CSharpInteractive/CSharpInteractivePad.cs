//
// CSharpInteractivePad.cs
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
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Components;
using Mono.CSharp;
using Mono.Debugging.Client;
using MonoDevelop.Components;
using MonoDevelop.Components.Declarative;
using MonoDevelop.Components.Docking;
using MonoDevelop.Core;
using MonoDevelop.Core.ProgressMonitoring;
using MonoDevelop.CSharpInteractive.Commands;
using MonoDevelop.CSharpInteractive.Debugging;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;

namespace MonoDevelop.CSharpInteractive
{
	class CSharpInteractivePad : PadContent
	{
		ConsoleViewController controller;
		Evaluator evaluator;
		LogTextWriter logTextWriter;
		ToolbarButtonItem clearButton;
		ToolbarButtonItem stopButton;
		bool onClearCalled;

		public CSharpInteractivePad ()
		{
			Instance = this;
		}

		public static CSharpInteractivePad Instance { get; private set; }

		public override Control Control {
			get { return controller.View; }
		}

		protected override void Initialize (IPadWindow window)
		{
			CreateToolbar (window);
			CreateEvaluator ();
			CreateConsoleView ();
		}

		void CreateToolbar (IPadWindow window)
		{
			var toolbar = new Toolbar ();

			stopButton = new ToolbarButtonItem (toolbar.Properties, nameof (stopButton));
			stopButton.Icon = Stock.Stop;
			stopButton.Clicked += StopButtonClicked;
			stopButton.Tooltip = GettextCatalog.GetString ("Stop running");
			toolbar.AddItem (stopButton);

			clearButton = new ToolbarButtonItem (toolbar.Properties, nameof (clearButton));
			clearButton.Icon = Stock.Broom;
			clearButton.Clicked += ClearButtonClicked;
			clearButton.Tooltip = GettextCatalog.GetString ("Clear");
			toolbar.AddItem (clearButton);

			window.SetToolbar (toolbar, DockPositionType.Right);

			// Workaround being unable to set the button's Enabled state before the underlying
			// NSView is created. It is created after SetToolbar is called.
			stopButton.Enabled = false;
			IsStopButtonEnabled = false;
		}

		void ClearButtonClicked (object sender, EventArgs e)
		{
			controller.Clear ();
		}

		void StopButtonClicked (object sender, EventArgs e)
		{
			try {
				evaluator.Interrupt ();
			} catch (Exception ex) {
				LoggingService.LogError ("Could not stop evaluation", ex);
			}
		}

		void CreateConsoleView ()
		{
			Debug.Assert (evaluator != null);

			controller = new ConsoleViewController (nameof (CSharpInteractivePad));
			controller.TextView.Properties[typeof(Evaluator)] = evaluator;

			string message = GettextCatalog.GetString ("Type \"#!help\" to see available commands.");
			controller.WriteOutput (message + "\n");

			controller.Editable = true;
			controller.Prompt (newLine: false);

			OnCustomOutputPadFontChanged (null, EventArgs.Empty);

			controller.ConsoleInput += OnConsoleInput;

			IdeApp.Preferences.CustomOutputPadFont.Changed += OnCustomOutputPadFontChanged;
		}

		void OnCustomOutputPadFontChanged (object sender, EventArgs e)
		{
			//view.SetFont (IdeApp.Preferences.CustomOutputPadFont);
		}

		void LogTextWriterTextWritten (string writtenText)
		{
			Runtime.RunInMainThread (() => {
				controller.WriteOutput (writtenText);
			}).Ignore ();
		}

		void CreateEvaluator ()
		{
			logTextWriter = new LogTextWriter ();
			logTextWriter.TextWritten += LogTextWriterTextWritten;

			evaluator = new Evaluator (logTextWriter);

			ClearCommandHandler.OnClear = OnClear;
			InspectCommandHandler.OnInspect = OnInspect;
		}

		void OnClear ()
		{
			onClearCalled = true;

			Runtime.RunInMainThread (() => {
				controller.Clear ();
				controller.Prompt (newLine: false);
			}).Ignore ();
		}

		string expression = null;

		void OnConsoleInput (object sender, ConsoleInputEventArgs e)
		{
			if (string.IsNullOrEmpty (e.Text)) {
				WritePrompt ();
				return;
			}

			if (expression == null) {
				expression = e.Text;
			} else {
				expression += "\n" + e.Text;
			}

			Task.Run (async () => {
				try {
					IsStopButtonEnabled = true;

					using (var consoleOutputWriter = ConsoleOutputTextWriter.Create (logTextWriter)) {
						expression = await EvaluateAsync (expression);
					}
				} catch (Exception ex) {
					LoggingService.LogError ("Evaluation error", ex);
				} finally {
					WritePrompt ();
					IsStopButtonEnabled = false;
				}
			}).Ignore ();
		}

		public override void Dispose ()
		{
			stopButton.Clicked -= StopButtonClicked;
			clearButton.Clicked -= ClearButtonClicked;

			controller.ConsoleInput -= OnConsoleInput;
			IdeApp.Preferences.CustomOutputPadFont.Changed -= OnCustomOutputPadFontChanged;
			logTextWriter.TextWritten -= LogTextWriterTextWritten;
		}

		void WritePrompt ()
		{
			bool multiline = expression != null;

			Runtime.RunInMainThread (() => {
				if (!onClearCalled) {
					controller.Prompt (newLine: false, multiLine: multiline);
				}
				onClearCalled = false;
			}).Ignore ();
		}

		async Task<string> EvaluateAsync (string input)
		{
			try {
				EvaluationResult result = await evaluator.EvaluateAsync (input);

				if (result.HasReturnValue) {
					PrettyPrinter.PrettyPrint (logTextWriter, result.ReturnValue.Value);
					WriteLine ();
				} else if (result.IsIncomplete) {
					return input;
				}
				return null;
			} catch (Exception ex) {
				Debug.WriteLine (ex.ToString ());

				return null;
			}
		}

		void WriteLine (string message)
		{
			Runtime.RunInMainThread(() => {
				controller.WriteOutput (message + "\n");
			}).Ignore ();
		}

		void WriteLine ()
		{
			WriteLine (string.Empty);
		}

		void OnInspect (object obj)
		{
			ObjectPath path = obj.GetObjectPath (expression);
			var value = ObjectValueCreator.Create (obj, path);

			Runtime.RunInMainThread(() => {
				Pad pad = IdeApp.Workbench.GetPad<ObjectInspectorPad> ();

				var objectInspectorPad = (ObjectInspectorPad)pad.Content;
				objectInspectorPad.Inspect (value);

				pad.BringToFront ();
			}).Ignore ();
		}

		internal static void EvaluateText (string text)
		{
			Runtime.AssertMainThread();

			Pad pad = IdeApp.Workbench.GetPad<CSharpInteractivePad> ();
			pad.BringToFront ();

			var input = new ConsoleInputEventArgs (text);
			Instance.OnConsoleInput (Instance.controller, input);
		}

		bool IsStopButtonEnabled {
			get { return stopButton.Enabled; }
			set {
				Runtime.RunInMainThread (() => {
					stopButton.Enabled = value;
				}).Ignore ();
			}
		}
	}
}
