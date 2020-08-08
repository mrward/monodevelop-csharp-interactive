//
// CSharpInteractiveObjectValueAdapter.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Mono.Debugging.Evaluation;
using Mono.Debugging.Soft;

namespace MonoDevelop.CSharpInteractive.Debugging
{
	class CSharpInteractiveObjectValueAdapter : ObjectValueAdaptor
	{
		public CSharpInteractiveObjectValueAdapter ()
		{
		}

		public override ICollectionAdaptor CreateArrayAdaptor (EvaluationContext ctx, object arr)
		{
			return new ArrayAdapter (ctx, arr);
		}

		public override object CreateNullValue (EvaluationContext ctx, object type)
		{
			throw new System.NotImplementedException ();
		}

		public override IStringAdaptor CreateStringAdaptor (EvaluationContext ctx, object str)
		{
			throw new System.NotImplementedException ();
		}

		public override object CreateValue (EvaluationContext ctx, object value)
		{
			return value;
		}

		public override object CreateValue (EvaluationContext ctx, object type, params object[] args)
		{
			if (type is Type asType) {
				return Activator.CreateInstance (asType, args);
			}
			return null;
		}

		public override object GetBaseType (EvaluationContext ctx, object type)
		{
			var asType = type as Type;
			return asType?.BaseType;
		}

		protected override object GetBaseTypeWithAttribute (EvaluationContext ctx, object type, object attrType)
		{
			var attributeAsType = attrType as Type;
			var asType = type as Type;

			while (asType != null) {
				if (asType.GetCustomAttributes (attributeAsType, false).Any ()) {
					return asType;
				}
				asType = asType.BaseType;
			}

			return null;
		}

		public override object GetType (EvaluationContext ctx, string name, object[] typeArgs)
		{
			switch (name) {
				case "System.Diagnostics.DebuggerTypeProxyAttribute":
					return typeof (DebuggerTypeProxyAttribute);
			}

			string fullName = QualifiedTypeName.GetName (name, typeArgs);
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
				Type type = assembly.GetType (fullName, false, false);
				if (type != null) {
					return type;
				}
			}

			return null;
		}

		public override object[] GetTypeArgs (EvaluationContext ctx, object type)
		{
			var asType = type as Type;
			if (asType != null) {
				return asType.GetGenericArguments();
			}
			return Array.Empty<object>();
		}

		public override string GetTypeName (EvaluationContext ctx, object type)
		{
			var asType = type as Type;
			return asType?.FullName;
		}

		public override object GetValueType (EvaluationContext ctx, object val)
		{
			return MemberResolver.GetValueType (val);
		}

		public override bool HasMember (EvaluationContext ctx, object type, string memberName, BindingFlags bindingFlags)
		{
			throw new System.NotImplementedException ();
		}

		public override bool HasMethod (EvaluationContext ctx, object targetType, string methodName, object[] genericTypeArgs, object[] argTypes, BindingFlags flags)
		{
			throw new System.NotImplementedException ();
		}

		public override bool IsArray (EvaluationContext ctx, object val)
		{
			Type type = val?.GetType ();
			return type?.IsArray == true;
		}

		public override bool IsClass (EvaluationContext ctx, object type)
		{
			var asType = type as Type;
			return asType?.IsClass == true;
		}

		public override bool IsEnum (EvaluationContext ctx, object val)
		{
			Type type = val?.GetType ();
			return type?.IsEnum == true;
		}

		public override bool IsNull (EvaluationContext ctx, object val)
		{
			return val == null;
		}

		public override bool IsPointer (EvaluationContext ctx, object val)
		{
			Type type = val?.GetType ();
			return type?.IsPrimitive == true;
		}

		public override bool IsPrimitive (EvaluationContext ctx, object val)
		{
			if (val == null) {
				return false;
			}

			if (val is string) {
				return true;
			}

			Type type = val.GetType ();
			if (type.IsPrimitive) {
				return true;
			}

			if (type.IsValueType &&
				type.Namespace == "System" &&
				(type.Name == "nfloat" || type.Name == "nint")) {
				return true;
			}

			return false;
		}

		public override bool IsString (EvaluationContext ctx, object val)
		{
			Type type = val?.GetType ();
			return typeof (string) == type;
		}

		public override bool IsValueType (object type)
		{
			var asType = type as Type;
			return asType?.IsValueType == true;
		}

		public override object RuntimeInvoke (EvaluationContext ctx, object targetType, object target, string methodName, object[] genericTypeArgs, object[] argTypes, object[] argValues)
		{
			switch (methodName) {
				case "GetEnumerator":
					if ((Type)targetType == typeof(IEnumerable) &&
						genericTypeArgs == null &&
						argTypes?.Length == 0 &&
						argValues?.Length == 0 &&
						target is IEnumerable enumerable) {
						return enumerable.GetEnumerator ();
					}
					break;
				case "MoveNext":
					if ((Type)targetType == typeof(IEnumerator) &&
						genericTypeArgs == null &&
						argTypes?.Length == 0 &&
						argValues?.Length == 0 &&
						target is IEnumerator enumerator) {
						return enumerator.MoveNext ();
					}
					break;
			}
			throw new System.NotImplementedException ();
		}

		public override object TryCast (EvaluationContext ctx, object val, object type)
		{
			var asType = type as Type;
			if (asType == null)
				return null;

			return System.Convert.ChangeType (val, asType);
		}

		protected override IEnumerable<ValueReference> GetMembers (
			EvaluationContext ctx,
			IObjectSource objectSource,
			object t,
			object co,
			BindingFlags bindingFlags)
		{
			var resolver = new MemberResolver ();
			return resolver.GetMembers (ctx, objectSource, t, co, bindingFlags);
		}

		protected override IEnumerable<ValueReference> GetMembers (EvaluationContext ctx, object t, object co, BindingFlags bindingFlags)
		{
			return GetMembers (ctx, null, t, co, bindingFlags);
		}

		protected override ValueReference GetMember (EvaluationContext ctx, object t, object co, string name)
		{
			var resolver = new MemberResolver ();
			return resolver.GetMember (ctx, null, t, co, name);
		}

		public override object TargetObjectToObject (EvaluationContext ctx, object obj)
		{
			Type type = obj?.GetType ();
			if (type != null) {
				if (type.IsPrimitive == true) {
					return obj;
				} else if (type.IsValueType) {
					return obj;
				} else if (type == typeof (string)) {
					return obj;
				}
			}
			return base.TargetObjectToObject (ctx, obj);
		}

		public override IEnumerable<object> GetNestedTypes (EvaluationContext ctx, object type)
		{
			var asType = type as Type;
			if (asType != null) {
				foreach (Type nested in asType.GetNestedTypes ()) {
					if (!nested.IsGenericType ()) {
						yield return nested;
					}
				}
			}
		}

		public override IEnumerable<object> GetImplementedInterfaces (EvaluationContext ctx, object type)
		{
			var asType = type as Type;
			if (asType != null) {
				foreach (Type nested in asType.GetInterfaces ()) {
					yield return nested;
				}
			}
		}

		protected override TypeDisplayData OnGetTypeDisplayData (EvaluationContext ctx, object type)
		{
			var asType = type as Type;
			return asType.GetTypeDisplayData ();
		}
	}
}
