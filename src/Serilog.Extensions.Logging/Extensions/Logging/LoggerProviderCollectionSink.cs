﻿// Copyright 2019 Serilog Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Extensions.Logging
{
    class LoggerProviderCollectionSink : ILogEventSink, IDisposable
    {
        readonly LoggerProviderCollection _providers;

        public LoggerProviderCollectionSink(LoggerProviderCollection providers)
        {
            _providers = providers ?? throw new ArgumentNullException(nameof(providers));
        }

        public void Emit(LogEvent logEvent)
        {
            string categoryName = null;

            if (logEvent.Properties.TryGetValue("SourceContext", out var sourceContextProperty) &&
                sourceContextProperty is ScalarValue sourceContextValue &&
                sourceContextValue.Value is string sourceContext)
            {
                categoryName = sourceContext;
            }

            var level = LevelMapping.ToExtensionsLevel(logEvent.Level);
            var slv = new SerilogLogValues(logEvent.MessageTemplate, logEvent.Properties);

            foreach (var provider in _providers.Providers)
            {
                var logger = provider.CreateLogger(categoryName);

                logger.Log(
                    level,
                    default, 
                    slv,
                    logEvent.Exception,
                    (s, e) => s.ToString());
            }
        }

        public void Dispose()
        {
            _providers.Dispose();
        }
    }
}
