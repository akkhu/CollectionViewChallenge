using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CollectionViewChallenge.Extensions;
using CollectionViewChallenge.Models;
using CollectionViewChallenge.ValueConverters;
using Xamarin.Forms;

namespace CollectionViewChallenge.Views
{
    public class FeedPreferencesPage : ContentPage
    {
        public static Color QantasLightGray = Color.FromRgb(243, 243, 243);
        public static Color QantasRed = Color.FromHex("E80000");
        public static Color QantasCyan = Color.FromHex("7ADFDD");
        public static Color QantasText = Color.FromHex("393939");
        
        public static string RegularFont =>
            Device.RuntimePlatform == Device.iOS
                ? "Ciutadella-Rg"
                : "ciutadella_regular.ttf#Regular";

        public static string MediumFont =>
            Device.RuntimePlatform == Device.iOS
                ? "Ciutadella-Md"
                : "ciutadella_medium.ttf#Regular";

        public CollectionView CollectionView;
        public Button SaveButton;
        public ObservableCollection<FeedCategory> FeedCategories;

        public FeedPreferencesPage()
        {
            Visual = VisualMarker.Material;
            BackgroundColor = QantasLightGray;

            Shell.SetTitleView(this, GetTitleView());

            Content = GetContent();
            SetupBindings();
            SetupEntranceAnimationSwitcher();
        }

        private View GetTitleView()
            => new Label
            {
                FontFamily = MediumFont,
                FontSize = 20,
                TextColor = QantasText,
                Text = "Feed preferences",
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            };

        private int NavOffset = Device.RuntimePlatform == Device.iOS ? 88 : 0;
        private View GetContent() 
            => new Grid
            {
                TranslationY = NavOffset,
                BackgroundColor = QantasLightGray, 
                Padding = 0,
                RowSpacing = 0, 
                VerticalOptions = LayoutOptions.StartAndExpand, 
                RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, 
                    new RowDefinition { Height = new GridLength(85, GridUnitType.Absolute) },   
                    new RowDefinition { Height = new GridLength(NavOffset + 2, GridUnitType.Absolute) },      
                }, 
                Children =
                {
                    new CollectionView
                    {
                        Margin = new Thickness(10, 0), 
                        ItemTemplate = new DataTemplate(() => GetCellTemplate()),
                        ItemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Vertical),
                        SelectionMode = SelectionMode.Multiple,
                        ItemSizingStrategy = ItemSizingStrategy.MeasureFirstItem,
                    }
                    .Row(0) 
                    .Assign(out CollectionView),
                    
                    new Frame
                    {
                        BackgroundColor = Color.White, 
                        Padding = new Thickness(18, 18, 18, 22), 
                        Content = new Button
                        {
                            Text = "Save",
                            BackgroundColor = QantasRed,
                            
                            // strangely they don't seem to use their own font here
                            Font = Font.SystemFontOfSize(16, FontAttributes.Bold), 
                        }.Assign(out SaveButton),
                    }
                    .Row(1)
                } 
            };
            
        private readonly BoolToOpacityConverter boolToOpacityConverter = new BoolToOpacityConverter();
        private View GetCellTemplate()
            => new ContentView 
            { 
                BackgroundColor = QantasLightGray,
                InputTransparent = true, 
                CascadeInputTransparent = true,
                Content = new Frame
                {                
                    HasShadow = false,
                    BorderColor = Color.Transparent,
                    BackgroundColor = QantasLightGray,
                    Padding = 6,
                    Content = new Frame
                    {
                        Visual = VisualMarker.Material,
                        BorderColor = QantasCyan,
                        CornerRadius = 2,
                        Padding = 0,
                        Content = new Grid
                        {
                            RowDefinitions =
                            {
                                new RowDefinition { Height = 115 },
                                new RowDefinition { Height = 60 }
                            },
                            ColumnDefinitions =
                            {
                                new ColumnDefinition { },
                                new ColumnDefinition { Width = 25 }
                            },
                            Children =
                            {
                                new Image
                                {
                                     BackgroundColor = QantasRed,
                                     Aspect = Aspect.AspectFill
                                }
                                .Bind(Image.SourceProperty, nameof(FeedCategory.Image))
                                .Row(0).Col(0).ColSpan(2),

                                new Label
                                {
                                     Margin = new Thickness(10, 5),
                                     FontFamily = RegularFont,
                                     FontSize = 16,
                                     TextColor = QantasText,
                                }
                                .Bind(Label.TextProperty, nameof(FeedCategory.Name))
                                .Row(1),

                                new Image
                                {
                                     Margin = new Thickness(0, 5, 0, 0),
                                     Source = ImageSource.FromFile("ic_confirmed_16.png"),
                                     HorizontalOptions = LayoutOptions.Start,
                                     VerticalOptions = LayoutOptions.Start,
                                     HeightRequest = 18,
                                }
                                .Bind(OpacityProperty, nameof(FeedCategory.Selected), converter: boolToOpacityConverter) 
                                .Row(1).Col(1),
                            }
                        },
                        Opacity = 0
                    }.Invoke(x => 
                    {
                        // hack city 1.0
                        var index = _itemCounter++;
                        var cvIndex = CollectionViewIndex.For(index -1 /* why? */, ((GridItemsLayout)CollectionView.ItemsLayout).Span);

                        OnItemAppearing(x, cvIndex);
                    }) 
                } 
            };

        // hack city 2.0
        private static readonly double _sideCoeff = 0.55;
        private readonly Easing _easing = new Easing((x) => (x - 1) * (x - 1) * ((_sideCoeff + 1) * (x - 1) + _sideCoeff) + 1);
        private readonly Random r = new Random();
        private const int XTranslation = 300;
        private const int YTranslation = 50;
        private const int AnimationDurationMs = 350;
        private const float FirstAnimationOffsetS = .025f;
        private const float DelayPerRowS = .05f;
        
        private int _itemCounter;
        private DateTimeOffset? _firstAnimationTime;
        private ItemEntranceKind _entranceKind;
        private void OnItemAppearing(View view, CollectionViewIndex index)
        {    
            var now = DateTimeOffset.Now;
            _firstAnimationTime = _firstAnimationTime ?? now.AddSeconds(FirstAnimationOffsetS);
            
            if (_entranceKind == ItemEntranceKind.None)
            {
                view.Opacity = 1;
                return; 
            } 
            
            var translateY = true;
            var timeOffsetForItem = TimeSpan.FromSeconds(index.RowIndex * DelayPerRowS);
            var expectedTimeForItem = _firstAnimationTime.Value.Add(timeOffsetForItem);       
            var timeToWait = expectedTimeForItem - now;

            if (timeToWait < TimeSpan.Zero)
            {
                timeToWait = TimeSpan.FromSeconds(FirstAnimationOffsetS);
                translateY = false; // essentially, 'dont ytranslate things that werent on the screen when we started'
            }

            var translationX = index.ColIndex == 1 ? XTranslation : -XTranslation;
            var translationY = (index.RowIndex + 1) * YTranslation;

            switch (_entranceKind)
            {
                case ItemEntranceKind.StaggeredFadeUp:
                    view.TranslationY = translateY ? translationY : 0;
                    break;
                     
                case ItemEntranceKind.StaggeredFadeDown:
                    view.TranslationY = translateY ? -translationY : 0;
                    break;                
                    
                case ItemEntranceKind.XTranslate:
                    view.TranslationX = translationX;
                    break;                
                    
                case ItemEntranceKind.CrissCross:
                    view.TranslationX = -translationX;
                    break;                
            }
            
            Task.Delay(timeToWait)
                .ContinueWith(_ =>
                {
                    if (_entranceKind.IsOneOf(ItemEntranceKind.CrissCross, ItemEntranceKind.XTranslate))
                        view.Opacity = 1; // dont fade these ones;
                
                    view.TranslateTo(0, 0, AnimationDurationMs, _easing); 
                    view.FadeTo(1, AnimationDurationMs);     
                }); 
        }
        
        private void SetupBindings()
        {
            FeedCategories =
                Enumerable
                    .Range(0, 10) // so we can test with more data 
                    .SelectMany(_ => _categories.Select(FeedCategory.ForCategory))
                    .ToObservableCollection();

            CollectionView.SelectionChanged += (sender, e) =>
            {
                // this is not so great
                var selection = e.CurrentSelection.ToSet();
                foreach (FeedCategory item in FeedCategories)
                    item.Selected = selection.Contains(item);
            };

            CollectionView.ItemsSource = FeedCategories;
        }

        private void SetupEntranceAnimationSwitcher()
        {
            var animationKindIndex = 0;
            var animationKinds =
                Enum.GetValues(typeof(ItemEntranceKind))
                    .OfType<ItemEntranceKind>()
                    .ToList();

            SaveButton.Clicked += async (sender, e) =>
            {
                var nextEntranceKind = animationKinds[++animationKindIndex % animationKinds.Count];
                await ChangeToItemEntranceKind(nextEntranceKind);
            };
        }

        private async Task ChangeToItemEntranceKind(ItemEntranceKind entranceKind)
        {
            _itemCounter = 0;
            _firstAnimationTime = null;
            _entranceKind = entranceKind;
                
           if (_entranceKind != ItemEntranceKind.None)
                await CollectionView.FadeTo(0, 50);

            CollectionView.ItemsSource = null;
                                
            Device.BeginInvokeOnMainThread(() =>
            {
                CollectionView.Opacity = 1;
                SaveButton.Text = $"{_entranceKind}";  
                CollectionView.ItemTemplate = new DataTemplate(() => GetCellTemplate());
                CollectionView.ItemsSource = FeedCategories.ToList();
            });
        }

        private string[] _categories =
        {
            "Travel",
            "Food & Wine",
            "Smart Money",
            "Entertainment & Experiences",
            "Health & Wellness",
            "Fashion & Shopping",
            "Offers & Discounts",
            "Competitions",
            "Sport",
            "Business Rewards & Advice",
            "Family & Kids",
            "Technology & Innovation"
        };
    }
}