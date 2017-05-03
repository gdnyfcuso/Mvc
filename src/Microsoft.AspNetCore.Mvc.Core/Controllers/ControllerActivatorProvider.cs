// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Mvc.Controllers
{
    /// <summary>
    /// Provides methods to create an MVC controller.
    /// </summary>
    public class ControllerActivatorProvider : IControllerActivatorProvider
    {
        private static readonly Func<Type, ObjectFactory> _createFactory = (type) => ActivatorUtilities.CreateFactory(type, Type.EmptyTypes);
        private static readonly Action<ControllerContext, object> _dispose = Dispose;
        private readonly Func<ControllerContext, object> _controllerActivatorCreate;
        private readonly Action<ControllerContext, object> _controllerActivatorRelease;

        public ControllerActivatorProvider(IEnumerable<IControllerActivator> activators)
        {
            var activator = activators.FirstOrDefault();
            if  (activator != null)
            {
                _controllerActivatorCreate = activator.Create;
                _controllerActivatorRelease = activator.Release;
            }
        }

        public Func<ControllerContext, object> CreateActivator(ControllerActionDescriptor descriptor)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            var controllerType = descriptor.ControllerTypeInfo?.AsType();
            if (controllerType == null)
            {
                throw new ArgumentException(Resources.FormatPropertyOfTypeCannotBeNull(
                    nameof(descriptor.ControllerTypeInfo),
                    nameof(descriptor)),
                    nameof(descriptor));
            }

            if (_controllerActivatorCreate != null)
            {
                return _controllerActivatorCreate;
            }

            var typeActivator = ActivatorUtilities.CreateFactory(controllerType, Type.EmptyTypes);
            return controllerContext => typeActivator(controllerContext.HttpContext.RequestServices, arguments: null);
        }

        public Action<ControllerContext, object> CreateReleaser(ControllerActionDescriptor descriptor)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            if (_controllerActivatorRelease != null)
            {
                return _controllerActivatorRelease;
            }

            if (typeof(IDisposable).GetTypeInfo().IsAssignableFrom(descriptor.ControllerTypeInfo))
            {
                return _controllerActivatorRelease;
            }

            return null;
        }

        private static void Dispose(ControllerContext context, object controller)
        {
            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

            ((IDisposable)controller).Dispose();
        }
    }
}
