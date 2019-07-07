﻿#define USE_ADMOB

#if USE_ADMOB
using GoogleMobileAds.Api;
using System;

namespace UnityEngine.Ad {
    public partial class AdMobManager : Singleton<AdMobManager>, IAdManager, IAdVideoManager, IAdBannerManager {

        private RewardBasedVideoAd rewardBasedVideo;
        private AdVideoRequestParam adVideoRequestParam;


        /// <summary>
        /// 初期化済みか
        /// </summary>
        public bool IsInitialized {
            get;
            private set;
        }

        public bool IsAdVideoLoaded {
            get { return rewardBasedVideo.IsLoaded(); }
        }

        public bool IsAdVideoRewarded {
            get;
            private set;
        }

        /// <summary>
        /// Request [app_id]
        /// </summary>
        /// <param name="adConfigParam"></param>
        public void Initialize(AdConfigParam adConfigParam) {
            if (IsInitialized) {
                Debug.LogError("initialize");
                return;
            }

            // 設定がNullの場合
            if (adConfigParam == null) {
                Debug.LogError("config is null");
                return;
            }
            // 設定ない場合
            if (string.IsNullOrEmpty(adConfigParam.AdMobAppId)) {
                Debug.LogError("config is null");
                return;
            }

            string appId = adConfigParam.AdMobAppId;

            // 初期化処理
            try {
                // Initialize the Google Mobile Ads SDK.
                MobileAds.Initialize(appId);

                // Get singleton reward based video ad reference.
                rewardBasedVideo = RewardBasedVideoAd.Instance;

                #region Ad Video Callback Initialize
                // Called when an ad request has successfully loaded.
                rewardBasedVideo.OnAdLoaded += HandleRewardBasedVideoLoaded;
                // Called when an ad request failed to load.
                rewardBasedVideo.OnAdFailedToLoad += HandleRewardBasedVideoFailedToLoad;
                // Called when an ad is shown.
                rewardBasedVideo.OnAdOpening += HandleRewardBasedVideoOpened;
                // Called when the ad starts to play.
                rewardBasedVideo.OnAdStarted += HandleRewardBasedVideoStarted;
                // Called when the user should be rewarded for watching a video.
                rewardBasedVideo.OnAdRewarded += HandleRewardBasedVideoRewarded;
                // Called when the ad is closed.
                rewardBasedVideo.OnAdClosed += HandleRewardBasedVideoClosed;
                // Called when the ad click caused the user to leave the application.
                rewardBasedVideo.OnAdLeavingApplication += HandleRewardBasedVideoLeftApplication;
                #endregion
            }

            // 初期化失敗時
            catch (Exception ex) {
                Debug.LogError(ex);
                return;
            }

            // 初期化が完了した場合
            IsInitialized = true;
        }

        #region AdMob イベント
        public void HandleRewardBasedVideoLoaded(object sender, EventArgs args) {
            Debug.Log("HandleRewardBasedVideoLoaded event received");
            if (adVideoRequestParam == null) return;
            if (adVideoRequestParam.OnAdVideoLoaded == null) return;
            adVideoRequestParam.OnAdVideoLoaded.Invoke(this);
        }

        public void HandleRewardBasedVideoFailedToLoad(object sender, AdFailedToLoadEventArgs args) {
            Debug.LogError("HandleRewardBasedVideoFailedToLoad event received with message: " + args.Message);
            if (adVideoRequestParam == null) return;
            if (adVideoRequestParam.OnAdVideoFailedToLoad == null) return;
            adVideoRequestParam.OnAdVideoFailedToLoad.Invoke(this);
        }

        public void HandleRewardBasedVideoOpened(object sender, EventArgs args) {
            Debug.Log("HandleRewardBasedVideoOpened event received");
            if (adVideoRequestParam == null) return;
            if (adVideoRequestParam.OnAdVideoOpening == null) return;
            adVideoRequestParam.OnAdVideoOpening.Invoke(this);
        }

        public void HandleRewardBasedVideoStarted(object sender, EventArgs args) {
            Debug.Log("HandleRewardBasedVideoStarted event received");
            if (adVideoRequestParam == null) return;
            if (adVideoRequestParam.OnAdVideoStarted == null) return;
            adVideoRequestParam.OnAdVideoStarted.Invoke(this);
        }

        public void HandleRewardBasedVideoClosed(object sender, EventArgs args) {
            Debug.Log("HandleRewardBasedVideoClosed event received");
            if (adVideoRequestParam == null) return;
            if (adVideoRequestParam.OnAdVideoClosed == null) return;
            adVideoRequestParam.OnAdVideoClosed.Invoke(this);
        }

        public void HandleRewardBasedVideoRewarded(object sender, Reward args) {
            Debug.Log("HandleRewardBasedVideoRewarded event received for " + args.Type + " " + args.Amount);
            IsAdVideoRewarded = true;
            if (adVideoRequestParam == null) return;
            if (adVideoRequestParam.OnAdVideoRewarded == null) return;
            adVideoRequestParam.OnAdVideoRewarded.Invoke(this);
        }

        public void HandleRewardBasedVideoLeftApplication(object sender, EventArgs args) {
            Debug.Log("HandleRewardBasedVideoLeftApplication event received");
            if (adVideoRequestParam == null) return;
            if (adVideoRequestParam.OnAdVideoLeavingApplication == null) return;
            adVideoRequestParam.OnAdVideoLeavingApplication.Invoke(this);
        }
        #endregion

        public void OnAdVideoLoad(AdVideoRequestParam adVideoRequestParam) {
            if (!IsInitialized) {
                Debug.LogError("dont initialize");
                return;
            }

            if (IsAdVideoLoaded) {
                if (adVideoRequestParam == null) return;
                if (adVideoRequestParam.OnAdVideoLoaded == null) return;
                adVideoRequestParam.OnAdVideoLoaded(this);
                return;
            }

            // 設定がNullの場合
            if (adVideoRequestParam == null) {
                Debug.LogError("config is null");
                return;
            }

            this.adVideoRequestParam = adVideoRequestParam;
            OnAdVideoLoadRetry();
        }

        /// <summary>
        /// 動画の読み込みのみ行う
        /// </summary>
        public void OnAdVideoLoadRetry() {
            if (!IsInitialized) {
                Debug.LogError("dont initialize");
                return;
            }

            if (adVideoRequestParam == null) {
                Debug.LogError("AdVideoRequestParam is null");
                return;
            }

            if (IsAdVideoLoaded) {
                if (adVideoRequestParam == null) return;
                if (adVideoRequestParam.OnAdVideoLoaded == null) return;
                adVideoRequestParam.OnAdVideoLoaded(this);
                return;
            }

            try {
                // Create an empty ad request.
                AdRequest request = new AdRequest.Builder().Build();
                // Load the rewarded video ad with the request.
                rewardBasedVideo.LoadAd(request, adVideoRequestParam.AdMobAdUnitId);
            }

            // 初期化失敗時
            catch (Exception ex) {
                Debug.LogError(ex);
                return;
            }
        }

        /// <summary>
        /// 動画再生
        /// </summary>
        public void OnAdVideoShow() {
            if (!IsInitialized) {
                Debug.LogError("dont initialized");
                return;
            }
            if (!IsAdVideoLoaded) {
                Debug.LogError("dont loaded");
                return;
            }

            try {
                // 動画を見始める前にリワードを初期化
                IsAdVideoRewarded = false;
                rewardBasedVideo.Show();
            }
            // 初期化失敗時
            catch (Exception ex) {
                Debug.LogError(ex);
                return;
            }
        }
    }

    #region Banner
    public partial class AdMobManager {
        private BannerView bannerView;
        private AdBannerRequestParam adBannerRequestParam;
        

        public bool IsAdBannerLoading {
            get;
            private set;
        }

        public bool IsAdBannerLoaded {
            get;
            private set;
        }

        public void OnAdBannerLoad(AdBannerRequestParam adBannerRequestParam) {
            if (!IsInitialized) {
                Debug.LogError("dont initialize");
                return;
            }

            if (IsAdBannerLoading) {
                Debug.LogError("loading now");
                return;
            }

            if (IsAdBannerLoaded) {
                if (adBannerRequestParam == null) return;
                if (adBannerRequestParam.OnAdBannerLoaded == null) return;
                adBannerRequestParam.OnAdBannerLoaded(this);
                return;
            }

            // 設定がNullの場合
            if (adBannerRequestParam == null) {
                Debug.LogError("config is null");
                return;
            }

            this.adBannerRequestParam = adBannerRequestParam;
            OnAdBannerLoadRetry();
        }

        public void OnAdBannerLoadRetry() {
            if (!IsInitialized) {
                Debug.LogError("dont initialize");
                return;
            }

            if (IsAdBannerLoading) {
                Debug.LogError("loading now");
                return;
            }

            if (IsAdBannerLoaded) {
                if (adBannerRequestParam == null) return;
                if (adBannerRequestParam.OnAdBannerLoaded == null) return;
                adBannerRequestParam.OnAdBannerLoaded(this);
                return;
            }

            try {
                // 初期化
                IsAdBannerLoading = true;
                IsAdBannerLoaded = false;

                bannerView = new BannerView(adBannerRequestParam.AdMobAdUnitId, adBannerRequestParam.AdMobAdSize, adBannerRequestParam.AdMobAdPosition);
                bannerView.OnAdLoaded += BannerViewOnAdLoaded;
                bannerView.OnAdFailedToLoad += BannerViewOnAdFailedToLoad;
                bannerView.OnAdOpening += BannerViewOnAdOpening;
                bannerView.OnAdClosed += BannerViewOnAdClosed;
                bannerView.OnAdLeavingApplication += BannerViewOnAdLeavingApplication;
                // Create an empty ad request.
                AdRequest request = new AdRequest.Builder().Build();
                // Load the rewarded video ad with the request.
                bannerView.LoadAd(request);
            }

            // 初期化失敗時
            catch (Exception ex) {
                Debug.LogError(ex);
                return;
            }
        }

        public void OnAdBannerShow() {
            if (!IsInitialized) {
                Debug.LogError("dont initialize");
                return;
            }
            if (bannerView == null) {
                Debug.LogError("BannerView is null");
                return;
            }
            try {
                bannerView.Show();
            }

            // 初期化失敗時
            catch (Exception ex) {
                Debug.LogError(ex);
                return;
            }
        }

        public void OnAdBannerHide() {
            if (!IsInitialized) {
                Debug.LogError("dont initialize");
                return;
            }
            if (bannerView == null) {
                Debug.LogError("BannerView is null");
                return;
            }
            try {
                bannerView.Hide();
            }

            // 初期化失敗時
            catch (Exception ex) {
                Debug.LogError(ex);
                return;
            }
        }

        private void BannerViewOnAdLoaded(object sender, EventArgs args) {
            IsAdBannerLoading = false;
            IsAdBannerLoaded = true;
            if (adBannerRequestParam == null) return;
            if (adBannerRequestParam.OnAdBannerLoaded == null) return;
            adBannerRequestParam.OnAdBannerLoaded.Invoke(this);
        }
        private void BannerViewOnAdFailedToLoad(object sender, EventArgs args) {
            IsAdBannerLoading = false;
            if (adBannerRequestParam == null) return;
            if (adBannerRequestParam.OnAdBannerFailedToLoad == null) return;
            adBannerRequestParam.OnAdBannerFailedToLoad.Invoke(this);
        }

        private void BannerViewOnAdOpening(object sender, EventArgs args) {
            if (adBannerRequestParam == null) return;
            if (adBannerRequestParam.OnAdBannerOpening == null) return;
            adBannerRequestParam.OnAdBannerOpening.Invoke(this);

        }
        private void BannerViewOnAdClosed(object sender, EventArgs args) {
            if (adBannerRequestParam == null) return;
            if (adBannerRequestParam.OnAdBannerClosed == null) return;
            adBannerRequestParam.OnAdBannerClosed.Invoke(this);

        }
        private void BannerViewOnAdLeavingApplication(object sender, EventArgs args) {
            if (adBannerRequestParam == null) return;
            if (adBannerRequestParam.OnAdBannerLeavingApplication == null) return;
            adBannerRequestParam.OnAdBannerLeavingApplication.Invoke(this);

        }
    }
    #endregion
}
#endif