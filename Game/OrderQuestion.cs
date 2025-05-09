using JetBrains.Annotations;
using LOIM.Util;

namespace LOIM.Game;

public readonly struct OrderQuestion
{
    public unsafe struct CharacterOrder : IEquatable<CharacterOrder>
    {
        internal fixed char Order[ItemCount];

        [PublicAPI]
        public static CharacterOrder Sequence([ValueRange('A', 'D')] char a, [ValueRange('A', 'D')] char b,
                                              [ValueRange('A', 'D')] char c, [ValueRange('A', 'D')] char d)
        {
            var ret = new CharacterOrder();
            ret.Order[0] = a;
            ret.Order[1] = b;
            ret.Order[2] = c;
            ret.Order[3] = d;

            return ret;
        }

        public readonly bool Equals(CharacterOrder other)
        {
            for (byte i = 0; i < ItemCount; i++)
                if (other.Order[i] != Order[i])
                    return false;

            return true;
        }

        public readonly override bool Equals(object? obj)
        {
            return obj is CharacterOrder other && Equals(other);
        }

        public readonly override int GetHashCode()
        {
            return Order[0].GetHashCode() ^ Order[1].GetHashCode() ^ Order[2].GetHashCode() ^ Order[3].GetHashCode();
        }

        public static bool operator ==(CharacterOrder left, CharacterOrder right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CharacterOrder left, CharacterOrder right)
        {
            return !(left == right);
        }
    }

    [PublicAPI] public const    byte           ItemCount = 4;
    [PublicAPI] public readonly string         Task;
    [PublicAPI] public readonly string[]       Items = new string[ItemCount];
    [PublicAPI] public readonly string         Category;
    [PublicAPI] public readonly CharacterOrder Order;

    [PublicAPI]
    public static OrderQuestion Parse(in string line, char delimiter = ';') => new(in line, delimiter);
    
    [PublicAPI]
    public bool Guess(CharacterOrder order) => order == Order;

    private OrderQuestion(in string repr, char delimiter = ';')
    {
        var src      = repr.AsSpan();
        var segments = src.Split(delimiter);

        segments.EnsureNext();
        Task = src[segments.Current].ToString();

        for (byte i = 0; i < ItemCount; i++)
        {
            segments.EnsureNext();
            Items[i] = src[segments.Current].ToString();
        }

        unsafe
        {
            segments.EnsureNext();
            fixed (char* ptr = Order.Order)
                src[segments.Current].CopyTo(new Span<char>(ptr, ItemCount));
        }

        segments.EnsureNext();
        Category = src[segments.Current].ToString();
    }
}
