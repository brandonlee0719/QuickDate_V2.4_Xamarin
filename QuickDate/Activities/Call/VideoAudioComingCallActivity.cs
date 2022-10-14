using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MaterialDialogsCore;
using Android.Animation;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics.Drawables;
using Android.Hardware;
using Android.OS; 
using Android.Text;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.Content;
using AT.Markushi.UI;
using Newtonsoft.Json;
using QuickDate.Activities.Call.Agora;
using QuickDate.Activities.Call.Twilio;
using QuickDate.Activities.Tabbes;
using QuickDate.Helpers.CacheLoaders;
using QuickDate.Helpers.Controller;
using QuickDate.Helpers.Utils;
using QuickDateClient.Classes.Call;
using QuickDateClient.Classes.Chat;
using QuickDateClient.Requests;
using Exception = System.Exception;

namespace QuickDate.Activities.Call
{
    [Activity(Icon ="@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize, ScreenOrientation = ScreenOrientation.Portrait)]
    public class VideoAudioComingCallActivity : AppCompatActivity, ValueAnimator.IAnimatorUpdateListener, ISensorEventListener, MaterialDialog.ISingleButtonCallback, MaterialDialog.IListCallback, MaterialDialog.IInputCallback
    {

        private string CallType = "0";
        private DataCallObject CallUserObject;

        private ImageView UserImageView;
        private TextView UserNameTextView, TypeCallTextView;
        private View GradientPreView;
        public static VideoAudioComingCallActivity CallActivity;
        private GradientDrawable GradientDrawableView;
        private int Start, Mid, End;

        private CircleButton AcceptCallButton, RejectCallButton, MessageCallButton;

        public static bool IsActive;

        private SensorManager SensorManager;
        private Sensor Proximity;
        private readonly int SensorSensitivity = 4;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                 
                SetContentView(Resource.Layout.TwilioCommingVideoCallLayout);
                Window?.AddFlags(WindowManagerFlags.KeepScreenOn);

                SensorManager = (SensorManager)GetSystemService(SensorService);
                Proximity = SensorManager.GetDefaultSensor(SensorType.Proximity); 

                CallActivity = this;

                CallType = Intent?.GetStringExtra("type") ?? "";

                if (!string.IsNullOrEmpty(Intent?.GetStringExtra("callUserObject")))
                    CallUserObject = JsonConvert.DeserializeObject<DataCallObject>(Intent?.GetStringExtra("callUserObject") ?? "");

                UserNameTextView = FindViewById<TextView>(Resource.Id.UsernameTextView);
                TypeCallTextView = FindViewById<TextView>(Resource.Id.TypecallTextView);
                UserImageView = FindViewById<ImageView>(Resource.Id.UserImageView);
                GradientPreView = FindViewById<View>(Resource.Id.gradientPreloaderView);
                AcceptCallButton = FindViewById<CircleButton>(Resource.Id.accept_call_button);
                RejectCallButton = FindViewById<CircleButton>(Resource.Id.end_call_button);
                MessageCallButton = FindViewById<CircleButton>(Resource.Id.message_call_button);

                StartAnimatedBackground();

                AcceptCallButton.Click += AcceptCallButton_Click;
                RejectCallButton.Click += RejectCallButton_Click;
                MessageCallButton.Click += MessageCallButton_Click;

                if (!string.IsNullOrEmpty(CallUserObject.Fullname))
                    UserNameTextView.Text = CallUserObject.Fullname;

                if (!string.IsNullOrEmpty(CallUserObject.Avater))
                    GlideImageLoader.LoadImage(this, CallUserObject.Avater, UserImageView, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                if (CallType == "Twilio_video_call" || CallType == "Agora_video_call_recieve")
                    TypeCallTextView.Text = GetText(Resource.String.Lbl_Video_call);
                else
                    TypeCallTextView.Text = GetText(Resource.String.Lbl_Voice_call);

                Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("mystic_call.mp3" , "Looping"); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            } 
        }

        protected override void OnStart()
        {
            base.OnStart();
            IsActive = true;
        }

        protected override void OnStop()
        {
            base.OnStop();
            IsActive = false;
        }


        protected override void OnResume()
        {
            try
            {
                base.OnResume();  
                SensorManager.RegisterListener(this, Proximity,SensorDelay.Normal ); 
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
                base.OnPause();
                SensorManager.UnregisterListener(this);
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

        private void MessageCallButton_Click(object sender, EventArgs e)
        {
            try
            {

                if (Methods.CheckConnectivity())
                {
                    var arrayAdapter = new List<string>();
                    var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                    arrayAdapter.Add(GetString(Resource.String.Lbl_MessageCall1));
                    arrayAdapter.Add(GetString(Resource.String.Lbl_MessageCall2));
                    arrayAdapter.Add(GetString(Resource.String.Lbl_MessageCall3));
                    arrayAdapter.Add(GetString(Resource.String.Lbl_MessageCall4));
                    arrayAdapter.Add(GetString(Resource.String.Lbl_MessageCall5));
                     
                    dialogList.Items(arrayAdapter);
                    dialogList.PositiveText(GetText(Resource.String.Lbl_Close)).OnNegative(this);
                    dialogList.AlwaysCallSingleChoiceCallback();
                    dialogList.ItemsCallback(this).Build().Show();
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception); 
            }
        }

        private void RejectCallButton_Click(object sender, EventArgs e)
        {
            try
            {
                switch (CallType)
                {
                    case "Agora_video_call_recieve":
                    case "Twilio_video_call":
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Call.DeclineCallAsync(CallUserObject.Id, TypeCall.Video) });
                        break;
                    case "Agora_audio_call_recieve":
                    case "Twilio_audio_call":
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Call.DeclineCallAsync(CallUserObject.Id, TypeCall.Audio) });
                        break;
                }

                FinishVideoAudio();
            }
            catch (Exception exception)
            {
                FinishVideoAudio();
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void AcceptCallButton_Click(object sender, EventArgs e)
        {
            try
            {
                switch (CallType)
                {
                    case "Twilio_video_call":
                        {
                            Intent intent = new Intent(this, typeof(TwilioVideoCallActivity));
                            intent.SetFlags(ActivityFlags.TaskOnHome | ActivityFlags.BroughtToFront);
                            intent.PutExtra("callUserObject", JsonConvert.SerializeObject(CallUserObject));
                            intent.PutExtra("type", CallType);
                            StartActivity(intent);
                            break;
                        }
                    case "Twilio_audio_call":
                        {
                            Intent intent = new Intent(this, typeof(TwilioAudioCallActivity));
                            intent.SetFlags(ActivityFlags.TaskOnHome | ActivityFlags.BroughtToFront | ActivityFlags.NewTask);
                            intent.PutExtra("callUserObject", JsonConvert.SerializeObject(CallUserObject));
                            intent.PutExtra("type", CallType);
                            StartActivity(intent);
                            break;
                        }
                    case "Agora_audio_call_recieve":
                        {
                            Intent intent = new Intent(this, typeof(AgoraAudioCallActivity));
                            intent.SetFlags(ActivityFlags.TaskOnHome | ActivityFlags.BroughtToFront | ActivityFlags.NewTask);
                            intent.PutExtra("callUserObject", JsonConvert.SerializeObject(CallUserObject));
                            intent.PutExtra("type", CallType);
                            StartActivity(intent);
                            break;
                        }
                    case "Agora_video_call_recieve":
                        {
                            Intent intent = new Intent(this, typeof(AgoraVideoCallActivity));

                            intent.SetFlags(ActivityFlags.TaskOnHome | ActivityFlags.BroughtToFront | ActivityFlags.NewTask);
                            intent.PutExtra("callUserObject", JsonConvert.SerializeObject(CallUserObject));
                            intent.PutExtra("type", CallType);
                            StartActivity(intent);
                            break;
                        }
                }

                FinishVideoAudio(); 
            }
            catch (Exception exception)
            {
                FinishVideoAudio();
                Methods.DisplayReportResultTrack(exception);
            }
        }
           
        private void StartAnimatedBackground()
        {
            GradientDrawableView = (GradientDrawable)GradientPreView.Background;
             Start = ContextCompat.GetColor(this, Resource.Color.accent);
             Mid = ContextCompat.GetColor(this, Resource.Color.primaryDark);
             End = ContextCompat.GetColor(this, Resource.Color.extraDark);
            var animator = ValueAnimator.OfFloat(0.0f, 1.0f);
            animator.SetDuration(1500);
            animator.RepeatCount = 1000;
            animator.RepeatMode = ValueAnimatorRepeatMode.Reverse;
            animator.AddUpdateListener(this); 
            animator.Start();
        }

        public void OnAnimationUpdate(ValueAnimator animation)
        {
            try
            {
                var evaluator = new ArgbEvaluator();
                var newStart = (int)evaluator.Evaluate(animation.AnimatedFraction, Start, End);
                var newMid = (int)evaluator.Evaluate(animation.AnimatedFraction, Mid, Start);
                var newEnd = (int)evaluator.Evaluate(animation.AnimatedFraction, End, Mid);
                int[] newArray = { newStart, newMid, newEnd };
                GradientDrawableView.SetColors(newArray);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }

        }

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
                if (e.Sensor.Type == SensorType.Proximity) 
                {
                    if (e.Values[0] >= -SensorSensitivity && e.Values[0] <= SensorSensitivity)
                    {
                        //near 
                        HomeActivity.GetInstance()?.SetOffWakeLock(); 
                    }
                    else
                    {
                        //far 
                        HomeActivity.GetInstance()?.SetOnWakeLock();  
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion
         
        #region MaterialDialog

        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (p1 == DialogAction.Positive)
                {
                }
                else if (p1 == DialogAction.Negative)
                {
                    p0.Dismiss();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                string text = itemString;

                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
                else
                {
                    if (text == GetString(Resource.String.Lbl_MessageCall5))
                    {
                        var dialogBuilder = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light); 
                        dialogBuilder.Input(Resource.String.Lbl_Write_your_message, 0, false, this);
                        dialogBuilder.InputType(InputTypes.TextFlagImeMultiLine);
                        dialogBuilder.PositiveText(GetText(Resource.String.Lbl_Send)).OnPositive(this);
                        dialogBuilder.NegativeText(GetText(Resource.String.Lbl_Cancel)).OnNegative(this);
                        dialogBuilder.Build().Show();
                        dialogBuilder.AlwaysCallSingleChoiceCallback();
                    }
                    else
                    {
                        SendMess(text);
                    } 
                }  
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnInput(MaterialDialog dialog, string itemString)
        {
            try
            {
                if (itemString.Length > 0)
                {
                    var text = itemString;
                    SendMess(text);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
         
        private async void SendMess(string text)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
                else
                {
                    var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    var hashId = unixTimestamp.ToString();

                    //Here on This function will send Selected audio file to the user 
                    var (apiStatus, respond) = await RequestsAsync.Chat.SendMessageAsync(CallUserObject.ToId, text, "", "", hashId);
                    if (apiStatus == 200)
                    {
                        if (respond is SendMessageObject result)
                        {
                           Console.WriteLine(result.Message);
                            if (!string.IsNullOrEmpty(CallUserObject.Id))
                            {
                                switch (CallType)
                                {
                                    case "Agora_video_call_recieve":
                                    case "Twilio_video_call":
                                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Call.DeclineCallAsync(CallUserObject.Id, TypeCall.Video) });
                                        break;
                                    case "Agora_audio_call_recieve":
                                    case "Twilio_audio_call":
                                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Call.DeclineCallAsync(CallUserObject.Id, TypeCall.Audio) });
                                        break;
                                }
                            }

                            FinishVideoAudio();
                        }
                    }
                    else Methods.DisplayReportResult(this, respond);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void FinishVideoAudio()
        {
            try
            {
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