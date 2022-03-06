//
// InspectCommandHandler.cs
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
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Events;
using Microsoft.VisualStudio.UI;

namespace MonoDevelop.CSharpInteractive.Commands
{
	class InspectCommandHandler
	{
		public static string CommandName = "#!inspect";

		internal static Action<object> OnInspect = obj => { };

		readonly TextWriter outputTextWriter;

		public InspectCommandHandler (TextWriter outputTextWriter)
		{
			this.outputTextWriter = outputTextWriter;
		}

		public async Task InspectAsync (KernelInvocationContext context)
		{
			if (context.Command is not SubmitCode submitCodeCommand) {
				return;
			}

			string code = submitCodeCommand.Code.Substring (CommandName.Length);
			code = code.TrimStart ();

			KernelCommandResult result = await context.HandlingKernel.SubmitCodeAsync (code);
			ReturnValueProduced returnValueProduced = await result.KernelEvents
				.OfType<ReturnValueProduced> ()
				.FirstOrDefaultAsync ();

			if (returnValueProduced != null) {
				OnInspect (returnValueProduced.Value);
			} else {
				CommandFailed commandFailed = await result.KernelEvents
					.OfType<CommandFailed> ()
					.FirstOrDefaultAsync ();

				if (commandFailed != null) {
					outputTextWriter.WriteLine (commandFailed.Message);
				} else {
					string message = GettextCatalog.GetString ("No value to inspect");
					outputTextWriter.WriteLine (message);
				}
			}
		}
	}


	static class KernelExtensions_InspectCommand
	{
		public static T AddInspectCommand<T> (this T kernel, TextWriter outputTextWriter)
			where T : Kernel
		{
			var command = new Command (
				InspectCommandHandler.CommandName,
				GettextCatalog.GetString ("Displays object in Object Inspector"));
			var argument = new Argument ();
			command.AddArgument (argument);

			var inspectCommandHandler = new InspectCommandHandler (outputTextWriter);
			command.Handler = CommandHandler.Create (inspectCommandHandler.InspectAsync);

			kernel.AddDirective (command);

			return kernel;
		}
	}
}

