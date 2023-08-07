namespace Starlk.Console.Client;

internal sealed class Timer
{
    private double time;

    private readonly double delayTime;

    public Timer(double delayTime)
    {
        this.delayTime = delayTime;
    }

    public bool CanTick(double elapsedSeconds)
    {
        if (time <= elapsedSeconds)
        {
            time = elapsedSeconds + delayTime;
            return true;
        }

        return false;
    }
}