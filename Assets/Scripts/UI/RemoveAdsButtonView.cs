using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class RemoveAdsButtonView : MonoBehaviour
    {
        [SerializeField]
        private Button purchaseButton;

        [SerializeField]
        private TMP_Text titleText;

        [SerializeField]
        private TMP_Text priceText;

        [SerializeField]
        private string readyText = "Remove Ads";

        [SerializeField]
        private string loadingText = "Loading...";

        [SerializeField]
        private string purchasedText = "Purchased";

        [SerializeField]
        private string unavailableText = "Unavailable";

        [SerializeField]
        private bool showStatusMessageInPriceText = true;

        private void Awake()
        {
#if UNITY_ANDROID
                gameObject.SetActive(true);
#else
                gameObject.SetActive(false);
#endif
            
            if (purchaseButton == null)
            {
                purchaseButton = GetComponent<Button>();
            }

            if (purchaseButton != null)
            {
                purchaseButton.onClick.AddListener(OnClickPurchase);
            }
        }

        private void OnEnable()
        {
            if (!GooglePlayIapManager.IsSupported)
            {
                gameObject.SetActive(false);
                return;
            }

            GooglePlayIapManager.Instance.OnStateChanged += UpdateState;
            RemoveAdsEntitlementStore.OnEntitlementChanged += OnEntitlementChanged;
            UpdateState();
        }

        private void OnDisable()
        {
            if (GooglePlayIapManager.IsSupported && GooglePlayIapManager.HasInstance)
            {
                GooglePlayIapManager.Instance.OnStateChanged -= UpdateState;
            }

            RemoveAdsEntitlementStore.OnEntitlementChanged -= OnEntitlementChanged;
        }

        private void OnClickPurchase()
        {
            GooglePlayIapManager.Instance.PurchaseRemoveAds();
            UpdateState();
        }

        private void OnEntitlementChanged(bool hasRemoveAds)
        {
            UpdateState();
        }

        private void UpdateState()
        {
            var manager = GooglePlayIapManager.Instance;
            var hasRemoveAds = RemoveAdsEntitlementStore.HasRemoveAds;

            if (purchaseButton != null)
            {
                purchaseButton.interactable = manager.IsReady && !manager.IsPurchasing && !hasRemoveAds;
            }

            if (titleText != null)
            {
                titleText.text = GetTitleText(manager, hasRemoveAds);
            }

            if (priceText != null)
            {
                priceText.text = GetPriceText(manager, hasRemoveAds);
            }
        }

        private string GetPriceText(GooglePlayIapManager manager, bool hasRemoveAds)
        {
            if (hasRemoveAds)
            {
                return string.Empty;
            }

            if (!string.IsNullOrEmpty(manager.RemoveAdsLocalizedPrice))
            {
                return manager.RemoveAdsLocalizedPrice;
            }

            return showStatusMessageInPriceText ? manager.LastStatusMessage : string.Empty;
        }

        private string GetTitleText(GooglePlayIapManager manager, bool hasRemoveAds)
        {
            if (hasRemoveAds)
            {
                return purchasedText;
            }

            if (manager.IsPurchasing || manager.State == GooglePlayIapManager.IapState.Initializing)
            {
                return loadingText;
            }

            if (manager.State == GooglePlayIapManager.IapState.Failed ||
                manager.State == GooglePlayIapManager.IapState.Unsupported)
            {
                return unavailableText;
            }

            return readyText;
        }
    }
}
