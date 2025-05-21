using JetBrains.Annotations;
using LOIM.Game.Display;
using LOIM.Util;

namespace LOIM.Game;

public readonly struct OrderQuestion : IQuestion
{
    public unsafe struct CharacterOrder : IEquatable<CharacterOrder>
    {
        internal fixed char Order[ItemCount];

        [PublicAPI]
        public static CharacterOrder Sequence(ReadOnlySpan<char> sequence) =>
            Sequence(sequence[0], sequence[1], sequence[2], sequence[3]);

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

        public override string ToString()
        {
            fixed (char* ptr = Order) return new string(ptr, 0, ItemCount);
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

    public void Display(IGameDisplay display)
    {
        display.DisplayLine(Category);
        display.DisplayLine(Task);
        display.DisplayGrid(2, 2, true, Items);
    }

    public bool CheckAnswer(ReadOnlySpan<char> answer) => Order == CharacterOrder.Sequence(answer);

    public string? ValidateAnswer(ReadOnlySpan<char> answer)
    {
        return !answer.ValidateAnswer(ItemCount)
            ? $"answer must be {ItemCount} characters long and must only contain characters between 'A' and {(char)('A' + ItemCount - 1)}"
            : null;
    }
}
