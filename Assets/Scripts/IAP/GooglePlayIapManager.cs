using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR || (UNITY_ANDROID && GOOGLE_PLAY)
using UnityEngine.Purchasing;
#endif

public class GooglePlayIapManager : MonoBehaviour
{
    public enum IapState
    {
        Unsupported,
        Initializing,
        Ready,
        Failed
    }

    private static GooglePlayIapManager instance;

    public static bool HasInstance => instance != null;

    public static GooglePlayIapManager Instance
    {
        get
        {
            if (instance == null)
            {
                CreateInstance();
            }

            return instance;
        }
    }

    public static bool IsSupported
    {
        get
        {
#if UNITY_EDITOR || (UNITY_ANDROID && GOOGLE_PLAY)
            return true;
#else
            return false;
#endif
        }
    }

    public event Action OnStateChanged;

    public IapState State { get; private set; } = IapState.Unsupported;
    public bool IsReady => State == IapState.Ready;
    public bool IsPurchasing { get; private set; }
    public string RemoveAdsLocalizedPrice { get; private set; } = string.Empty;
    public string LastStatusMessage { get; private set; } = "IAP has not started.";

#if UNITY_EDITOR || (UNITY_ANDROID && GOOGLE_PLAY)
    private StoreController storeController;
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (IsSupported)
        {
            CreateInstance();
        }
    }

    private static void CreateInstance()
    {
        if (instance != null)
        {
            return;
        }

        var gameObject = new GameObject(nameof(GooglePlayIapManager));
        instance = gameObject.AddComponent<GooglePlayIapManager>();
        DontDestroyOnLoad(gameObject);
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Initialize();
    }

    public async void Initialize()
    {
        if (!IsSupported)
        {
            SetStatusMessage("IAP is not supported in this build.");
            SetState(IapState.Unsupported);
            return;
        }

        if (State == IapState.Initializing || State == IapState.Ready)
        {
            return;
        }

        SetState(IapState.Initializing);
        LogEnvironment();
        SetStatusMessage("Connecting to Google Play Billing...");

#if UNITY_EDITOR || (UNITY_ANDROID && GOOGLE_PLAY)
        try
        {
            storeController = UnityIAPServices.StoreController();
            RegisterCallbacks();

            await storeController.Connect();
            SetStatusMessage("Store connected. Fetching remove ads product...");
            storeController.FetchProducts(CreateProductDefinitions());
        }
        catch (Exception exception)
        {
            SetStatusMessage($"IAP initialization failed: {exception.Message}");
            Debug.LogError($"Unity IAP initialization failed: {exception}");
            SetState(IapState.Failed);
        }
#endif
    }

    public void PurchaseRemoveAds()
    {
        if (RemoveAdsEntitlementStore.HasRemoveAds)
        {
            SetStatusMessage("Remove ads is already owned.");
            Debug.Log("Remove ads is already owned.");
            return;
        }

        if (!IsSupported)
        {
            SetStatusMessage("Remove ads purchase is only supported on the Google Play build.");
            Debug.LogWarning("Remove ads purchase is only supported on the Google Play build.");
            return;
        }

        if (!IsReady)
        {
            SetStatusMessage($"IAP is not ready. Current state: {State}");
            Debug.LogWarning("Unity IAP is not ready yet.");
            Initialize();
            return;
        }

#if UNITY_EDITOR || (UNITY_ANDROID && GOOGLE_PLAY)
        IsPurchasing = true;
        SetStatusMessage("Opening Google Play purchase flow...");
        NotifyStateChanged();
        storeController.PurchaseProduct(GooglePlayProductIds.RemoveAds);
#endif
    }

    public void RestorePurchases()
    {
        if (!IsSupported)
        {
            return;
        }

        if (!IsReady)
        {
            SetStatusMessage($"Cannot restore yet. Current state: {State}");
            Initialize();
            return;
        }

#if UNITY_EDITOR || (UNITY_ANDROID && GOOGLE_PLAY)
        SetStatusMessage("Fetching existing purchases...");
        storeController.FetchPurchases();
#endif
    }

#if UNITY_EDITOR || (UNITY_ANDROID && GOOGLE_PLAY)
    private static List<ProductDefinition> CreateProductDefinitions()
    {
        Debug.Log($"[GooglePlayIAP] Requested product: {GooglePlayProductIds.RemoveAds}, type: {ProductType.NonConsumable}");
        return new List<ProductDefinition>
        {
            new ProductDefinition(GooglePlayProductIds.RemoveAds, ProductType.NonConsumable)
        };
    }

    private void RegisterCallbacks()
    {
        storeController.OnStoreConnected -= OnStoreConnected;
        storeController.OnStoreDisconnected -= OnStoreDisconnected;
        storeController.OnProductsFetched -= OnProductsFetched;
        storeController.OnProductsFetchFailed -= OnProductsFetchFailed;
        storeController.OnPurchasesFetched -= OnPurchasesFetched;
        storeController.OnPurchasesFetchFailed -= OnPurchasesFetchFailed;
        storeController.OnPurchasePending -= OnPurchasePending;
        storeController.OnPurchaseConfirmed -= OnPurchaseConfirmed;
        storeController.OnPurchaseFailed -= OnPurchaseFailed;
        storeController.OnPurchaseDeferred -= OnPurchaseDeferred;

        storeController.OnStoreConnected += OnStoreConnected;
        storeController.OnStoreDisconnected += OnStoreDisconnected;
        storeController.OnProductsFetched += OnProductsFetched;
        storeController.OnProductsFetchFailed += OnProductsFetchFailed;
        storeController.OnPurchasesFetched += OnPurchasesFetched;
        storeController.OnPurchasesFetchFailed += OnPurchasesFetchFailed;
        storeController.OnPurchasePending += OnPurchasePending;
        storeController.OnPurchaseConfirmed += OnPurchaseConfirmed;
        storeController.OnPurchaseFailed += OnPurchaseFailed;
        storeController.OnPurchaseDeferred += OnPurchaseDeferred;
    }

    private void OnStoreConnected()
    {
        SetStatusMessage("Store connected callback received.");
    }

    private void OnStoreDisconnected(StoreConnectionFailureDescription failure)
    {
        SetStatusMessage($"Store disconnected: {failure.Message}, retryable: {failure.IsRetryable}");
        SetState(IapState.Failed);
    }

    private void OnProductsFetched(List<Product> products)
    {
        var removeAds = products.FirstOrDefault(IsRemoveAdsProduct);
        if (removeAds != null)
        {
            RemoveAdsLocalizedPrice = removeAds.metadata.localizedPriceString;
            SetStatusMessage($"Product ready: {GooglePlayProductIds.RemoveAds}, price: {RemoveAdsLocalizedPrice}");
            SetState(IapState.Ready);
            storeController.FetchPurchases();
        }
        else
        {
            var fetchedProductIds = products == null ? "null" : string.Join(", ", products.Select(product => product.definition.id));
            SetStatusMessage($"Product not found: {GooglePlayProductIds.RemoveAds}. Fetched: {fetchedProductIds}");
            Debug.LogError($"Unity IAP product not found: {GooglePlayProductIds.RemoveAds}. Fetched products: {fetchedProductIds}");
            SetState(IapState.Failed);
        }
    }

    private void OnProductsFetchFailed(ProductFetchFailed failure)
    {
        SetStatusMessage($"Product fetch failed: {failure.FailureReason}");
        Debug.LogError($"Unity IAP product fetch failed: {failure.FailureReason}");
        SetState(IapState.Failed);
    }

    private void OnPurchasesFetched(Orders orders)
    {
        var hasRemoveAds = ContainsRemoveAds(orders.ConfirmedOrders);

        foreach (var pendingOrder in orders.PendingOrders)
        {
            if (ContainsRemoveAds(pendingOrder))
            {
                hasRemoveAds = true;
                storeController.ConfirmPurchase(pendingOrder);
            }
        }

        RemoveAdsEntitlementStore.SetRemoveAds(hasRemoveAds);
        SetStatusMessage(hasRemoveAds ? "Remove ads purchase restored." : "No remove ads purchase found.");
        NotifyStateChanged();
    }

    private void OnPurchasesFetchFailed(PurchasesFetchFailureDescription failure)
    {
        SetStatusMessage($"Purchase fetch failed: {failure.FailureReason}, {failure.Message}");
        Debug.LogWarning($"Unity IAP purchase fetch failed: {failure.FailureReason}, {failure.Message}");
    }

    private void OnPurchasePending(PendingOrder order)
    {
        if (!ContainsRemoveAds(order))
        {
            SetStatusMessage("Pending purchase received for an unknown product.");
            Debug.LogWarning("Received a pending purchase for an unknown product.");
            return;
        }

        SetStatusMessage("Remove ads purchase pending. Confirming...");
        RemoveAdsEntitlementStore.SetRemoveAds(true);
        storeController.ConfirmPurchase(order);
    }

    private void OnPurchaseConfirmed(Order order)
    {
        IsPurchasing = false;

        switch (order)
        {
            case ConfirmedOrder confirmedOrder when ContainsRemoveAds(confirmedOrder):
                RemoveAdsEntitlementStore.SetRemoveAds(true);
                SetStatusMessage("Remove ads purchase confirmed.");
                Debug.Log("Remove ads purchase confirmed.");
                break;
            case FailedOrder failedOrder:
                SetStatusMessage($"Purchase confirmation failed: {failedOrder.FailureReason}, {failedOrder.Details}");
                Debug.LogError($"Remove ads purchase confirmation failed: {failedOrder.FailureReason}, {failedOrder.Details}");
                break;
        }

        NotifyStateChanged();
    }

    private void OnPurchaseFailed(FailedOrder order)
    {
        IsPurchasing = false;
        SetStatusMessage($"Purchase failed: {order.FailureReason}, {order.Details}");
        Debug.LogWarning($"Remove ads purchase failed: {order.FailureReason}, {order.Details}");
        NotifyStateChanged();
    }

    private void OnPurchaseDeferred(DeferredOrder order)
    {
        IsPurchasing = false;
        SetStatusMessage("Purchase was deferred by Google Play.");
        Debug.Log("Remove ads purchase was deferred by the store.");
        NotifyStateChanged();
    }

    private static bool ContainsRemoveAds(IEnumerable<Order> orders)
    {
        return orders != null && orders.Any(ContainsRemoveAds);
    }

    private static bool ContainsRemoveAds(Order order)
    {
        return order?.CartOrdered?.Items()?.Any(item => IsRemoveAdsProduct(item?.Product)) == true;
    }

    private static bool IsRemoveAdsProduct(Product product)
    {
        return product != null &&
               (product.definition.id == GooglePlayProductIds.RemoveAds ||
                product.definition.storeSpecificId == GooglePlayProductIds.RemoveAds);
    }
#endif

    private void LogEnvironment()
    {
        Debug.Log($"[GooglePlayIAP] Application.identifier: {Application.identifier}");
        Debug.Log($"[GooglePlayIAP] Product id: {GooglePlayProductIds.RemoveAds}");
        Debug.Log($"[GooglePlayIAP] Runtime platform: {Application.platform}");
        Debug.Log($"[GooglePlayIAP] Installer package: {GetInstallerPackageName()}");
    }

    private string GetInstallerPackageName()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var packageManager = activity.Call<AndroidJavaObject>("getPackageManager"))
            {
                var packageName = activity.Call<string>("getPackageName");
                var installerName = packageManager.Call<string>("getInstallerPackageName", packageName);
                return string.IsNullOrEmpty(installerName) ? "null" : installerName;
            }
        }
        catch (Exception exception)
        {
            return $"unknown ({exception.Message})";
        }
#else
        return "editor";
#endif
    }

    private void SetState(IapState state)
    {
        State = state;
        NotifyStateChanged();
    }

    private void SetStatusMessage(string message)
    {
        LastStatusMessage = message;
        Debug.Log($"[GooglePlayIAP] {message}");
        NotifyStateChanged();
    }

    private void NotifyStateChanged()
    {
        OnStateChanged?.Invoke();
    }
}
