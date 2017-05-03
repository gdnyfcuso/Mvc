// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Core;
using Microsoft.AspNetCore.Mvc.Internal;

namespace Microsoft.AspNetCore.Mvc.Controllers
{
    public class ControllerFactoryProvider : IControllerFactoryProvider
    {
        private readonly IControllerActivatorProvider _activatorProvider;
        private readonly Func<ControllerContext, object> _factoryCreateController;
        private readonly Action<ControllerContext, object> _factoryReleaseController;
        private readonly IControllerPropertyActivatorFactory[] _activatorProviders;

        public ControllerFactoryProvider(
            IControllerActivatorProvider activatorProvider,
            IEnumerable<IControllerFactory> controllerFactories,
            IEnumerable<IControllerPropertyActivatorFactory> propertyActivators)
        {
            _activatorProvider = activatorProvider;

            var factory = controllerFactories.FirstOrDefault();
            if (factory != null)
            {
                _factoryCreateController = factory.CreateController;
                _factoryReleaseController = factory.ReleaseController;
            }

            _activatorProviders = propertyActivators.ToArray();
        }

        public Func<ControllerContext, object> CreateControllerFactory(ControllerActionDescriptor descriptor)
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

            if (_factoryCreateController != null)
            {
                return _factoryCreateController;
            }

            var controllerActivator = _activatorProvider.CreateActivator(descriptor);
            var propertyActivators = GetPropertiesToActivate(descriptor);
            object CreateController(ControllerContext controllerContext)
            {
                var controller = controllerActivator(controllerContext);
                for (var i = 0; i < propertyActivators.Length; i++)
                {
                    var propertyActivator = propertyActivators[i];
                    propertyActivator(controllerContext, controller);
                }

                return controller;
            }

            return CreateController;
        }
        public Action<ControllerContext, object> CreateControllerReleaser(ControllerActionDescriptor descriptor)
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

            if (_factoryReleaseController != null)
            {
                return _factoryReleaseController;
            }

            return _activatorProvider.CreateReleaser(descriptor);
        }

        private Action<ControllerContext, object>[] GetPropertiesToActivate(ControllerActionDescriptor actionDescriptor)
        {
            var propertyActivators = new Action<ControllerContext, object>[_activatorProviders.Length];
            for (var i = 0; i < _activatorProviders.Length; i++)
            {
                var activatorProvider = _activatorProviders[i];
                propertyActivators[i] = activatorProvider.GetPropertyActivator(actionDescriptor);
            }

            return propertyActivators;
        }
    }
}
