using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button connectButton;
    [SerializeField] private Button disconnectButton;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI addressText;
    [SerializeField] private TextMeshProUGUI balanceText;

    [Header("Settings")]
    [SerializeField] private Color connectedColor = Color.green;
    [SerializeField] private Color disconnectedColor = Color.red;
    [SerializeField] private Color errorColor = Color.yellow;

    private void Start()
    {
        // Subscribe to WalletManager events
        WalletManager.Instance.OnWalletConnected += HandleWalletConnected;
        WalletManager.Instance.OnWalletDisconnected += HandleWalletDisconnected;
        WalletManager.Instance.OnBalanceUpdated += HandleBalanceUpdated;
        WalletManager.Instance.OnError += HandleError;

        // Setup button listeners
        connectButton.onClick.AddListener(OnConnectButtonClicked);
        disconnectButton.onClick.AddListener(OnDisconnectButtonClicked);

        // Initial state
        UpdateUIState(false);
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (WalletManager.Instance != null)
        {
            WalletManager.Instance.OnWalletConnected -= HandleWalletConnected;
            WalletManager.Instance.OnWalletDisconnected -= HandleWalletDisconnected;
            WalletManager.Instance.OnBalanceUpdated -= HandleBalanceUpdated;
            WalletManager.Instance.OnError -= HandleError;
        }

        // Remove button listeners
        connectButton.onClick.RemoveListener(OnConnectButtonClicked);
        disconnectButton.onClick.RemoveListener(OnDisconnectButtonClicked);
    }

    private void OnConnectButtonClicked()
    {
        Debug.Log("Connect button clicked");
        statusText.text = "Wallet Status: Connecting...";
        statusText.color = Color.white;
        WalletManager.Instance.ConnectWallet();
    }

    private void OnDisconnectButtonClicked()
    {
        Debug.Log("Disconnect button clicked");
        WalletManager.Instance.DisconnectWallet();
    }

    private void HandleWalletConnected(string address)
    {
        Debug.Log($"UI: Wallet connected - {address}");
        statusText.text = "Wallet Status: Connected";
        statusText.color = connectedColor;
        addressText.text = $"Address: {address}";
        balanceText.text = "KTA Balance: Loading...";
        UpdateUIState(true);
    }

    private void HandleWalletDisconnected(string message)
    {
        Debug.Log($"UI: Wallet disconnected - {message}");
        statusText.text = "Wallet Status: Not Connected";
        statusText.color = disconnectedColor;
        addressText.text = "Address: -";
        balanceText.text = "KTA Balance: -";
        UpdateUIState(false);
    }

    private void HandleBalanceUpdated(string balance)
    {
        Debug.Log($"UI: Balance updated - {balance}");
        // Format balance (assuming balance is in smallest unit)
        if (decimal.TryParse(balance, out decimal balanceDecimal))
        {
            // Convert from smallest unit to KTA (KTA uses 9 decimal places)
            decimal ktaBalance = balanceDecimal / 1000000000m; // Divide by 10^9
            balanceText.text = $"KTA Balance: {ktaBalance:F9} KTA";
        }
        else
        {
            balanceText.text = $"KTA Balance: {balance}";
        }
    }

    private void HandleError(string errorMessage)
    {
        Debug.LogError($"UI: Error - {errorMessage}");
        statusText.text = $"Error: {errorMessage}";
        statusText.color = errorColor;
    }

    private void UpdateUIState(bool connected)
    {
        connectButton.gameObject.SetActive(!connected);
        disconnectButton.gameObject.SetActive(connected);
    }
}
