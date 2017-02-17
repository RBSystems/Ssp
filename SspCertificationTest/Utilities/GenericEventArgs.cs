using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace SspCertificationTest.Utilities
{
    public class GenericEventArgs<T> : EventArgs
    {
        public T Value { get; private set; }
        public GenericEventArgs(T arg)
        {
            Value = arg;
        }
    }

    public class GenericEventArgs<T, Y> : EventArgs
    {
        public T Target { get; private set; }
        public Y Value { get; private set; }
        public GenericEventArgs(T target, Y value)
        {
            Target = target;
            Value = value;
        }
    }
}