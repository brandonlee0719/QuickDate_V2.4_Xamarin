using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using QuickDate.Library.Anjo.IntegrationRecyclerView;
using Bumptech.Glide.Util;
using IAmMert.ReadableBottomBarLib;
using QuickDate.Activities.Tabbes.Adapters;
using QuickDate.Helpers.Controller;
using QuickDate.Helpers.Model;
using QuickDate.Helpers.Utils;
using QuickDateClient.Classes.Common;
using QuickDateClient.Classes.Users;
using QuickDateClient.Requests;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using static IAmMert.ReadableBottomBarLib.ReadableBottomBar;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace QuickDate.Activities.Tabbes.Fragment
{
    public class NotificationsFragment : AndroidX.Fragment.App.Fragment, IItemSelectListener
    {
        #region Variables Basic

        private RecyclerView MRecycler;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private LinearLayoutManager NotifyLayoutManager;
        private Toolbar ToolbarView;
        private NotificationsAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;

        private HomeActivity GlobalContext;
        private RecyclerViewOnScrollListener MainScrollEvent;
        private ReadableBottomBar ReadableTopBar;
        private string NameTap = "Match";

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            GlobalContext = (HomeActivity)Activity;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                View view = inflater.Inflate(Resource.Layout.TNotificationsLayout, container, false);

                if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                {
                    Activity.Window?.ClearFlags(WindowManagerFlags.TranslucentStatus);
                    Activity.Window?.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                    Activity.Window?.SetStatusBarColor(Color.ParseColor("#DB2251"));
                }

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

                InitComponent(view);
                InitToolbar(view);
                SetRecyclerViewAdapters();

                SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;

                StartApiService();

                if (Methods.CheckConnectivity())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadMatchAsync() });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
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

                MRecycler = (RecyclerView)view.FindViewById(Resource.Id.NotifcationRecyler);
                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);

                ReadableTopBar = view.FindViewById<ReadableBottomBar>(Resource.Id.TopBar);
                ReadableTopBar.SetOnItemSelectListener(this);

                SwipeRefreshLayout = (SwipeRefreshLayout)view.FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
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
                ToolbarView = view.FindViewById<Toolbar>(Resource.Id.toolbar);
                ToolbarView.Title = GetString(Resource.String.Lbl_Notifications);
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
                MAdapter = new NotificationsAdapter(Activity) { NotificationsList = new ObservableCollection<GetNotificationsObject.Datum>(ListUtils.MatchList) };

                NotifyLayoutManager = new LinearLayoutManager(Activity);
                MRecycler.SetLayoutManager(NotifyLayoutManager);
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.HasFixedSize = true;
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new ViewPreloadSizeProvider();
                var preLoader = new RecyclerViewPreloader<GetNotificationsObject.Datum>(Activity, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);

                MAdapter.OnItemClick += MAdapterOnItemClick;
                MAdapter.AddButtonItemClick += MAdapterOnAddButtonItemClick;
                MAdapter.DeleteButtonItemClick += MAdapterOnDeleteButtonItemClick;

                TranslateAnimation animation1 = new TranslateAnimation(0.0f, 0.0f, 1500.0f, 0.0f) { Duration = 700 };
                // animation duration
                MRecycler.StartAnimation(animation1);

                RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(NotifyLayoutManager);
                MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
                MRecycler.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                MainScrollEvent.IsLoading = false;

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region TopBar

        public void OnItemSelected(int index)
        {
            try
            {
                switch (index)
                {
                    case 0:
                        NameTap = "Match";
                        MatchesButtonOnClick();
                        break;
                    case 1:
                        NameTap = "Visits";
                        VisitsButtonOnClick();
                        break;
                    case 2:
                        NameTap = "Likes";
                        LikesButtonOnClick();
                        break;
                    case 3:
                        NameTap = "Requests";
                        FriendsRequestsButtonOnClick();
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event

        //FriendRequests
        private void MAdapterOnAddButtonItemClick(object sender, NotificationsAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position < 0) return;
               
                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    if (Methods.CheckConnectivity())
                    {
                        ListUtils.RequestsList.Remove(item);
                        MAdapter.NotificationsList.Remove(item);

                        ShowEmptyPage(true);
                         
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Friends.ApproveFiendRequestAsync(item.Id.ToString()) }); // true >> Accept 
                    }
                    else
                    {
                        Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    }
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MAdapterOnDeleteButtonItemClick(object sender, NotificationsAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position < 0) return;

                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    if (Methods.CheckConnectivity())
                    {
                        ListUtils.RequestsList.Remove(item);
                        MAdapter.NotificationsList.Remove(item);

                        ShowEmptyPage(true);
                     
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Friends.DisapproveFiendRequestAsync(item.Id.ToString()) });// false >> Decline
                    }
                    else
                    {
                        Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Scroll
        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                //Code get last id where LoadMore >>
                var item = MAdapter.NotificationsList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.Id.ToString()) && !MainScrollEvent.IsLoading)
                {
                    switch (NameTap)
                    {
                        case "Match":
                        {
                            if (Methods.CheckConnectivity())
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadMatchAsync(item.Id.ToString()) });
                            break;
                        }
                        case "Visits":
                        {
                            if (Methods.CheckConnectivity())
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadVisitsAsync(item.Id.ToString()) });
                            break;
                        }
                        case "Likes":
                        {
                            if (Methods.CheckConnectivity())
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadLikesAsync });
                            break;
                        }
                        case "Requests":
                        {
                            if (Methods.CheckConnectivity())
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadFriendsRequestsAsync(item.Id.ToString()) });
                            break;
                        }
                        default:
                            StartApiService(item.Id.ToString());
                            break;
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open user profile
        private void MAdapterOnItemClick(object sender, NotificationsAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position > -1)
                {
                    var item = MAdapter.GetItem(e.Position);
                    if (item != null)
                    {
                        string eventPage;
                        switch (item.Type)
                        {
                            case "got_new_match":
                                eventPage = "HideButton";
                                break;
                            case "like":
                                eventPage = "likeAndClose";
                                break;
                            default:
                                eventPage = "Close";
                                break;
                        }

                        QuickDateTools.OpenProfile(Activity, eventPage, item.Notifier, e.Image);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void VisitsButtonOnClick()
        {
            try
            {
                MAdapter.NotificationsList.Clear();

                MAdapter.NotificationsList = new ObservableCollection<GetNotificationsObject.Datum>(ListUtils.VisitsList);
                MAdapter.NotifyDataSetChanged();

                ToolbarView.SetBackgroundResource(Resource.Drawable.Shape_Gradient_Normal);

                if (MAdapter.NotificationsList.Count > 0)
                {
                    MRecycler.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }

                if (Methods.CheckConnectivity())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadVisitsAsync() });
                 
                if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
                    return;

                Activity.Window?.ClearFlags(WindowManagerFlags.TranslucentStatus);
                Activity.Window?.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                Activity.Window?.SetStatusBarColor(Color.ParseColor(AppSettings.MainColor));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void LikesButtonOnClick()
        {
            try
            {
                MAdapter.NotificationsList.Clear();

                MAdapter.NotificationsList = new ObservableCollection<GetNotificationsObject.Datum>(ListUtils.LikesList); 
                MAdapter.NotifyDataSetChanged();

                ToolbarView.SetBackgroundResource(Resource.Drawable.Shape_Gradient_Normal2);

                if (MAdapter.NotificationsList.Count > 0)
                {
                    MRecycler.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }

                if (Methods.CheckConnectivity())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadLikesAsync });

                if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
                    return;

                Activity.Window?.ClearFlags(WindowManagerFlags.TranslucentStatus);
                Activity.Window?.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                Activity.Window?.SetStatusBarColor(Color.ParseColor(AppSettings.MainColor));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MatchesButtonOnClick()
        {
            try
            {
                MAdapter.NotificationsList.Clear();

                MAdapter.NotificationsList = new ObservableCollection<GetNotificationsObject.Datum>(ListUtils.MatchList);
                MAdapter.NotifyDataSetChanged();

                ToolbarView.SetBackgroundResource(Resource.Drawable.Shape_Gradient_Normal3);

                if (MAdapter.NotificationsList.Count > 0)
                {
                    MRecycler.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }

                if (Methods.CheckConnectivity())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadMatchAsync() });

                if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
                    return;

                Activity.Window?.ClearFlags(WindowManagerFlags.TranslucentStatus);
                Activity.Window?.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                Activity.Window?.SetStatusBarColor(Color.ParseColor("#DB2251"));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void FriendsRequestsButtonOnClick()
        {
            try
            {
                MAdapter.NotificationsList.Clear();

                MAdapter.NotificationsList = new ObservableCollection<GetNotificationsObject.Datum>(ListUtils.RequestsList);
                MAdapter.NotifyDataSetChanged();

                ToolbarView.SetBackgroundResource(Resource.Drawable.Shape_Gradient_Normal2);

                if (MAdapter.NotificationsList.Count > 0)
                {
                    MRecycler.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }

                if (Methods.CheckConnectivity())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadFriendsRequestsAsync() });

                if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
                    return;

                Activity.Window?.ClearFlags(WindowManagerFlags.TranslucentStatus);
                Activity.Window?.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                Activity.Window?.SetStatusBarColor(Color.ParseColor(AppSettings.MainColor));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                MAdapter.NotificationsList.Clear();
                MAdapter.NotifyDataSetChanged();

                MainScrollEvent.IsLoading = false;

                switch (NameTap)
                {
                    case "Match":
                    {
                        ListUtils.MatchList.Clear();
                        if (Methods.CheckConnectivity())
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadMatchAsync() });
                        break;
                    }
                    case "Visits":
                    {
                        ListUtils.VisitsList.Clear();
                        if (Methods.CheckConnectivity())
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadVisitsAsync() });
                        break;
                    }
                    case "Likes":
                    {
                        ListUtils.LikesList.Clear();
                        if (Methods.CheckConnectivity())
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadLikesAsync });
                        break;
                    }
                    case "Requests" when AppSettings.EnableFriendSystem:
                    {
                        ListUtils.RequestsList.Clear();
                        if (Methods.CheckConnectivity())
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadFriendsRequestsAsync() });
                        break;
                    }
                    default:
                        ListUtils.MatchList.Clear();
                        ListUtils.LikesList.Clear();
                        ListUtils.VisitsList.Clear();
                        if (AppSettings.EnableFriendSystem)
                            ListUtils.RequestsList.Clear();

                        StartApiService();
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load Data 

        private void StartApiService(string offset = "")
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadDataAsync(offset) });
        }

        private async Task LoadDataAsync(string offset = "")
        {
            if (MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;
                //int countList = MAdapter.NotificationsList.Count;
                var (apiStatus, respond) = await RequestsAsync.Common.GetNotificationsAsync("25", offset, UserDetails.DeviceId);
                if (apiStatus != 200 || respond is not GetNotificationsObject result || result.Data == null)
                {
                    MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(Activity, respond);
                }
                else
                {
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    {
                        foreach (var item in result.Data)
                        {
                            item.Text = QuickDateTools.GetNotification(item);
                        }
                    }
                    else
                    {
                        if (MAdapter.NotificationsList.Count > 10 && !MRecycler.CanScrollVertically(1))
                            Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreUsers), ToastLength.Short)?.Show();
                    }
                }

                Activity?.RunOnUiThread(() => { ShowEmptyPage(true); });
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
                MainScrollEvent.IsLoading = false;
            }
            MainScrollEvent.IsLoading = false;
        }

        private async Task LoadMatchAsync(string offset = "")
        {
            if (MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;
                //int countList = MAdapter.NotificationsList.Count;
                var (apiStatus, respond) = await RequestsAsync.Users.MatchesAsync(offset);
                if (apiStatus != 200 || respond is not ListUsersObject result || result.Data == null)
                {
                    MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(Activity, respond);
                }
                else
                {
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    {
                        foreach (var item in from item in result.Data let check = ListUtils.MatchList.FirstOrDefault(a => a.NotifierId == item.Id) where check == null select item)
                        {
                            ListUtils.MatchList.Add(new GetNotificationsObject.Datum
                            {
                                Text = Context.GetText(Resource.String.Lbl_YouGotMatch),
                                Notifier = item,
                                NotifierId = Convert.ToInt32(item.Id),
                                Type = "got_new_match"
                            });
                        }
                    }
                    else
                    {
                        if (MAdapter.NotificationsList.Count > 10 && !MRecycler.CanScrollVertically(1))
                            Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreUsers), ToastLength.Short)?.Show();
                    }
                }

                Activity?.RunOnUiThread(() => { ShowEmptyPage(true); });
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
            MainScrollEvent.IsLoading = false;
        }

        private async Task LoadVisitsAsync(string offset = "")
        {
            if (MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;
                //int countList = MAdapter.NotificationsList.Count;
                var (apiStatus, respond) = await RequestsAsync.Users.ListVisitsAsync("25", offset);
                if (apiStatus != 200 || respond is not ListUsersObject result || result.Data == null)
                {
                    MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(Activity, respond);
                }
                else
                {
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    {
                        foreach (var item in from item in result.Data let check = ListUtils.VisitsList.FirstOrDefault(a => a.NotifierId == item.Id) where check == null select item)
                        {
                            ListUtils.VisitsList.Add(new GetNotificationsObject.Datum
                            {
                                Text = Context.GetText(Resource.String.Lbl_VisitYou),
                                Notifier = item,
                                NotifierId = Convert.ToInt32(item.Id),
                                Type = "visit",
                            });
                        }
                    }
                    else
                    {
                        if (MAdapter.NotificationsList.Count > 10 && !MRecycler.CanScrollVertically(1))
                            Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreUsers), ToastLength.Short)?.Show();
                    }
                }

                Activity?.RunOnUiThread(() => { ShowEmptyPage(true); });
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
                MainScrollEvent.IsLoading = false;
            }
            MainScrollEvent.IsLoading = false;
        }

        private async Task LoadLikesAsync()
        {
            if (Methods.CheckConnectivity())
            {
                var isPro = ListUtils.MyUserInfo?.FirstOrDefault()?.IsPro ?? "0";
                if (isPro == "0")
                {
                    Activity?.RunOnUiThread(() => { ShowEmptyPage(true); });
                    return;
                }
                 
                //int countList = MAdapter.NotificationsList.Count;
                 var (apiStatus, respond) = await RequestsAsync.Users.ProfileAsync(UserDetails.UserId.ToString(), "likes");
                if (apiStatus != 200 || respond is not ProfileObject result || result.Data == null)
                {
                    Methods.DisplayReportResult(Activity, respond);
                }
                else
                {
                    var respondList = result.Data.Likes.Count;
                    if (respondList > 0)
                    {
                        foreach (var item in from item in result.Data.Likes let check = ListUtils.LikesList.FirstOrDefault(a => a.NotifierId == item.Id) where check == null select item)
                        {
                            ListUtils.LikesList.Add(new GetNotificationsObject.Datum
                            {
                                Text = Context.GetText(Resource.String.Lbl_LikeYou),
                                Notifier = item.Data,
                                NotifierId = item.Id,
                                Type = "like"
                            });
                        }
                    }
                    else
                    {
                        if (MAdapter.NotificationsList.Count > 10 && !MRecycler.CanScrollVertically(1))
                            Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreUsers), ToastLength.Short)?.Show();
                    }
                }

                Activity?.RunOnUiThread(() => { ShowEmptyPage(true); });
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
                MainScrollEvent.IsLoading = false;
            }
            MainScrollEvent.IsLoading = false;
        }

        private async Task LoadFriendsRequestsAsync(string offset = "0")
        {
            if (MainScrollEvent.IsLoading || !AppSettings.EnableFriendSystem)
                return;

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;
                int countList = MAdapter.NotificationsList.Count;
                var (apiStatus, respond) = await RequestsAsync.Friends.GetFriendsRequestsAsync("30", offset);
                if (apiStatus != 200 || respond is not ListUsersObject result || result.Data == null)
                {
                    MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(Activity, respond);
                }
                else
                {
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    {
                        foreach (var item  in result.Data)
                        {
                            var duplicateItem = ListUtils.RequestsList.FirstOrDefault(a => a.Notifier.Id == item.Id);
                            ListUtils.RequestsList.Remove(duplicateItem);

                            ListUtils.RequestsList.Add(new GetNotificationsObject.Datum
                            {
                                Text = Context.GetText(Resource.String.Lbl_FriendRequestAccepted),
                                Notifier = item,
                                NotifierId = Convert.ToInt32(item.Id),
                                Type = "friend_request"
                            });
                        }

                        MAdapter.NotificationsList = new ObservableCollection<GetNotificationsObject.Datum>(ListUtils.RequestsList);
                        Activity?.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                    }
                    else
                    {
                        if (MAdapter.NotificationsList.Count > 10 && !MRecycler.CanScrollVertically(1))
                            Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreUsers), ToastLength.Short)?.Show();
                    }
                }
                Activity?.RunOnUiThread(() => { ShowEmptyPage(true); });
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

                Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                MainScrollEvent.IsLoading = false;
            }
            MainScrollEvent.IsLoading = false;
        }
         
        private void ShowEmptyPage(bool showEmpty = false)
        {
            try
            {
                MainScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;

                switch (NameTap)
                {
                    case "Match":
                        MAdapter.NotificationsList = new ObservableCollection<GetNotificationsObject.Datum>(ListUtils.MatchList);
                        break;
                    case "Visits":
                        MAdapter.NotificationsList = new ObservableCollection<GetNotificationsObject.Datum>(ListUtils.VisitsList);
                        break;
                    case "Likes":
                        var isPro = ListUtils.MyUserInfo?.FirstOrDefault()?.IsPro ?? "0";
                        if (isPro != "0")
                            MAdapter.NotificationsList = new ObservableCollection<GetNotificationsObject.Datum>(ListUtils.LikesList);
                        break;
                    case "Requests":
                        MAdapter.NotificationsList = new ObservableCollection<GetNotificationsObject.Datum>(ListUtils.RequestsList);
                        break;
                }

                switch (MAdapter.NotificationsList.Count)
                {
                    case > 0:
                        MRecycler.Visibility = ViewStates.Visible;
                        EmptyStateLayout.Visibility = ViewStates.Gone;

                        Activity?.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                        break;
                    case 0 when showEmpty:
                    {
                        MRecycler.Visibility = ViewStates.Gone;

                        if (Inflated == null)
                            Inflated = EmptyStateLayout.Inflate();

                        EmptyStateInflater x = new EmptyStateInflater();
                        //switch (NameTap)
                        //{
                        //    case "Match":
                        //        x.InflateLayout(Inflated, EmptyStateInflater.Type.NoMatches);
                        //        break;
                        //    case "Visits":
                        //        x.InflateLayout(Inflated, EmptyStateInflater.Type.NoUsers);
                        //        break;
                        //    case "Likes":
                        //        x.InflateLayout(Inflated, EmptyStateInflater.Type.NoUsers);
                        //        break;
                        //    case "Requests":
                        //        x.InflateLayout(Inflated, EmptyStateInflater.Type.NoFriendsRequests);
                        //        break;
                        //    default:
                        //        x.InflateLayout(Inflated, EmptyStateInflater.Type.NoNotifications);
                        //        break;
                        //}

                        if (NameTap == "Likes")
                        {
                            var isPro = ListUtils.MyUserInfo?.FirstOrDefault()?.IsPro ?? "0";
                            x.InflateLayout(Inflated, isPro == "0" ? EmptyStateInflater.Type.GetPremium : EmptyStateInflater.Type.NoNotifications);
                        }
                        else
                            x.InflateLayout(Inflated, EmptyStateInflater.Type.NoNotifications);

                        if (!x.EmptyStateButton.HasOnClickListeners)
                        {
                            x.EmptyStateButton.Click += null!;
                        }
                        EmptyStateLayout.Visibility = ViewStates.Visible;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                MainScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;
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
         
    }
}