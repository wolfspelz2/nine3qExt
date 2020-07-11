using System;
using System.Threading.Tasks;
using n3q.Items;
using n3q.Tools;

namespace n3q.Aspects
{
    public static class TimedExtensions
    {
        public static Timed AsTimed(this ItemStub self) { return new Timed(self); }
    }

    public class Timed : Aspect
    {
        public Timed(ItemStub item) : base(item) { }
        public override Pid GetAspectPid() => Pid.TimedAspect;

        public async Task<DateTime> CurrentTime()
        {
            var presetTime = await this.GetInt(Pid.TimedTime);
            if (presetTime == 0) {
                return DateTime.UtcNow;
            }

            return new DateTime().FromLong(presetTime);
        }

        public async Task SetCurrentTime(DateTime fakeNow)
        {
            await this.Set(Pid.TimedTime, fakeNow);
        }
    }
}
