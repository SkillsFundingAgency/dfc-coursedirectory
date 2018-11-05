using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Dfc.CourseDirectory.Common
{
    public abstract class ValueObject<T> where T : ValueObject<T>
    {
        public static bool operator !=(ValueObject<T> a, ValueObject<T> b)
        {
            return !(a == b);
        }

        public static bool operator ==(ValueObject<T> a, ValueObject<T> b)
        {
            if (a is null && b is null)
                return true;

            if (a is null || b is null)
                return false;

            return a.Equals(b);
        }

        public override bool Equals(object obj)
        {
            var valueObject = obj as ValueObject<T>;

            if (valueObject is null)
                return false;

            if (GetType() != obj.GetType())
                return false;

            var tuples = GetEqualityComponents()
                .Zip(valueObject.GetEqualityComponents(), (item1, item2) => Tuple.Create(item1, item2));

            foreach (var tuple in tuples)
            {
                if (!InternalEquals(tuple.Item1, tuple.Item2))
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return GetEqualityComponents()
                .Aggregate(1, (current, obj) =>
                {
                    unchecked
                    {
                        return current * 23 + (obj?.GetHashCode() ?? 0);
                    }
                });
        }

        internal static bool InternalEquals(object a, object b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (a is null || b is null)
                return false;

            if (typeof(IEnumerable).IsAssignableFrom(a.GetType()) && a.GetType() != typeof(string)
             && typeof(IEnumerable).IsAssignableFrom(b.GetType()) && b.GetType() != typeof(string))
            {
                var a1 = ((IEnumerable)a).Cast<object>().ToArray();
                var b1 = ((IEnumerable)b).Cast<object>().ToArray();

                if (a1.Length > 0 && a1.Length != b1.Length)
                    return false;

                for (int i = 0; i < a1.Length; i++)
                {
                    if (!InternalEquals(a1[i], b1[i]))
                        return false;
                }

                return true;
            }

            return a.Equals(b);
        }

        protected abstract IEnumerable<object> GetEqualityComponents();
    }
}