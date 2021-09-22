namespace Didimo.Networking
{
    /// <summary>
    /// Use this class when you expect an empty response from the server. E.g. you only care that it returns an http status of
    /// success (200, 201, etc).
    /// When we expect a response of this type, we won't try to parse the text returned by the server.
    /// </summary>
    public class DidimoEmptyResponse
    {
    }
}