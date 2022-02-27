//
// Evaluator.cs
//
// Author:
//       Matt Ward <matt.ward@microsoft.com>
//
// Copyright (c) 2022 Microsoft Corporation
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
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.CSharp;
using Microsoft.DotNet.Interactive.Events;
using Microsoft.DotNet.Interactive.Formatting;

namespace MonoDevelop.CSharpInteractive
{
	class Evaluator
	{
		CSharpKernel kernel;
		TextWriter writer;
		ReturnValueProduced returnValueProduced;

		public Evaluator (TextWriter writer)
		{
			this.writer = writer;

			Initialize ();
		}

		void Initialize ()
		{
			kernel = new CSharpKernel ();
			kernel.AddHelpCommand (writer);
			kernel.AddClearCommand ();
			kernel.AddInspectCommand (writer);

			Formatter.DefaultMimeType = PlainTextFormatter.MimeType;
		}

		public void Interrupt ()
		{
			throw new NotImplementedException ();
		}

		public async Task<EvaluationResult> EvaluateAsync (string input)
		{
			returnValueProduced = null;

			bool completeCode = await kernel.IsCompleteSubmissionAsync (input);
			if (!completeCode) {
				return EvaluationResult.FromIncompleteSubmission (input);
			}

			using (IDisposable subscription = kernel.KernelEvents.Subscribe (e => OnEvent (e))) {
				await kernel.SubmitCodeAsync (input);

				if (returnValueProduced != null) {
					return EvaluationResult.FromReturnValue (returnValueProduced);
				}
				return new EvaluationResult ();
			}
		}

		public async Task<CompletionResult> GetCompletionsAsync (string textToComplete)
		{
			var position = new LinePosition (0, textToComplete.Length);
			var command = new RequestCompletions (textToComplete, position);
			var context = KernelInvocationContext.Establish (command);

			await kernel.HandleAsync (command, context);

			CompletionsProduced completionsProduced = await context
				.KernelEvents
				.OfType<CompletionsProduced> ()
				.FirstOrDefaultAsync ();

			if (completionsProduced == null) {
				return CompletionResult.None;
			} else {
				return CompletionResult.FromCompletionItems (textToComplete, completionsProduced);
			}
		}

		void OnEvent (KernelEvent e)
		{
			switch (e) {
				case DiagnosticsProduced diagnosticsProduced:
					// Errors will be reported in CommandFailed.
					break;
				case CommandFailed commandFailed:
					WriteLine (commandFailed.Message);
					break;
				case StandardOutputValueProduced standardOutputValueProduced:
					foreach (FormattedValue value in standardOutputValueProduced.FormattedValues) {
						Write (value.Value);
					}
					break;
				case StandardErrorValueProduced standardErrorValueProduced:
					foreach (FormattedValue value in standardErrorValueProduced.FormattedValues) {
						WriteError (value.Value);
					}
					break;
				case ReturnValueProduced returnValueProduced:
					this.returnValueProduced = returnValueProduced;
					break;
			}
		}

		void WriteLine (string text)
		{
			writer.WriteLine (text);
		}

		void Write (string text)
		{
			writer.Write (text);
		}

		void WriteError (string text)
		{
			writer.Write (text);
		}
	}
}

