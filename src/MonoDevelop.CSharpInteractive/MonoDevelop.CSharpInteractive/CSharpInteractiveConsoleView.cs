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
using System.Reflection;
using Gdk;
using Gtk;
using Mono.CSharp;
using MonoDevelop.Components;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide.CodeCompletion;
using MonoDevelop.Ide.Commands;
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

			UpdateInputLineBegin ();

			TextView.KeyReleaseEvent += TextViewKeyReleaseEvent;
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

			bool dotCompletion = IsDotCompletion (key);
			if (dotCompletion) {
				Buffer.InsertAtCursor (".");
			} else if (key != Gdk.Key.Tab) {
				return base.ProcessKeyPressEvent (args);
			}

			TriggerCompletion (InputLine, completionWidget.Position, autoComplete: !dotCompletion);

			return true;
		}

		bool IsDotCompletion (Gdk.Key key)
		{
			if (key != Gdk.Key.period || completionWindow.Visible) {
				return false;
			}

			// TODO: Handle numbers like mono's REPL?
			// https://github.com/mono/mono/blob/71307187cb1e9d8202d30fcaa1f5c4e4a66bc551/mcs/tools/csharp/getline.cs#L1060
			return true;
		}

		void TriggerCompletion (string line, int caretIndex, bool autoComplete)
		{
			string textToComplete = line.Substring (0, caretIndex);
			string[] completions = TryGetCompletions (textToComplete, out string prefix);
			if (completions == null || completions.Length == 0) {
				return;
			}

			if (completions.Length == 1 && autoComplete) {
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

		public void ClearWithoutPrompt ()
		{
			Buffer.Text = string.Empty;

			// HACK: Clear scriptLines string. This is done in ConsoleView's Clear method but we do
			// not want a prompt to be displayed. Should investigate to see if Clear can be called
			// instead and fix whatever the problem was with the prompt.
			var flags = BindingFlags.Instance | BindingFlags.NonPublic;
			FieldInfo field = typeof (ConsoleView).GetField ("scriptLines", flags);
			if (field != null) {
				field.SetValue (this, string.Empty);
			}
		}

		/// <summary>
		/// Not sure why Copy command does not work with the keyboard shortcut when copying from the read-only
		/// text. The copy command in the current input line works. If the currently focused
		/// window is a TextView then it should work. The Immediate Pad does not have this problem and
		/// it does not have its own Copy command handler. It seems that the IdeApp.Workebench.RootWindow
		/// is the TextArea for the text editor not the pad.
		/// </summary>
		[CommandHandler (EditCommands.Copy)]
		void CopyText ()
		{
			// This is based on what the DefaultCopyCommandHandler does.
			var clipboard = Gtk.Clipboard.Get (Gdk.Atom.Intern ("CLIPBOARD", false));
			TextView.Buffer.CopyClipboard (clipboard);
		}
	}
}
