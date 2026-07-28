using System.Runtime.CompilerServices;

// LaunchStrategy'lerin BuildCommand'i (internal) test projesinden doğrudan çağrılabilsin diye —
// gerçek bir process başlatmadan komutun ne üreteceğini doğrulamak için.
[assembly: InternalsVisibleTo("Gamora.Core.Tests")]
