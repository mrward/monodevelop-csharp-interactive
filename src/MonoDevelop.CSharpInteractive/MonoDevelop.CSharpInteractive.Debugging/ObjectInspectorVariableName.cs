//
// ObjectInspectorVariableName.cs
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
	static class ObjectInspectorVariableName
	{
		public static ObjectPath GetObjectPath (this object obj, string expression)
		{
			string variableName = ParseInspectParameterName (expression);
			if (!string.IsNullOrEmpty (variableName)) {
				return new ObjectPath (variableName);
			}

			Type type = obj?.GetType ();
			return GetObjectPath (type);
		}

		static ObjectPath GetObjectPath (Type type)
		{
			if (type == null)
				return new ObjectPath ("null");

			return new ObjectPath (type.Name);
		}

		static readonly string InspectMethodName = "Inspect";

		static string ParseInspectParameterName (string expression)
		{
			if (string.IsNullOrEmpty (expression))
				return null;

			int startIndex = expression.IndexOf (InspectMethodName);
			if (startIndex < 0)
				return null;

			startIndex += InspectMethodName.Length;

			startIndex = expression.IndexOf ('(', startIndex);
			if (startIndex < 0)
				return null;

			startIndex += 1;

			int endIndex = expression.IndexOf (')', startIndex);
			if (endIndex < 0)
				return null;

			return expression.Substring (startIndex, endIndex - startIndex);
		}
	}
}
