using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ColorMemory
{
    public class CardViewModel : ViewModelBase
    {
        private Color _color;
        private bool _isFlipped;
        private bool _isVisible = true;

        public CardViewModel(Color color)
        {
            _color = color;
        }

        public Brush Background
        {
            get { return _isFlipped ? new SolidColorBrush(_color) : null; }
        }

        public bool IsFlipped
        {
            get { return _isFlipped; }
            set { _isFlipped = value; OnPropertyChanged(nameof(IsFlipped), nameof(Background)); }
        }

        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                _isVisible = value;

                if (!value)
                {
                    _isFlipped = false;
                }

                OnPropertyChanged(nameof(IsVisible), nameof(IsFlipped));
            }
        }

        public override bool Equals(object obj)
        {
            var other = obj as CardViewModel;
            return other != null && _color == other._color;
        }

        public override int GetHashCode()
        {
            return _color.GetHashCode();
        }
    }
}
