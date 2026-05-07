using MissedTeacherRate.Models;

namespace MissedTeacherRate.Algorithms.COMAS.Models
{
    public class Trust<T, V> where T : Person where V : Person
    {
        public T First { get; }

        public V Second { get; }

        public double? Value { get; set; }

        public Trust(T first, V second)
        {
            First = first;
            Second = second;
        }

        // Trust is equal if it connects the same two persons, regardless of order
        public override bool Equals(object? obj)
        {
            if (obj is not Trust<T, V> other) return false;
            return First.Equals(other.First) && Second.Equals(other.Second) ||
                   First.Equals(other.Second) && Second.Equals(other.First);
        }

        public override int GetHashCode()
        {
            // Order-independent hash code
            int hash1 = First.GetHashCode();
            int hash2 = Second.GetHashCode();
            return hash1 ^ hash2;
        }

        public static bool operator ==(Trust<T, V> left, Trust<T, V> right)
        {
            if (left is null && right is null) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        public static bool operator !=(Trust<T, V> left, Trust<T, V> right) => !(left == right);

        // Checks if this trust is between the two given persons (order-insensitive)
        public bool IsBetween(Person p1, Person p2)
        {
            return First.Equals(p1) && Second.Equals(p2) ||
                   First.Equals(p2) && Second.Equals(p1);
        }
    }
}
