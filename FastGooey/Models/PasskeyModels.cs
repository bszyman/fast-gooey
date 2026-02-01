using Fido2NetLib;

namespace FastGooey.Models;

public record PasskeyRegistrationStartRequest(string? DisplayName);

public record PasskeyRegistrationFinishRequest(
    string RequestId,
    AuthenticatorAttestationRawResponse AttestationResponse,
    string? DisplayName);

public record PasskeyAssertionStartRequest(string? Email);

public record PasskeyAssertionFinishRequest(
    string RequestId,
    AuthenticatorAssertionRawResponse AssertionResponse);

public record PasskeyOptionsResponse(string RequestId, CredentialCreateOptions Options);

public record PasskeyAssertionOptionsResponse(string RequestId, AssertionOptions Options);
