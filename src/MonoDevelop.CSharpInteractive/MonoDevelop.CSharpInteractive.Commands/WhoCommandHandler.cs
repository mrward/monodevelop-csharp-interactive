//
// WhoCommandHandler.cs
//
// Based on KernelExtensions.cs from:
// https://github.com/dotnet/interactiveinteractive/src/Microsoft.DotNet.Interactive/
//
// Author:
//       Matt Ward <matt.ward@microsoft.com>
//
// Copyright (c) .NET Foundation and contributors. All rights reserved.
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
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Events;
using Microsoft.DotNet.Interactive.ValueSharing;
using Mono.CSharp;
using MonoDevelop.Core;

namespace MonoDevelop.CSharpInteractive.Commands
{
	class WhoCommandHandler : ICommandHandler
	{
		readonly TextWriter outputTextWriter;
		bool detailed;

		public WhoCommandHandler (TextWriter outputTextWriter, bool detailed)
		{
			this.outputTextWriter = outputTextWriter;
			this.detailed = detailed;
		}

		public async Task<int> InvokeAsync (InvocationContext context)
		{
			var kernelContext = context.BindingContext.GetService(typeof(KernelInvocationContext)) as KernelInvocationContext;

			if (kernelContext == null ||
				kernelContext.Command is not SubmitCode ||
				kernelContext.HandlingKernel is not ISupportGetValue) {
				return 0;
			}

			var nameEvents = new List<ValueInfosProduced> ();

			var requestValues = new RequestValueInfos (kernelContext.Command.TargetKernelName);
			KernelCommandResult result = await kernelContext.HandlingKernel.SendAsync (requestValues, kernelContext.CancellationToken);

			using var _ = result.KernelEvents
				.OfType<ValueInfosProduced> ()
				.Subscribe (e => nameEvents.Add (e));

			if (!detailed) {
				DisplayVariableNames (nameEvents);
				return 0;
			}

			IEnumerable<string> valueNames = nameEvents
				.SelectMany (e => e.ValueInfos.Select (d => d.Name))
				.Distinct ();

			var valueEvents = new List<ValueProduced> ();
			IEnumerable<RequestValue> valueCommands = valueNames
				.Select (valueName => new RequestValue (valueName, kernelContext.HandlingKernel.Name));

			foreach (RequestValue valueCommand in valueCommands) {
				result = await kernelContext.HandlingKernel.SendAsync (valueCommand, kernelContext.CancellationToken);
				using var __ = result.KernelEvents
					.OfType<ValueProduced> ()
					.Subscribe (e => valueEvents.Add (e));
			}

			DisplayVariableValues (valueEvents);

			return 0;
		}

		void DisplayVariableNames (IEnumerable<ValueInfosProduced> allValueInfosProduced)
		{
			foreach (ValueInfosProduced valuesProduced in allValueInfosProduced) {
				foreach (KernelValueInfo valueInfo in valuesProduced.ValueInfos) {
					outputTextWriter.WriteLine (valueInfo.Name);
				}
			}
		}

		void DisplayVariableValues (IEnumerable<ValueProduced> allValuesProduced)
		{
			foreach (ValueProduced valueProduced in allValuesProduced) {
				outputTextWriter.Write (GetValueType (valueProduced.Value));
				outputTextWriter.Write (' ');
				outputTextWriter.Write (valueProduced.Name);
				outputTextWriter.Write (" = ");

				PrettyPrinter.PrettyPrint (outputTextWriter, valueProduced.Value);

				outputTextWriter.WriteLine ();
			}
		}

		static string GetValueType (object value)
		{
			if (value != null) {
				return ShortTypeNameProvider.GetShortTypeName (value.GetType ());
			}
			return string.Empty;
		}
	}

	static class KernelExtensions_WhoCommand
	{
		public static T AddWhoCommand<T> (this T kernel, TextWriter outputTextWriter)
			where T : Kernel
		{
			// who command
			var command = new Command (
				"#!who",
				GettextCatalog.GetString ("Display the names of the current top-level variables."));

			command.Handler = new WhoCommandHandler (outputTextWriter, detailed: false);
			kernel.AddDirective (command);

			// whos command
			command = new Command (
				"#!whos",
				GettextCatalog.GetString ("Display the names of the current top-level variables and their values."));

			command.Handler = new WhoCommandHandler (outputTextWriter, detailed: true);

			kernel.AddDirective (command);

			return kernel;
		}
	}
}

