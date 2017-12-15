using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using MtgApiManager;
using MtgApiManager.Lib.Core;
using MtgApiManager.Lib.Model;
using MtgApiManager.Lib.Service;

namespace MagicTheGatheringApp
{
    public static class StaticValues
    {
        public static CardService cardService { get; set; } = new CardService();
        public static SetService setService { get; set; } = new SetService();
        public static List<Card> CurrentBooster { get; set; } = new List<Card>();
        public static List<Set> Sets { get; set; } = new List<Set>();
        public static List<SetWithYear> SetsYear { get; set; } = new List<SetWithYear>();
        public static List<Card> CurrentSearchResults { get; set; } = new List<Card>();
        public static int VisualIndex { get; set; } = 1;
        

        public static void InitSets()
        {
            var allSets = setService.All().Value;
            var boosterableSets = new List<Set>();
            foreach (Set set in allSets)
            {
                if (set.Booster != null)
                {
                    boosterableSets.Add(set);
                }
            }
            Sets = boosterableSets;
            Sets.Sort((x, y) => DateTime.Compare(DateTime.Parse(x.ReleaseDate), DateTime.Parse(y.ReleaseDate)));
            foreach (Set seta in Sets)
            {
                SetsYear.Add(new SetWithYear(seta));
            }

        }
    }
    public class CardWithBImage
    {
        public Card Card { get; set; }
        public BitmapImage BImage { get; set; }

        public CardWithBImage(Card card)
        {
            Card = card;
            BImage = GetBitmapImage(card);
        }

        public BitmapImage GetBitmapImage(Card card)
        {
            var url = card.ImageUrl.OriginalString;
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(url); ;
            bitmapImage.EndInit();
            return bitmapImage;
        }
        

    }
    public class SetWithYear
    {
        public Set Set { get; set; }
        public String Year { get; set; } = "";

        public SetWithYear(Set set)
        {
            Set = set;
            Year = Set.ReleaseDate.Substring(0, 4);
        }
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            
            InitializeComponent();
            StaticValues.InitSets();
            SetsButtons.ItemsSource = StaticValues.SetsYear;
        }
        public void PrintCard(Card Card)
        {
            var url = Card.ImageUrl.OriginalString;
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(url); ;
            bitmapImage.EndInit();
            SearchVisual.Source = bitmapImage;
            SearchEdition.Text = Card.SetName;
        }

        private void SetButton(object sender, RoutedEventArgs e)
        {
            var SetCode = ((Button)sender).Tag.ToString();
            var booster = GetBoosterContentOfSet(SetCode);
            var cardService = StaticValues.cardService;
            StaticValues.CurrentBooster = booster;
            ItemsControl.ItemsSource = GetBIBooster(StaticValues.CurrentBooster);
            BoosterSetName.Text = SetCode;
            /*
            if (booster[0] != null && booster[0].Set != null)
            {
                var set = StaticValues.setService.Find(booster[0].Set);
                if (set.Value.Name != null)
                {
                    BoosterSetName.Text = set.Value.Name;
                }
            }*/
        }

        private List<String> GetSetsNames()
        {
            SetService service = new SetService();
            var result = service.All();
            List<String> setsNames = new List<String>();
            foreach (Set set in result.Value)
            {
                setsNames.Add(set.Code);
            }
            return setsNames;
        }

        private List<Card> GetBoosterContentOfSet(string SetName)
        {
            SetService service = StaticValues.setService;
            var result = service.GenerateBooster(SetName);
            List<Card> cards = new List<Card>();
            foreach (Card card in result.Value)
            {
                cards.Add(card);
            }
            return cards;
        }

        private List<CardWithBImage> GetBIBooster(List<Card> booster)
        {
            List<CardWithBImage> nBooster = new List<CardWithBImage>();
            foreach (Card card in booster)
            {
                nBooster.Add(new CardWithBImage(card));
            }
            return nBooster;
        }

        public void GenerateAndDisplayNewBoosterOfSet(string SetCode)
        {
            var booster = GetBoosterContentOfSet(SetCode);
            if (booster != null)
            {
                var cardService = StaticValues.cardService;
                var printableCards = new List<Card>();
                foreach (Card card in booster)
                {
                    if (card.ImageUrl != null)
                    {
                        printableCards.Add(card);
                    }
                }
                StaticValues.CurrentBooster = printableCards;
                ItemsControl.ItemsSource = GetBIBooster(StaticValues.CurrentBooster);
                var set = StaticValues.setService.Find(SetCode);
                BoosterSetName.Text = set.Value.Name ;
            }  
        }

        public List<Card> GetCardByName(string Cardname)
        {

            var result = StaticValues.cardService.Where(x => x.Name, Cardname).All().Value;
            return result;
        }

        public BitmapImage GetBImage(string cardUrl)
        {
            var url = cardUrl;
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(url); ;
            bitmapImage.EndInit();
            return bitmapImage;
        }

        private void SubmitButtonClick(object sender, RoutedEventArgs e)
        {
            List<Card> cardFound = GetCardByName(UserInput.Text);
            List<Card> printableCard = new List<Card>();
            foreach (Card card in cardFound)
            {
                if (card.ImageUrl != null)
                {
                    printableCard.Add(card);
                }
            }
            SearchNResultsPrintable.Text = "/  " + (printableCard.Count).ToString();
            if (printableCard.Count != 0)
            {
                PrintCard(printableCard[0]);
                SearchEdition.Text = printableCard[0].SetName;
            } else
            {
                Result.Text = "No Cards were found";
            }
            StaticValues.CurrentSearchResults = printableCard;
            CurrentIndex.Text = "1";
            StaticValues.VisualIndex = 1;

        }

        private void NextClick(object sender, RoutedEventArgs e)
        {
            int currentIndex = int.Parse(CurrentIndex.Text);
            Result.Text = currentIndex.ToString();
            Result.Text = StaticValues.CurrentSearchResults.Count.ToString();
            if (currentIndex < StaticValues.CurrentSearchResults.Count)
            {
                PrintCard(StaticValues.CurrentSearchResults[currentIndex]);
                StaticValues.VisualIndex += 1;
                CurrentIndex.Text = StaticValues.VisualIndex.ToString();
            }
        }

        private void PrevClick(object sender, RoutedEventArgs e)
        {
            int currentIndex = int.Parse(CurrentIndex.Text);
            if (currentIndex > 1)
            {
                PrintCard(StaticValues.CurrentSearchResults[currentIndex - 2]);
                StaticValues.VisualIndex -= 1;
                CurrentIndex.Text = StaticValues.VisualIndex.ToString();
            }
        }

        private void SubmitKey(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
               Result.Text = "Submited";
               SubmitButtonClick(sender, e);
            }
        }
    }
}
