//
// ObjectValueCreator.cs
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
using Mono.Debugging.Client;
using MonoDevelop.Debugger;

namespace MonoDevelop.CSharpInteractive.Debugging
{
	static class ObjectValueCreator
	{
		static readonly CSharpInteractiveObjectValueAdapter objectValueAdapter
			= new CSharpInteractiveObjectValueAdapter ();

		public static ObjectValue Create (object obj, ObjectPath path)
		{
			var sessionOptions = DebuggingService.GetUserOptions ();
			var options = sessionOptions.EvaluationOptions;
			var context = new CSharpInteractiveEvaluationContext (options);
			context.Adapter = objectValueAdapter;
			context.Evaluator = new CSharpInteractiveExpressionEvaluator ();

			var source = new CSharpInteractiveObjectValueSource (path.Join ("."), obj, context);

			try {
				// Cannot update top level value types from the REPL so mark the top level objects as read-only.
				var flags = ObjectValueFlags.ReadOnly;
				ObjectValue value = objectValueAdapter.CreateObjectValue (context, source, path, obj, flags);
				AttachStackFrame (value, sessionOptions);
				return value;
			} catch (Exception ex) {
				return ObjectValue.CreateError (null, path, string.Empty, ex.Message, ObjectValueFlags.None);
			}
		}

		static void AttachStackFrame (ObjectValue value, DebuggerSessionOptions sessionOptions)
		{
			var location = new SourceLocation (string.Empty, "test.cs", 1, 1, 1, 1);
			var stackFrame = new StackFrame (0, location, "C#", false, true);
			var session = new CSharpInteractiveDebuggerSession ();
			session.Run (new DebuggerStartInfo (), sessionOptions);

			stackFrame.Attach (session);
			stackFrame.ConnectCallbacks (value);
		}
	}
}
