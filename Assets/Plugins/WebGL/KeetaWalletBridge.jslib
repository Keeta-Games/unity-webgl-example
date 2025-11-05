mergeInto(LibraryManager.library, {
    DetectKeethingsWallet: function() {
        return typeof window.keeta !== 'undefined' ||
               typeof window.keethings !== 'undefined';
    },

    ConnectWalletAsync: function(gameObject, successCallback, errorCallback) {
        var objectName = UTF8ToString(gameObject);
        var successCb = UTF8ToString(successCallback);
        var errorCb = UTF8ToString(errorCallback);

        // Helper function to safely send message to Unity (with retry)
        function sendToUnity(callback, message) {
            var maxAttempts = 10;
            var attempts = 0;

            function trySend() {
                if (window.unityInstance && window.unityInstance.SendMessage) {
                    window.unityInstance.SendMessage(objectName, callback, message);
                    return true;
                }

                attempts++;
                if (attempts >= maxAttempts) {
                    console.error('[jslib] Unity instance not available after ' + maxAttempts + ' attempts');
                    return false;
                }

                setTimeout(trySend, 100);
            }

            trySend();
        }

        // Async wallet detection with retry logic
        function detectWalletWithRetry() {
            return new Promise(function(resolve, reject) {
                var attempts = 0;
                var maxAttempts = 6;

                function checkWallet() {
                    var wallet = window.keeta || window.keethings;

                    if (wallet) {
                        resolve(wallet);
                        return;
                    }

                    attempts++;

                    if (attempts >= maxAttempts) {
                        reject('WALLET_NOT_FOUND');
                        return;
                    }

                    var delay = attempts === 1 ? 500 : 2000;
                    setTimeout(checkWallet, delay);
                }

                checkWallet();
            });
        }

        detectWalletWithRetry()
            .then(function(wallet) {
                return wallet.request({ method: 'keeta_requestAccounts' });
            })
            .then(function(accounts) {
                if (accounts && accounts.length > 0) {
                    sendToUnity(successCb, accounts[0]);
                } else {
                    sendToUnity(errorCb, 'NO_ACCOUNTS');
                }
            })
            .catch(function(error) {
                if (error === 'WALLET_NOT_FOUND') {
                    sendToUnity(errorCb, 'WALLET_NOT_FOUND');
                } else {
                    sendToUnity(errorCb, 'CONNECTION_REJECTED');
                }
            });
    },

    GetKTABalance: function(address, gameObject, callback) {
        var addressStr = UTF8ToString(address);
        var objectName = UTF8ToString(gameObject);
        var callbackName = UTF8ToString(callback);

        // Helper function to safely send message to Unity (with retry)
        function sendToUnity(message) {
            var maxAttempts = 10;
            var attempts = 0;

            function trySend() {
                if (window.unityInstance && window.unityInstance.SendMessage) {
                    window.unityInstance.SendMessage(objectName, callbackName, message);
                    return true;
                }

                attempts++;
                if (attempts >= maxAttempts) {
                    console.error('[jslib] Unity instance not available after ' + maxAttempts + ' attempts');
                    return false;
                }

                setTimeout(trySend, 100);
            }

            trySend();
        }

        // Wait for SDK to be ready
        function waitForSDK(callback) {
            var maxAttempts = 50;
            var attempts = 0;

            function checkSDK() {
                if (typeof KeetaClient !== 'undefined') {
                    callback();
                    return;
                }

                attempts++;
                if (attempts >= maxAttempts) {
                    console.error('[jslib] Keeta Client SDK not loaded');
                    sendToUnity('SDK_NOT_LOADED');
                    return;
                }

                setTimeout(checkSDK, 100);
            }

            checkSDK();
        }

        // Wait for SDK, then fetch balance
        waitForSDK(function() {
            (async function() {
                try {
                    var SDK = KeetaClient.default || KeetaClient;
                    var Client = SDK.Client;

                    if (!Client) {
                        console.error('[jslib] Client not found in SDK');
                        sendToUnity('SDK_STRUCTURE_ERROR');
                        return;
                    }

                    var client = await Client.fromNetwork('test');
                    var accountInfo = await client.getAccountInfo(addressStr, 'ANY');

                    if (!accountInfo || !accountInfo.balances || accountInfo.balances.length === 0) {
                        sendToUnity('0');
                        await client.destroy();
                        return;
                    }

                    var ktaBalance = accountInfo.balances[0].balance;
                    var balanceStr = (typeof ktaBalance === 'bigint') ?
                        ktaBalance.toString() :
                        (ktaBalance ? ktaBalance.toString() : '0');

                    sendToUnity(balanceStr);
                    await client.destroy();

                } catch (error) {
                    console.error('[jslib] Error fetching balance:', error.message);
                    sendToUnity('ERROR');
                }
            })();
        });
    },

    SetupAccountChangeListener: function(gameObject, callback) {
        var objectName = UTF8ToString(gameObject);
        var callbackName = UTF8ToString(callback);

        // Helper function to safely send message to Unity (with retry)
        function sendToUnity(message) {
            var maxAttempts = 10;
            var attempts = 0;

            function trySend() {
                if (window.unityInstance && window.unityInstance.SendMessage) {
                    window.unityInstance.SendMessage(objectName, callbackName, message);
                    return true;
                }

                attempts++;
                if (attempts >= maxAttempts) {
                    console.error('[jslib] Unity instance not available after ' + maxAttempts + ' attempts');
                    return false;
                }

                setTimeout(trySend, 100);
            }

            trySend();
        }

        var wallet = window.keeta || window.keethings;

        if (wallet && wallet.on) {
            wallet.on('accountsChanged', function(accounts) {
                if (accounts.length === 0) {
                    sendToUnity('DISCONNECTED');
                } else {
                    sendToUnity(accounts[0]);
                }
            });
        }
    }
});
