using Taqlyn;
using Xunit;

public class SignerTests
{
    [Fact]
    public void RejectsSkHandle()
    {
        Assert.Throws<ArgumentException>(() =>
            new TaqlynClient("https://api.example.test", "app_test_x", "sk_test_not_a_key"));
    }

    [Fact]
    public void CanonicalMessageHasNoTrailingNewline()
    {
        var msg = Signer.CanonicalMessage("POST", "/v1/short-links", 1, "app_test_x", "{}"u8.ToArray());
        Assert.False(msg.EndsWith('\n'));
        Assert.StartsWith("taqlyn-v1\nPOST\n/v1/short-links\n1\napp_test_x\n", msg);
    }
}
