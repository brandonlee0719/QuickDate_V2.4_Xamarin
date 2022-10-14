using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Hardware;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using AT.Markushi.UI;
using Newtonsoft.Json;
using QuickDate.Activities.Tabbes;
using QuickDate.Helpers.CacheLoaders;
using QuickDate.Helpers.Controller;
using QuickDate.Helpers.Utils;
using QuickDateClient.Classes.Call;
using QuickDateClient.Classes.Global;
using QuickDateClient.Requests;
using TwilioVideo;
using Manifest = Android.Manifest;
using VideoView = TwilioVideo.VideoView;

namespace QuickDate.Activities.Call.Twilio
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize, ResizeableActivity = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class TwilioVideoCallActivity : AppCompatActivity, ISensorEventListener, TwilioVideoHelper.IListener
    { 
        #region Variables Basic

        private TwilioVideoHelper TwilioVideo { get; set; }
        private string CallType = "0";
        private DataCallObject CallUserObject;

        private RelativeLayout MainUserViewProfile;
        private Button SwitchCamButton;
        private CircleButton EndCallButton, MuteAudioButton, MuteVideoButton;
        private ImageView UserImageView, PictureInToPictureButton;
        private TextView UserNameTextView, NoteTextView;
        private Timer TimerRequestWaiter = new Timer();
        private VideoView UserPrimaryVideo, ThumbnailVideo;
        private LocalVideoTrack LocalVideoTrack;
        private VideoTrack UserVideoTrack;
        private bool DataUpdated;
        private int CountSecondsOfOutgoingCall;
        private string LocalVideoTrackId, RemoteVideoTrackId;
        private HomeActivity GlobalContext;
        private SensorManager SensorManager;
        private Sensor Proximity;
        private readonly int SensorSensitivity = 4;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                Methods.App.FullScreenApp(this);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                Window?.AddFlags(WindowManagerFlags.KeepScreenOn);

                // Create your application here
                SetContentView(Resource.Layout.TwilioVideoCallActivityLayout);

                SensorManager = (SensorManager)GetSystemService(SensorService);
                Proximity = SensorManager?.GetDefaultSensor(SensorType.Proximity);

                GlobalContext = HomeActivity.GetInstance();
                //Get Value And Set Toolbar
                InitComponent();
                InitTwilioCall();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                SensorManager.RegisterListener(this, Proximity, SensorDelay.Normal);
                AddOrRemoveEvent(true);
                UpdateState();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnStart()
        {
            try
            {
                base.OnStart();
                UpdateState();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnPause()
        {
            try
            {
                DataUpdated = false;
                base.OnPause();
                AddOrRemoveEvent(false);
                SensorManager.UnregisterListener(this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnRestart()
        {
            try
            {
                base.OnRestart();
                TwilioVideo = TwilioVideoHelper.GetOrCreate(this , TypeCall.Video);
                UpdateState();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
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

        private void InitComponent()
        {
            try
            {
                MainUserViewProfile = FindViewById<RelativeLayout>(Resource.Id.userInfoview_container);
                UserPrimaryVideo = FindViewById<VideoView>(Resource.Id.userthumbnailVideo); // userthumbnailVideo 
                ThumbnailVideo = FindViewById<VideoView>(Resource.Id.local_video_view_container); //local_video_view_container
                SwitchCamButton = FindViewById<Button>(Resource.Id.switch_cam_button);
                MuteVideoButton = FindViewById<CircleButton>(Resource.Id.mute_video_button);
                EndCallButton = FindViewById<CircleButton>(Resource.Id.end_call_button);
                MuteAudioButton = FindViewById<CircleButton>(Resource.Id.mute_audio_button);
                UserImageView = FindViewById<ImageView>(Resource.Id.userImageView);
                UserNameTextView = FindViewById<TextView>(Resource.Id.userNameTextView);
                NoteTextView = FindViewById<TextView>(Resource.Id.noteTextView);

                PictureInToPictureButton = FindViewById<ImageView>(Resource.Id.pictureintopictureButton);
                if (!PackageManager.HasSystemFeature(PackageManager.FeaturePictureInPicture))
                    PictureInToPictureButton.Visibility = ViewStates.Gone;

                MuteVideoButton.Selected = true;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    SwitchCamButton.Click += Switch_cam_button_Click;
                    MuteVideoButton.Click += Mute_video_button_Click;
                    EndCallButton.Click += End_call_button_Click;
                    MuteAudioButton.Click += Mute_audio_button_Click;
                    PictureInToPictureButton.Click += PictureInToPictureButton_Click;
                }
                else
                {
                    SwitchCamButton.Click -= Switch_cam_button_Click;
                    MuteVideoButton.Click -= Mute_video_button_Click;
                    EndCallButton.Click -= End_call_button_Click;
                    MuteAudioButton.Click -= Mute_audio_button_Click;
                    PictureInToPictureButton.Click -= PictureInToPictureButton_Click;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void PictureInToPictureButton_Click(object sender, EventArgs e)
        {
            try
            {
                var actions = new List<RemoteAction>();
                //.SetActions(new List<RemoteAction>().Add(new RemoteAction().Title = "")
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    var param = new PictureInPictureParams.Builder().SetAspectRatio(new Rational(9, 16)).Build();
                    EnterPictureInPictureMode(param);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Switch_cam_button_Click(object sender, EventArgs e)
        {
            try
            {
                TwilioVideo.FlipCamera();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Mute_video_button_Click(object sender, EventArgs e)
        {
            try
            {
                if (MuteVideoButton.Selected)
                {
                    MuteVideoButton.SetImageResource(Resource.Drawable.ic_camera_video_mute);

                    MuteVideoButton.Selected = false;
                }
                else
                {
                    MuteVideoButton.SetImageResource(Resource.Drawable.ic_camera_video_open);
                    MuteVideoButton.Selected = true;
                }

                var isVideoEnabled = MuteVideoButton.Selected;
                FindViewById(Resource.Id.local_video_container).Visibility = isVideoEnabled ? ViewStates.Visible : ViewStates.Gone;
                LocalVideoTrack.Enable(isVideoEnabled);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Mute_audio_button_Click(object sender, EventArgs e)
        {

            try
            {
                if (MuteAudioButton.Selected)
                {
                    MuteAudioButton.Selected = false;
                    MuteAudioButton.SetImageResource(Resource.Drawable.ic_camera_mic_open);
                }
                else
                {
                    MuteAudioButton.Selected = true;
                    MuteAudioButton.SetImageResource(Resource.Drawable.ic_camera_mic_mute);
                }

                TwilioVideo.Mute(MuteAudioButton.Selected);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void End_call_button_Click(object sender, EventArgs e)
        {
            try
            {
                FinishCall();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Sensor System

        public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
        {
            try
            {
                // Do something here if sensor accuracy changes.
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnSensorChanged(SensorEvent e)
        {
            try
            {
                if (e?.Sensor?.Type == SensorType.Proximity)
                {
                    if (e?.Values[0] >= -SensorSensitivity && e.Values[0] <= SensorSensitivity)
                    {
                        //near 
                        GlobalContext?.SetOffWakeLock();
                    }
                    else
                    {
                        //far 
                        GlobalContext?.SetOnWakeLock();
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Twilio  

        private async void InitTwilioCall()
        {
            try
            {
                bool granted =
                    ContextCompat.CheckSelfPermission(ApplicationContext, Manifest.Permission.Camera) ==
                    Permission.Granted &&
                    ContextCompat.CheckSelfPermission(ApplicationContext, Manifest.Permission.RecordAudio) ==
                    Permission.Granted;

                CheckVideoCallPermissions(granted);

                CallType = Intent?.GetStringExtra("type") ?? ""; // Twilio_video_call , Twilio_audio_call 

                if (!string.IsNullOrEmpty(Intent?.GetStringExtra("callUserObject")))
                    CallUserObject = JsonConvert.DeserializeObject<DataCallObject>(Intent?.GetStringExtra("callUserObject") ?? "");
                 
                switch (CallType)
                {
                    case "Twilio_video_call":
                    {
                        if (!string.IsNullOrEmpty(CallUserObject.AccessToken))
                        {
                            if (!string.IsNullOrEmpty(CallUserObject.ToId))
                                Load_userWhenCall();

                            TwilioVideo = TwilioVideoHelper.GetOrCreate(this, TypeCall.Video);
                            UpdateState();
                            NoteTextView.Text = GetText(Resource.String.Lbl_Waiting_for_answer);

                            var (apiStatus, respond) = await RequestsAsync.Call.SendAnswerCallAsync(CallUserObject.Id, TypeCall.Video);
                            if (apiStatus == 200)
                            {
                                if (respond is AnswerCallObject result)
                                {
                                    if (result.Data != null)
                                    {
                                        ConnectToRoom();
                                    }
                                }
                            }
                            else Methods.DisplayReportResult(this, respond);
                        }

                        break;
                    }
                    case "Twilio_video_calling_start":
                        StartApiService();

                        NoteTextView.Text = GetText(Resource.String.Lbl_Calling_video);
                        TwilioVideo = TwilioVideoHelper.GetOrCreate(this, TypeCall.Video);

                        Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("mystic_call.mp3");

                        UpdateState();
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void Load_userWhenCall()
        {
            try
            {
                MainUserViewProfile.Visibility = ViewStates.Visible;
                UserNameTextView.Text = CallUserObject.Fullname;

                //profile_picture
                GlideImageLoader.LoadImage(this, CallUserObject.Avater, UserImageView, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadProfileFromUserId });
        }

        private async Task LoadProfileFromUserId()
        {
            Load_userWhenCall();
            var (apiStatus, respond) = await RequestsAsync.Call.CreateNewCallAsync(CallUserObject.ToId, TypeCall.Video);
            if (apiStatus == 200)
            {
                if (respond is CreateNewCallObject result)
                {
                    CallUserObject.Id = result.Data.Id.ToString();
                    CallUserObject.AccessToken = result.Data.AccessToken;
                    CallUserObject.AccessToken2 = result.Data.AccessToken2;
                    CallUserObject.RoomName = result.Data.RoomName;

                    TimerRequestWaiter = new Timer { Interval = 5000 };
                    TimerRequestWaiter.Elapsed += TimerCallRequestAnswer_Waiter_Elapsed;
                    TimerRequestWaiter.Start();
                }
            }
            else
            {
                FinishCall();

                Methods.DisplayReportResult(this, respond);
            }
        }

        private async void TimerCallRequestAnswer_Waiter_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                var (apiStatus, respond) = await RequestsAsync.Call.CheckForAnswerAsync(CallUserObject.Id, TypeCall.Audio);
                switch (apiStatus)
                {
                    case 200:
                    {
                        if (respond is AnswerCallObject result)
                        {
                            Methods.AudioRecorderAndPlayer.StopAudioFromAsset();
                                 
                            if (!string.IsNullOrEmpty(CallUserObject.AccessToken))
                            { 
                                RunOnUiThread(async () =>
                                {
                                    try
                                    {
                                        TimerRequestWaiter.Enabled = false;
                                        TimerRequestWaiter.Stop();
                                        TimerRequestWaiter.Close();

                                        await Task.Delay(1000);

                                        TwilioVideo?.UpdateToken(CallUserObject.AccessToken2);
                                        TwilioVideo?.JoinRoom(ApplicationContext, CallUserObject.RoomName);
                                    }
                                    catch (Exception exception)
                                    {
                                        Methods.DisplayReportResultTrack(exception);
                                    }
                                });
                            }
                        }

                        break;
                    }
                    case 300:
                    {
                        if (respond is InfoObject result)
                        {
                            switch (result.Message)
                            {
                                case "calling" when CountSecondsOfOutgoingCall < 70:
                                    CountSecondsOfOutgoingCall += 10;
                                    break;
                                case "calling":
                                    RunOnUiThread(() =>
                                    {
                                        try
                                        {
                                            //Call Is inactive 
                                            TimerRequestWaiter.Enabled = false;
                                            TimerRequestWaiter.Stop();
                                            TimerRequestWaiter.Close();
                                            FinishCall();
                                        }
                                        catch (Exception exception)
                                        {
                                            Methods.DisplayReportResultTrack(exception);
                                        }
                                    }); 
                                    break;
                                case "declined":
                                    RunOnUiThread(() =>
                                    {
                                        try
                                        {
                                            //Call Is inactive 
                                            TimerRequestWaiter.Enabled = false;
                                            TimerRequestWaiter.Stop();
                                            TimerRequestWaiter.Close();
                                            FinishCall();
                                        }
                                        catch (Exception exception)
                                        {
                                            Methods.DisplayReportResultTrack(exception);
                                        }
                                    }); 
                                    break;
                            }
                        }

                        break;
                    }
                    default:
                        RunOnUiThread(() =>
                        {
                            try
                            {
                                //Call Is inactive 
                                TimerRequestWaiter.Enabled = false;
                                TimerRequestWaiter.Stop();
                                TimerRequestWaiter.Close();
                                 
                                FinishCall();

                                Methods.DisplayReportResult(this, respond);
                            }
                            catch (Exception exception)
                            {
                                Methods.DisplayReportResultTrack(exception);
                            }
                        });
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region TwilioVideoHelper.IListener

        public void SetLocalVideoTrack(LocalVideoTrack track)
        {
            try
            {
                if (LocalVideoTrack == null)
                {
                    LocalVideoTrack = track;
                    var trackId = track?.Name;
                    if (LocalVideoTrackId == trackId)
                    {
                    }
                    else
                    {
                        LocalVideoTrackId = trackId;
                        LocalVideoTrack.AddSink(ThumbnailVideo);
                        ThumbnailVideo.Visibility = LocalVideoTrack == null ? ViewStates.Invisible : ViewStates.Visible;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetRemoteVideoTrack(VideoTrack track)
        {
            try
            {
                var trackId = track?.Name;

                if (RemoteVideoTrackId == trackId)
                    return;

                RemoteVideoTrackId = trackId;
                if (UserVideoTrack == null)
                {
                    UserVideoTrack = track;
                    UserVideoTrack?.AddSink(UserPrimaryVideo);
                    ThumbnailVideo.Visibility = LocalVideoTrack == null ? ViewStates.Invisible : ViewStates.Visible;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void RemoveLocalVideoTrack(LocalVideoTrack track)
        {
            try
            {
                SetLocalVideoTrack(null);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void RemoveRemoteVideoTrack(VideoTrack track)
        {
            try
            {
                MainUserViewProfile.Visibility = ViewStates.Visible;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnRoomConnected(string roomId)
        {

        }

        public void OnRoomDisconnected(TwilioVideoHelper.StopReason reason)
        {
            try
            {
                Toast.MakeText(this, GetString(Resource.String.Lbl_Room_Disconnected), ToastLength.Short)?.Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnParticipantConnected(string participantId)
        {
            try
            {
                MainUserViewProfile.Visibility = ViewStates.Gone;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnParticipantDisconnected(string participantId)
        {
            RunOnUiThread(FinishCall);
        }

        public void SetCallTime(int seconds)
        {

        }

        #endregion

        #region Permissions

        private void RequestCameraAndMicrophonePermissions()
        {
            if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.Camera) ||
                ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.RecordAudio))
                Toast.MakeText(this, GetString(Resource.String.Lbl_Need_Camera), ToastLength.Short)?.Show(); 
            else
                ActivityCompat.RequestPermissions(this,
                    new[] { Manifest.Permission.Camera, Manifest.Permission.RecordAudio, Manifest.Permission.ModifyAudioSettings }, 1);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions,
            [GeneratedEnum] Permission[] grantResults)
        {
            if (requestCode == 1)
                CheckVideoCallPermissions(grantResults.Any(p => p == Permission.Denied));
        }

        private void CheckVideoCallPermissions(bool granted)
        {
            if (!granted)
                RequestCameraAndMicrophonePermissions();
        }


        #endregion

        private void ConnectToRoom()
        {
            TwilioVideo?.UpdateToken(CallUserObject.AccessToken);
            TwilioVideo?.JoinRoom(this, CallUserObject.RoomName);
        }

        private void UpdateState()
        {
            try
            {
                if (DataUpdated)
                    return;
                DataUpdated = true;
                TwilioVideo?.Bind(this);
                UpdatingState();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override bool OnSupportNavigateUp()
        {
            TryCancelCall();
            return true;
        }

        protected virtual void UpdatingState()
        {
        }

        private void TryCancelCall()
        {
            CloseScreen();
        }

        private void CloseScreen()
        {
            Finish();
        }

        public override void OnBackPressed()
        {
            FinishCall();
        }

        protected virtual void FinishCall()
        {
            try
            {
                //Close Api Starts here >> 
                if (!Methods.CheckConnectivity())
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show(); 
                else
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Call.DeleteCallAsync(CallUserObject.Id, TypeCall.Video) });

                if (TwilioVideo != null && TwilioVideo.ClientIsReady)
                {
                    TwilioVideo.Unbind(this);
                    TwilioVideo.FinishCall();
                }
                Methods.AudioRecorderAndPlayer.StopAudioFromAsset();
                Finish();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}