using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Locations;
using Android.OS;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AT.Markushi.UI;
using Java.Lang;
using ME.Alexrs.Wavedrawable;
using Plugin.Geolocator;
using QuickDate.Activities.Premium;
using QuickDate.Activities.SettingsUser;
using QuickDate.Activities.Tabbes.Adapters;
using QuickDate.ButtomSheets;
using QuickDate.Helpers.Controller;
using QuickDate.Helpers.Model;
using QuickDate.Helpers.Utils;
using QuickDate.Library.Anjo.CardStackView;
using QuickDateClient.Classes.Global;
using QuickDateClient.Classes.Users;
using QuickDateClient.Requests;  
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace QuickDate.Activities.Tabbes.Fragment
{
    public class CardMachFragment : AndroidX.Fragment.App.Fragment, ICardStackListener 
    {
        #region Variables Basic

        private CardStackLayoutManager LayoutManager; 
        private CardStackView CardStack;
        public CardAdapter CardDateAdapter;
        private CircleButton LikeButton, DesLikeButton, UndoButton;
        private HomeActivity GlobalContext;
        private WaveDrawable WaveDrawableAnimation;
        private ImageView ImageView, FilterButton;
        private ImageView PopularityImage;
        private RelativeLayout CardViewBig;
        private LinearLayout BtnLayout;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private int TotalCount, SwipeCount;
        private string TotalIdLiked = "", TotalIdDisLiked = "";
        private SwipeDirection Direction;
        private int Index;
        public Handler MainHandler = new Handler(Looper.MainLooper);
        public IRunnable Runnable;
        private LocationManager LocationManager;
        private bool ShowAlertDialogGps = true;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            GlobalContext = (HomeActivity)Activity;
            
            SwipeCount = MainSettings.GetSwipeCountValue();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
               var view = inflater.Inflate(Resource.Layout.TCardMachLayout, container, false);
                return view;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }
         
        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            {
                base.OnViewCreated(view, savedInstanceState);

                InitComponent(View);
                InitToolbar(View);
                SetRecyclerViewAdapters();

                InitializeLocationManager();

                var doubleClickHelper = new DoubleClickHandler(500);

                LikeButton.Click += (s, e) => doubleClickHelper.HandleDoubleClick(LikeButtonOnClick);
                DesLikeButton.Click += (s, e) => doubleClickHelper.HandleDoubleClick(DesLikeButtonOnClick);
                UndoButton.Click += (s, e) => doubleClickHelper.HandleDoubleClick(UndoButtonOnClick);

                PopularityImage.Click += PopularityImageOnClick;
                FilterButton.Click += FilterButtonOnClick;

                StartApiService();

                Runnable = new PostUpdaterHelper(GlobalContext, MainHandler);
                //Start Updating the news feed every few minus 
                MainHandler.PostDelayed(Runnable, 5000);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);

            }
        }
         
        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                CardStack = view.FindViewById<CardStackView>(Resource.Id.activity_main_card_stack_view);

                LikeButton = view.FindViewById<CircleButton>(Resource.Id.likebutton2);
                DesLikeButton = view.FindViewById<CircleButton>(Resource.Id.closebutton1);
                UndoButton = view.FindViewById<CircleButton>(Resource.Id.Undobutton1);
                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);
                PopularityImage = view.FindViewById<ImageView>(Resource.Id.coinImage);
                CardViewBig = view.FindViewById<RelativeLayout>(Resource.Id.CardViewBig);
                FilterButton = view.FindViewById<ImageView>(Resource.Id.Filterbutton);

                LayoutManager = new CardStackLayoutManager(Context, this);

                LayoutManager.SetSwipeableMethod(SwipeableMethod.AutomaticAndManual);
                LayoutManager.SetStackFrom(StackFrom.None);
                LayoutManager.SetVisibleCount(3);
                LayoutManager.SetTranslationInterval(8.0f);
                LayoutManager.SetScaleInterval(0.95f);
                LayoutManager.SetSwipeThreshold(0.3f);
                LayoutManager.SetMaxDegree(20.0f);
                LayoutManager.SetDirections(SwipeDirection.Horizontal);
                LayoutManager.SetCanScrollHorizontal(true);
                LayoutManager.SetCanScrollVertical(false);
                LayoutManager.SetSwipeableMethod(SwipeableMethod.AutomaticAndManual);
                LayoutManager.SetOverlayInterpolator(new LinearInterpolator());
                CardStack.SetLayoutManager(LayoutManager);
                CardStack.SetItemAnimator(new DefaultItemAnimator());
                 
                BtnLayout = view.FindViewById<LinearLayout>(Resource.Id.buttonLayout);
                ImageView = view.FindViewById<ImageView>(Resource.Id.userImageView);

                WaveDrawableAnimation = new WaveDrawable(Color.ParseColor(AppSettings.MainColor), 800);
                if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBean)
                    ImageView.Background = WaveDrawableAnimation;
                WaveDrawableAnimation.SetWaveInterpolator(new LinearInterpolator());
                WaveDrawableAnimation.StartAnimation();

                ImageView.Visibility = ViewStates.Visible;
                CardStack.Visibility = ViewStates.Invisible;
                BtnLayout.Visibility = ViewStates.Invisible; 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitToolbar(View view)
        {
            try
            {
                var toolbar = view.FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    GlobalContext.SetToolBar(toolbar, "", false, false);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetRecyclerViewAdapters()
        {
            try
            {
                CardDateAdapter = new CardAdapter(Activity)
                {
                    UsersDateList = new ObservableCollection<UserInfoObject>()
                };
                CardDateAdapter.ItemClick += CardDateAdapterOnItemClick;
                CardStack.SetAdapter(CardDateAdapter);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void CardDateAdapterOnItemClick(object sender, CardAdapterClickEventArgs e)
        {
            try
            {
                var item = CardDateAdapter.GetItem(e.Position);
                if (item != null)
                {
                    QuickDateTools.OpenProfile(Activity, "LikeAndMoveCardMach", item, e.Image);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception); 
            }
        }

        #endregion

        #region Events
         
        //Get User By >> gender
        private void FilterButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                UsersFilterBottomDialogFragment bottomDialogFragment = new UsersFilterBottomDialogFragment("CardMach");
                bottomDialogFragment.Show(ChildFragmentManager, bottomDialogFragment.Tag);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void PopularityImageOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(Context, typeof(PopularityActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void UndoButtonOnClick()
        {
            try
            {
                var data = ListUtils.OldMatchesList.LastOrDefault();
                if (data != null)
                {
                    CardDateAdapter.UsersDateList.Insert(0, data);
                    CardDateAdapter.NotifyDataSetChanged();

                    ListUtils.OldMatchesList.Remove(data);
                }

                RewindAnimationSetting settings = new RewindAnimationSetting.Builder()
                    .SetDirection(SwipeDirection.Left)
                    .SetDuration(SwipeDuration.Slow.Duration)
                    .SetInterpolator(new AccelerateInterpolator())
                    .Build();

                CardStackLayoutManager cardStackLayoutManager2 = new CardStackLayoutManager(Context);
                cardStackLayoutManager2.SetRewindAnimationSetting(settings);
                CardStack.SetLayoutManager(cardStackLayoutManager2);
                CardStack.Rewind();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void DesLikeButtonOnClick()
        {
            try
            {
                SetDesLikeDirection();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void LikeButtonOnClick()
        {
            try
            {
                SetLikeDirection();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void SetLikeDirection()
        {
            try
            {
                var maxSwaps = ListUtils.SettingsSiteList?.MaxSwaps;
                var isPro = ListUtils.MyUserInfo?.FirstOrDefault()?.IsPro ?? "0";
                if (isPro == "0" && SwipeCount == Convert.ToInt32(maxSwaps))
                {
                    //You have exceeded the maximum of likes or swipes per this day
                    var window = new DialogController(Activity);
                    window.OpenDialogGetToPremium();

                    Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_ErrorMaxSwaps), ToastLength.Short)?.Show();
                    return;
                }
                 
                SwipeAnimationSetting setting = new SwipeAnimationSetting.Builder()
                    .SetDirection(SwipeDirection.Right)
                    .SetDuration(SwipeDuration.Slow.Duration)
                    .SetInterpolator(new AccelerateInterpolator())
                    .Build();
                LayoutManager.SetSwipeAnimationSetting(setting);
                CardStack.Swipe(); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetDesLikeDirection()
        {
            try
            {
                var maxSwaps = ListUtils.SettingsSiteList?.MaxSwaps;
                var isPro = ListUtils.MyUserInfo?.FirstOrDefault()?.IsPro ?? "0";
                if (isPro == "0" && SwipeCount == Convert.ToInt32(maxSwaps))
                {
                    //You have exceeded the maximum of likes or swipes per this day
                    var window = new DialogController(Activity);
                    window.OpenDialogGetToPremium();

                    Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_ErrorMaxSwaps), ToastLength.Short)?.Show();
                    return;
                }

                SwipeAnimationSetting setting = new SwipeAnimationSetting.Builder()
                    .SetDirection(SwipeDirection.Left)
                    .SetDuration(SwipeDuration.Slow.Duration)
                    .SetInterpolator(new AccelerateInterpolator())
                    .Build();
                LayoutManager.SetSwipeAnimationSetting(setting);
                CardStack.Swipe();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SaveSwipeCount()
        {
            MainSettings.StoreSwipeCountValue(SwipeCount);
        }

        #endregion

        #region CardEventListener

        public void OnCardDragging(SwipeDirection direction, float ratio)
        {

        }

        public void OnCardSwiped(SwipeDirection direction)
        {
            try
            {
                Index = LayoutManager.GetTopPosition() - 1;
                Direction = direction;
                new Handler(Looper.MainLooper).Post(new Runnable(Run));
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void Run()
        {
            try
            {
                if (Direction == SwipeDirection.Right)
                {
                    SwipeCount++;
                    CardAppeared(Index);
                    GlobalContext?.TracksCounter?.CheckTracksCounter();
                }
                else if (Direction == SwipeDirection.Left)
                {
                    SwipeCount++;
                    CardDisappeared(Index);
                    GlobalContext?.TracksCounter?.CheckTracksCounter();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnCardRewound()
        {

        }

        public void OnCardCanceled()
        {

        }

        public void OnCardAppeared(View view, int position)
        {

        }

        public void OnCardDisappeared(View view, int position)
        {

        }

        public void CardAppeared(int position)
        {
            try
            {
                if (CardDateAdapter.UsersDateList.Count > 0)
                {
                    if (position == -1)
                        position = 0;

                    if (position >= 0)
                    {
                        //var textView = view.FindViewById<TextView>(Resource.Id.item_tourist_spot_card_name);
                        //if (textView != null)
                        //{
                        //    string name = textView.Text;
                        //}

                        if (position == CardDateAdapter.UsersDateList.Count)
                            position = CardDateAdapter.UsersDateList.Count - 1;

                        var data = CardDateAdapter.UsersDateList[position];

                        if (data != null)
                        {
                            if (data.IsLiked != null && data.IsLiked.Value)
                            {
                                Activity?.RunOnUiThread(() =>
                                {
                                    new DialogController(Activity).OpenDialogMatchFound(data);
                                });
                            }

                            ListUtils.LikedList.Add(data);
                            ListUtils.OldMatchesList.Add(data);

                            CardDateAdapter.UsersDateList.Remove(data);
                            CardDateAdapter.NotifyDataSetChanged();

                            TotalCount += 1;
                        }

                        CheckerCountCard();
                    }
                }
                else
                {
                    CheckerCountCard();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void CardDisappeared(int position)
        {
            try
            {
                if (CardDateAdapter.UsersDateList.Count > 0)
                {
                    if (position == -1)
                        position = 0;

                    if (position >= 0)
                    {
                        if (position == CardDateAdapter.UsersDateList.Count)
                            position = CardDateAdapter.UsersDateList.Count - 1;

                        var data = CardDateAdapter.UsersDateList[position];
                        if (data != null)
                        {
                            ListUtils.DisLikedList.Add(data);
                            ListUtils.OldMatchesList.Add(data);
                            TotalCount += 1;

                            Activity?.RunOnUiThread(() =>
                            {
                                try
                                {
                                    CardDateAdapter.UsersDateList.Remove(data);
                                    CardDateAdapter.NotifyDataSetChanged();
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });
                        }
                        CheckerCountCard();
                    }
                    else
                    {
                        CheckerCountCard();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void CheckerCountCard()
        {
            try
            {
                if (TotalCount >= 2 || CardDateAdapter.UsersDateList.Count == 0)
                {
                    if (ListUtils.LikedList.Count > 0)
                    {
                        TotalIdLiked = "";
                        //Get all id 
                        foreach (var item in ListUtils.LikedList)
                        {
                            TotalIdLiked += item.Id + ",";
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Users.AddLikesAsync(item.Id.ToString(), "") });
                        }
                    }

                    if (ListUtils.DisLikedList.Count > 0)
                    {
                        TotalIdDisLiked = "";
                        //Get all id 
                        foreach (var item in ListUtils.DisLikedList)
                        {
                            TotalIdDisLiked += item.Id + ",";
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Users.AddLikesAsync("", item.Id.ToString()) });
                        }
                    }

                    //if (!string.IsNullOrEmpty(TotalIdLiked))
                    //    TotalIdLiked = TotalIdLiked.Remove(TotalIdLiked.Length - 1, 1);

                    //if (!string.IsNullOrEmpty(TotalIdDisLiked))
                    //    TotalIdDisLiked = TotalIdDisLiked.Remove(TotalIdDisLiked.Length - 1, 1);

                    //if (!string.IsNullOrEmpty(TotalIdLiked) || !string.IsNullOrEmpty(TotalIdDisLiked)) //sent api 
                    //    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Users.AddLikesAsync(TotalIdLiked, TotalIdDisLiked) });

                    TotalCount = 0;
                    ListUtils.LikedList.Clear();
                    ListUtils.DisLikedList.Clear();
                    TotalIdDisLiked = "";
                    TotalIdLiked = "";
                }

                //Load More
                int count = CardDateAdapter.UsersDateList.Count;
                if (count <= 5)
                {
                    var offset = CardDateAdapter.UsersDateList.LastOrDefault()?.Id ?? 0;
                    StartApiService(offset.ToString());
                }

                var maxSwaps = ListUtils.SettingsSiteList?.MaxSwaps;
                var isPro = ListUtils.MyUserInfo?.FirstOrDefault()?.IsPro ?? "0";
                if (isPro == "0" && SwipeCount == Convert.ToInt32(maxSwaps))
                {
                    //You have exceeded the maximum of likes or swipes per this day
                    var window = new DialogController(Activity);
                    window.OpenDialogGetToPremium();

                    Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_ErrorMaxSwaps), ToastLength.Short)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Load Random Users

        private void StartApiService(string offset = "0")
        {
            if (!Methods.CheckConnectivity())
            {
                ImageView.Visibility = ViewStates.Gone;
                CardStack.Visibility = ViewStates.Gone;
                BtnLayout.Visibility = ViewStates.Gone;

                Inflated = EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null!;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadUser });
        }

        private async Task LoadUser()
        {
            // Check if we're running on Android 5.0 or higher
            if ((int)Build.VERSION.SdkInt < 23)
                await GetPosition();
            else
            {
                if (Context.CheckSelfPermission(Manifest.Permission.AccessFineLocation) == Permission.Granted && Context.CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) == Permission.Granted)
                    await GetPosition();
                else
                    new PermissionsController(Activity).RequestPermission(105);
            }
        }
         
        private async Task LoadMatches(string offset = "0")
        {
            if (Methods.CheckConnectivity())
            { 
                if (UserDetails.Lat == "" && UserDetails.Lng == "")
                {
                    var locator = CrossGeolocator.Current;
                    locator.DesiredAccuracy = 50;
                    var position = await locator.GetPositionAsync(TimeSpan.FromMilliseconds(10000));
                    Console.WriteLine("Position Status: {0}", position.Timestamp);
                    Console.WriteLine("Position Latitude: {0}", position.Latitude);
                    Console.WriteLine("Position Longitude: {0}", position.Longitude);

                    UserDetails.Lat = position.Latitude.ToString(CultureInfo.InvariantCulture);
                    UserDetails.Lng = position.Longitude.ToString(CultureInfo.InvariantCulture);
                }

                UserDetails.Location = ListUtils.MyUserInfo?.FirstOrDefault()?.Location;
                 
                var dictionary = new Dictionary<string, string>
                {
                    {"limit", "30"},
                    {"offset", offset},
                    {"genders", UserDetails.UsersFilterGender},
                    {"online", UserDetails.UsersFilterIsOnline ? "1" : "0"},
                    {"birthday", UserDetails.UsersFilterBirthday},
                    //{"lat", UserDetails.Lat},
                    //{"lng", UserDetails.Lng},
                };

                var (apiStatus, respond) = await RequestsAsync.Users.GetRandomUsersAsync(dictionary);
                if (apiStatus != 200 || respond is not ListUsersObject result || result.Data == null)
                {
                    Methods.DisplayReportResult(Activity, respond);
                }
                else
                {
                    if (result.Data?.Count > 0)
                    {
                        foreach (var item in from item in result.Data let data = ListUtils.AllMatchesList.FirstOrDefault(a => a.Id == item.Id) where data == null select item)
                        {
                            CardDateAdapter.UsersDateList.Add(item);

                            ListUtils.AllMatchesList.Add(item);
                        }

                        Activity?.RunOnUiThread(() => { CardDateAdapter.NotifyDataSetChanged(); });
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(offset))
                            Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreUsers), ToastLength.Long)?.Show();
                    }
                }

                Activity?.RunOnUiThread(ShowEmptyPage);

                // Open Dialog Tutorial
                OpenDialog();
            }
            else
            {
                Inflated = EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null!;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
        }

        private void ShowEmptyPage()
        {
            try
            {
                if (CardDateAdapter.UsersDateList.Count > 0)
                {
                    CardDateAdapter.NotifyDataSetChanged(); 

                    ImageView.Visibility = ViewStates.Gone;
                    CardStack.Visibility = ViewStates.Visible;
                    BtnLayout.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;

                    if (WaveDrawableAnimation.IsAnimationRunning)
                        WaveDrawableAnimation?.StopAnimation();
                }
                else
                {
                    ImageView.Visibility = ViewStates.Gone;
                    CardStack.Visibility = ViewStates.Gone;
                    BtnLayout.Visibility = ViewStates.Gone;

                    Inflated ??= EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoMatches);
                    if (x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click += null!;
                    }

                    EmptyStateLayout.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //No Internet Connection 
        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion
         
        #region Location

        private void InitializeLocationManager()
        {
            try
            {
                LocationManager = (LocationManager)Activity.GetSystemService(Context.LocationService);
                var criteriaForLocationService = new Criteria
                {
                    Accuracy = Accuracy.Fine
                };
                var acceptableLocationProviders = LocationManager.GetProviders(criteriaForLocationService, true);
                var locationProvider = acceptableLocationProviders.Any() ? acceptableLocationProviders.First() : string.Empty;
                Console.WriteLine(locationProvider);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Get Position GPS Current Location
        private async Task GetPosition()
        {
            try
            {
                if (CrossGeolocator.Current.IsGeolocationAvailable)
                {
                    // Check if we're running on Android 5.0 or higher
                    if ((int)Build.VERSION.SdkInt < 23)
                    {
                        CheckAndGetLocation();
                    }
                    else
                    {
                        if (Context.CheckSelfPermission(Manifest.Permission.AccessFineLocation) == Permission.Granted && Context.CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) == Permission.Granted)
                        {
                            CheckAndGetLocation();
                        }
                        else
                        {
                            new PermissionsController(Activity).RequestPermission(105);
                        }
                    }
                }

                await Task.Delay(0);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void CheckAndGetLocation()
        {
            try
            {
                Activity?.RunOnUiThread(async () =>
                {
                    try
                    {
                        if (!LocationManager.IsProviderEnabled(LocationManager.GpsProvider))
                        {
                            if (ShowAlertDialogGps)
                            {
                                ShowAlertDialogGps = false;

                                Activity?.RunOnUiThread(() =>
                                {
                                    try
                                    {
                                        // Call your Alert message
                                        AlertDialog.Builder alert = new AlertDialog.Builder(Context);
                                        alert.SetTitle(GetString(Resource.String.Lbl_Use_Location) + "?");
                                        alert.SetMessage(GetString(Resource.String.Lbl_GPS_is_disabled) + "?");

                                        alert.SetPositiveButton(GetString(Resource.String.Lbl_Ok), (senderAlert, args) =>
                                        {
                                            //Open intent Gps
                                            new IntentController(Activity).OpenIntentGps(LocationManager);
                                        });

                                        alert.SetNegativeButton(GetString(Resource.String.Lbl_Cancel), (senderAlert, args) => { });

                                        Dialog gpsDialog = alert.Create();
                                        gpsDialog.Show();
                                    }
                                    catch (Exception e)
                                    {
                                        Methods.DisplayReportResultTrack(e);
                                    }
                                });
                            }
                        }
                        else
                        {
                            var locator = CrossGeolocator.Current;
                            locator.DesiredAccuracy = 50;
                            var position = await locator.GetPositionAsync(TimeSpan.FromMilliseconds(10000));
                            Console.WriteLine("Position Status: {0}", position.Timestamp);
                            Console.WriteLine("Position Latitude: {0}", position.Latitude);
                            Console.WriteLine("Position Longitude: {0}", position.Longitude);

                            UserDetails.Lat = position.Latitude.ToString(CultureInfo.InvariantCulture);
                            UserDetails.Lng = position.Longitude.ToString(CultureInfo.InvariantCulture);

                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadMatches() });
                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion 

        private void OpenDialog()
        {
            try
            {
                var showTutorialDialog = MainSettings.GetShowTutorialDialogValue();

                if (showTutorialDialog)
                {
                    new DialogController(Activity).OpenDialogSkipTutorial();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private class PostUpdaterHelper : Java.Lang.Object, IRunnable
        {
            private readonly Handler MainHandler;
            private readonly HomeActivity Activity;

            public PostUpdaterHelper(HomeActivity activity, Handler mainHandler)
            {
                MainHandler = mainHandler;
                Activity = activity;
            }

            public void Run()
            {
                try
                {
                    HomeActivity.GetInstance()?.GetNotifications();
                    //ApiRequest.GetInfoData(Activity, UserDetails.UserId.ToString()).ConfigureAwait(false);
                    MainHandler?.PostDelayed(new PostUpdaterHelper(Activity, new Handler(Looper.MainLooper)), 5000);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        public void ApplyFilter()
        {
            try
            {
                CardDateAdapter.UsersDateList.Clear();
                ListUtils.AllMatchesList.Clear();
                CardDateAdapter.NotifyDataSetChanged();

                WaveDrawableAnimation.StartAnimation();

                ImageView.Visibility = ViewStates.Visible;
                CardStack.Visibility = ViewStates.Invisible;
                BtnLayout.Visibility = ViewStates.Invisible;
                EmptyStateLayout.Visibility = ViewStates.Gone;

                StartApiService();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}