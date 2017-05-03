// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    public static class ControllerPropertyBinderFactory
    {
        public static Func<ControllerContext, object, Task> CreateBinder(
            ParameterBinder parameterBinder,
            IModelMetadataProvider modelMetadataProvider,
            ControllerActionDescriptor actionDescriptor)
        {
            if (parameterBinder == null)
            {
                throw new ArgumentNullException(nameof(parameterBinder));
            }

            if (modelMetadataProvider == null)
            {
                throw new ArgumentNullException(nameof(modelMetadataProvider));
            }

            if (actionDescriptor == null)
            {
                throw new ArgumentNullException(nameof(actionDescriptor));
            }

            var properties = actionDescriptor.BoundProperties;
            if (properties == null || properties.Count == 0)
            {
                return null;
            }

            var controllerType = actionDescriptor.ControllerTypeInfo.AsType();
            var metadata = new ModelMetadata[properties.Count];
            for (var i = 0; i < properties.Count; i++)
            {
                metadata[i] = modelMetadataProvider.GetMetadataForProperty(controllerType, properties[i].Name);
            }

            return Bind;

            async Task Bind(ControllerContext controllerContext, object controller)
            {
                if (controllerContext == null)
                {
                    throw new ArgumentNullException(nameof(controllerContext));
                }

                if (controller == null)
                {
                    throw new ArgumentNullException(nameof(controller));
                }

                var valueProvider = await CompositeValueProvider.CreateAsync(controllerContext);
                for (var i = 0; i < properties.Count; i++)
                {
                    var result = await parameterBinder.BindModelAsync(controllerContext, valueProvider, properties[i]);
                    if (result.IsModelSet)
                    {
                        PropertyValueSetter.SetValue(metadata[i], controller, result.Model);
                    }
                }
            }
        }
    }
}
