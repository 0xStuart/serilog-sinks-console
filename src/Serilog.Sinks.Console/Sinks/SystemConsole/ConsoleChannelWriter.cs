// Copyright 2017 Serilog Contributors
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

using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.SystemConsole.Platform;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.IO;
using System.Text;
using System.Threading.Channels;

namespace Serilog.Sinks.SystemConsole
{
    class ConsoleChannelWriter : ILogEventSink
    {
        readonly LogEventLevel? _standardErrorFromLevel;
        readonly ConsoleTheme _theme;
        readonly ITextFormatter _formatter;
        readonly object _syncRoot;
        public Channel<(LogEvent, byte[])> _channelWriter { get; private set; }
        private MemoryStream ms;
        private TextWriter _textWriter;


        public ConsoleChannelWriter(Channel<(LogEvent, byte[])> textWriter,
            ConsoleTheme theme,
            ITextFormatter formatter,
            LogEventLevel? standardErrorFromLevel,
            object syncRoot)
        {
            _channelWriter = textWriter ?? throw new ArgumentNullException(nameof(theme));
            _standardErrorFromLevel = standardErrorFromLevel;
            _theme = theme ?? throw new ArgumentNullException(nameof(theme));
            _formatter = formatter;
            _syncRoot = syncRoot ?? throw new ArgumentNullException(nameof(syncRoot));
            
            ms = new MemoryStream();
            _textWriter = new StreamWriter(ms);
        }

        public void Emit(LogEvent logEvent)
        {
            lock (_syncRoot)
            {
                _formatter.Format(logEvent, _textWriter);
                _textWriter.Flush();
                _channelWriter.Writer.WriteAsync((logEvent, ms.ToArray()));
                ms.Position = 0;
            }
        }
    }
}