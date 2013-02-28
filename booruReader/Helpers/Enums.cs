
namespace booruReader.Helpers
{
    public enum PostRating
    {
        Safe,
        Questionable,
        Explicit
    };

    /// <summary>
    /// This aims to generalise some of the api structuring.
    /// </summary>
    public enum ProviderAccessType
    {
        XML,
        JSON,
        Gelbooru,
        DanbooruV2,
        Sankaku,
        INVALID
    };
}
