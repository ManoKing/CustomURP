using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using System;

namespace Frame
{
	[ILAdapter]
	public class ExceptionClassInheritanceAdaptor : CrossBindingAdaptor
	{
		public override Type AdaptorType
		{
			get
			{
				return typeof(ExceptionAdaptor);
			}
		}

		public override Type BaseCLRType
		{
			get
			{
				return typeof(Exception);
			}
		}

		public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
		{
			return new ExceptionAdaptor(appdomain, instance);
		}

		public class ExceptionAdaptor : Exception, CrossBindingAdaptorType
		{
			private readonly object[] param0 = new object[0];
			private ILRuntime.Runtime.Enviorment.AppDomain appDomain;
			private IMethod iDisposable;
			private ILTypeInstance instance;

			public ExceptionAdaptor()
			{
			}

			public ExceptionAdaptor(ILRuntime.Runtime.Enviorment.AppDomain appDomain, ILTypeInstance instance)
			{
				this.appDomain = appDomain;
				this.instance = instance;
			}

			public ILTypeInstance ILInstance
			{
				get
				{
					return instance;
				}
			}
		}
	}
}