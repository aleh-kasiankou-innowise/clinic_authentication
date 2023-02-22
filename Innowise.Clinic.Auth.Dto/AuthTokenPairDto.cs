namespace Innowise.Clinic.Auth.Dto;

/// <summary>
///     A pair of tokens used for authentication and authorizations.
///     A short-living security token and long-living refresh token
/// </summary>
public class AuthTokenPairDto
{
    public AuthTokenPairDto(string securityToken, string refreshToken)
    {
        SecurityToken = securityToken;
        RefreshToken = refreshToken;
    }

    /// <summary>
    ///     Short-living security token for authentication and authorization.
    /// </summary>
    /// <example>
    ///     eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.
    ///     eyJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLz
    ///     IwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3ByaW1hcnlzaWQi
    ///     OiJkZDFjYThlNi01ZmUyLTRjYzctY2M5OS0wOGRiMDdhZW
    ///     ViMmIiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29t
    ///     L3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOi
    ///     JQYXRpZW50IiwiZXhwIjoxNjc1NjI2MTA1LCJpc3MiOiJo
    ///     dHRwOi8vbG9jYWxob3N0OjI1NTcwIn0.acLuMB4OHsW4at
    ///     8Y9hmXNwopYDxbcZ8TSiAvWVgf7L8
    /// </example>
    public string SecurityToken { get; }

    /// <summary>
    ///     Long-living token for refreshing security tokens and logging out
    /// </summary>
    /// <example>
    ///     eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.
    ///     eyJqdGkiOiI0OThlN2QwMy1kNzUzLTRjOGMtYmVjMS0wOG
    ///     RiMDdhZWViNzkiLCJodHRwOi8vc2NoZW1hcy5taWNyb3Nv
    ///     ZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3
    ///     ByaW1hcnlzaWQiOiJkZDFjYThlNi01ZmUyLTRjYzctY2M5
    ///     OS0wOGRiMDdhZWViMmIiLCJleHAiOjE2NzU4ODQ0MDUsIm
    ///     lzcyI6Imh0dHA6Ly9sb2NhbGhvc3Q6MjU1NzAifQ.FCTTM
    ///     cvg9O8AhF7yHWcONuUBaGn6vGsys73dqz1jj8s
    /// </example>
    /// >
    public string RefreshToken { get; }
}