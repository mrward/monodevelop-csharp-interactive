// 
// From SoftDebuggerAdaptor.cs
//  
// Authors: Lluis Sanchez Gual <lluis@novell.com>
//          Jeffrey Stedfast <jeff@xamarin.com>
// 
// Copyright (c) 2009 Novell, Inc (http://www.novell.com)
// Copyright (c) 2011,2012 Xamain Inc. (http://www.xamarin.com)
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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Mono.Debugging.Evaluation;
using MonoDevelop.Core;

namespace Mono.Debugging.Soft
{
	static class TypeExtensions
	{
		public static bool IsGenericType (this Type type)
		{
			//
			// This should cover all C# generated special containers
			// - anonymous methods
			// - lambdas
			// - iterators
			// - async methods
			//
			// which allow stepping into
			//

			// Note: mcs uses the form <${NAME}>c__${KIND}${NUMBER} where the leading '<' seems to have been dropped in 3.4.x
			//       csc uses the form <${NAME}>d__${NUMBER}
			//		 roslyn uses the form <${NAME}>d

			return type.Name.IndexOf (">c__", StringComparison.Ordinal) > 0 ||
				type.Name.IndexOf (">d", StringComparison.Ordinal) > 0;
		}

		public static TypeDisplayData GetTypeDisplayData (this Type type)
		{
			if (type == null)
				return null;

			Dictionary<string, DebuggerBrowsableState> memberData = null;
			bool isCompilerGenerated = false;
			string displayValue = null;
			string displayName = null;
			string displayType = null;
			string proxyType = null;

			try {
				foreach (var attr in type.GetCustomAttributes (true)) {
					if (attr is DebuggerDisplayAttribute display) {
						displayValue = display.Value;
						displayName = display.Name;
						displayType = display.Type;
					} else if (attr is DebuggerTypeProxyAttribute proxy) {
						proxyType = proxy.ProxyTypeName;
					} else if (attr is CompilerGeneratedAttribute) {
						isCompilerGenerated = true;
					}
				}

				foreach (var field in type.GetFields ()) {
					var attrs = field.GetCustomAttributes (true);
					var browsable = GetAttribute<DebuggerBrowsableAttribute> (attrs);

					if (browsable == null) {
						var generated = GetAttribute<CompilerGeneratedAttribute> (attrs);
						if (generated != null)
							browsable = new DebuggerBrowsableAttribute (DebuggerBrowsableState.Never);
					}

					if (browsable != null) {
						if (memberData == null)
							memberData = new Dictionary<string, DebuggerBrowsableState> ();
						memberData[field.Name] = browsable.State;
					}
				}

				foreach (var property in type.GetProperties ()) {
					var browsable = GetAttribute<DebuggerBrowsableAttribute> (property.GetCustomAttributes (true));
					if (browsable != null) {
						if (memberData == null)
							memberData = new Dictionary<string, DebuggerBrowsableState> ();
						memberData[property.Name] = browsable.State;
					}
				}
			} catch (Exception ex) {
				LoggingService.LogError ("GetTypeDisplayData error", ex);
			}

			return new TypeDisplayData (proxyType, displayValue, displayType, displayName, isCompilerGenerated, memberData);
		}

		public static T GetAttribute<T> (object[] attrs)
		{
			foreach (var attr in attrs) {
				if (attr is T typedAttr) {
					return typedAttr;
				}
			}

			return default (T);
		}

		public static bool IsAnonymousType (this Type type)
		{
			if (type == null)
				return false;

			return type.Name.StartsWith ("<>__AnonType", StringComparison.Ordinal);
		}

		public static bool IsValueTuple (this object type)
		{
			return IsValueTuple (type as Type);
		}

		public static bool IsValueTuple (this Type type)
		{
			if (type == null)
				return false;

			return type.Name.StartsWith ("ValueTuple`", StringComparison.Ordinal);
		}
	}
}
