using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using System;

namespace Frame
{
	[ILAdapter]
	public class EventArgsClassInheritanceAdaptor : CrossBindingAdaptor
	{
		public override Type AdaptorType
		{
			get
			{
				return typeof(EventArgsAdaptor);
			}
		}

		public override Type BaseCLRType
		{
			get
			{
				return typeof(System.EventArgs);
			}
		}

		public override Type[] BaseCLRTypes
		{
			get
			{
				return null;
			}
		}

		public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
		{
			return new EventArgsAdaptor(appdomain, instance);
		}

		public class EventArgsAdaptor : EventArgs, CrossBindingAdaptorType
		{
			private ILRuntime.Runtime.Enviorment.AppDomain appdomain;
			private ILTypeInstance instance;

			public EventArgsAdaptor()
			{
			}

			public EventArgsAdaptor(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
			{
				this.appdomain = appdomain;
				this.instance = instance;
			}

			public ILTypeInstance ILInstance { get { return instance; } }
		}
	}
}