using System;

namespace Hangfire.Console.Extensions
{
    [Serializable]
    public class JobFailedException : Exception
    {
        public JobFailedException() { }
        public JobFailedException(string message) : base(message) { }
        public JobFailedException(string message, Exception inner) : base(message, inner) { }
        protected JobFailedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
