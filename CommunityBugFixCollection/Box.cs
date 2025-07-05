using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityBugFixCollection
{
    internal sealed class Box<T>
        where T : struct
    {
        public T Value { get; set; }

        public Box(T value)
        {
            Value = value;
        }

        public Box()
        { }

        public static implicit operator Box<T>(T value) => new(value);

        public static implicit operator T(Box<T> box) => box.Value;
    }
}