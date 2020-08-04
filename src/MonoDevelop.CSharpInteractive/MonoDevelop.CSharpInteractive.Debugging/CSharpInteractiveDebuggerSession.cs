//
// CSharpInteractiveDebuggerSession.cs
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

namespace MonoDevelop.CSharpInteractive.Debugging
{
	class CSharpInteractiveDebuggerSession : DebuggerSession
	{
		public CSharpInteractiveDebuggerSession ()
		{
		}

		protected override void OnAttachToProcess (long processId)
		{
		}

		protected override void OnContinue ()
		{
		}

		protected override void OnDetach ()
		{
		}

		protected override void OnEnableBreakEvent (BreakEventInfo eventInfo, bool enable)
		{
		}

		protected override void OnExit ()
		{
		}

		protected override void OnFinish ()
		{
		}

		protected override ProcessInfo[] OnGetProcesses ()
		{
			throw new NotImplementedException ();
		}

		protected override Backtrace OnGetThreadBacktrace (long processId, long threadId)
		{
			throw new NotImplementedException ();
		}

		protected override ThreadInfo[] OnGetThreads (long processId)
		{
			throw new NotImplementedException ();
		}

		protected override BreakEventInfo OnInsertBreakEvent (BreakEvent breakEvent)
		{
			throw new NotImplementedException ();
		}

		protected override void OnNextInstruction ()
		{
		}

		protected override void OnNextLine ()
		{
		}

		protected override void OnRemoveBreakEvent (BreakEventInfo eventInfo)
		{
		}

		protected override void OnRun (DebuggerStartInfo startInfo)
		{
		}

		protected override void OnSetActiveThread (long processId, long threadId)
		{
		}

		protected override void OnStepInstruction ()
		{
		}

		protected override void OnStepLine ()
		{
		}

		protected override void OnStop ()
		{
		}

		protected override void OnUpdateBreakEvent (BreakEventInfo eventInfo)
		{
		}
	}
}
