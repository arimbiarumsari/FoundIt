using FoundIt.Data;

namespace FoundIt.Api.Tests.Data;

public sealed class PasswordHasherTests {
    [Fact]
    public void Hash_ThenVerify_Works() {
        var hash = PasswordHasher.Hash("Passw0rd1");

        Assert.NotEqual("Passw0rd1", hash);
        Assert.True(PasswordHasher.Verify("Passw0rd1", hash));
        Assert.False(PasswordHasher.Verify("SalahPass1", hash));
    }

    [Fact]
    public void Hash_SamePasswordTwice_ProducesDifferentHashes() {
        Assert.NotEqual(PasswordHasher.Hash("Passw0rd1"), PasswordHasher.Hash("Passw0rd1"));
    }
}