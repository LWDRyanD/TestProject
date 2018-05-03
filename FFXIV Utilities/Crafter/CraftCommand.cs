namespace Crafter
{
    using System;
    using WindowsInput.Native;

    public class CraftCommand
    {
        public VirtualKeyCode Key { get; set; }
        public VirtualKeyCode? Modifier { get; set; }
        public int BaseWait { get; set; }
        public int MinimumOffset { get; set; }
        public int MaximumOffset { get; set; }
        
        public TimeSpan GetSleepTime(Random rand)
        {
            return TimeSpan.FromSeconds(this.BaseWait) + TimeSpan.FromMilliseconds(rand.Next(this.MinimumOffset, this.MaximumOffset));
        }

        public override string ToString()
        {
            return this.Modifier.HasValue ? $"{this.Key}+{this.Modifier}" : this.Key.ToString();
        }
    }
}
