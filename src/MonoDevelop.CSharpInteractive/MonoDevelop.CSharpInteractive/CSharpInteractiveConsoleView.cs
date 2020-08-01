//
// CSharpInteractiveConsoleView.cs
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
using Gdk;
using Gtk;
using Mono.CSharp;
using MonoDevelop.Components;
using MonoDevelop.Ide.CodeCompletion;
using MonoDevelop.Ide.Editor.Extension;

namespace MonoDevelop.CSharpInteractive
{
	class CSharpInteractiveConsoleView : ConsoleView
	{
		readonly CSharpInteractiveConsoleCompletionWidget completionWidget;
		readonly CompletionListWindow completionWindow;
		readonly Evaluator evaluator;
		bool keyHandled;

		public CSharpInteractiveConsoleView (Evaluator evaluator)
		{
			this.evaluator = evaluator;

			completionWidget = new CSharpInteractiveConsoleCompletionWidget (this);
			completionWindow = new CompletionListWindow ();
		}

		/// <summary>
		/// Tab key pressed
		///	  If caret in read - only region ignore
		///	  If completion list window active select item and close window
		///	  Else trigger completion async
		///
		/// Trigger completion
		///	  If completion list window shown end completion session
		///
		///	  Get caret position and current line text
		///	  Evaluator.GetCompletions
		///	  If one item returned use it
		///	  Else start completion session and show window
		/// </summary>
		protected override bool ProcessKeyPressEvent (KeyPressEventArgs args)
		{
			var keyChar = (char)args.Event.Key;
			ModifierType modifier = args.Event.State;
			Gdk.Key key = args.Event.Key;

			if ((key == Gdk.Key.Down || key == Gdk.Key.Up)) {
				keyChar = '\0';
			}

			if (completionWindow.Visible) {
				if (key == Gdk.Key.Return) {
					// AutoSelect is off and the completion window will only
					// complete if tab is pressed. So cheat by converting return
					// into Tab. Otherwise the command will be run.
					key = Gdk.Key.Tab;
					keyChar = '\t';
				}
				var descriptor = KeyDescriptor.FromGtk (key, keyChar, modifier);
				keyHandled = completionWindow.PreProcessKeyEvent (descriptor);
				if (keyHandled) {
					return true;
				}
			}

			if (key != Gdk.Key.Tab) {
				return base.ProcessKeyPressEvent (args);
			}

			int caretIndex = completionWidget.Position;
			TriggerCompletion (InputLine, caretIndex);

			return true;
		}

		void TriggerCompletion (string line, int caretIndex)
		{
			string textToComplete = line.Substring (0, caretIndex);
			string[] completions = TryGetCompletions (textToComplete, out string prefix);
			if (completions == null || completions.Length == 0) {
				return;
			}

			if (completions.Length == 1) {
				CompleteText (completions [0], prefix, caretIndex);
				return;
			}

			CompletionDataList list = CreateCompletionList (completions, prefix);

			int triggerOffset = caretIndex - prefix.Length;
			var context = completionWidget.CreateCodeCompletionContext (triggerOffset);

			completionWindow.ShowListWindow ('\0', list, completionWidget, context);
		}

		string[] TryGetCompletions (string textToComplete, out string prefix)
		{
			try {
				return evaluator.GetCompletions (textToComplete, out prefix);
			} catch (Exception ex) {
				Debug.WriteLine (ex);

				prefix = null;
				return null;
			}
		}

		static CompletionDataList CreateCompletionList (string[] completions, string prefix)
		{
			var list = new CompletionDataList {
				AutoSelect = false
			};

			foreach (string item in completions) {
				list.Add (new CompletionData (prefix + item));
			}

			return list;
		}

		void CompleteText (string completion, string prefix, int caretIndex)
		{
			TextIter start = Buffer.GetIterAtOffset (InputLineBegin.Offset + caretIndex);
			TextIter end = Buffer.GetIterAtOffset (start.Offset + completion.Length);

			Buffer.Delete (ref start, ref end);
			Buffer.Insert (ref start, completion);
		}

		void HideWindow ()
		{
			completionWindow.HideWindow ();
		}

		void TextViewFocusOutEvent (object sender, FocusOutEventArgs args)
		{
			HideWindow ();
		}

		void TextViewKeyReleaseEvent (object sender, KeyReleaseEventArgs args)
		{
			if (keyHandled || !completionWindow.Visible)
				return;

			var keyChar = (char)args.Event.Key;
			ModifierType modifier = args.Event.State;
			Gdk.Key key = args.Event.Key;

			var descriptor = KeyDescriptor.FromGtk (key, keyChar, modifier);
			completionWindow.PostProcessKeyEvent (descriptor);
		}

		internal TextView GetTextView ()
		{
			return TextView;
		}

		protected override void UpdateInputLineBegin ()
		{
			completionWidget?.OnUpdateInputLineBegin ();
			base.UpdateInputLineBegin ();
		}
	}
}
