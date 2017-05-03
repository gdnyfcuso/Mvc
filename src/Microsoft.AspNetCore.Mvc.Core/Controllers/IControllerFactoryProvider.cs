// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Mvc.Controllers
{
    public interface IControllerFactoryProvider
    {
        /// <summary>
        /// Creates a factory for producing Razor pages for the specified <paramref name="descriptor"/>.
        /// </summary>
        /// <param name="descriptor">The <see cref="ControllerActionDescriptor"/>.</param>
        /// <returns>The Razor page factory.</returns>
        Func<ControllerContext, object> CreateControllerFactory(ControllerActionDescriptor descriptor);

        /// <summary>
        /// Releases a Razor page.
        /// </summary>
        /// <param name="descriptor">The <see cref="ControllerActionDescriptor"/>.</param>
        /// <returns>The delegate used to release the created page.</returns>
        Action<ControllerContext, object> CreateControllerReleaser(ControllerActionDescriptor descriptor);

    }
}
