/*
 * xPilot: X-Plane pilot client for VATSIM
 * Copyright (C) 2019-2021 Justin Shannon
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see http://www.gnu.org/licenses/.
*/
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using SimpleInjector;

namespace Vatsim.Xpilot.Common
{
    public static class AutomaticFactorExtensions
    {
        public static void RegisterFactory<TFactory>(this Container container)
        {
            if (!typeof(TFactory).IsInterface)
            {
                throw new ArgumentException(typeof(TFactory).Name + " is not an interface");
            }

            container.ResolveUnregisteredType += (s, e) => 
            {
                if (e.UnregisteredServiceType == typeof(TFactory))
                {
                    e.Register(Expression.Constant(value: CreateFactory(typeof(TFactory), container), type: typeof(TFactory)));
                }
            };
        }

        private static object CreateFactory(Type factoryType, Container container)
        {
            var proxy = new AutomaticFactoryProxy(factoryType, container);
            return proxy.GetTransparentProxy();
        }

        private sealed class AutomaticFactoryProxy : RealProxy
        {
            private readonly Type factoryType;
            private readonly Container container;

            public AutomaticFactoryProxy(Type factoryType, Container container)
                : base(factoryType)
            {
                this.factoryType = factoryType;
                this.container = container;
            }

            public override IMessage Invoke(IMessage msg)
            {
                if (msg is IMethodCallMessage)
                {
                    return InvokeFactory(msg as IMethodCallMessage);
                }

                return msg;
            }

            private IMessage InvokeFactory(IMethodCallMessage msg)
            {
                if (msg.MethodName == "GetType")
                {
                    return new ReturnMessage(factoryType, null, 0, null, msg);
                }

                if (msg.MethodName == "ToString")
                {
                    return new ReturnMessage(factoryType.Name, null, 0, null, msg);
                }

                var method = (MethodInfo)msg.MethodBase;
                object instance = container.GetInstance(method.ReturnType);
                return new ReturnMessage(instance, null, 0, null, msg);
            }
        }
    }
}
