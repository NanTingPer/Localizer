#nullable enable
using System.Collections.Generic;
using System.Text.Json.Serialization;
namespace Localizer.DataModel;

public class FileCommitResponse
{
    [JsonPropertyName("content")]
    public FileContent? Content { get; set; }

    [JsonPropertyName("commit")]
    public CommitInfo Commit { get; set; } = null!;
}

public class FileContent
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("path")]
    public string? Path { get; set; }

    [JsonPropertyName("sha")]
    public string? Sha { get; set; }

    [JsonPropertyName("size")]
    public int? Size { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("html_url")]
    public string? HtmlUrl { get; set; }

    [JsonPropertyName("git_url")]
    public string? GitUrl { get; set; }

    [JsonPropertyName("download_url")]
    public string? DownloadUrl { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("_links")]
    public FileLinks? Links { get; set; }
}

public class FileLinks
{
    [JsonPropertyName("self")]
    public string? Self { get; set; }

    [JsonPropertyName("git")]
    public string? Git { get; set; }

    [JsonPropertyName("html")]
    public string? Html { get; set; }
}

public class CommitInfo
{
    [JsonPropertyName("sha")]
    public string? Sha { get; set; }

    [JsonPropertyName("node_id")]
    public string? NodeId { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("html_url")]
    public string? HtmlUrl { get; set; }

    [JsonPropertyName("author")]
    public CommitPerson? Author { get; set; }

    [JsonPropertyName("committer")]
    public CommitPerson? Committer { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("tree")]
    public CommitTree? Tree { get; set; }

    [JsonPropertyName("parents")]
    public List<CommitParent>? Parents { get; set; }

    [JsonPropertyName("verification")]
    public Verification? Verification { get; set; }
}

public class CommitPerson
{
    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }
}

public class CommitTree
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("sha")]
    public string? Sha { get; set; }
}

public class CommitParent
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("html_url")]
    public string? HtmlUrl { get; set; }

    [JsonPropertyName("sha")]
    public string? Sha { get; set; }
}

public class Verification
{
    [JsonPropertyName("verified")]
    public bool Verified { get; set; }

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }

    [JsonPropertyName("signature")]
    public string? Signature { get; set; }

    [JsonPropertyName("payload")]
    public string? Payload { get; set; }

    [JsonPropertyName("verified_at")]
    public string? VerifiedAt { get; set; }
}
