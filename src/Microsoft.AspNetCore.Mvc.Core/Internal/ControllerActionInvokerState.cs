// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    public class ControllerActionInvokerState
    {
        public ControllerActionInvokerState(
            FilterItem[] filters,
            Func<ControllerContext, object> controllerFactory,
            Action<ControllerContext, object> controllerReleaser,
            Func<ControllerContext, object, Task> propertyBinderFactory,
            ObjectMethodExecutor actionMethodExecutor)
        {
            ControllerFactory = controllerFactory;
            ControllerReleaser = controllerReleaser;
            PropertyBinderFactory = propertyBinderFactory;
            Filters = filters;
            ActionMethodExecutor = actionMethodExecutor;
        }

        public FilterItem[] Filters { get; }

        public Func<ControllerContext, object> ControllerFactory { get; }

        public Action<ControllerContext, object> ControllerReleaser { get; }

        public Func<ControllerContext, object, Task> PropertyBinderFactory { get; }

        public ObjectMethodExecutor ActionMethodExecutor { get; }
    }
}
