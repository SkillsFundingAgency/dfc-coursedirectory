using System;

namespace Dfc.CourseDirectory.Core.Models
{
    /// <summary>
    /// Represents the day, month and year elements from a GDS Date Input component.
    /// </summary>
    /// <remarks>
    /// Contains either a valid <see cref="DateTime"/> or <see cref="InvalidDateInputReasons"/>.
    /// </remarks>
    public sealed class DateInput
    {
        private readonly DateTime? _value;

        public DateInput(DateTime value)
        {
            _value = value;
            InvalidReasons = InvalidDateInputReasons.None;
        }

        public DateInput(InvalidDateInputReasons invalidReasons)
        {
            if (invalidReasons == InvalidDateInputReasons.None)
            {
                throw new ArgumentOutOfRangeException(nameof(invalidReasons));
            }

            InvalidReasons = invalidReasons;
        }

        public InvalidDateInputReasons InvalidReasons { get; }

        public bool IsValid => _value.HasValue;

        public static implicit operator DateInput(DateTime dt) => new DateInput(dt);

        public static implicit operator DateInput(DateTime? dt) => dt.HasValue ? new DateInput(dt.Value) : null;

        public DateTime Value
        {
            get
            {
                if (!IsValid)
                {
                    throw new InvalidOperationException("No value.");
                }

                return _value.Value;
            }
        }

        public bool TryGetValue(out DateTime value)
        {
            if (_value.HasValue)
            {
                value = _value.Value;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }
    }

    public static class DateInputExtensions
    {
        public static DateTime? ToDateTime(this DateInput dateInput) =>
            dateInput != null && dateInput.TryGetValue(out var value) ? (DateTime?)value : null;
    }

    [Flags]
    public enum InvalidDateInputReasons
    {
        None = 0,
        InvalidDate = 1,
        MissingDay = 2,
        MissingMonth = 4,
        MissingYear = 8
    }
}
