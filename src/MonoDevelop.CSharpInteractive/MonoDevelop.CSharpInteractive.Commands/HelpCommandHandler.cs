//
// HelpCommandHandler.cs
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

using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.DotNet.Interactive;
using Microsoft.VisualStudio.UI;

namespace MonoDevelop.CSharpInteractive.Commands
{
	class HelpCommandHandler : ICommandHandler
	{
		readonly Kernel kernel;
		readonly TextWriter outputWriter;

		public HelpCommandHandler (Kernel kernel, TextWriter outputWriter)
		{
			this.kernel = kernel;
			this.outputWriter = outputWriter;
		}

		public Task<int> InvokeAsync (InvocationContext context)
		{
			kernel.VisitSubkernelsAndSelf (currentKernel => {
				foreach (Command command in currentKernel.Directives.Where (d => !d.IsHidden)) {
					if (command.Name == "#!inspect") {
						DisplayReferenceCommandHelpText ();
					}

					string helpText = GetCommandHelpText (command);
					outputWriter.WriteLine (helpText);
				}
			});

			return Task.FromResult (0);
		}

		void DisplayReferenceCommandHelpText ()
		{
			// #!r command seems to work without using Kernel.AddNugetDirectives.
			// However this is not a command registered with the kernel, presumably
			// supported by Roslyn itself. Show help text for this command.

			outputWriter.WriteLine ("#!r - Add a metadata reference to specified assembly and all its dependencies, e.g. #r \"myLib.dll\"");
		}

		static string GetCommandHelpText (Command command)
		{
			if (string.IsNullOrEmpty (command.Description)) {
				return command.Name;
			}

			return string.Format ("{0} – {1}", command.Name, command.Description);
		}
	}

	static class KernelExtensions_HelpCommand
	{
		public static T AddHelpCommand<T> (this T kernel, TextWriter outputWriter)
			where T : Kernel
		{
			var command = new Command ("#!help", GettextCatalog.GetString("Show Help for C# Interactive"));
			command.Handler = new HelpCommandHandler (kernel, outputWriter);

			kernel.AddDirective (command);

			return kernel;
		}
	}
}

