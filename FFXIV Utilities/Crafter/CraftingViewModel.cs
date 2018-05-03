using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Crafter
{
    public class CraftingViewModel : INotifyPropertyChanged
    {
        private CraftPattern _selectedPattern;

        public event PropertyChangedEventHandler PropertyChanged;
        
        public ObservableCollection<CraftPattern> Patterns { get; } = new ObservableCollection<CraftPattern>();
        
        public CraftPattern SelectedPattern
        {
            get
            {
                if (this._selectedPattern == null)
                {
                    this._selectedPattern = this.Patterns.FirstOrDefault();
                }

                return this._selectedPattern;
            }

            set
            {
                this._selectedPattern = value;
                this.NotifyPropertyChanged();
            }
        }

        public bool Collectible { get; set; }

        public int Count { get; set; } = 0;
        
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
