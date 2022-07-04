using System.Threading.Tasks;
using Hippo.Application.Accounts.Commands;
using Hippo.Application.Common.Exceptions;
using Xunit;

namespace Hippo.FunctionalTests.Application.Accounts.Commands;

public class CreateTokenTests : TestBase
{
    [Fact]
    public async Task ShouldRaiseLoginFailedException()
    {
        var username = RandomString(10);
        var password = "Passw0rd!";

        await SendAsync(new CreateAccountCommand
        {
            Username = username,
            Password = password
        });

        await Assert.ThrowsAsync<LoginFailedException>(
            async () => await SendAsync(
                new CreateTokenCommand
                {
                    Username = username,
                    Password = RandomString(10),
                }
            )
        );
    }
}
