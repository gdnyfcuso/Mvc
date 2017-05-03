// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Core;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    public class DefaultControllerPropertyActivator : IControllerPropertyActivatorFactory
    {
        public Action<ControllerContext, object> GetPropertyActivator(ControllerActionDescriptor actionDescriptor)
        {
            if (actionDescriptor == null)
            {
                throw new ArgumentNullException(nameof(actionDescriptor));
            }

            var controllerType = actionDescriptor.ControllerTypeInfo?.AsType();
            if (controllerType == null)
            {
                throw new ArgumentException(Resources.FormatPropertyOfTypeCannotBeNull(
                    nameof(actionDescriptor.ControllerTypeInfo),
                    nameof(actionDescriptor)),
                    nameof(actionDescriptor));
            }

            var propertiesToActivate = GetPropertiesToActivate(controllerType);
            void Activate(ControllerContext controllerContext, object controller)
            {
                for (var i = 0; i < propertiesToActivate.Length; i++)
                {
                    var activateInfo = propertiesToActivate[i];
                    activateInfo.Activate(controller, controllerContext);
                }
            }

            return Activate;
        }

        private PropertyActivator<ControllerContext>[] GetPropertiesToActivate(Type type)
        {
            IEnumerable<PropertyActivator<ControllerContext>> activators;
            activators = PropertyActivator<ControllerContext>.GetPropertiesToActivate(
                type,
                typeof(ActionContextAttribute),
                p => new PropertyActivator<ControllerContext>(p, c => c));

            activators = activators.Concat(PropertyActivator<ControllerContext>.GetPropertiesToActivate(
                type,
                typeof(ControllerContextAttribute),
                p => new PropertyActivator<ControllerContext>(p, c => c)));

            return activators.ToArray();
        }
    }
}
