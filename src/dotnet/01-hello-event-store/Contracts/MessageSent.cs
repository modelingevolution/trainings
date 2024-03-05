namespace Contracts
{
    public record MessageSent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Text { get; set; }
    }
}
