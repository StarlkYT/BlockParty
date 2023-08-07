using Microsoft.Extensions.Logging;
using Starlk.Console.Components;
using Starlk.Console.Networking.Packets.Play;

namespace Starlk.Console;

internal sealed class Session
{
    public bool HasStarted { get; set; }

    private int countDownSecond = 10;
    private bool countDownStarted;

    private readonly ILogger<Session> logger;
    private readonly GraniteServer server;

    public Session(ILogger<Session> logger, GraniteServer server)
    {
        this.logger = logger;
        this.server = server;
    }

    public async Task StartCountDownAsync()
    {
        if (countDownStarted)
        {
            return;
        }

        countDownStarted = true;

        while (countDownSecond != -1)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));

            if (countDownSecond != 0)
            {
                if (countDownStarted)
                {
                    await server.SendChatMessageAsync(Chat.Create($"{countDownSecond} seconds remaining!",
                        color: "green",
                        bold: true));
                    await server.PlaySoundEffectAsync(SoundEffect.RandomClick);
                }
                else
                {
                    break;
                }
            }
            else
            {
                await server.PlaySoundEffectAsync(SoundEffect.RandomExplode);
                HasStarted = true;

                logger.LogInformation("Session started");
            }

            countDownSecond--;
        }

        if (countDownStarted)
        {
            StartSession();
        }
        else
        {
            countDownSecond = 10;
        }
    }

    public async Task CancelAsync()
    {
        if (countDownStarted)
        {
            countDownStarted = false;
        }
        else
        {
            return;
        }

        countDownSecond = 10;

        await server.SendChatMessageAsync(Chat.Create("Need more players!", color: "red"));
        await server.PlaySoundEffectAsync(SoundEffect.FireIgnite);
        HasStarted = false;

        logger.LogInformation("Session cancelled");
    }

    private void StartSession()
    {
        var seconds = 4;

        _ = Task.Run(async () =>
        {
            try
            {
                while (HasStarted)
                {
                    await server.UpdateSlotAsync();

                    while (seconds != 0 && HasStarted)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        seconds--;

                        await server.SendChatMessageAsync(
                            Chat.Create($"§9{seconds}",
                                bold: true), ChatPosition.AboveHotBar);

                        await server.PlaySoundEffectAsync(SoundEffect.RandomClick);
                    }

                    await server.DestroyFloor();
                    await Task.Delay(TimeSpan.FromSeconds(3));
                    await server.BuildRandomFloorAsync();

                    seconds = 4;
                }

                await server.UpdateSlotAsync(true);
            }
            catch (Exception exception)
            {
                logger.LogError("An error has occured {Message}", exception.Message);
                await server.KickAllAsync(Chat.Create("An error has occured!"));
            }
        });
    }
}