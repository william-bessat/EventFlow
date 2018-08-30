﻿// The MIT License (MIT)
// 
// Copyright (c) 2015-2016 Rasmus Mikkelsen
// Copyright (c) 2015-2016 eBay Software Foundation
// https://github.com/rasmus/EventFlow
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Linq;
using System.Reflection;
using EventFlow.Configuration;
using EventFlow.Configuration.Decorators;

namespace EventFlow.Core.IoC
{
    internal interface IRegistration
    {
        Lifetime Lifetime { get; }
        object Create(IResolverContext resolverContext, Type[] genericTypeArguments);
    }

    internal class Registration : IRegistration
    {
        private readonly Type _serviceType;
        private readonly IFactory _factory;
        private readonly IDecoratorService _decoratorService;
        public Lifetime Lifetime { get; }

        public Registration(
            Type serviceType,
            Lifetime lifetime,
            IFactory factory,
            IDecoratorService decoratorService)
        {
            _serviceType = serviceType;
            _factory = factory;
            _decoratorService = decoratorService;
            Lifetime = lifetime;
        }

        public object Create(IResolverContext resolverContext, Type[] genericTypeArguments)
        {
            var serviceType = genericTypeArguments.Any() && _serviceType.GetTypeInfo().IsGenericType
                ? _serviceType.MakeGenericType(genericTypeArguments)
                : _serviceType;

            return CreateDecorated(resolverContext, serviceType, genericTypeArguments);
        }

        private object CreateDecorated(IResolverContext resolverContext, Type serviceType, Type[] genericTypeArguments)
        {
            var service = _factory.Create(resolverContext, genericTypeArguments);
            var decoratedService = _decoratorService.Decorate(
                serviceType,
                service,
                resolverContext);
            return decoratedService;
        }
    }
}