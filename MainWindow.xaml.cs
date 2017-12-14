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
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public static class StaticValues
    {
        public static  CardService cardService { get; set; } = new CardService();
        public static SetService setService { get; set; } = new SetService();
        public static List<Card> CurrentBooster { get; set; } = new List<Card>();
        public static List<Set> Sets { get; set; } = new List<Set>();

        public static void InitSets()
        {
            Sets = setService.All().Value;
            
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
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            StaticValues.InitSets();
            GenerateAndDisplayNewBoosterOfSet("M15");
            SetsButtons.ItemsSource = StaticValues.Sets;
            Console.WriteLine();
        }
        public void PrintCard(Card Card)
        {
            var url = Card.ImageUrl.OriginalString;
            Result.Text = url;
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(url); ;
            bitmapImage.EndInit();
            Visual.Source = bitmapImage;
        }
        

        private void SetButton(object sender, RoutedEventArgs e)
        {
            var SetCode = ((Button)sender).Tag.ToString();
            var booster = GetBoosterContentOfSet(SetCode);
            var cardService = StaticValues.cardService;
            StaticValues.CurrentBooster = booster;
            ItemsControl.ItemsSource = GetBIBooster(StaticValues.CurrentBooster);
            BoosterSetName.Text = SetCode;

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

        private Exceptional<Set> GetASetWithCode(string SetCode)
        {
            SetService service = new SetService();
            var result = service.Find(SetCode);
            Result.Text = result.Value.Code;
            return result;
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
            
           
            var cardService = StaticValues.cardService;
            StaticValues.CurrentBooster = booster;
            ItemsControl.ItemsSource = GetBIBooster(StaticValues.CurrentBooster);
            BoosterSetName.Text = SetCode;
            
            
        }
    }
}
