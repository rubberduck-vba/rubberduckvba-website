using rubberduckvba.Server.Model;

namespace rubberduckvba.Server.Api.Tags;

public record struct LatestTagsViewModel
{
    /* corresponding client-side model
     * 
        export interface LatestTags {
          main: Tag;
          next: Tag;
        }
     */

    public LatestTagsViewModel() { }
    public LatestTagsViewModel(Tag main, Tag? next)
    {
        Main = new TagViewModel(main);
        if (next != null)
        {
            Next = new TagViewModel(next);
        }
    }

    public TagViewModel Main { get; init; }
    public TagViewModel? Next { get; init; }
}