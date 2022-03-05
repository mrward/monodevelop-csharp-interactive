//
// ShortTypeNameProvider.cs
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
using System.Collections.Generic;

namespace MonoDevelop.CSharpInteractive
{
	static class ShortTypeNameProvider
	{
		static Dictionary<string, string> typeNameMappings =
			new Dictionary<string, string> (StringComparer.OrdinalIgnoreCase);

		static ShortTypeNameProvider()
		{
			typeNameMappings["System.Void"] = "void";
			typeNameMappings["System.Object"] = "object";
			typeNameMappings["System.Boolean"] = "bool";
			typeNameMappings["System.Byte"] = "byte";
			typeNameMappings["System.SByte"] = "sbyte";
			typeNameMappings["System.Char"] = "char";
			typeNameMappings["System.Enum"] = "enum";
			typeNameMappings["System.Int16"] = "short";
			typeNameMappings["System.Int32"] = "int";
			typeNameMappings["System.Int64"] = "long";
			typeNameMappings["System.UInt16"] = "ushort";
			typeNameMappings["System.UInt32"] = "uint";
			typeNameMappings["System.UInt64"] = "ulong";
			typeNameMappings["System.Single"] = "float";
			typeNameMappings["System.Double"] = "double";
			typeNameMappings["System.Decimal"] = "decimal";
			typeNameMappings["System.String"] = "string";
		}

		public static string GetShortTypeName (Type type)
		{
			if (type == null) {
				return string.Empty;
			}

			return GetShortTypeName (type.FullName);
		}

		public static string GetShortTypeName (string typeName)
		{
			if (typeNameMappings.TryGetValue (typeName, out string shortTypeName)) {
				return shortTypeName;
			}
			return typeName;
		}
	}
}

