﻿using System;
using System.Collections.Generic;
using System.Text;
using Paillave.Etl.Core.Streams;

namespace Paillave.Etl
{
    public class StreamProcessDefinition<TConfig> : IStreamProcessDefinition<TConfig>
    {
        private Action<IStream<TConfig>> _defineJob;
        public StreamProcessDefinition(string name, Action<IStream<TConfig>> defineJob)
        {
            this.Name = name;
            _defineJob = defineJob;
        }
        public StreamProcessDefinition(Action<IStream<TConfig>> defineJob) : this(null, defineJob) { }
        public string Name { get; }
        public void DefineProcess(IStream<TConfig> rootStream) => _defineJob(rootStream);
    }
}
