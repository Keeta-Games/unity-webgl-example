using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class WalletManager : MonoBehaviour
{
    // Events
    public event Action<string> OnWalletConnected;
    public event Action<string> OnWalletDisconnected;
    public event Action<string> OnBalanceUpdated;
    public event Action<string> OnError;

    // State
    private string currentAddress = null;
    private string currentBalance = null;
    private bool isConnected = false;

    // Singleton pattern
    public static WalletManager Instance { get; private set; }

    // External JavaScript functions
    #if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern bool DetectKeethingsWallet();

    [DllImport("__Internal")]
    private static extern void ConnectWalletAsync(string gameObject, string successCallback, string errorCallback);

    [DllImport("__Internal")]
    private static extern void GetKTABalance(string address, string gameObject, string callback);

    [DllImport("__Internal")]
    private static extern void SetupAccountChangeListener(string gameObject, string callback);
    #endif

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
        // Setup account change listener
        SetupAccountChangeListener(gameObject.name, nameof(HandleAccountChanged));
        #endif

        Debug.Log("WalletManager initialized");
    }

    // Public API
    public void ConnectWallet()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
        Debug.Log("Requesting wallet connection (will retry if extension is still loading)...");
        ConnectWalletAsync(gameObject.name, nameof(HandleWalletConnected), nameof(HandleConnectionError));
        #else
        Debug.LogWarning("ConnectWallet only works in WebGL builds. Please build to WebGL to test wallet functionality.");
        OnError?.Invoke("Wallet connection only available in WebGL builds");
        #endif
    }

    public void DisconnectWallet()
    {
        currentAddress = null;
        currentBalance = null;
        isConnected = false;
        OnWalletDisconnected?.Invoke("Wallet disconnected");
        Debug.Log("Wallet disconnected");
    }

    public void RefreshBalance()
    {
        if (!isConnected || string.IsNullOrEmpty(currentAddress))
        {
            OnError?.Invoke("No wallet connected");
            return;
        }

        #if UNITY_WEBGL && !UNITY_EDITOR
        Debug.Log($"Fetching balance for {currentAddress}...");
        GetKTABalance(currentAddress, gameObject.name, nameof(HandleBalanceReceived));
        #else
        Debug.LogWarning("RefreshBalance only works in WebGL builds. Please build to WebGL to test wallet functionality.");
        OnError?.Invoke("Balance queries only available in WebGL builds");
        #endif
    }

    public bool IsConnected() => isConnected;
    public string GetAddress() => currentAddress;
    public string GetBalance() => currentBalance;

    // Callbacks called from JavaScript via SendMessage
    public void HandleWalletConnected(string address)
    {
        Debug.Log($"Wallet connected: {address}");
        currentAddress = address;
        isConnected = true;
        OnWalletConnected?.Invoke(address);

        // Automatically fetch balance after connection
        RefreshBalance();
    }

    public void HandleConnectionError(string error)
    {
        Debug.LogError($"Connection error: {error}");
        string userMessage = error switch
        {
            "WALLET_NOT_FOUND" => "Keethings Wallet not found. Please install the extension.",
            "CONNECTION_REJECTED" => "Connection request rejected by user.",
            "NO_ACCOUNTS" => "No accounts found in wallet.",
            _ => $"Failed to connect wallet: {error}"
        };
        OnError?.Invoke(userMessage);

        // Open Chrome Web Store if wallet not found
        #if UNITY_WEBGL && !UNITY_EDITOR
        if (error == "WALLET_NOT_FOUND")
        {
            Application.ExternalEval("window.open('https://chromewebstore.google.com/detail/keythings-wallet/jhngbkboonmpephhenljbljnpffabloh', '_blank')");
        }
        #endif
    }

    public void HandleBalanceReceived(string balance)
    {
        Debug.Log($"Balance received: {balance}");

        if (balance == "ERROR" || balance == "SDK_NOT_LOADED" || balance == "SDK_STRUCTURE_ERROR")
        {
            string errorMsg = balance switch
            {
                "SDK_NOT_LOADED" => "Keeta SDK is still loading. Please try again.",
                "SDK_STRUCTURE_ERROR" => "Keeta SDK version mismatch. Please refresh the page.",
                _ => "Failed to fetch balance from Keeta Network"
            };
            OnError?.Invoke(errorMsg);
            return;
        }

        currentBalance = balance;
        OnBalanceUpdated?.Invoke(balance);
    }

    public void HandleAccountChanged(string newAddressOrStatus)
    {
        Debug.Log($"Account changed: {newAddressOrStatus}");

        if (newAddressOrStatus == "DISCONNECTED")
        {
            DisconnectWallet();
        }
        else
        {
            // Account switched
            currentAddress = newAddressOrStatus;
            OnWalletConnected?.Invoke(newAddressOrStatus);
            RefreshBalance();
        }
    }
}
