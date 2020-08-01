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
using Gtk;
using Mono.CSharp;
using MonoDevelop.Components;
using MonoDevelop.Components.Docking;
using MonoDevelop.Core;
using MonoDevelop.Core.ProgressMonitoring;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;

namespace MonoDevelop.CSharpInteractive
{
	class CSharpInteractivePad : PadContent
	{
		CSharpInteractiveConsoleView view;
		Evaluator evaluator;
		LogTextWriter logTextWriter;

		public CSharpInteractivePad ()
		{
		}

		public override Control Control {
			get { return view; }
		}

		protected override void Initialize (IPadWindow window)
		{
			CreateToolbar (window);
			CreateEvaluator ();
			InitializeEvaluator ();
			CreateConsoleView ();
		}

		void CreateToolbar (IPadWindow window)
		{
			DockItemToolbar toolbar = window.GetToolbar (DockPositionType.Right);

			var clearButton = new Button (new ImageView (Ide.Gui.Stock.Broom, IconSize.Menu));
			clearButton.Clicked += ButtonClearClicked;
			clearButton.TooltipText = GettextCatalog.GetString ("Clear");
			toolbar.Add (clearButton);

			toolbar.ShowAll ();
		}

		void ButtonClearClicked (object sender, EventArgs e)
		{
			view.Clear ();
		}

		void CreateConsoleView ()
		{
			view = new CSharpInteractiveConsoleView (evaluator);

			OnCustomOutputPadFontChanged (null, EventArgs.Empty);

			view.ConsoleInput += OnConsoleInput;
			view.ShadowType = Gtk.ShadowType.None;
			view.ShowAll ();

			IdeApp.Preferences.CustomOutputPadFont.Changed += OnCustomOutputPadFontChanged;
		}

		void OnCustomOutputPadFontChanged (object sender, EventArgs e)
		{
			view.SetFont (IdeApp.Preferences.CustomOutputPadFont);
		}

		void LogTextWriterTextWritten (string writtenText)
		{
			Runtime.RunInMainThread (() => {
				view.WriteOutput (writtenText);
			});
		}

		void CreateEvaluator ()
		{
			logTextWriter = new LogTextWriter ();
			logTextWriter.TextWritten += LogTextWriterTextWritten;

			var settings = new CompilerSettings ();
			var printer = new ConsoleViewReportPrinter (view);
			var context = new CompilerContext (settings, printer);
			evaluator = new Evaluator (context);
			evaluator.DescribeTypeExpressions = true;
			evaluator.WaitOnTask = true;

			evaluator.InteractiveBaseClass = typeof (InteractiveBase);
			InteractiveBase.Output = logTextWriter;
			InteractiveBase.Error = logTextWriter;
		}

		/// <summary>
		/// Match what 'csharp' does on mono.
		/// </summary>
		void InitializeEvaluator ()
		{
			Evaluate ("using System; using System.Linq; using System.Collections.Generic; using System.Collections;");
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

			Task.Run (() => {
				using (var consoleOutputWriter = ConsoleOutputTextWriter.Create (logTextWriter)) {
					expression = Evaluate (expression);
				}
				WritePrompt ();
			}).Ignore ();
		}

		public override void Dispose ()
		{
			view.ConsoleInput -= OnConsoleInput;
			IdeApp.Preferences.CustomOutputPadFont.Changed -= OnCustomOutputPadFontChanged;
			logTextWriter.TextWritten -= LogTextWriterTextWritten;
		}

		void WritePrompt ()
		{
			bool multiline = expression != null;

			Runtime.RunInMainThread (() => {
				view.Prompt (true, multiline);
			});
		}

		string Evaluate (string input)
		{
			bool result_set;
			object result;

			try {
				input = evaluator.Evaluate (input, out result, out result_set);

				if (result_set) {
					PrettyPrinter.PrettyPrint (logTextWriter, result);
					WriteLine ();
				}
			} catch (Exception ex) {
				WriteLine (ex.Message);
				Debug.WriteLine (ex.ToString ());
				return null;
			}

			return input;
		}

		void WriteLine (string message)
		{
			Runtime.RunInMainThread(() => {
				view.WriteOutput (message + "\n");
			});
		}

		void WriteLine ()
		{
			WriteLine (string.Empty);
		}
	}
}
