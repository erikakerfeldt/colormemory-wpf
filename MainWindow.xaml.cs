using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ColorMemory
{
    public partial class MainWindow : Window
    {
        private const int SideLength = 4;
        private const int SuccessScore = 2;
        private const int FailureScore = -1;
        private const int FlipTimeMillis = 1000;
        private const int SucessTimeMillis = 2000;

        private static readonly Color[] CardColors =
        {
            Colors.Red,
            Colors.Green,
            Colors.Blue,
            Colors.Yellow,
            Colors.Black,
            Colors.Orange,
            Colors.Magenta,
            Colors.HotPink
        };

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ViewModel();
        }

        private class ViewModel : ViewModelBase
        {
            private IEnumerable<CardViewModel> _cards;
            private int _score;
            private DateTime _startTime;
            private readonly Timer _updateTimeTimer;

            public ViewModel()
            {
                FlipCommand = new CommandViewModel("Flip", HandleFlip, HandleCanFlip);

                _updateTimeTimer = new Timer(UpdateTime, null, FlipTimeMillis, FlipTimeMillis);

                Inititialize();
            }

            public CommandViewModel FlipCommand
            {
                get;
                private set;
            }

            public IEnumerable<CardViewModel> Cards
            {
                get { return _cards; }
                private set { _cards = value; OnPropertyChanged(nameof(Cards)); }
            }

            public int FlippedCount
            {
                get { return Cards.Count(c => c.IsFlipped); }
            }

            public int RemainingCount
            {
                get { return Cards.Count(c => c.IsVisible); }
            }

            public int Score
            {
                get { return _score; }
                set { _score = value; OnPropertyChanged(nameof(Score)); }
            }

            public TimeSpan ElapsedTime
            {
                get { return DateTime.Now - _startTime; }
            }

            private void HandleFlip(object parameter)
            {
                // get card's view model
                var vm = (CardViewModel)parameter;

                // flip it!
                vm.IsFlipped = !vm.IsFlipped;

                // two cards are now flipped => do the dancing
                if (FlippedCount == 2)
                {
                    Task.Delay(SucessTimeMillis).ContinueWith(
                        HandleFlippedCards,
                        Cards.Where(c => c.IsFlipped).ToArray()
                    );
                }
            }

            private void HandleFlippedCards(Task t, object state)
            {
                App.Current.Dispatcher.Invoke(
                    () =>
                    {
                        // get the cards from state
                        var cards = (CardViewModel[])state;

                        // equal?
                        var success = cards[0].Equals(cards[1]);

                        // update score
                        Score += success ? SuccessScore : FailureScore;

                        // remove cards if success
                        if (success)
                        {
                            cards[0].IsVisible = false;
                            cards[1].IsVisible = false;
                        }

                        // else flip them back again
                        else
                        {
                            cards[0].IsFlipped = false;
                            cards[1].IsFlipped = false;
                        }

                        // if game is finished, show message
                        if (RemainingCount == 0)
                        {
                            var result = MessageBox.Show(
                                string.Format(Score > 0 ? "Well played! You scored {0} points." : "Sad! You finally got there ... Score was {0} points.", Score) + "\n\n" +
                                string.Format("Time: {0:m\\:ss}\n\n", ElapsedTime) +
                                "Play again?",
                                "Game Over",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question
                            );
                            if (result == MessageBoxResult.Yes)
                            {
                                Inititialize();
                            }
                            else
                            {
                                App.Current.Shutdown();
                            }
                        }

                        // refresh can execute state
                        CommandManager.InvalidateRequerySuggested();
                    }
                );
            }

            private bool HandleCanFlip(object parameter)
            {
                // get card's view model
                var vm = (CardViewModel)parameter;

                // can flip is not already flipped, and none or one cards are currently flipped
                return vm != null && !vm.IsFlipped && FlippedCount < 2;
            }

            private CardViewModel[] CreateCards(int sideLength)
            {
                // assert sidelength is valid
                if (sideLength < 2)
                {
                    throw new ArgumentException("Must be at least two.", nameof(sideLength));
                }

                // get grid size
                var gridSize = sideLength * sideLength;
                
                // assert there are enough colors
                if (gridSize / 2 > CardColors.Length)
                {
                    throw new InvalidOperationException("Maximum side length supported is " + (int)Math.Sqrt(CardColors.Length * 2) + ".");
                }

                // add the colors to be used to the initial color source
                var colors = new List<Color>(
                    CardColors.Take(gridSize / 2).Concat(CardColors.Take(gridSize / 2))
                );

                // init resulting card list
                var cards = new List<CardViewModel>(gridSize);

                // init random
                var random = new Random();

                // create all cards
                for (int i = 0; i < gridSize; i++)
                {
                    var ci = random.Next(colors.Count);
                    cards.Add(new CardViewModel(colors[ci]));
                    colors = new List<Color>(colors.Where((c, j) => j != ci));
                }

                // return as array
                return cards.ToArray();
            }

            private void Inititialize()
            {
                Cards = CreateCards(SideLength);
                Score = 0;
                _startTime = DateTime.Now;
            }

            private void UpdateTime(object state)
            {
                OnPropertyChanged(nameof(ElapsedTime));
            }
        }
    }
}
