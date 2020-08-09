//
// CSharpInteractiveExpressionEvaluator.cs
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
using Mono.Debugging.Evaluation;
using MonoDevelop.Debugger.VsCodeDebugProtocol;

namespace MonoDevelop.CSharpInteractive.Debugging
{
	class CSharpInteractiveExpressionEvaluator : ExpressionEvaluator
	{
		public override string Resolve (DebuggerSession session, SourceLocation location, string exp)
		{
			return exp;
		}

		public override ValueReference Evaluate (EvaluationContext ctx, string exp, object expectedType)
		{
			var asType = expectedType as Type;
			if (asType == null) {
				// Cannot do anything.
			} else if (asType == typeof (string)) {
				exp = VSCodeObjectSource.Unquote (exp);
				return LiteralValueReference.CreateObjectLiteral (ctx, exp, exp);
			} else if (asType.IsPrimitive) {
				return LiteralValueReference.CreateObjectLiteral (ctx, exp, exp);
			}
			return base.Evaluate (ctx, exp, expectedType);
		}
	}
}
