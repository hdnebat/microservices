﻿using System;
using System.Threading.Tasks;
using Framework.Api.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Framework.Api.Middleware {
    public class CorrelationIdMiddleware {
        private readonly RequestDelegate _next;
        private readonly CorrelationIdOptions _options;

        public CorrelationIdMiddleware(RequestDelegate next, IOptions<CorrelationIdOptions> options) {
            if (options == null) throw new ArgumentNullException(nameof(options));

            _next = next ?? throw new ArgumentNullException(nameof(next));

            _options = options.Value;
        }

        public Task Invoke(HttpContext context) {
            if (context.Request.Headers.TryGetValue(_options.Header, out var correlationId))
                context.TraceIdentifier = correlationId;

            if (_options.IncludeInResponse)
                context.Response.OnStarting(() => {
                    context.Response.Headers.Add(_options.Header, new[] {context.TraceIdentifier});
                    return Task.CompletedTask;
                });

            return _next(context);
        }
    }
}