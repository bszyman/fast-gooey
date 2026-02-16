const { startRegistration, startAuthentication } = SimpleWebAuthnBrowser;

(() => {

  const getAntiForgeryToken = () => {
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    return tokenInput ? tokenInput.value : null;
  };
  
  const postJson = async (url, payload) => {
    const headers = { "Content-Type": "application/json" };
    const token = getAntiForgeryToken();
    if (token) {
      headers.RequestVerificationToken = token;
    }

    const response = await fetch(url, {
      method: "POST",
      headers,
      body: JSON.stringify(payload)
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({}));
      throw new Error(error.message || "Request failed.");
    }

    return response.json();
  };
  
  const getOptionsFromResponse = (response) => {
    return {
      requestId: response.requestId || response.RequestId,
      optionsJSON: response.options || response.Options
    };
  };
  
  const handlePasskeyLogin = async (button) => {
    if (!window.PublicKeyCredential) {
      alert("Passkeys are not supported in this browser.");
      return;
    }

    const beginUrl = button.dataset.passkeyLoginUrl;
    const completeUrl = button.dataset.passkeyLoginCompleteUrl;
    const emailInputId = button.dataset.passkeyEmailInputId || "Email";
    const emailInput = document.getElementById(emailInputId);
    const email = emailInput ? emailInput.value.trim() : "";

    const beginResponse = await postJson(beginUrl, { email });
    const { requestId, optionsJSON } = getOptionsFromResponse(beginResponse);
    if (!optionsJSON) {
      throw new Error("Missing passkey options.");
    }

    const assertionResponse = await startAuthentication({ optionsJSON });

    const result = await postJson(completeUrl, {
      requestId,
      assertionResponse  // Already JSON-serialized
    });
    
    if (result.redirectUrl) {
      window.location = result.redirectUrl;
    }
  };

  const handlePasskeyRegistration = async (button) => {
    if (!window.PublicKeyCredential) {
      alert("Passkeys are not supported in this browser.");
      return;
    }

    const beginUrl = button.dataset.passkeyRegisterUrl;
    const completeUrl = button.dataset.passkeyRegisterCompleteUrl;
    const displayNameId = button.dataset.passkeyDisplayNameId || "passkeyDisplayName";
    const displayNameInput = document.getElementById(displayNameId);
    const displayName = displayNameInput ? displayNameInput.value.trim() : "";

    const beginResponse = await postJson(beginUrl, { displayName });
    const { requestId, optionsJSON } = getOptionsFromResponse(beginResponse);
    if (!optionsJSON) {
      throw new Error("Missing passkey options.");
    }
    
    const attestationResponse = await startRegistration({ optionsJSON });

    await postJson(completeUrl, {
      requestId,
      attestationResponse,  // Already JSON-serialized by the library
      displayName
    });

    window.location.reload();
  };

  const wireHandlers = () => {
    document.querySelectorAll("[data-passkey-login-url]").forEach((button) => {
      button.addEventListener("click", () => {
        handlePasskeyLogin(button).catch((error) => {
          alert(error.message || "Passkey login failed.");
        });
      });
    });

    document.querySelectorAll("[data-passkey-register-url]").forEach((button) => {
      button.addEventListener("click", () => {
        handlePasskeyRegistration(button).catch((error) => {
          alert(error.message || "Passkey registration failed.");
        });
      });
    });
  };

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", wireHandlers);
  } else {
    wireHandlers();
  }
})();
