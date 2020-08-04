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
using System.Collections.Generic;
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
			throw new System.NotImplementedException ();
		}

		public override object CreateValue (EvaluationContext ctx, object type, params object[] args)
		{
			throw new System.NotImplementedException ();
		}

		public override object GetBaseType (EvaluationContext ctx, object type)
		{
			var asType = type as Type;
			return asType?.BaseType;
		}

		public override object GetType (EvaluationContext ctx, string name, object[] typeArgs)
		{
			throw new System.NotImplementedException ();
		}

		public override object[] GetTypeArgs (EvaluationContext ctx, object type)
		{
			throw new System.NotImplementedException ();
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
			Type type = val?.GetType ();
			return type?.IsPrimitive == true;
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
			throw new System.NotImplementedException ();
		}

		public override object TryCast (EvaluationContext ctx, object val, object type)
		{
			throw new System.NotImplementedException ();
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
	}
}
