//
// CSharpInteractiveAsyncCompletionSource.cs
//
// Author:
//       Matt Ward <matt.ward@microsoft.com>
//
// Copyright (c) 2021 Microsoft Corporation
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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Components;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Mono.CSharp;

namespace MonoDevelop.CSharpInteractive
{
	/// <summary>
	/// Implementing IAsyncCompletionUniversalSource since this allows the completion items to
	/// define their own ApplicableToSpan. Implementing only the IAsyncCompletionSource
	/// results in the applicableToSpan being fixed when returned from InitializeCompletion.
	/// InitializeCompletion is synchronous so we do not want to run the C# evaluator there
	/// so we cannot use IAsyncCompletionSource. IAsyncCompletionUniversalSource being
	/// implemented means GetCompletionContextAsync is called without InitializeCompletion
	/// being called and the ApplicableToSpan set on each CompletionItem will be used.
	/// </summary>
	class CSharpInteractiveAsyncCompletionSource : IAsyncCompletionUniversalSource
	{
		readonly ITextView textView;
		readonly ConsoleViewController controller;
		readonly Evaluator evaluator;

		public CSharpInteractiveAsyncCompletionSource (ITextView textView)
		{
			this.textView = textView;
			textView.Properties.TryGetProperty (typeof (ConsoleViewController), out controller);
			textView.Properties.TryGetProperty (typeof (Evaluator), out evaluator);
		}

		/// <summary>
		/// Not called when IAsyncCompletionUniversalSource is implemented. 
		/// </summary>
		public Task<CompletionContext> GetCompletionContextAsync (
			IAsyncCompletionSession session,
			CompletionTrigger trigger,
			SnapshotPoint triggerLocation,
			SnapshotSpan applicableToSpan,
			CancellationToken token)
		{
			string text = applicableToSpan.GetText ();
			string[] completions = TryGetCompletions (text, out string prefix);
			if (completions == null || completions.Length == 0) {
				return null;
			}

			SnapshotPoint snapshot = triggerLocation.Subtract (prefix.Length);
			var span = new SnapshotSpan (snapshot, prefix.Length);

			var completionItems = new List<CompletionItem> ();
			foreach (string completionText in completions) {
				var item = new CompletionItem (prefix + completionText, this, span);
				completionItems.Add (item);
			}

			var context = new CompletionContext (completionItems.ToImmutableArray ());
			return Task.FromResult (context);
		}

		public Task<CompletionContext> GetCompletionContextAsync (
			CompletionTrigger trigger,
			SnapshotPoint triggerLocation,
			CancellationToken token)
		{
			var line = triggerLocation.GetContainingLine ();
			if (line == null) {
				return null;
			}

			string text = line.GetText ();
			string[] completions = TryGetCompletions (text, out string prefix);
			if (completions == null || completions.Length == 0) {
				return null;
			}

			SnapshotPoint snapshot = triggerLocation.Subtract (prefix.Length);
			var span = new SnapshotSpan (snapshot, prefix.Length);

			var completionItems = new List<CompletionItem> ();
			foreach (string completionText in completions) {
				var item = new CompletionItem (prefix + completionText, this, span);
				completionItems.Add (item);
			}

			var context = new CompletionContext (completionItems.ToImmutableArray ());
			return Task.FromResult (context);
		}

		public Task<object> GetDescriptionAsync (
			IAsyncCompletionSession session,
			CompletionItem item,
			CancellationToken token)
		{
			return Task.FromResult<object> (null);
		}

		public CompletionContinuation HandleTypedChar (
			IAsyncCompletionSession session,
			CompletionItem selectedItem,
			SnapshotPoint location,
			char typedChar,
			CancellationToken token)
		{
			return CompletionContinuation.Continue;
		}

		/// <summary>
		/// Not called when IAsyncCompletionUniversalSource is implemented.
		/// </summary>
		public CompletionStartData InitializeCompletion (
			CompletionTrigger trigger,
			SnapshotPoint triggerLocation,
			CancellationToken token)
		{
			if (trigger.Reason != CompletionTriggerReason.Insertion) {
				return new CompletionStartData (CompletionParticipation.DoesNotProvideItems);
			}

			if (trigger.Character == '\t' || char.IsLetterOrDigit (trigger.Character) || trigger.Character == '.') {
				var line = triggerLocation.GetContainingLine ();
				if (line != null) {
					var snapshotSpan = new SnapshotSpan (line.Start, triggerLocation.Position - line.Start.Position);
					return new CompletionStartData (CompletionParticipation.ProvidesItems, snapshotSpan);
				}
			}
			return new CompletionStartData (CompletionParticipation.DoesNotProvideItems);
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

		public CommitResult TryCommit (IAsyncCompletionSession session, ITextBuffer buffer, CompletionItem item, char typedChar, CancellationToken token)
		{
			return CommitResult.Unhandled;
		}
	}
}

