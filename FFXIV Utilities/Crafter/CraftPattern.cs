namespace Crafter
{
    using System.Collections.Generic;
    using System.Linq;

    public class CraftPattern
    {
        public string Name { get; set; }
        public List<CraftCommand> Commands { get; set; }
        public bool IsValid
        {
            get
            {
                return !string.IsNullOrWhiteSpace(this.Name) && this.Commands != null && this.Commands.All(c => c != null);
            }
        }
    }
}
