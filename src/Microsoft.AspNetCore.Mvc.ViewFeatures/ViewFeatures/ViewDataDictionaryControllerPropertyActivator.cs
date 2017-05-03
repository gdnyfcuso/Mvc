// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures
{
    public class ViewDataDictionaryControllerPropertyActivator : IControllerPropertyActivatorFactory
    {
        private readonly IModelMetadataProvider _modelMetadataProvider;
        private readonly Func<Type, PropertyActivator<ControllerContext>[]> _getPropertiesToActivate;

        public ViewDataDictionaryControllerPropertyActivator(IModelMetadataProvider modelMetadataProvider)
        {
            _modelMetadataProvider = modelMetadataProvider;
            _getPropertiesToActivate = GetPropertiesToActivate;
        }

        public Action<ControllerContext, object> GetPropertyActivator(ControllerActionDescriptor actionDescriptor)
        {
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
            var activators = PropertyActivator<ControllerContext>.GetPropertiesToActivate(
                type,
                typeof(ViewDataDictionaryAttribute),
                p => new PropertyActivator<ControllerContext>(p, GetViewDataDictionary));

            return activators;
        }

        private ViewDataDictionary GetViewDataDictionary(ControllerContext context)
        {
            return new ViewDataDictionary(
                _modelMetadataProvider,
                context.ModelState);
        }
    }
}
